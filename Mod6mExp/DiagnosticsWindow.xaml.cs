using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Machine.Common;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Machine
{
    public partial class DiagnosticsWindow : Window, INotifyPropertyChanged
    {
        [System.ComponentModel.Browsable(false)]
        public new System.Windows.Forms.Control Parent { get; set; }

        MainWindow ui;
        DispatcherTimer timerRefresh;
        TabItem SelectedTabItem;
        bool RefreshHvaluesOnce = false;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value,
        [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private string _errorText;
        public string ErrorText
        {
            get { return _errorText; }
            set { SetField(ref _errorText, value); }
        }

        public DiagnosticsWindow(MainWindow uiInstance)
        {
            InitializeComponent();
            ui = uiInstance;
            this.Top = 0;
            this.Left = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CognexCameraUC.AttachCognexToUI(ref MainWindow.CognexCameras, true, false);         //TODO: attach the "true, false" to Properites.Settings... 

            dataGridMarkLaser.ItemsSource = MainWindow.ListMLmess;
            ReloadControlsValues();
            btnSetFirmwareSettings.IsEnabled = false;

            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
            {
                comboBoxPortRFID.Items.Add(s);
                comboBoxPortBarcode.Items.Add(s);
                comboBoxPortMarkLaser.Items.Add(s);
            }

            timerRefresh = new DispatcherTimer();
            timerRefresh.Tick += new EventHandler(timerRefresh_Tick);
            timerRefresh.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timerRefresh.Start();

            bitsCommonInputs1.ItemsSource = UpdateIO.PlcCommonInputs1;
            bitsCommonOutputs.ItemsSource = UpdateIO.PlcCommonOutputs;
            bitsPos1Inputs.ItemsSource = UpdateIO.PlcPos1Inputs;
            bitsPos2Inputs.ItemsSource = UpdateIO.PlcPos2Inputs;
            bitsPos3Inputs.ItemsSource = UpdateIO.PlcPos3Inputs;
            bitsPos1Outputs.ItemsSource = UpdateIO.PlcPos1Outputs;
            bitsPos2Outputs.ItemsSource = UpdateIO.PlcPos2Outputs;
            bitsPos3Outputs.ItemsSource = UpdateIO.PlcPos3Outputs;
            bitsFestoInputs.ItemsSource = UpdateIO.PlcFestoInputs;
            bitsFestoOutputs.ItemsSource = UpdateIO.PlcFestoOutputs;
            bitsLinMotInputs.ItemsSource = UpdateIO.PlcLinMotInputs;
            bitsLinMotOutputs.ItemsSource = UpdateIO.PlcLinMotOutputs;
            bitsWeldLasInputs.ItemsSource = UpdateIO.PlcWeldLasInputs;
            bitsWeldLasOutputs.ItemsSource = UpdateIO.PlcWeldLasOutputs;
            bitsMarkLasInputs.ItemsSource = UpdateIO.PlcMarkLasInputs;
            gridRotationEnabled.DataContext = MainWindow.updIO;
            tbError.DataContext = this;
            tbFestoCurrentPos.DataContext = MainWindow.updIO;
            tbLinMotCurrentPos.DataContext = MainWindow.updIO;
            lblStateLM.DataContext = MainWindow.updIO;
            lblStateFE.DataContext = MainWindow.updIO;

            // за да може да премества прозорец без рамка
            MouseDown += new MouseButtonEventHandler(TittleBar_MouseDown);
            MouseUp += new MouseButtonEventHandler(TittleBar_MouseUp);
            MouseMove += new MouseEventHandler(TittleBar_MouseMove);
        }

        #region Преместване на прозореца

        bool WindowMoving = false;
        Point WindowMovingStartPoint = new Point();

        void TittleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (WindowMoving)
            {
                Point p1 = e.GetPosition(this);
                Point p2 = this.PointToScreen(p1);
                Point p3 = new Point(p2.X - WindowMovingStartPoint.X, p2.Y - WindowMovingStartPoint.Y);
                this.Left = p3.X;
                this.Top = p3.Y;
            }
        }

        void TittleBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            WindowMoving = false;
        }

        void TittleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.GetPosition(this).Y <= 60)
            {
                WindowMoving = true;
                WindowMovingStartPoint = e.GetPosition(this);
            }
        }
        #endregion

        private void UpdateScrollBar(ListBox listBox)
        {
            if (listBox != null)
            {
                var border = (Border)VisualTreeHelper.GetChild(listBox, 0);
                var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }
        void timerRefresh_Tick(object sender, EventArgs e)
        {
            if (!(bool)cbStopScrolling.IsChecked && MainWindow.ListMLmess.Count > 1) dataGridMarkLaser.ScrollIntoView(MainWindow.ListMLmess[MainWindow.ListMLmess.Count - 1]);
            SelectedTabItem = (TabItem)tabControlDiag.SelectedItem;
            if (MainWindow.TCP.ValidTableState())
            {
                if (MainWindow.updIO.iCommonStation1) { gbPos1Title.Header = "Load (1)"; gbPos2Title.Header = "Weld (3)"; gbPos3Title.Header = "Mark (2)"; }
                else if (MainWindow.updIO.iCommonStation2) { gbPos1Title.Header = "Load (2)"; gbPos2Title.Header = "Weld (1)"; gbPos3Title.Header = "Mark (3)"; }
                else if (MainWindow.updIO.iCommonStation3) { gbPos1Title.Header = "Load (3)"; gbPos2Title.Header = "Weld (2)"; gbPos3Title.Header = "Mark (1)"; }
                else { gbPos1Title.Header = "Pos.1 -> ?"; gbPos2Title.Header = "Pos.2 -> ?"; gbPos3Title.Header = "Pos.3 -> ?"; return; }
            }
            btnSetFirmwareSettings.IsEnabled = (MainWindow.plc.Connected && MainWindow.TCP.ValidHvalues);
            if (MainWindow.TCP.ValidHvalues && RefreshHvaluesOnce)
            {
                RefreshHvaluesOnce = false;
                cbUseMarking.IsChecked = (TCPcom.hMemory[0] & 0x0001) != 0;
                cbUseWelding.IsChecked = (TCPcom.hMemory[0] & 0x0002) != 0;
                cbUseStation1.IsChecked = (TCPcom.hMemory[0] & 0x2000) != 0;
                cbUseStation2.IsChecked = (TCPcom.hMemory[0] & 0x4000) != 0;
                cbUseStation3.IsChecked = (TCPcom.hMemory[0] & 0x8000) != 0;
                tbWeldingProgram.Text = TCPcom.hMemory[1].ToString();
                tbFestoVelocity.Text = TCPcom.hMemory[5].ToString();
                tbTotalHeight.Text = ((float)(TCPcom.hMemory[6] + (TCPcom.hMemory[7] << 16)) / 10000).ToString("F2");
                tbLinMotVelocity.Text = ((float)(TCPcom.hMemory[8] + (TCPcom.hMemory[9] << 16)) / 1000).ToString("F2");
                tbProductLength.Text = ((float)(TCPcom.hMemory[10] + (TCPcom.hMemory[11] << 16)) / 10000).ToString("F2");
                tbCameraCapture.Text = ((float)(TCPcom.hMemory[12] + (TCPcom.hMemory[13] << 16)) / 10000).ToString("F2");
                tbLinMotAccDec.Text = ((float)(TCPcom.hMemory[14] + (TCPcom.hMemory[15] << 16)) / 100).ToString("F2");
                ErrorText = "";
            }
        }

        private void ReloadControlsValues()
        {
            textBoxMachineID.Text = Properties.Settings.Default.MachineID;
            textBoxTraceabilityServerIP.Text = Properties.Settings.Default.TraceabilityServerIP;
            textBoxProductsServerIP.Text = Properties.Settings.Default.ProductsServerIP;
            textBoxMonitoringServerIP.Text = Properties.Settings.Default.MonitoringServerIP;
            textBoxServerDBname.Text = Properties.Settings.Default.TraceabilityDBName;
            textBoxLocalDBname.Text = Properties.Settings.Default.LocalDBname;
            textBoxTableProductsName.Text = Properties.Settings.Default.TableProductsName;
            textBoxTableSensorsName.Text = Properties.Settings.Default.TableSensorsName;
            comboBoxPortRFID.Text = Properties.Settings.Default.PortRFID;
            comboBoxPortBarcode.Text = Properties.Settings.Default.PortBarcode;
            comboBoxPortMarkLaser.Text = Properties.Settings.Default.PortMarkLaser;
            cbProcessFlowLog.IsChecked = Properties.Settings.Default.ProcessFlowLog;
            cbShowErrors.IsChecked = Properties.Settings.Default.ShowCloseLotErrors;
            cbRomanShifts.IsChecked = Properties.Settings.Default.RomanShifts;
            cbTraceabilityEnabled.IsChecked = Properties.Settings.Default.Traceability;
            cbShowEndLotWindow.IsChecked = Properties.Settings.Default.ShowEndLotWindow;
            ui.TraceabilityEnabled = Properties.Settings.Default.Traceability;
            CameraOffset.Text = Convert.ToString(Properties.Settings.Default.CameraOffset);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (MainWindow.CognexCameras != null && MainWindow.CognexCameras.RunningContinuous) MainWindow.CognexCameras.StopVisionRunning();

            Properties.Settings.Default.MachineID = textBoxMachineID.Text;
            Properties.Settings.Default.TraceabilityServerIP = textBoxTraceabilityServerIP.Text;
            Properties.Settings.Default.ProductsServerIP = textBoxProductsServerIP.Text;
            Properties.Settings.Default.MonitoringServerIP = textBoxMonitoringServerIP.Text;
            Properties.Settings.Default.TraceabilityDBName = textBoxServerDBname.Text;
            Properties.Settings.Default.LocalDBname = textBoxLocalDBname.Text;
            Properties.Settings.Default.TableProductsName = textBoxTableProductsName.Text;
            Properties.Settings.Default.TableSensorsName = textBoxTableSensorsName.Text;
            Properties.Settings.Default.PortRFID = comboBoxPortRFID.Text;
            Properties.Settings.Default.PortBarcode = comboBoxPortBarcode.Text;
            Properties.Settings.Default.PortMarkLaser = comboBoxPortMarkLaser.Text;
            Properties.Settings.Default.ProcessFlowLog = cbProcessFlowLog.IsChecked == true;
            Properties.Settings.Default.ShowCloseLotErrors = cbShowErrors.IsChecked == true;
            Properties.Settings.Default.RomanShifts = cbRomanShifts.IsChecked == true;
            Properties.Settings.Default.Traceability = cbTraceabilityEnabled.IsChecked == true;
            Properties.Settings.Default.ShowEndLotWindow = cbShowEndLotWindow.IsChecked == true;
            ui.TraceabilityEnabled = cbTraceabilityEnabled.IsChecked == true;
            Properties.Settings.Default.CameraOffset = Convert.ToInt32(CameraOffset.Text);
            Properties.Settings.Default.Save();
            timerRefresh.Stop();
        }

        private void tabControlDiag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void dataGridRFID_AutoGeneratedColumns(object sender, EventArgs e)
        {
            //Gen.SetupDataGridColumnsWidth(dataGridRFID, new string[] { "Име", "Ниво на достъп" }, new int[] { 130 });
        }

        private void LedOutput_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LedIndicator2 li = (LedIndicator2)sender;
            IObitDetails bd = (IObitDetails)((LedIndicator2)sender).DataContext;
            //set/reset here the individual output by direct command to PLC -> 
            //  it will send it to the PLC -> output port will be read back by TCP/IP and 
            //  it will change the bit to be displayed
            //DO NOT set any bits in UpdateIO class for it will mess with the PLC polling thread which reads the CIO area !!!
            bd.BitValue = !bd.BitValue; //toggle output bit
            UpdateIO.ModifyOutputBit(bd);
        }

        private void buttonRestoreSettings_Click(object sender, RoutedEventArgs e)
        {
            AppSettings ap = new AppSettings(Properties.Settings.Default.PropertyValues, Properties.Settings.Default.Properties);
            RestoreSettingsWindow dlg = new RestoreSettingsWindow(
                Properties.Settings.Default.MachineID,
                ap);
            dlg.ShowDialog();
            ReloadControlsValues();
        }

        public static float GetRadioValue(StackPanel sp)
        {
            float stepValue = 0f;
            var checkedButton = sp.Children.OfType<RadioButton>()
                                      .FirstOrDefault(r => (bool)r.IsChecked);
            RadioButton rb = checkedButton;
            string contentStr = (string)rb.Content; ;
            string stepStr = System.Text.RegularExpressions.Regex.Match(contentStr, @"[-+]?[0-9]*\.?[0-9]+").Groups[0].ToString();
            if (stepStr != null && stepStr.Length > 0) stepValue = float.Parse(stepStr, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            return stepValue;
        }

        private void buttonClearTableInfo_Click(object sender, RoutedEventArgs e)
        {
            bool res;
            Button b = (Button)sender;
            if (b.Name.EndsWith("0")) res = MainWindow.ClearTablePosition(0);
            else if (b.Name.EndsWith("1")) res = MainWindow.ClearTablePosition(1);
            else if (b.Name.EndsWith("2")) res = MainWindow.ClearTablePosition(2);
            else if (b.Name.EndsWith("3")) res = MainWindow.ClearTablePosition(3);
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            dg.UnselectAll();
        }

        private void btnFestoInit_Click(object sender, RoutedEventArgs e)
        {
            IObitDetails bd = new IObitDetails("", "", true, "", 110, 0);
            UpdateIO.ModifyOutputBit(bd);
            ErrorText = "";
        }


        private void btnFestoClear_Click(object sender, RoutedEventArgs e)
        {
            IObitDetails bd = new IObitDetails("", "", true, "", 110, 1);
            UpdateIO.ModifyOutputBit(bd);
            ErrorText = "";
        }
        private void btnFestoHome_Click(object sender, RoutedEventArgs e)
        {
            IObitDetails bd = new IObitDetails("", "", true, "", 110, 2);
            UpdateIO.ModifyOutputBit(bd);
            ErrorText = "";
        }

        private void btnFestoGo_Click(object sender, RoutedEventArgs e)
        {
            float pos;
            ushort vel;
            if (!float.TryParse(tbFestoTargetPos.Text, out pos)) { ErrorText = "Лош формат за Festo TargetPos"; return; }
            if (!ushort.TryParse(tbFestoTargetVel.Text, out vel)) { ErrorText = "Лош формат за Festo TargetVel"; return; }
            if (vel == 0 || vel > 100) { ErrorText = "Festo TargetVel трябва да е в проценти (1-100)"; return; }
            UpdateIO.ModifyOutputWord(mcOMRON.MemoryArea.CIO, 135, (ushort)vel);
            UpdateIO.ModifyOutputDoubleWord(mcOMRON.MemoryArea.CIO, 151, (uint)(pos * 10000));
            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 110, 3));
            ErrorText = "";
        }

        private void btnLinMotInit_Click(object sender, RoutedEventArgs e)
        {
            IObitDetails bd = new IObitDetails("", "", true, "", 110, 4);
            UpdateIO.ModifyOutputBit(bd);
            ErrorText = "";
        }

        private void btnLinMotClear_Click(object sender, RoutedEventArgs e)
        {
            IObitDetails bd = new IObitDetails("", "", true, "", 110, 5);
            UpdateIO.ModifyOutputBit(bd);
            ErrorText = "";
        }

        private void btnLinMotHome_Click(object sender, RoutedEventArgs e)
        {
            IObitDetails bd = new IObitDetails("", "", true, "", 110, 6);
            UpdateIO.ModifyOutputBit(bd);
            ErrorText = "";
        }

        private void btnLinMotGo_Click(object sender, RoutedEventArgs e)
        {
            float pos;
            float vel;
            float acc;
            if (!float.TryParse(tbLinMotTargetPos.Text, out pos)) { ErrorText = "Лош формат за LinMot TargetPos"; return; }
            if (!float.TryParse(tbLinMotTargetVel.Text, out vel)) { ErrorText = "Лош формат за LinMot TargetVel"; return; }
            if (!float.TryParse(tbLinMotTargetAcc.Text, out acc)) { ErrorText = "Лош формат за LinMot Acc/Dec"; return; }
            UpdateIO.ModifyOutputDoubleWord(mcOMRON.MemoryArea.CIO, 139, (uint)(10000 * pos));
            UpdateIO.ModifyOutputDoubleWord(mcOMRON.MemoryArea.CIO, 141, (uint)(1000 * vel));
            UpdateIO.ModifyOutputDoubleWord(mcOMRON.MemoryArea.CIO, 143, (uint)acc * 100);
            UpdateIO.ModifyOutputDoubleWord(mcOMRON.MemoryArea.CIO, 145, (uint)acc * 100);
            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 110, 8));
            ErrorText = "";
        }

        private void btnFestoGetTargetPos_Click(object sender, RoutedEventArgs e)
        {
            tbFestoTargetPos.Text = MainWindow.updIO.FEO_TargetPosition.ToString();
        }

        private void btnLinMotGetTargetPos_Click(object sender, RoutedEventArgs e)
        {
            tbFestoTargetPos.Text = (MainWindow.updIO.LMO_TargetPosition / 10000).ToString("F2");
        }

        private void btnGetFirmwareSettings_Click(object sender, RoutedEventArgs e)
        {
            RefreshHvaluesOnce = true;
            MainWindow.TCP.ReadHmemory = true;
        }

        public void btnSetFirmwareSettings_Click(object sender, RoutedEventArgs e)
        {
            ushort u = 0;
            uint ui = 0;
            int cci;
            float f = 0;
            if ((bool)cbUseMarking.IsChecked) TCPcom.hMemory[0] |= 0x0001;
            else TCPcom.hMemory[0] &= 0xFFFE;
            if ((bool)cbUseWelding.IsChecked) TCPcom.hMemory[0] |= 0x0002;
            else TCPcom.hMemory[0] &= 0xFFFD;
            if ((bool)cbUseStation1.IsChecked) TCPcom.hMemory[0] |= 0x2000;
            else TCPcom.hMemory[0] &= 0xDFFF;
            if ((bool)cbUseStation2.IsChecked) TCPcom.hMemory[0] |= 0x4000;
            else TCPcom.hMemory[0] &= 0xBFFF;
            if ((bool)cbUseStation3.IsChecked) TCPcom.hMemory[0] |= 0x8000;
            else TCPcom.hMemory[0] &= 0x7FFF;
            if (!ushort.TryParse(tbFestoVelocity.Text, out u)) { ErrorText = "Невалиден формат за Festo Velocity"; return; }
            if (u == 0 || u > 100) { ErrorText = "Festo Velocity трябва да е стойност от 1 до 100"; return; }
            TCPcom.hMemory[5] = u;
            if (!float.TryParse(tbLinMotVelocity.Text, out f)) { ErrorText = "Невалиден формат за LinMot Velocity"; return; }
            if (f <= 0) { ErrorText = "LinMot трябва да е стойност > 0"; return; }
            ui = (uint)(1000 * f);
            TCPcom.hMemory[8] = (ushort)(ui & 0xFFFF);
            TCPcom.hMemory[9] = (ushort)((ui >> 16) & 0xFFFF);
            if (!float.TryParse(tbLinMotVelocity.Text, out f)) { ErrorText = "Невалиден формат за LinMot Acc/Dec"; return; }
            if (f <= 0) { ErrorText = "LinMot Acc/Dec трябва да е стойност > 0"; return; }
            ui = (uint)(100 * f);
            TCPcom.hMemory[14] = (ushort)(ui & 0xFFFF);
            TCPcom.hMemory[15] = (ushort)((ui >> 16) & 0xFFFF);
            if (!float.TryParse(tbTotalHeight.Text, out f)) { ErrorText = "Невалиден формат за Total Height"; return; }
            if (f <= 0) { ErrorText = "Total Height трябва да е стойност > 0"; return; }
            ui = (uint)(10000 * f);
            TCPcom.hMemory[6] = (ushort)(ui & 0xFFFF);
            TCPcom.hMemory[7] = (ushort)((ui >> 16) & 0xFFFF);
            if (!float.TryParse(tbProductLength.Text, out f)) { ErrorText = "Невалиден формат за Product Length"; return; }
            if (f < 0) { ErrorText = "Product Length трябва да е стойност > 0"; return; }
            ui = (uint)(10000 * f);
            TCPcom.hMemory[10] = (ushort)(ui & 0xFFFF);
            TCPcom.hMemory[11] = (ushort)((ui >> 16) & 0xFFFF);
            if (!float.TryParse(tbCameraCapture.Text, out f)) { ErrorText = "Невалиден формат за Camera Capture"; return; }
            cci = (int)(10000 * f);
            TCPcom.hMemory[12] = (ushort)(cci & 0xFFFF);
            TCPcom.hMemory[13] = (ushort)((cci >> 16) & 0xFFFF);
            MainWindow.TCP.WriteHmemory = true;
        }

        private void dataGridMarkLaser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox dg = (ComboBox)sender;
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => MainWindow.mLaser.ChangePort(dg.SelectedItem.ToString())));
            }
            catch (Exception ex)
            {

            }
        }

        private void btnMLreset_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.mLaser.Command != 0) return;
            MainWindow.mLaser.Command = MarkingLaserWithCom.LCMD_RESETLASER;

        }

        private void btnMLloadFile_Click(object sender, RoutedEventArgs e)
        {
            if (tbMarkFileName.Text == String.Empty) return;
            if (MainWindow.mLaser.Command != 0) return;
            MainWindow.mLaser.MarkingFileName = tbMarkFileName.Text;
            MainWindow.mLaser.Command = MarkingLaserWithCom.LCMD_LDMARKFILE;
        }

        private void btnMLsetVar_Click(object sender, RoutedEventArgs e)
        {
            if (tbVarText.Text == String.Empty) return;
            MainWindow.mLaser.NameVar = "V01";
            MainWindow.mLaser.ValueVar = tbVarText.Text;
            MainWindow.mLaser.Command = MarkingLaserWithCom.LCMD_SETVAR;

        }

        private void btnMLshutter_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.mLaser.Command != 0) return;
            MainWindow.mLaser.Command = MarkingLaserWithCom.LCMD_LASSHUTTER;

        }

        private void btnMLmark_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.mLaser.Command != 0) return;
            //if (!MainWindow.updIO..)
            MainWindow.mLaser.Command = MarkingLaserWithCom.LCMD_MARKING;
        }

        private void btnFestoSetCapture_Click(object sender, RoutedEventArgs e)
        {
            float pos = MainWindow.updIO.FEI_CurrentPosition;
            UpdateIO.ModifyOutputDoubleWord(mcOMRON.MemoryArea.HR, 12, (uint)(pos * 10000));
        }

        private void LoadFHhome(object sender, RoutedEventArgs e)
        {
            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 7, 15), mcOMRON.MemoryArea.WR_Bit);
        }

        private void WeldFHfast(object sender, RoutedEventArgs e)
        {
            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 8, 4), mcOMRON.MemoryArea.WR_Bit);
        }

        private void WeldFHstop(object sender, RoutedEventArgs e)
        {
            UpdateIO.ModifyOutputBit(new IObitDetails("", "", false, "", 8, 4), mcOMRON.MemoryArea.WR_Bit);
        }

        private void MarkFHstep(object sender, RoutedEventArgs e)
        {
            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 8, 9), mcOMRON.MemoryArea.WR_Bit);
        }
    }
    public class ContentToMarginConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new Thickness(0, 0, -((ContentPresenter)value).ActualHeight, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public class ContentToPathConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var ps = new PathSegmentCollection(4);
            ContentPresenter cp = (ContentPresenter)value;
            double h = cp.ActualHeight > 10 ? 1.4 * cp.ActualHeight : 10;
            double w = cp.ActualWidth + 10;
            ps.Add(new LineSegment(new Point(1, 0.7 * h), true));
            ps.Add(new BezierSegment(new Point(1, 0.9 * h), new Point(0.1 * h, h), new Point(0.3 * h, h), true));
            ps.Add(new LineSegment(new Point(w, h), true));
            ps.Add(new BezierSegment(new Point(w + 0.6 * h, h), new Point(w + h, 0), new Point(w + h * 1.3, 0), true));
            PathFigure figure = new PathFigure(new Point(1, 0), ps, false);
            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return geometry;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
