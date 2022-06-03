using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Windows.Threading;
using System.Data;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using TwinCatIO;
using DioEct;
using System.Configuration;
using Machine.PDL.Base;
using Machine.Common;
using CognexQBVisionControl;
using System.Diagnostics;
using Machine.Entities;
using Machine.DAL.Traceability;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;
using System.Text;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace Machine
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region Mod6MexInputs
        public const int IN_PowerOK = 0;
        public const int IN_Pedal = 1;
        public const int IN_AirOK = 2;
        public const int IN_ArgonOK = 3;
        public const int IN_RT_InPos1 = 4;
        public const int IN_RT_InPos2 = 5;
        public const int IN_RT_InPos3 = 6;
        public const int IN_PusherInHome = 7;

        public const int IN_Argon_Out = 8;
        public const int IN_Argon_In = 9;
        public const int IN_RTready = 10;
        public const int IN_RT_InPos = 11;
        public const int IN_RT_MotorOK = 12;
        public const int IN_PushCyl_Dn = 13;

        public const int IN_Pos1_NutOK = 16;
        public const int IN_Pos1_Bucket_Op = 17;
        public const int IN_Pos1_Bucket_Cl = 18;
        public const int IN_Pos2_NutOK = 19;
        public const int IN_Pos2_Bucket_Op = 20;
        public const int IN_Pos2_Bucket_Cl = 21;
        public const int IN_Pos3_NutOK = 22;
        public const int IN_Pos3_Bucket_Op = 23;

        public const int IN_Pos3_Bucket_Cl = 24;

        public const int IN_Pos1_Al_Op = 32;
        public const int IN_Pos1_Al_Cl = 33;
        public const int IN_Pos1_SR_Cl = 34;
        public const int IN_Pos1_SensorPresent = 35;
        public const int IN_Pos1_ClampGrp_Op = 36;
        public const int IN_Pos1_ClampGrp_Cl = 37;
        public const int IN_Pos1_FH_Status = 38;
        public const int IN_Pos1_FH_Home = 39;

        public const int IN_Pos2_Al_Op = 40;
        public const int IN_Pos2_Al_Cl = 41;
        public const int IN_Pos2_SR_Cl = 42;
        public const int IN_Pos2_SensorPresent = 43;
        public const int IN_Pos2_ClampGrp_Op = 44;
        public const int IN_Pos2_ClampGrp_Cl = 45;
        public const int IN_Pos2_FH_Status = 46;
        public const int IN_Pos2_FH_Home = 47;

        public const int IN_Pos3_Al_Op = 48;
        public const int IN_Pos3_Al_Cl = 49;
        public const int IN_Pos3_SR_Cl = 50;
        public const int IN_Pos3_SensorPresent = 51;
        public const int IN_Pos3_ClampGrp_Op = 52;
        public const int IN_Pos3_ClampGrp_Cl = 53;
        public const int IN_Pos3_FH_Status = 54;
        public const int IN_Pos3_FH_Home = 55;
        #endregion

        #region Outputs CapWelder
        public const int OUT_CW_TL_Green = 7;
        public const int OUT_CW_TL_Yellow = 8;
        public const int OUT_CW_TL_Red = 9;
        public const int OUT_CW_TL_Blue = 10;
        public const int OUT_CW_TL_Alarm = 11;
        #endregion

        #region Machine States
        public const int STATE_WAIT_FOR_INIT = 0;
        public const int STATE_INITIALIZING = 1;
        public const int STATE_WAIT_FOR_INIT_AFTER_ERROR = 2;
        public const int STATE_WAIT_FOR_LOT = 11;
        public const int STATE_WAIT_FOR_ITEM = 12;
        public const int STATE_DIAGNOSTIC_MODE = 13;
        public const int STATE_PROCESS = 14;
        public const int STATE_WAIT_TO_REMOVE_ITEM = 15;
        public const int STATE_WAIT_PEDAL_RELEASE = 20;
        public const int STATE_ERROR_NO_CONNECTION_PLC = 100;
        public const int STATE_ERROR_NO_POWER = 101;
        public const int STATE_ERROR_NO_AIR = 102;
        public const int STATE_ERROR_TABLE_DOES_NOT_START = 103;
        public const int STATE_ERROR_TABLE_BLOCKED = 104;
        public const int STATE_ERROR_TABLE_OVERLOADED = 105;
        public const int STATE_ERROR_TABLE_DOES_NOT_RESET = 106;
        public const int STATE_ERROR_IN_STATIONS = 110;
        public const int STATE_ERROR_NO_CONNECTION_ML = 111;
        #endregion

        #region Scrap codes
        public const int RESULT_NO_ERROR = 0;
        public const int RESULT_NOK_FROM_VISUAL_CONTROL = 1663;
        #endregion

        public static TwincatSystemManager twincatSystem;
        public static TwinCatTask IOUpdateTask;
        public static TwinCatTask ASInterfaceTask;
        public static DigitalIo Dio;
        public static Station1 Loader;
        public static Station2 Welder;
        public static Station3 Marker;
        public static UpdateIO updIO;
        public static TCPcom TCP;
        public static mcOMRON.OmronPLC plc;
        public static MarkingLaserWithCom mLaser;
        public static readonly object lockInit = new object();

        public static TraceabilityDataAccess TraceabilityServer;

        public static LogTextInfo TLog;     //timings log (from CheckPoint's)
        public static bool bDiagnosticWindowOpened = false;
        public static bool bPedal = false;
        public static bool bProcessNoRotate = false;
        public static bool bUnload = false;
        public static CognexCamera CognexCameras;
        public DataTracking.Models.ComponentInfo currentComp;
        public int lotFinishedAsync = -99;
        public int lotStartedAsync = -99;
        public bool bCanContinue = false;
        public bool bMSAlaserCamera = false;
        public static bool markingFileLoadedOK = false;
        public static string markingFileLoaded = "";
        public static LotInfo CurrentLot;
        public static ProductInfo CurrentProduct;
        public static bool PLCdataRefreshed = false;
        Common.SplashScreen LoadingSplash = new Common.SplashScreen();

        public static bool fiat_sensor = false;
        
        public static bool daimler_sensor = false;
        public static bool mopar_sensor = false;
        public static bool detroit_sensor = false;
        public static bool fuso_sensor = false;
        public static bool dec_sensor = false;
        private bool _TraceabilityEnabled;
        public bool TraceabilityEnabled
        {
            get { return _TraceabilityEnabled; }
            set
            {
                if (_TraceabilityEnabled != value)
                {
                    _TraceabilityEnabled = value;
                    OnPropertyChanged("TraceabilityEnabled");
                }
            }
        }

        Thread mainThread;
        DispatcherTimer timerMain, timerOneSecond, timerDialogs;
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string callerName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName));
        }

        // клас за Datagrid
        public class SensorItem
        {
            public string Number { get; set; }
            public string StationNo { get; set; }
            public string Result { get; set; }
            public string RecDateTime { get; set; }
            public Brush StationNoForeground { get { return brushStationNoForeground; } set { brushStationNoForeground = value; } }
            public Brush StationNoBackground { get { return brushStationNoBackground; } set { brushStationNoBackground = value; } }
            public Brush ResultForeground { get { return brushResultForeground; } set { brushResultForeground = value; } }
            public Brush ResultBackground { get { return brushResultBackground; } set { brushResultBackground = value; } }
            public Brush TimeForeground { get { return brushTimeForeground; } set { brushTimeForeground = value; } }
            public Brush TimeBackground { get { return brushTimeBackground; } set { brushTimeBackground = value; } }

            private Brush brushStationNoForeground = Brushes.Brown;
            private Brush brushStationNoBackground = Brushes.AntiqueWhite;
            private Brush brushResultForeground;
            private Brush brushResultBackground;
            private Brush brushTimeForeground = Brushes.Black;
            private Brush brushTimeBackground;

            public SensorItem(string number, string stno, int result, string rec_date_time)
            {
                Number = number;
                StationNo = stno;
                RecDateTime = rec_date_time;

                if (result == 0)
                {
                    Result = "Годен";
                    brushResultBackground = Brushes.White;
                    brushResultForeground = Brushes.Green;
                }
                else
                {
                    Result = "Необработен";
                    brushResultBackground = Brushes.Yellow;
                    brushResultForeground = Brushes.Red;
                }
            }
        }

        public static ObservableCollection<SensorItem> ListSensors = new ObservableCollection<SensorItem>();

        public MainWindow()
        {
            InitializeComponent();
            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
            {
                MessageBox.Show("Приложението вече работи!");
                Application.Current.Shutdown();
            }
            LoadingSplash.Show();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TLog = new LogTextInfo("\\Machine\\Logs\\Timings.csv", false);
            TraceabilityEnabled = Properties.Settings.Default.Traceability;
            ListMLmess = new ObservableCollection<MLcomms>();
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(ListMLmess, MyMLcommsLock);
            CheckForErrorsFunction = CheckForErrors;
            buttonNewLot.Visibility = TraceabilityEnabled ? Visibility.Hidden : Visibility.Visible;
            bitsHflags.ItemsSource = UpdateIO.PlcHflags;
            CurrentLot = new LotInfo();
            CurrentProduct = new ProductInfo();
            dataGridLots.ItemsSource = ListLot;
            dataGridLogs.ItemsSource = ListLog;
            dataGridSensors.ItemsSource = ListSensors;
            indTraceabilityEnabled.DataContext = this;
            InitGeneral("Mod6MX", labelStatus, dataGridLots, dataGridLogs, 2, pieChartWorkload);
            ProductsDataAccess.Init(Properties.Settings.Default.ProductsServerIP, "MachinesTrace");
            var result = ProductsDataAccess.GetProducts();
            //   Marker = new Marking("Mark", MainWindow.Instance);
            if (result.MessagesList.Count > 0)
            {
                foreach (var message in result.MessagesList)
                {
                    Log(message);
                }
            }
            if (!result.Success)
            {
                Log("Maшината не може да се използва, поради липса на процесни настройки");
            }
            try
            {
                TraceabilityServer = new TraceabilityDataAccess(Properties.Settings.Default.TraceabilityServerIP
                    , Properties.Settings.Default.TraceabilityDBName
                    , Properties.Settings.Default.MachineName);
            }
            catch (Exception ex)
            {
                Log("Грешка при инициализация на трейсъбилити сървъра" + ex.ToString());
            }
            try
            {
                pieChartWorkload.AddPiePiece(1, "Обработка", Color.FromRgb(64, 255, 64));
                pieChartWorkload.AddPiePiece(2, "Престой", Color.FromRgb(255, 255, 64));
                pieChartWorkload.AddPiePiece(3, "Престой лот", Color.FromRgb(255, 192, 64));
                pieChartWorkload.AddPiePiece(4, "Поддръжка", Color.FromRgb(255, 64, 64));
                pieChartWorkload.FontSize = 11;
                pieChartWorkload.DatabaseName = LocalDatabaseName;
                pieChartWorkload.LoadMachineWorkloadFromDB();
                pieChartWorkload.SetCurrentTime(1);

            }
            catch (Exception ex)
            {
                Log("Грешка при инициализиране на PieChart! " + ex.ToString());
            }

            try
            {
                //if (!Debugger.IsAttached)
                //{
                    CognexCameras = new CognexCamera(@"D:\Machine\Vision\Module6MX.vpp");
                //}
            }
            catch (Exception ex)
            {
                Log("Грешка при инициализиране на камерата! " + ex.ToString());
            }

            try
            {
                plc = new mcOMRON.OmronPLC(mcOMRON.TransportType.Tcp);
            }
            catch (Exception ex)
            {
                Log("Грешка при инициализиране на plc! " + ex.ToString());
            }

            try
            {
                ReaderRFID = new RfidReader(Properties.Settings.Default.PortRFID);
                ReaderBarcode = new BarcodeReader(Properties.Settings.Default.PortBarcode);
            }
            catch (Exception ex)
            {
                Log("Грешка при инициализиране на RFID, BarcodeReader! " + ex.ToString());
            }

            try
            {
                mLaser = new MarkingLaserWithCom(Properties.Settings.Default.PortMarkLaser, this);
                updIO = new UpdateIO();
                TCP = new TCPcom(ref plc);
                Loader = new Station1("Loader", this);
                Welder = new Station2("Welder", this);
                Marker = new Station3("Marker", this);
            }
            catch (Exception ex)
            {
                Log("Грешка при инициализиране на Станциите! " + ex.ToString());
            }

            mainThread = new System.Threading.Thread(new System.Threading.ThreadStart(MainThread));
            mainThread.IsBackground = true;

            timerMain = new DispatcherTimer();
            timerMain.Tick += new EventHandler(timerMain_Tick);
            timerMain.Interval = TimeSpan.FromMilliseconds(100);
            timerMain.Start();

            timerDialogs = new DispatcherTimer();
            timerDialogs.Tick += new EventHandler(timerDialogs_Tick);
            timerDialogs.Interval = TimeSpan.FromMilliseconds(100);
            timerDialogs.Start();

            timerOneSecond = new DispatcherTimer();
            timerOneSecond.Tick += new EventHandler(timerOneSecond_Tick);
            timerOneSecond.Interval = TimeSpan.FromSeconds(1);
            timerOneSecond.Start();

            RestoreLastSettings();
            RestoreLastLogs();
            RestoreLastLots();
            if (Lot != "")
            {
                RestorePiecesFromLastLot();
                if (Product != "")
                {
                    var prodResult = ProductsDataAccess.GetProduct(Product);
                    if (prodResult.MessagesList.Count > 0)
                    {
                        for (int i = 0; i < prodResult.MessagesList.Count; i++) Log(prodResult.MessagesList[i]);
                        return;
                    }
                    CurrentProduct = prodResult.ResultObject;
                    CurrentLot.Lot = Lot;
                    CurrentLot.Product = Product;
                    CurrentLot.Order = Order;
                    CurrentLot.Quantity = Quantity;
                    CurrentLot.Operator = Operator;
                    CurrentLot.ShiftLeader = "H";
                    CurrentLot.MachineID = Properties.Settings.Default.MachineID;
                    CurrentLot.LotIdent = LastLotIdent;
                    CurrentLot.PiecesLeft = Pieces;
                    CurrentLot.PiecesOK = PiecesOK;
                    CurrentLot.PiecesNOK = PiecesNOK;
                    CurrentLot.ChipLot = Properties.Settings.Default.ChipLot;
                    CurrentLot.StartSN = Properties.Settings.Default.StartSN;
                    CurrentLot.CurrentSN = Properties.Settings.Default.CurrentSN;
                }
                timerLot.Start();
            }
            if (Pieces > 0)
            {
                labelLot.Content = Lot;
                labelProduct.Content = Product;
                labelOrder.Content = Order;
                labelQuantity.Content = Quantity;
                labelOperator.Content = Operator;
                labelStartSN.Content = Properties.Settings.Default.StartSN;
            }

            Log("Стартиране на програмата");
            mainThread.Start();

            if (!plc.Connected) MessageBox.Show("Няма връзка с PLC", "Грешка!",
                MessageBoxButton.OK,
                MessageBoxImage.Error);


            tbL1.Text = CurrentProduct.L1;
            tbL2.Text = CurrentProduct.L2;
            tbL3.Text = CurrentProduct.L3;
            tbL4.Text = CurrentProduct.L4;
        }

        public  void MarkLabels()
        {
            tbL1.Text = CurrentProduct.L1;
            tbL2.Text = CurrentProduct.L2;
            tbL3.Text = CurrentProduct.L3;
            tbL4.Text = CurrentProduct.L4;
        }
       
        private void RestoreLastSettings()
        {
            SettingsPropertyValueCollection settings = Properties.Settings.Default.PropertyValues;
            SettingsPropertyCollection col = Properties.Settings.Default.Properties;
            string[] lastSettings = new string[Properties.Settings.Default.Properties.Count];
            string lastSettingsString = string.Empty;
            string[] split_set;
            bool notMatch = false;
            using (SqlConnection connection = new SqlConnection("user id=rch;password=rchrch!;server=" + Properties.Settings.Default.TraceabilityServerIP + ";database=MachinesTrace;connection timeout=5"))
            using (SqlCommand command = new SqlCommand("SELECT TOP 1 Settings FROM MachineSettings WHERE MachineID=@machineid ORDER BY RecDateTime DESC", connection))
            {
                command.Parameters.Add("@machineid", SqlDbType.NVarChar, 5).Value = Properties.Settings.Default.MachineID;
                try
                {
                    connection.Open();
                    lastSettingsString = (string)command.ExecuteScalar();
                }
                catch { }
            }
            if (lastSettingsString == null) return;
            string[] split_set_list = lastSettingsString.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] propName = col.Cast<SettingsProperty>().Select(p => p.Name).ToArray();
            List<string> propList = propName.Cast<string>().ToList();

            for (int i = 0; i < split_set_list.Length; i++)
            {
                split_set = split_set_list[i].Replace(" ", "").Split('=');
                if (propList.Exists(x => x == split_set[0]))
                {
                    if (settings[split_set[0]].PropertyValue.ToString() != split_set[1])
                    {
                        if (split_set[0] != "ScrapCnt" && split_set[0] != "RepairCnt")
                        {
                           // Log("Разлика в стойностите за настройката " + split_set[0]);
                            notMatch = true;
                        }
                    }
                }
            }
            if (notMatch)
            {
                // Log("Несъвпадение в настройките от SET-файла и сървъра");
            }
        }

        private int UpdateDimensions(string product = "")
        {
            int result = 0;
            return result;
        }

        public static bool CheckForErrors()
        {
            if (bInit) return false;
            if (!MainWindow.plc.Connected)
            {
                MachineState = STATE_ERROR_NO_CONNECTION_PLC;
                return true;
            }
            if (!updIO.iCommonPowerOK)
            {
                MachineState = STATE_ERROR_NO_POWER;
                return true;
            }
            if (!updIO.iCommonAirOK)
            {
                MachineState = STATE_ERROR_NO_AIR;
                return true;
            }
            if (!MainWindow.mLaser.Connected)
            {
                MachineState = STATE_ERROR_NO_CONNECTION_ML;
                return true;
            }
            if (updIO.StateI > 0 && updIO.StateI < 5)
            {
                if (updIO.ErrorI == 3)
                {
                    MachineState = STATE_ERROR_TABLE_DOES_NOT_START;
                    return true;
                }
                if (updIO.ErrorI == 4)
                {
                    MachineState = STATE_ERROR_TABLE_BLOCKED;
                    return true;
                }
                if (updIO.ErrorI == 5)
                {
                    MachineState = STATE_ERROR_TABLE_OVERLOADED;
                    return true;
                }
                if (updIO.ErrorI == 6)
                {
                    MachineState = STATE_ERROR_TABLE_DOES_NOT_RESET;
                    return true;
                }
                if (updIO.ErrorI == 50)
                {
                    MachineState = STATE_ERROR_IN_STATIONS;
                    return true;
                }
            }
            return false;
        }

        public static void UpdateLabel(Label lbl, string txt, SolidColorBrush col)
        {
            lbl.Dispatcher.BeginInvoke((Action)delegate
            {
                lbl.Background = col;
                lbl.Content = txt;
            });
        }

        void timerDialogs_Tick(object sender, EventArgs e)
        {
            timerDialogs.Stop();

            if (DoInitialization)
            {
                if (ManualInitialization)
                {
                    Log("Анонимен натисна 'Инициализация'");
                    ManualInitialization = false;
                    DoInitialization = false;
                    bInit = true;
                }
                else
                {
                    DoInitialization = false;
                   // Wait\CardWindow card_win = new WaitCardWindow(ReaderRFID);
                   // if (card_win.ShowDialog() == true)
                   // {
                   //     Log(card_win.PersonName + " (" + card_win.CardID + ") " + " натисна 'Инициализация'");
                        bInit = true;
                   // }
                }
            }

            if (ShowDiagnostic)
            {
                ShowDiagnostic = false;

                if (ManualDiagnostic)
                {
                    Log("Анонимен отвори ''Диагностика''");
                    BeforeChangeSettings();
                    DiagnosticsWindow dlg = new DiagnosticsWindow(this);
                    bDiagnosticWindowOpened = true;
                    dlg.ShowDialog();
                    bDiagnosticWindowOpened = false;
                    AfterChangeSettings("", "");
                    buttonNewLot.Visibility = TraceabilityEnabled ? Visibility.Hidden : Visibility.Visible;
                    //if (Properties.Settings.Default.TraceabilityEnabled) TraceabilityStatus = 1; else TraceabilityStatus = 0;
                }
                else
                {
                    WaitCardWindow card_win = new WaitCardWindow(ReaderRFID);
                    if (card_win.ShowDialog() == true)
                    {
                        Log(card_win.PersonName + " (" + card_win.CardID + ") " + " отвори ''Диагностика''.");
                        DiagnosticsWindow dlg = new DiagnosticsWindow(this);
                        BeforeChangeSettings();
                        bDiagnosticWindowOpened = true;
                        dlg.ShowDialog();
                        bDiagnosticWindowOpened = false;
                        AfterChangeSettings(card_win.CardID, card_win.PersonName);
                        buttonNewLot.Visibility = TraceabilityEnabled ? Visibility.Hidden : Visibility.Visible;
                    }
                }

                ManualDiagnostic = false;
            }

            if (ShowProducts)
            {
                ShowProducts = false;

                if (ManualProducts)
                {
                    Log("Анонимен отвори ''Продукти''");
                    BeforeChangeSettings();
                    WindowProducts dlg = new WindowProducts();
                    dlg.ShowDialog();
                    AfterChangeSettings("", "");
                }
                else
                {
                    WaitCardWindow card_win = new WaitCardWindow(ReaderRFID);
                    //dlg.GroupRequired = ACCESS_EXPERT + ACCESS_AUTOMATION + ACCESS_PROCESS + ACCESS_PROCESS_EXPERT;
                    if (card_win.ShowDialog() == true)
                    {
                        Log(card_win.PersonName + " отвори 'Продукти'.");
                        BeforeChangeSettings();
                        WindowProducts dlg = new WindowProducts();
                        dlg.ShowDialog();
                        AfterChangeSettings(card_win.CardID, card_win.PersonName);
                    }
                }
                ManualProducts = false;
            }

            // -- new lot --
            if (ReaderBarcode.ReceivedLot)
            {
                ReaderBarcode.ReceivedOperator = false;
                ShowNewLotDialog();

                MarkLabels();
            }

            // -- end lot --
            if (bStartFinishTraceabilityReport)
            {
                bStartFinishTraceabilityReport = false;
                if (TraceabilityEnabled) EndLot(Properties.Settings.Default.ShowCloseLotErrors, true);
                //StatusServer.ForceUpdate();
            }

            if (bShowEndLotWindow)
            {
                bShowEndLotWindow = false;
                if (!TraceabilityEnabled) lotFinishedAsync = 0;
                else EndLot(Properties.Settings.Default.ShowCloseLotErrors, true);
                MarkLotAsSent(Lot);
            }
            timerDialogs.Start();
            LoadingSplash.Close();
        }

        private DateTime GetStartTimeCurrentShft()
        {
            DateTime CurDateTime = DateTime.Now;
            DateTime YesterDate, CurShiftStart = DateTime.Now;

            if ((CurDateTime.Hour >= 22) || (CurDateTime.Hour < 6))
            {
                if (CurDateTime.Hour >= 22)
                {
                    CurShiftStart = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, 22, 0, 0);
                }
                else
                {
                    YesterDate = CurDateTime.Subtract(TimeSpan.Parse("6:0:0"));
                    CurShiftStart = new DateTime(YesterDate.Year, YesterDate.Month, YesterDate.Day, 22, 0, 0);
                }
            }
            else if ((CurDateTime.Hour >= 6) && (CurDateTime.Hour < 14))
            {
                YesterDate = CurDateTime.Subtract(TimeSpan.Parse("14:0:0"));
                CurShiftStart = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, 6, 0, 0);
            }
            else if ((CurDateTime.Hour >= 14) && (CurDateTime.Hour < 22))
            {
                CurShiftStart = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, 14, 0, 0);
            }
            return CurShiftStart;
        }

        void timerOneSecond_Tick(object sender, EventArgs e)
        {
            int res = 0;
            DateTime StartCurShift = GetStartTimeCurrentShft();
            TimeSpan lenCurShift = DateTime.Now - StartCurShift;
            int numSecInCurShift = (int)lenCurShift.TotalSeconds;
            string ConnectionString = "user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5";
            string CmdString = string.Empty;
            timerOneSecond.Stop();

            if (Pieces == 0) buttonAddScrap.Visibility = Visibility.Hidden;
            else buttonAddScrap.Visibility = Visibility.Visible;

            int stno = 0;
            if (updIO.iCommonStation1 && !updIO.iCommonStation2 && !updIO.iCommonStation3) stno = 1;
            if (!updIO.iCommonStation1 && updIO.iCommonStation2 && !updIO.iCommonStation3) stno = 2;
            if (!updIO.iCommonStation1 && !updIO.iCommonStation2 && updIO.iCommonStation3) stno = 3;
            tbTablePosition.Text = stno.ToString();

            int secs = 0;
            long l = timerUpTime.ElapsedMilliseconds;
            l /= 1000;
            long days = l / 86400;
            l = l % 86400;
            int hours = (int)(l / 3600);
            l = l % 3600;
            int minutes = (int)(l / 60);
            int seconds = (int)(l % 60);
            labelUpTime.Content = String.Format((days == 0 ? "" : " {0} дни") + " {1:00}:{2:00}:{3:00}", days, hours, minutes, seconds);

            // серия време
            int i = (int)(timerLot.ElapsedMilliseconds + TimeStartLotOffset) / 1000;
            string s = "";
            if (i < 3600) s = String.Format("{0:0}:{1:00}", i / 60, i % 60);
            else s = String.Format("{0:0}:{1:00}:{2:00}", i / 3600, (i % 3600) / 60, i % 60);
            labelLotTime.Content = s;

            // цикъл време
            secs = DateTime.Now.Second;
            if (secs % 60 == 0)
            {
                res = SyncRemoteDB();
                if (res != 0)
                {
                    switch (res)
                    {
                        case -1:
                            Log("Не се свързва със сървъра за Sync");
                            break;
                        case -2:
                            Log("Не се свързва с ЛБД за Sync");
                            break;
                        case -3:
                            Log("Не се свързва с ЛБД за актуализация");
                            break;
                        case -4:
                            Log("Не чете данните от ЛБД за Sync");
                            break;
                        case -5:
                            Log("Не създава в сървъра запис за Sync");
                            break;
                        default:
                            Log("SyncRemoteDB връща " + res.ToString());
                            break;
                    }
                }
            }

            try
            {
                QueueCount.Content = TCP.SetOutCommands.Count;
            }
            catch (Exception ex)
            {

            }

            timerOneSecond.Start();
        }

        void timerMain_Tick(object sender, EventArgs e)
        {
            timerMain.Stop();

            try
            {
                if (oldMachineState != MachineState)
                {
                    oldMachineState = MachineState;
                    ShowStatus(MachineState);
                }

                while (Gen.MachineStatusQueue.Count > 0)
                {
                    Log(Gen.MachineStatusQueue.Dequeue());
                }

                lblSt1Duration.Content = Loader.ProcessDuration.ToString(@"s\.f");
                lblSt2Duration.Content = Welder.ProcessDuration.ToString(@"s\.f");
                lblSt3Duration.Content = Marker.ProcessDuration.ToString(@"s\.f");

                labelPieces.Content = Pieces.ToString();
                labelPiecesNOK.Content = PiecesNOK.ToString();
                labelCurrentSN.Content = CurrentLot.CurrentSN.ToString();

                if (!updIO.iCommonPowerOK) labelSafetyOff.Visibility = Visibility.Visible;
                else labelSafetyOff.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {

            }

            timerMain.Start();
        }

        public string GetStatusString(int status)
        {
            switch (status)
            {
                case STATE_WAIT_FOR_INIT: return "Изчакване за инициализация";
                case STATE_INITIALIZING: return "Инициализиране...";
                case STATE_PROCESS: return "Обработка...";
                case STATE_WAIT_FOR_LOT: return "Изчакване за нова серия";
                case STATE_WAIT_FOR_ITEM: return "Изчакване за модул";
                case STATE_WAIT_FOR_INIT_AFTER_ERROR: return "Има станция с грешка, отстранете я и инициализирайте...";
                case STATE_DIAGNOSTIC_MODE: return "Сервизен режим";
                case STATE_WAIT_TO_REMOVE_ITEM: return "Свалете изделието от гнездото";
                case STATE_WAIT_PEDAL_RELEASE: return "Отпуснете педала!";
                case STATE_ERROR_NO_CONNECTION_PLC: return "Няма връзка с PLC контролера!";
                case STATE_ERROR_NO_CONNECTION_ML: return "Няма връзка с маркиращия лазер!";
                case STATE_ERROR_NO_POWER: return "Авариен стоп или няма захранване!";
                case STATE_ERROR_NO_AIR: return "Няма налягане на въздуха!";
                case STATE_ERROR_TABLE_DOES_NOT_START: return "Масата не започва въртене";
                case STATE_ERROR_TABLE_BLOCKED: return "Масата е блокирана";
                case STATE_ERROR_TABLE_OVERLOADED: return "Масата е претоварена";
                case STATE_ERROR_TABLE_DOES_NOT_RESET: return "Масата не се рисетира";
                case STATE_ERROR_IN_STATIONS: return "Станции в грешка: отстранете причината и инициализирайте";
                default: return "Неописана грешка с код " + status.ToString();
            }
        }

        public void ShowStatus(int status)
        {
            switch (status)
            {
                case STATE_WAIT_FOR_INIT:
                    labelStatus.Background = Brushes.White;
                    pieChartWorkload.SetCurrentTime(1);
                    break;
                case STATE_INITIALIZING:
                    labelStatus.Background = Brushes.White;
                    pieChartWorkload.SetCurrentTime(0);
                    break;
                case STATE_PROCESS:
                    labelStatus.Background = Brushes.LightGreen;
                    pieChartWorkload.SetCurrentTime(0);
                    break;
                case STATE_WAIT_FOR_LOT:
                    labelStatus.Background = Brushes.Yellow;
                    pieChartWorkload.SetCurrentTime(1);
                    break;
                case STATE_WAIT_FOR_ITEM:
                case STATE_WAIT_TO_REMOVE_ITEM:
                    labelStatus.Background = Brushes.Yellow;
                    pieChartWorkload.SetCurrentTime(2);
                    break;
                case STATE_DIAGNOSTIC_MODE:
                    labelStatus.Background = Brushes.Cyan;
                    break;
                case STATE_ERROR_NO_CONNECTION_PLC:
                case STATE_ERROR_NO_CONNECTION_ML:
                case STATE_ERROR_NO_POWER:
                case STATE_ERROR_NO_AIR:
                case STATE_ERROR_TABLE_DOES_NOT_START:
                case STATE_ERROR_TABLE_BLOCKED:
                case STATE_ERROR_TABLE_OVERLOADED:
                case STATE_ERROR_TABLE_DOES_NOT_RESET:
                case STATE_ERROR_IN_STATIONS:
                    labelStatus.Background = Brushes.Red;
                    break;
                default:
                    labelStatus.Background = Brushes.Red;
                    pieChartWorkload.SetCurrentTime(3);
                    break;
            }

            if (labelStatus.Background == Brushes.Red) labelStatus.Foreground = Brushes.Yellow;
            else labelStatus.Foreground = Brushes.Black;

            string s = GetStatusString(status);
            labelStatus.Content = s;
        }

        public static bool DioIn(int input)
        {
            return true;
        }

        public static void DioOut(int output, bool state)
        {
            return;
        }

        public static bool DioGetOutput(int output)
        {
            return true;
        }

        private void buttonInit_Click(object sender, RoutedEventArgs e)
        {
            DoInitialization = true;
        }

        private void buttonDiag_Click(object sender, RoutedEventArgs e)
        {
            ShowDiagnostic = true;
        }
        private void GetFirmwareSettings()
        {
            bool RefreshHvaluesOnce = false;
            RefreshHvaluesOnce = true;
            MainWindow.TCP.ReadHmemory = true;
        }
        private async void btnSetFirmwareSettings_Click(object sender, RoutedEventArgs e)
        {
            GetFirmwareSettings();
            Thread.Sleep(600);
            if (updIO.iCommonRotTableInPos) bProcessNoRotate = true;
            if ((bool)cbUseMarking.IsChecked) TCPcom.hMemory[0] |= 0x0001;
            else TCPcom.hMemory[0] &= 0xFFFE;
            if ((bool)cbUseWelding.IsChecked) TCPcom.hMemory[0] |= 0x0002;
            else TCPcom.hMemory[0] &= 0xFFFD;
            MainWindow.TCP.WriteHmemory = true;
        }
        //private void btnUseWelding_Click(object sender, RoutedEventArgs e)
        //{
        //    if (updIO.HF_UseWelding)
        //         TCPcom.hMemory[0] |= 0x0002;
        //    else
        //         TCPcom.hMemory[0] &= 0xFFFD;
        //    updIO.H_Flags = TCPcom.PlcControl[33];
        // }
        //
        //private void btnUseMarking_Click(object sender, RoutedEventArgs e)
        //{
        //    if (updIO.HF_UseMarking)
        //         TCPcom.hMemory[0] |= 0x0001;
        //    else
        //         TCPcom.hMemory[0] &= 0xFFFE;
        //    updIO.H_Flags = TCPcom.PlcControl[33];
        // }

        private void buttonNewLot_Click(object sender, RoutedEventArgs e)
        {
            ShowNewLotDialog();
        }

        private void buttonProducts_Click(object sender, RoutedEventArgs e)
        {
            ShowProducts = true;
        }


        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void buttonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void buttonEndLot_Click(object sender, RoutedEventArgs e)
        {
            bShowEndLotWindow = true;
        }

        private void buttonAddScrap_Click(object sender, RoutedEventArgs e)
        {
            string eStr = "";
            AddScrapWindow dlg = new AddScrapWindow();
            //dlg.ExcludeCodes(1,2,3);

            if (dlg.ShowDialog() == true)
            {
                DateTime current_time = DateTime.Now;
                if (!UpdateDB(dlg.Result, out eStr))
                {
                    MessageBox.Show("Грешка при заместване на последния годен с брак:\n" + eStr);
                    return;
                }
                TablePositionInfo tpi = new TablePositionInfo();
                tpi.Snumber = 0;
                tpi.Result = (short)dlg.Result;
                WriteSensorIntoDatagrid(tpi, DateTime.Now, -1);
                PiecesOK--;
                PiecesNOK++;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (timerMain != null) timerMain.Stop();
            if (timerOneSecond != null) timerOneSecond.Stop();
            if (timerDialogs != null) timerDialogs.Stop();
            CognexCameras.DisposeCognexVision();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);  // клавишите с ALT са системни
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Alt)
            {
                switch (key)
                {
                    case Key.I:     // Alt I
                        ManualInitialization = true;
                        DoInitialization = true;
                        e.Handled = true;
                        break;
                    case Key.Z:     // Alt Z
                        ManualDiagnostic = true;
                        ShowDiagnostic = true;
                        e.Handled = true;
                        break;
                    case Key.P:     // ALT P
                        ManualProducts = true;
                        ShowProducts = true;
                        e.Handled = true;
                        break;
                    case Key.Q:     // ALT Q
                        e.Handled = true;
                        Close();
                        break;
                }
            }
        }


        #region Traceability Func

        private void ShowNewLotDialog()
        {
            if (!bLotFinished)
            {
                var response = MessageBox.Show("Текущата серия не е завършена.Искате ли да я прекратите?", "Серията не е завършена", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (response == MessageBoxResult.Yes)
                {
                    if (CurrentLot.Lot != "") Log(String.Format("Оператор {0} прекрати серия {1} предварително. Годни: {2}, Брак: {3}", CurrentLot.Operator, CurrentLot.Lot, CurrentLot.PiecesOK, CurrentLot.PiecesNOK));
                    EndLot(Properties.Settings.Default.ShowCloseLotErrors, true);
                }
                else
                {
                    ReaderBarcode.ReceivedLot = false;
                    return;
                }
            }

            NewLotWindow dlg = new NewLotWindow(Properties.Settings.Default.TraceabilityServerIP, "MachinesTrace", Properties.Settings.Default.TableProductsName, ReaderBarcode, true, false);
            if (dlg.ShowDialog().Value)
            {
                var result = ProductsDataAccess.GetProduct(dlg.comboBoxProduct.Text);
                if (result.MessagesList.Count > 0)
                {
                    for (int i = 0; i < result.MessagesList.Count; i++)
                    {
                        Log(result.MessagesList[i]);
                    }
                    return;
                }
                CurrentProduct = result.ResultObject;
                int err = StartNewLotInTraceOrLocal
                            (
                                dlg.textBoxLot.Text,
                                dlg.comboBoxProduct.Text,
                                dlg.Order,
                                dlg.textBoxQuantity.Text,
                                dlg.textBoxOperator.Text,
                                dlg.ChipLot
                            );
            }
        }

        public int StartNewLotInTraceOrLocal(string lot, string prod, string ord, string qty, string oper, string chLot)
        {
            int quantity = 0;
            int result = 0;
            string eStr;
            if (!Int32.TryParse(qty, out quantity) || quantity <= 0)
            {
                eStr = "Старт на лот: невалидно количество!";
                Log(eStr);
                MessageBox.Show(eStr);
                return -1;
            }

            if (TraceabilityEnabled)
            {
                string ResultText = "";

                DataTracking.Models.LotInfo structLot = new DataTracking.Models.LotInfo()
                {
                    Lot = lot,
                    Product = prod,
                    Order = ord,
                    Quantity = quantity,
                    Operator = oper,
                    ShiftLeader = "H",
                    MachineID = Properties.Settings.Default.MachineID
                };
                TraceabilityServer.StartLot(structLot, out result, out ResultText);
                if (result != 0)
                {
                    eStr = "Старт на лот: " + ResultText;
                    Log(eStr);
                    LastLotIdent = 0;
                }
                else
                {
                    LastLotIdent = int.Parse(structLot.LotIdent.ToString());
                    quantity = (short)structLot.Quantity;
                }
            }
            else
            {
                quantity = Convert.ToInt32(qty);
            }

            int StartSN;
            string errorMsg;
            bool res = TraceabilityServer.GetMod6StartSN(chLot, quantity, out StartSN, out errorMsg);
            if (!res)
            {
                MessageBox.Show("Грешка при получаване на начален SN", "Грешка", MessageBoxButton.OK, MessageBoxImage.Error);
                return -4;
            }

            result = StartLotLocal(lot, prod, ord, quantity.ToString(), oper);
            if (result != 0)
            {
                eStr = "Старт на лот: не може да добави лота в ЛБД";
                Log(eStr);
                MessageBox.Show(eStr, "Грешка", MessageBoxButton.OK, MessageBoxImage.Error);
                return -5;
            }

            Lot = lot;
            Product = prod;
            Order = ord;
            Quantity = (short)quantity;
            Operator = oper;
            WriteLotIntoDatagrid(Lot, Product, Order, Quantity.ToString(), Operator, DateTime.Now.ToString("yyyy.MM.dd  HH:mm"));

            MarkLabels();
            CurrentLot.Lot = Lot;
            CurrentLot.Product = Product;
            CurrentLot.Order = Order;
            CurrentLot.Quantity = Quantity;
            CurrentLot.Operator = Operator;
            CurrentLot.ShiftLeader = "H";
            CurrentLot.MachineID = Properties.Settings.Default.MachineID;
            CurrentLot.LotIdent = LastLotIdent;
            CurrentLot.PiecesLeft = Quantity;
            CurrentLot.PiecesOK = 0;
            CurrentLot.PiecesNOK = 0;
            CurrentLot.ChipLot = chLot;
            CurrentLot.StartSN = StartSN;
            CurrentLot.CurrentSN = StartSN;
            Properties.Settings.Default.ChipLot = chLot;
            Properties.Settings.Default.StartSN = StartSN;
            Properties.Settings.Default.CurrentSN = StartSN;

            labelLot.Content = Lot;
            labelProduct.Content = Product;
            labelOrder.Content = Order;
            labelQuantity.Content = Quantity;
            labelOperator.Content = Operator;
            labelStartSN.Content = StartSN;

            timerLot.Reset();
            timerLot.Start();
            TimeStartLotOffset = 0;
            ClearSensorDatagrid();

            Pieces = quantity;
            PiecesOK = 0;
            PiecesNOK = 0;
            Properties.Settings.Default.Save();

            //StatusServer.ForceUpdate();
            return 0;
        }

        private bool UpdateDB(int Result, out string errString)
        {
            string ConnectionString = "user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5";
            string CmdString = string.Empty;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand commandUpdate = new SqlCommand("WITH a AS (SELECT TOP 1 * from Sensors WHERE Lot ='" + Lot + "' AND Result=0 ORDER BY RecDateTime DESC) UPDATE a SET Result=" + Result.ToString(), connection))
            {
                try
                {
                    connection.Open();
                    commandUpdate.ExecuteNonQuery();
                    errString = "";
                    return true;
                }
                catch (Exception e)
                {
                    errString = e.Message + "\nDB command text is:\n" + commandUpdate.CommandText;
                    return false;
                }
            }
        }

        private int RestorePiecesFromLastLot()
        {
            string ConnectionString = "user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5";
            string CmdString = string.Empty;
            TablePositionInfo tpi = new TablePositionInfo();
            DateTime dt = new DateTime();
            short pi;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand commandRead = new SqlCommand("select SeqNo,StationNo,Result,RecDateTime from Sensors where Lot=@lot order by RecDateTime", connection))
                {
                    commandRead.Parameters.Add("@lot", SqlDbType.NVarChar, 12).Value = Lot;
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = commandRead.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tpi.Snumber = (Int16)reader["SeqNo"];
                                tpi.Result = (Int16)reader["Result"];
                                pi = (short)reader["StationNo"];
                                dt = (DateTime)reader["RecDateTime"];
                                WriteSensorIntoDatagrid(tpi, dt, pi);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorString = ex.ToString();
                        return -1;
                    }
                }
            }
            return 0;
        }

        public void WriteSensorIntoDatagrid(TablePositionInfo tpi, DateTime dt, int posIdx)
        {
            if (this.Dispatcher.CheckAccess())
            {
                ListSensors.Add(new SensorItem(tpi.SensorID.ToString(),// Snumber.ToString()
                                                posIdx.ToString(),
                                                tpi.Result,
                                                dt.ToString("HH:mm:ss")));
                if (dataGridSensors.Items.Count > 0) dataGridSensors.ScrollIntoView(dataGridSensors.Items[dataGridSensors.Items.Count - 1]);
            }
            else
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    ListSensors.Add(new SensorItem(tpi.SensorID.ToString(),
                                                    posIdx.ToString(),
                                                    tpi.Result,
                                                    DateTime.Now.ToString("HH:mm:ss")));
                    if (dataGridSensors.Items.Count > 0) dataGridSensors.ScrollIntoView(dataGridSensors.Items[dataGridSensors.Items.Count - 1]);
                }));
            }
        }

        public void InsertSensorIntoDatabase(TablePositionInfo tpi, int posIdx)
        {
            string ConnectionString = "user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5";
            string CmdString = string.Empty;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand commandInsert =
                    new SqlCommand("INSERT INTO Sensors ([Lot],[Product],[Operator],[SeqNo],[StationNo],[Loaded],[Marked],[Welded],[VisionOK],[WeldedFromCam],[GoodForMark],[MarkedWith],[Result],[RecDateTime],[Sent]) VALUES " +
                    "(@Lot,@Product,@Operator,@Snumber,@StationNo,@Loaded,@Marked,@Welded,@VisionOK,@WeldedFromCam,@GoodForMark,@MarkedWith,@Result,@RecDateTime,@Sent)", connection))
                {
                    commandInsert.Parameters.AddWithValue("@Lot", tpi.Lot);
                    commandInsert.Parameters.AddWithValue("@Product", tpi.Product);
                    commandInsert.Parameters.AddWithValue("@Operator", tpi.Operator);
                    commandInsert.Parameters.AddWithValue("@Snumber", tpi.Snumber);
                    commandInsert.Parameters.AddWithValue("@StationNo", posIdx);
                    commandInsert.Parameters.AddWithValue("@Loaded", tpi.Loaded);
                    commandInsert.Parameters.AddWithValue("@Welded", tpi.Welded);
                    commandInsert.Parameters.AddWithValue("@Marked", tpi.Marked);
                    commandInsert.Parameters.AddWithValue("@VisionOK", tpi.VisionOK);
                    commandInsert.Parameters.AddWithValue("@WeldedFromCam", tpi.WeldedFromCam);
                    commandInsert.Parameters.AddWithValue("@GoodForMark", tpi.GoodForMark);
                    commandInsert.Parameters.AddWithValue("@MarkedWith", tpi.MarkedWith);
                    commandInsert.Parameters.AddWithValue("@Result", tpi.Result);
                    commandInsert.Parameters.AddWithValue("@RecDateTime", DateTime.Now);
                    commandInsert.Parameters.AddWithValue("@Sent", false);
                    try
                    {
                        connection.Open();
                        commandInsert.ExecuteNonQuery();
                        ErrorString = "";
                    }
                    catch (Exception e)
                    {
                        ErrorString = e.Message;
                        Log(ErrorString + "\nCommand text is:\n" + commandInsert.CommandText);
                    }
                }
                if (ErrorString == "")
                {
                    tpi.SensorID = GetPieceID();
                    if (tpi.SensorID == -1) { Log("Неуспешно четене на SensorID"); }
                }
            }
        }

        public static int GetPieceID()
        {
            ErrorString = "";
            int result = -1;    // no data avalable
            try
            {
                using (SqlConnection sCon = new SqlConnection("user id=machine;password=qipe;server=" +
                    "127.0.0.1" + ";database=" +
                    LocalDatabaseName + ";connection timeout=5"))
                {
                    using (SqlCommand sCom = new SqlCommand("SELECT TOP 1 [SensorID] FROM [Sensors] ORDER BY SensorID DESC", sCon))
                    {
                        sCon.Open();
                        SqlDataReader sRea = sCom.ExecuteReader();
                        sRea.Read();
                        result = int.Parse(sRea[0].ToString());
                        sRea.Close();
                        sCon.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorString = ex.ToString();
            }

            return result;
        }

        public void UpdateSensorSt2IntoDatabase(TablePositionInfo tpi, int posIdx)
        {
            string ConnectionString = "user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5";
            string CmdString = string.Empty;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand commandInsert =
                    new SqlCommand("Update [dbo].[Sensors] " +
                    "SET [StationNo] = @StationNo," +
                    "[Welded] = @Welded," +
                    "[VisionOK] = @VisionOK," +
                    "[WeldedFromCam] = @WeldedFromCam," +
                    "[GoodForMark] = @GoodForMark," +
                    "[Result] = @Result," +
                    "[RecDateTime] = @RecDateTime " +
                    "Where SensorID = " + tpi.SensorID, connection))
                {
                    commandInsert.Parameters.AddWithValue("@StationNo", posIdx);
                    commandInsert.Parameters.AddWithValue("@Welded", tpi.Welded);
                    commandInsert.Parameters.AddWithValue("@VisionOK", tpi.VisionOK);
                    commandInsert.Parameters.AddWithValue("@WeldedFromCam", tpi.WeldedFromCam);
                    commandInsert.Parameters.AddWithValue("@GoodForMark", tpi.GoodForMark);
                    commandInsert.Parameters.AddWithValue("@Result", tpi.Result);
                    commandInsert.Parameters.AddWithValue("@RecDateTime", DateTime.Now);
                    try
                    {
                        connection.Open();
                        commandInsert.ExecuteNonQuery();
                        ErrorString = "";
                    }
                    catch (Exception e)
                    {
                        ErrorString = e.Message;
                        MessageBox.Show(e.Message + "\nCommand text is:\n" + commandInsert.CommandText);
                    }
                }
            }
        }

        public void UpdateSensorSt3IntoDatabase(TablePositionInfo tpi, int posIdx)
        {
            string ConnectionString = "user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5";
            string CmdString = string.Empty;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand commandInsert =
                    new SqlCommand("Update [dbo].[Sensors] " +
                    "SET [StationNo] = @StationNo," +
                    "[Marked] = @Marked," +
                    "[MarkedWith] = @MarkedWith," +
                    "[Result] = @Result," +
                    "[RecDateTime] = @RecDateTime " +
                    "Where SensorID = " + tpi.SensorID, connection))
                {
                    commandInsert.Parameters.AddWithValue("@StationNo", posIdx);
                    commandInsert.Parameters.AddWithValue("@Marked", tpi.Marked);
                    commandInsert.Parameters.AddWithValue("@MarkedWith", tpi.MarkedWith);
                    commandInsert.Parameters.AddWithValue("@Result", tpi.Result);
                    commandInsert.Parameters.AddWithValue("@RecDateTime", DateTime.Now);
                    try
                    {
                        connection.Open();
                        commandInsert.ExecuteNonQuery();
                        ErrorString = "";
                    }
                    catch (Exception e)
                    {
                        ErrorString = e.Message;
                        MessageBox.Show(e.Message + "\nCommand text is:\n" + commandInsert.CommandText);
                    }
                }
            }
        }

        public void ClearSensorDatagrid()
        {
            if (dataGridSensors.Dispatcher.CheckAccess())
            {
                ListSensors.Clear();
            }
            else
            {
                labelStatus.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    ListSensors.Clear();
                }));
            }
        }

        static int SyncRemoteDB()
        {
            int _result = 0;

            using (SqlConnection connectionNet = new SqlConnection("user id=machine;password=qipe;server=" + Properties.Settings.Default.TraceabilityServerIP + ";database=MachinesTrace;connection timeout=5"))
            using (SqlConnection connectionLocal = new SqlConnection("user id=machine;password=qipe;server=127.0.0.1;database=" + Properties.Settings.Default.LocalDBname + ";connection timeout=5"))
            using (SqlConnection connectionLocalUpdate = new SqlConnection("user id=machine;password=qipe;server=127.0.0.1;database=" + Properties.Settings.Default.LocalDBname + ";connection timeout=5"))
            using (SqlCommand commandRead = new SqlCommand("select top 1000 * from Sensors where Sent=0", connectionLocal))
            {
                try
                {
                    _result = -1;
                    connectionNet.Open();
                    _result = -2;
                    connectionLocal.Open();
                    _result = -3;
                    connectionLocalUpdate.Open();
                    _result = -4;
                    using (SqlDataReader reader = commandRead.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            using (SqlCommand commandWrite = new SqlCommand("INSERT INTO [" + Properties.Settings.Default.TableSensorsName + "] ([Lot],[Product],[MachineID],[SeqNo],[StationNo],[Result],[RecDateTime]) " +
                                    "VALUES (@lot,@product,@machineID,@sq,@st,@mw,@res,@rdt)", connectionNet))
                            {
                                commandWrite.Parameters.Add("@lot", SqlDbType.NVarChar, 9).Value = reader["Lot"].ToString();
                                commandWrite.Parameters.Add("@product", SqlDbType.NVarChar, 16).Value = reader["Product"].ToString();
                                commandWrite.Parameters.Add("@machineID", SqlDbType.NVarChar, 5).Value = Properties.Settings.Default.MachineID;
                                commandWrite.Parameters.Add("@st", SqlDbType.Int).Value = reader["SeqNo"].ToString();
                                commandWrite.Parameters.Add("@sq", SqlDbType.Int).Value = reader["StationNo"].ToString();
                                commandWrite.Parameters.Add("@mw", SqlDbType.Int).Value = reader["MarkedWith"].ToString();
                                commandWrite.Parameters.Add("@res", SqlDbType.SmallInt).Value = (Int16)reader["Result"];
                                commandWrite.Parameters.Add("@rdt", SqlDbType.DateTime).Value = reader["RecDateTime"].ToString();

                                //_result = -5;
                                // commandWrite.ExecuteNonQuery();
                                using (SqlCommand commandUpdate = new SqlCommand("UPDATE Sensors SET Sent=1 WHERE SensorID=" + reader["SensorID"].ToString(), connectionLocalUpdate))
                                {
                                    _result = -6;
                                    commandUpdate.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    ErrorString = "";
                    _result = 0;
                }
                catch (Exception e)
                {
                }
                return _result;
            }
        }

        private static void GetTableIndexForStation(int stno, out int stix)
        {
            int timeout = 0;
            int curix = -1;
            stix = -1;
            while (!updIO.iCommonRotTableInPos)
            {
                Thread.Sleep(10);
                if (timeout > 500) { return; }
                timeout++;
            };
            if (updIO.iCommonStation1 && !updIO.iCommonStation2 && !updIO.iCommonStation3) curix = 0;
            else if (!updIO.iCommonStation1 && updIO.iCommonStation2 && !updIO.iCommonStation3) curix = 1;
            else if (!updIO.iCommonStation1 && !updIO.iCommonStation2 && updIO.iCommonStation3) curix = 2;
            else return;
            switch (stno)
            {
                case 1:
                    stix = (curix + 0) % 3;
                    break;
                case 2:
                    stix = (curix + 2) % 3;
                    break;
                case 3:
                    stix = (curix + 1) % 3;
                    break;
            }
            stix++;
        }

        public static bool ClearTablePosition(int posNo)
        {
            using (SqlConnection conn = new SqlConnection("user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5"))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "UPDATE TableInfo SET Loaded=0,Welded=0,VisionOK=0,WeldedFromCam=0,MarkedWith='',Result=0 WHERE TablePosition=" + posNo.ToString();
                        cmd.ExecuteScalar();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    ErrorString = ex.ToString();
                    MessageBox.Show("Грешка при изчистване информацията за масата!" + ErrorString);
                    return false;
                }
            }
        }

        public static bool GetNestInfo(int stno, out int posIndex, ref TablePositionInfo tpi)
        {
            posIndex = -1;
            GetTableIndexForStation(stno, out posIndex);
            if (posIndex == -1)
            {
                MessageBox.Show("Грешен индекс на станция или маса не на позиция/TableInfo\n" + ErrorString);
                return false;
            }
            using (SqlConnection conn = new SqlConnection("user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5"))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "SELECT * FROM TableInfo WHERE TablePosition=@tabpos";
                        cmd.Parameters.Add("@tabpos", SqlDbType.SmallInt).Value = (short)posIndex;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tpi.Lot = (string)(reader["Lot"]);
                                tpi.LotIdent = (long)(reader["LotIdent"]);
                                tpi.Product = (string)(reader["Product"]);
                                tpi.LotOrder = (string)(reader["LotOrder"]);
                                tpi.Operator = (string)(reader["Operator"]);
                                tpi.Snumber = (Int16)(reader["Snumber"]);
                                tpi.Quantity = (Int16)(reader["Quantity"]);
                                tpi.Loaded = (bool)(reader["Loaded"]);
                                tpi.Welded = (bool)(reader["Welded"]);
                                tpi.Marked = (bool)(reader["Marked"]);
                                tpi.VisionOK = (bool)(reader["VisionOK"]);
                                tpi.WeldedFromCam = (bool)(reader["WeldedFromCam"]);
                                tpi.GoodForMark = (bool)(reader["GoodForMark"]);
                                tpi.MarkedWith = (string)(reader["MarkedWith"]);
                                tpi.Result = (Int16)(reader["Result"]);
                                tpi.SensorID = (int)(reader["SensorID"]);
                            }
                            else
                            {
                                MessageBox.Show("Няма данни за серия и номер на изделието в TableInfo за Ст. №!" + stno.ToString());
                                return false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorString = ex.ToString();
                    MessageBox.Show("Грешка при четене от ЛБД Mod6MX/TableInfo\n" + ErrorString);
                    return false;
                }
            }
            return true;
        }
        public static bool SetNestInfo(int stno, TablePositionInfo tpi)
        {
            int posIndex = -1;
            GetTableIndexForStation(stno, out posIndex);
            if (posIndex == -1)
            {
                MessageBox.Show("Грешен индекс на станция или маса не на позиция/TableInfo\n" + ErrorString);
                return false;
            }
            using (SqlConnection conn = new SqlConnection("user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5"))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "UPDATE TableInfo SET Lot=@lt,LotIdent=@lti,Product=@pr,LotOrder=@lo,Operator=@op,Snumber=@sn,Quantity=@qu,Loaded=@ld,Welded=@wd,Marked=@mk,VisionOK=@vok,WeldedFromCam=@wdc,GoodForMark=@gfm,MarkedWith=@mkw,Result=@rs,SensorID=@sid WHERE TablePosition=@tp";
                        cmd.Parameters.Add("@lt", SqlDbType.NVarChar).Value = tpi.Lot;
                        cmd.Parameters.Add("@lti", SqlDbType.BigInt).Value = tpi.LotIdent;
                        cmd.Parameters.Add("@pr", SqlDbType.NVarChar).Value = tpi.Product;
                        cmd.Parameters.Add("@lo", SqlDbType.NVarChar).Value = tpi.LotOrder;
                        cmd.Parameters.Add("@op", SqlDbType.NVarChar).Value = tpi.Operator;
                        cmd.Parameters.Add("@sn", SqlDbType.SmallInt).Value = tpi.Snumber;
                        cmd.Parameters.Add("@qu", SqlDbType.SmallInt).Value = tpi.Quantity;
                        cmd.Parameters.Add("@ld", SqlDbType.Bit).Value = tpi.Loaded;
                        cmd.Parameters.Add("@wd", SqlDbType.Bit).Value = tpi.Welded;
                        cmd.Parameters.Add("@mk", SqlDbType.Bit).Value = tpi.Marked;
                        cmd.Parameters.Add("@vok", SqlDbType.Bit).Value = tpi.VisionOK;
                        cmd.Parameters.Add("@wdc", SqlDbType.Bit).Value = tpi.WeldedFromCam;
                        cmd.Parameters.Add("@gfm", SqlDbType.Bit).Value = tpi.GoodForMark;
                        cmd.Parameters.Add("@mkw", SqlDbType.NVarChar).Value = tpi.MarkedWith;
                        cmd.Parameters.Add("@rs", SqlDbType.SmallInt).Value = tpi.Loaded;
                        cmd.Parameters.Add("@tp", SqlDbType.SmallInt).Value = (short)posIndex;
                        cmd.Parameters.Add("@sid", SqlDbType.Int).Value = tpi.SensorID;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    ErrorString = ex.ToString();
                    MessageBox.Show("Грешка при update на ЛБД Mod6MX/TableInfo\n" + ErrorString);
                    return false;
                }
            }
            return true;
        }
        public static void TransferNestInfoToLotInfo(TablePositionInfo ni, LotInfo li)
        {
            li.Lot = ni.Lot;
            li.LotIdent = ni.LotIdent;
            li.Operator = ni.Operator;
            li.Order = ni.LotOrder;
            li.Product = ni.Product;
            li.Quantity = ni.Quantity;
        }
        public static void TransferLotInfoToNestInfo(LotInfo li, TablePositionInfo ni)
        {
            ni.Lot = li.Lot;
            ni.LotIdent = li.LotIdent;
            ni.Operator = li.Operator;
            ni.Product = li.Product;
            ni.Quantity = li.Quantity;
        }

        public static string CreateNestShortStatus(TablePositionInfo ni)
        {
            string s = "L=" + ni.Loaded.ToString() + ", W=" + ni.Welded.ToString() + ", M=" + ni.Marked.ToString() + ", R=" + ni.Result.ToString();
            return s;
        }
        #endregion


        private void MainThread()
        {
            // IO
            IObitDetails RotaryTableReset = new IObitDetails("", "", true, "", 100, 11); //bit F_Reset = 1 Rotary Table Reset

            // CT
            TimeSpan cycleTS;
            string currentCycleTime = string.Empty;

            MachineState = STATE_WAIT_FOR_INIT;
            int nStep = 0;

            while (true)
            {
                if (CheckForErrors())
                {
                    nStep = 0;
                }
                else if (bDiagnosticWindowOpened)
                {
                    MachineState = STATE_DIAGNOSTIC_MODE;
                }
                else if (bInit)
                {
                    nStep = 0;
                }
                switch (nStep)
                {
                        case 0: // Waiting for INIT
                            MachineState = STATE_WAIT_FOR_INIT;
                            if (bInit)
                            {
                                bInit = false;
                                updIO.StateI = 0;
                                bProcessNoRotate = true;
                                RotaryTableReset.BitValue = true;
                                UpdateIO.ModifyOutputBit(RotaryTableReset);
                                nStep = 10;
                            }
                            break;

                        case 10: // Init Stations
                            Loader.Init();
                            Welder.Init();
                            Marker.Init();
                            MachineState = STATE_INITIALIZING;
                            Log("Инциализация");
                            RotaryTableReset.BitValue = false;
                            UpdateIO.ModifyOutputBit(RotaryTableReset);
                            nStep = 20;
                            break;

                        case 20: //Wait StationS Ready
                            if (Loader.Ready && !Loader.Error &&
                            Welder.Ready && !Welder.Error &&
                            Marker.Ready && !Marker.Error)
                            {
                                nStep = 31;
                            }
                            else if (Loader.Error || Welder.Error || Marker.Error)
                            {
                                MachineState = STATE_WAIT_FOR_INIT_AFTER_ERROR;
                                bInit = false;
                                nStep = 100;
                            }
                            break;

                        
                        case 31: // Wait new lot
                        if (Pieces == 0)
                        {
                            MachineState = STATE_WAIT_FOR_LOT;
                        }
                        else if (Pieces > 0)
                        { 
                            nStep = 40;
                        }
                         break;

                        case 40: // Start process
                            cycleTS = timerCycleTime.Elapsed;
                            MachineState = STATE_PROCESS;
                            bProcessNoRotate = false;
                            if (Pieces > 0) // check lot
                            {
                                Loader.Process();
                                Welder.Process();
                                Marker.Process();
                            }
                            else
                            {
                                nStep = 31;
                            }
                            break;
                        
                        case 100: //Error
                            if (bInit)
                            {
                                nStep = 0;
                            }
                            break;
                        default:
                            break;
                    }
               // Thread.Sleep(1);
            }
        }


        #region Station1
        public class Station1 : AProcess
        {
            TimeSpan cycleTS;
            string currentCycleTime = string.Empty;
            private Thread thWork;
            private Brush StatusBackground_ = Brushes.White;
            private string logFileName = @"D:\Machine\Loader_processFlow.txt";
            private Stopwatch stopWatch = new Stopwatch();
            private LotInfo LotInfo = new LotInfo();
            private TablePositionInfo NestInfo = new TablePositionInfo();
            private int posIndex;
            List<StationError> Errors = new List<StationError>();
            List<StationError> Prompts = new List<StationError>();
            public Station1(string name, MainWindow uiInstance)
            {
                Name = name;
                ui = uiInstance;
                ui.lblSt1Status.DataContext = this;
                stopWatch.Start();
                InitErrors();
                InitPrompts();

                thWork = new System.Threading.Thread(new System.Threading.ThreadStart(WorkThreadMethod));
                thWork.Name = Name;
                thWork.IsBackground = true;
                thWork.Start();
            }

            private void InitErrors()
            {
                //Errors in Load procedure
                Errors.Add(new StationError(1, "Ротаторът не спира в началото на процеса"));
                Errors.Add(new StationError(2, "Ротаторът не започва хомиране"));
                Errors.Add(new StationError(3, "Ротаторът не се хомира"));
                Errors.Add(new StationError(4, "Гриперът не се отваря"));
                Errors.Add(new StationError(5, "Ролките не се отварят"));
                Errors.Add(new StationError(6, "Кофата не се отваря"));
                Errors.Add(new StationError(7, "Кофата не се затваря"));
                Errors.Add(new StationError(9, "Алайнерът не се отваря"));
                Errors.Add(new StationError(10, "Алайнерът не се затваря"));
                Errors.Add(new StationError(11, "Гриперът не се затваря"));
                Errors.Add(new StationError(12, "Ролките не се затварят"));
                //Errors for Loader in Init procedure
                Errors.Add(new StationError(38, "Init: Кофата не се отваря"));
                Errors.Add(new StationError(39, "Init: Има сензор, но гриперът не се затваря"));
                Errors.Add(new StationError(40, "Init: Ролките не се отварят"));
                Errors.Add(new StationError(41, "Init: Алайнерът не се отваря"));
                Errors.Add(new StationError(42, "Init: Ротаторът не спира да се върти"));
            }
            private void InitPrompts()
            {
                //Prompts in Load procedure
                Prompts.Add(new StationError(1, "Чака стартиране на процесите от РС"));
                Prompts.Add(new StationError(2, "Чака освобождаване на педала"));
                Prompts.Add(new StationError(4, "Чака сваляне на изделието"));
                Prompts.Add(new StationError(8, "Чака натискане на педала"));
                Prompts.Add(new StationError(16, "Има изделие, но няма гайка (а трябва да има)"));
            }

            private string DisplayStationStatus(uint errorCode, uint stateCode)
            {
                if (errorCode != 0)
                {
                    int index = Errors.FindIndex(eCode => eCode.ErrCode == errorCode);
                    if (index >= 0) return Errors[index].ErrorText;
                    else return "Неизвестен код за грешка " + errorCode.ToString();
                }
                else return "";
            }
            private string DisplayStationPrompt(uint promptCode)
            {
                if (promptCode != 0)
                {
                    int index = Prompts.FindIndex(eCode => eCode.ErrCode == promptCode);
                    if (index >= 0) return Prompts[index].ErrorText;
                    else return "Неизвестен код за подкана " + promptCode.ToString();
                }
                else return "";
            }
            private bool InitProcedure()
            {
                // Инициализацията се изпълнява от стейт Start до стейт End
                // Ако няма грешки и стейта стане по-голям от End инициализацията приключва
                Paused = false;
                int InitState_Start = 5;
                int InitState_End = 12;

                if (!updIO.iCommonRotTableInPos) { ErrorEvent("Масата не е на позиция"); Initialized = false; return true; }

                while (true)
                {
                    if (updIO.StateI > 0 && updIO.StateI < InitState_Start) ChangeStatus("Очаква условия за инициализация...", Brushes.Yellow);
                    else if (updIO.StateI >= InitState_Start && updIO.StateI < InitState_End)
                    {
                        ChangeStatus("Инициализира се...", Brushes.Yellow);
                        while (updIO.StateI < InitState_End && updIO.ErrorI == 0) // Инициализация
                        {
                            if (CheckForErrors()) return true;
                            if (Command == CMD_INIT) return true;
                            Thread.Sleep(5);
                        }
                        if (updIO.ErrorI > 0) // Проверка за грешки
                        {
                            ChangeStatus(DisplayStationStatus((ushort)(30 + updIO.ErrorI), updIO.StateI), Brushes.Red);
                            return true;
                        }
                    }
                    else if (updIO.StateI >= InitState_End) break; // Край на инициализация

                    Thread.Sleep(1);
                }

                Initialized = true;
                ChangeStatus("Инициализирана", Brushes.LightGreen);
                return false;
            }

            private bool ProcessProcedure()
            {
                DateTime StartProcess;
                bool resNestInfo;
                string posStatus = "";
                int stepProcess = 0;
                if (!Initialized)
                {
                    ErrorEvent("Станцията не е инициализирана!");
                    return true;
                }
                else
                {
                    if (!updIO.iCommonRotTableInPos)
                    {
                        ErrorEvent("Масата не е на позиция");
                        Initialized = false;
                        return true;
                    }
                    else
                    {
                        StartProcess = DateTime.Now;
                        stopWatch.Reset();
                        LogDuration(logFileName, ref stopWatch, "Начало");
                        if (!GetNestInfo(1, out posIndex, ref NestInfo))
                        {
                            ErrorEvent("Не изтегля данните за позицията на масата");
                            Initialized = false;
                            return true;
                        }
                        else
                        {
                            updIO.StateL = 0; // Reset State
                            ChangeStatus("Стартиране на процеса...", Brushes.Yellow);
                            while (true) // Process
                            {
                                if (updIO.ErrorL != 0)
                                {
                                    ChangeStatus(DisplayStationStatus(updIO.ErrorL, updIO.StateL), Brushes.Salmon);
                                    return true;
                                }
                                else if (updIO.Prompts != 0)
                                {
                                    ChangeStatus(DisplayStationPrompt(updIO.Prompts), Brushes.Yellow);
                                }
                                if (CheckForErrors()) return true;
                                if (Command == CMD_INIT) return true;
                                switch (stepProcess)
                                {
                                    case 0:
                                        if (updIO.StateL == 1 && !updIO.F_EnableLoaderStart) // Start the process
                                        {
                                            ui.timerCycleTime.Start();
                                            cycleTS = ui.timerCycleTime.Elapsed;
                                            currentCycleTime = String.Format("{0:00}:{1:00}.{2:00}", cycleTS.Minutes, cycleTS.Seconds, cycleTS.Milliseconds / 10);
                                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                                            {
                                                ui.labelCycleTime.Content = currentCycleTime;
                                            }));
                                            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 109, UpdateIO.bitFlagsC109_EnableLoaderStart), mcOMRON.MemoryArea.CIO_Bit);
                                            stepProcess = 1;
                                        }
                                        else if (updIO.StateL == 22 && updIO.Prompts == 0) // Waiting for Piece
                                        {
                                            ChangeStatus("Изчаква зареждане на нов сензор...", Brushes.Yellow);
                                        }
                                        else if (updIO.StateL == 98 && !updIO.F_EnableLoaderEnd
                                            && updIO.HF_load_done) // End Process
                                        {
                                            TransferLotInfoToNestInfo(CurrentLot, NestInfo);
                                            NestInfo.Loaded = true;
                                            NestInfo.Welded = false;
                                            NestInfo.Marked = false;
                                            NestInfo.VisionOK = false;
                                            NestInfo.WeldedFromCam = false;
                                            NestInfo.GoodForMark = false;
                                            NestInfo.MarkedWith = "";
                                            NestInfo.Result = 0;
                                            NestInfo.Snumber = (short)CurrentLot.CurrentSN;
                                            CurrentLot.CurrentSN++;
                                            Properties.Settings.Default.CurrentSN = CurrentLot.CurrentSN;
                                            ui.InsertSensorIntoDatabase(NestInfo, posIndex); //запис в локалната БД
                                            if (!SetNestInfo(1, NestInfo))
                                            {
                                                ErrorEvent("Не записа данните за позицията на масата");
                                                return true;
                                            }
                                            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 109, UpdateIO.bitFlagsC109_EnableLoaderEnd), mcOMRON.MemoryArea.CIO_Bit);
                                            stepProcess = 2;
                                        }
                                        break;

                                    case 1: // Wait Start Confirmed
                                        if (updIO.F_EnableLoaderStart || updIO.StateL > 1)
                                        {
                                            stepProcess = 0;
                                        }
                                        else
                                        {
                                            if (updIO.Prompts == 0)
                                                ChangeStatus("Изчаква Старт на процеса...", Brushes.Yellow);
                                        }
                                        break;

                                    case 2: // Wait Start Confirmed
                                        if ((updIO.F_EnableLoaderEnd && updIO.StateL == 99) || updIO.StateL == 1)
                                        {
                                            ChangeStatus("Край на процеса, очаква готовност на всички станции", Brushes.Yellow);
                                            ProcessDuration = DateTime.Now - StartProcess;
                                            using (System.IO.StreamWriter sw = File.AppendText(logFileName))
                                            {
                                                sw.WriteLine(String.Format("{0:00}.{1:000}", ProcessDuration.Seconds, ProcessDuration.Milliseconds) + " Общо време за процеса с нов статус за SN=" + NestInfo.Snumber.ToString() + ": " + posStatus);
                                            }
                                            stepProcess = 10;
                                        }
                                        else
                                        {
                                            ChangeStatus("Изчаква Край на процеса...", Brushes.Yellow);
                                        }
                                        break;

                                    case 10: // wait start process 
                                        if (updIO.StateL == 1 && updIO.iCommonRotTableInPos)
                                        {
                                            if (!GetNestInfo(1, out posIndex, ref NestInfo))
                                            {
                                                ErrorEvent("Не изтегля данните за позицията на масата");
                                                Initialized = false;
                                                return true;
                                            }
                                            else
                                            {
                                                posStatus = CreateNestShortStatus(NestInfo);
                                                LogDuration(logFileName, ref stopWatch, "Начало");
                                                stepProcess = 0;
                                            }
                                        }
                                        else if (updIO.StateL == 98 && !updIO.F_EnableLoaderEnd)
                                        {
                                            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 109, UpdateIO.bitFlagsC109_EnableLoaderEnd), mcOMRON.MemoryArea.CIO_Bit);
                                            stepProcess = 2;
                                        }
                                        break;
                                }

                                Thread.Sleep(1);
                            }
                        }
                    }
                }
            }

            private void WorkThreadMethod()
            {
                if (updIO.ErrorL == 0) ChangeStatus("Изчакване за инициализация");
                Command = 0;
                Error = true;
                while (true)
                {
                    if (Command > 0)
                    {
                        Ready = false;
                        switch (Command)
                        {
                            case CMD_INIT:
                                Command = 0;
                                Error = InitProcedure();
                                Ready = true;
                                break;

                            case CMD_PROCESS:
                                Command = 0;
                                Error = ProcessProcedure();
                                Ready = true;
                                break;
                        }
                    }

                    Thread.Sleep(25);
                }
            }
        }
        #endregion

        #region Station2
        public class Station2 : AProcess
        {
            private System.Threading.Thread thWork;
            private string logFileName = @"D:\Machine\Welder_processFlow.txt";
            private Stopwatch stopWatch = new Stopwatch();
            private LotInfo LotInfo = new LotInfo();
            private TablePositionInfo NestInfo = new TablePositionInfo();
            List<StationError> Errors = new List<StationError>();
            public Station2(string name, MainWindow uiInstance)
            {
                Name = name;
                ui = uiInstance;
                ui.lblSt2Status.DataContext = this;
                stopWatch.Start();
                InitErrors();

                thWork = new System.Threading.Thread(new System.Threading.ThreadStart(WorkThreadMethod));
                thWork.Name = Name;
                thWork.IsBackground = true;
                thWork.Start();
            }

            private void InitErrors()
            {
                //Errors in Weld procedure
                Errors.Add(new StationError(1, "Festo не се инициализира"));
                Errors.Add(new StationError(2, "LinMot не се инициализира"));
                Errors.Add(new StationError(3, "Ролките не се затварят"));
                Errors.Add(new StationError(4, "Ротаторът не спира в началото на процеса"));
                Errors.Add(new StationError(5, "Алайнерът не се отваря"));
                Errors.Add(new StationError(6, "Гриперът не се отваря"));
                Errors.Add(new StationError(7, "LinMot не спира при притискане"));
                Errors.Add(new StationError(8, "Пушерът не прави контакт с изделието"));
                Errors.Add(new StationError(9, "Гриперът не се затваря"));
                Errors.Add(new StationError(10, "Festo не отива на позиция за заваряване"));
                Errors.Add(new StationError(11, "Ротаторът не започва въртене"));
                Errors.Add(new StationError(12, "Дюзата на аргона не влиза в позиция за заваряване"));
                Errors.Add(new StationError(13, "Налягането на аргона не е в граници"));
                Errors.Add(new StationError(14, "Лазерът не се включва (ErrorLaser=1: On=0)"));
                Errors.Add(new StationError(15, "Не може да вземе лазера (ErrorLaser=2: Assigned=0)"));
                Errors.Add(new StationError(16, "Лазерът е в грешка (ErrorLaser=3: Fault=1)"));
                Errors.Add(new StationError(17, "Лазерът не влиза в готовност (ErrorLaser=4: ExtAct=0 || Assigned=0 || Ready=0)"));
                Errors.Add(new StationError(18, "Лазерът е в грешка (ErrorLaser=5: Fault=1)"));
                Errors.Add(new StationError(19, "Лазерът е в грешка (ErrorLaser=6: Fault=1)"));
                Errors.Add(new StationError(20, "Таймаут на лазерната програма (ErrorLaser=7: ProgramCompleted=0 || Ready=0)"));
                Errors.Add(new StationError(21, "LinMot не се вдига на безопасна позиция"));
                Errors.Add(new StationError(22, "Дюзата на аргона не напуска позиция за заваряване"));

                //Errors for Welder in Init procedure
                Errors.Add(new StationError(43, "Init: Festo не се инициализира"));
                Errors.Add(new StationError(44, "Init: LinMot не се инициализира"));
                Errors.Add(new StationError(45, "Init: Има сензор, но гриперът не се затваря"));
                Errors.Add(new StationError(46, "Init: Ролките не се затварят"));
                Errors.Add(new StationError(47, "Init: Алайнерът не се отваря"));
                Errors.Add(new StationError(48, "Init: Дюзата на аргона не излиза от зоната"));
                Errors.Add(new StationError(49, "Init: Ротаторът не спира да се върти"));
                Errors.Add(new StationError(50, "Init: Пушерът не се вдига на безопасна позиция"));
                Errors.Add(new StationError(51, "Init: Festo не реферирал"));
                Errors.Add(new StationError(52, "Init: Festo не се е придвижил до позиция за заварка"));
            }
            private string DisplayStationStatus(uint errorCode, uint stateCode)
            {
                if (errorCode != 0)
                {
                    int index = Errors.FindIndex(eCode => eCode.ErrCode == errorCode);
                    if (index >= 0) return Errors[index].ErrorText;
                    else return "Неизвестен код за грешка " + errorCode.ToString();
                }
                else return stateCode.ToString();
            }
            private bool InitProcedure()
            {
                // Инициализацията се изпълнява от стейт Start до стейт End
                // Ако няма грешки и стейта стане по-голям от End инициализацията приключва
                Paused = false;
                int InitState_Start = 12;
                int InitState_End = 30;

                if (!updIO.iCommonRotTableInPos) { ErrorEvent("Масата не е на позиция"); Initialized = false; return true; }

                while (true)
                {
                    if (updIO.StateI > 0 && updIO.StateI < InitState_Start) ChangeStatus("Изчаква инициализация на станция Loading...", Brushes.Yellow);
                    else if (updIO.StateI >= InitState_Start && updIO.StateI < InitState_End)
                    {
                        ChangeStatus("Инициализира се...", Brushes.Yellow);
                        while (updIO.StateI >= InitState_Start && updIO.StateI < InitState_End
                            && updIO.ErrorI == 0) // Инициализация
                        {
                            if (CheckForErrors()) return true;
                            if (Command == CMD_INIT) return true;
                            Thread.Sleep(5);
                        }
                        if (updIO.ErrorI > 0) // Проверка за грешки
                        {
                            ChangeStatus(DisplayStationStatus((ushort)(30 + updIO.ErrorI), updIO.StateI), Brushes.Red);
                            return true;
                        }
                    }
                    else if (updIO.StateI >= InitState_End) break; // край на инициализация

                    Thread.Sleep(1);
                }

                Initialized = true;
                ChangeStatus("Инициализирана", Brushes.LightGreen);
                return false;
            }

            private bool ProcessProcedure()
            {
                int posIndex;
                string posStatus;
                DateTime StartProcess;
                stopWatch.Reset();
                int stepProcess = 0;
                int tTimeout = 0;
                bool bTimeout = false;

                if (!Initialized)
                {
                    ErrorEvent("Станцията не е инициализирана!");
                    return true;
                }
                else
                {
                    if (!updIO.iCommonRotTableInPos)
                    {
                        ErrorEvent("Масата не е на позиция");
                        Initialized = false;
                        return true;
                    }
                    else
                    {
                        StartProcess = DateTime.Now;
                        stopWatch.Reset();
                        LogDuration(logFileName, ref stopWatch, "Начало");
                        if (!GetNestInfo(1, out posIndex, ref NestInfo))
                        {
                            ErrorEvent("Не изтегля данните за позицията на масата");
                            Initialized = false;
                            return true;
                        }
                        else
                        {
                            posStatus = CreateNestShortStatus(NestInfo);
                            LogDuration(logFileName, ref stopWatch, "Начало");
                            UpdateIO.ModifyOutputBit(new IObitDetails("", "", false, "", 100, UpdateIO.bitFlagsC100_Welded), mcOMRON.MemoryArea.CIO_Bit);
                            int VisionResult = -1;
                            string eStr = String.Empty;
                            double beam_offset = 0;
                            bool VisionOK = false;
                            bool WeldedFromCamera = false;
                            updIO.StateW = 0;
                            ChangeStatus("Стартиране на процеса...", Brushes.Yellow);
                            while (true) // start process
                            {
                                if (updIO.ErrorW != 0)
                                {
                                    ChangeStatus(DisplayStationStatus(updIO.ErrorW, updIO.StateW), Brushes.Salmon);
                                    return true;
                                }
                                if (CheckForErrors()) return true;
                                if (Command == CMD_INIT) return true;
                                switch (stepProcess)
                                {
                                    case 0:
                                        if (updIO.StateW == 1 && !updIO.F_EnableWelderStart)
                                        {
                                            UpdateIO.ModifyOutputDoubleWord(mcOMRON.MemoryArea.CIO, 162, (uint)(10000 * CurrentProduct.ProductLength)); // Update Product Length
                                            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 109, UpdateIO.bitFlagsC109_EnableWelderStart), mcOMRON.MemoryArea.CIO_Bit);
                                            stepProcess = 1;
                                        }
                                        else if (updIO.StateW == 14 && updIO.F_LaserGetOffset)
                                        {
                                            ChangeStatus("Прави снимка и изчислява корекцията...", Brushes.Yellow);
                                            CognexCameras.RunCogJobOnce("Cam");
                                            stepProcess = 10;
                                        }
                                        else if (updIO.StateW == 98 && !updIO.F_EnableWelderEnd
                                            && updIO.HF_weld_done)
                                        {
                                            NestInfo.Welded = updIO.F_Welded;
                                            NestInfo.GoodForMark = updIO.F_Welded;
                                            if (!SetNestInfo(2, NestInfo))
                                            {
                                                ErrorEvent("Не записа данните за позицията на масата");
                                                return true;
                                            }
                                            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 109, UpdateIO.bitFlagsC109_EnableWelderEnd), mcOMRON.MemoryArea.CIO_Bit);
                                            stepProcess = 2;
                                        }
                                        break;

                                    case 1: // Wait Start Process
                                        if (updIO.F_EnableWelderStart || updIO.StateW > 1)
                                        {
                                            stepProcess = 0;
                                        }
                                        else
                                        {
                                            ChangeStatus("Изчаква Стартиране на процеса...", Brushes.Yellow);
                                        }
                                        break;

                                    case 2: // Wait Process end
                                        if (updIO.F_EnableWelderEnd && updIO.StateW == 99)
                                        {
                                            ProcessDuration = DateTime.Now - StartProcess;
                                            using (System.IO.StreamWriter sw = File.AppendText(logFileName))
                                            {
                                                sw.WriteLine(String.Format("{0:00}.{1:000}", ProcessDuration.Seconds, ProcessDuration.Milliseconds) + " Общо време за процеса с нов статус: " + posStatus);
                                            }
                                            ChangeStatus("Край на процеса, очаква готовност на всички станции", Brushes.Yellow);
                                            ui.UpdateSensorSt2IntoDatabase(NestInfo, posIndex); //запис в локалната БД
                                            stepProcess = 60;
                                        }
                                        else
                                        {
                                            ChangeStatus("Изчаква Край на процеса", Brushes.Yellow);
                                        }

                                        break;

                                    case 10: // Wait Camera ready
                                        while (!CognexCameras.ResultsReady)
                                        {
                                            Thread.Sleep(10);
                                            if (tTimeout >= 200)
                                            {
                                                tTimeout = 0;
                                                bTimeout = true;
                                                break;
                                            }
                                        };
                                        if (CognexCameras.VisionError)
                                        {
                                            ui.Log(eStr + " [" + Name + "]");
                                            LogDuration(logFileName, ref stopWatch, "Резултат за грешка от камерата " + VisionResult.ToString());
                                            ErrorEvent("Грешка от камерата (" + VisionResult.ToString() + ")");
                                            return true;
                                        }
                                        if (bTimeout)
                                        {
                                            ErrorEvent("Изтекло време за резултат от камерата (" + tTimeout + ")");
                                            return true;
                                        }
                                        else
                                        {
                                            stepProcess = 20;
                                        }
                                        break;

                                    case 20: // Camera results
                                        try
                                        {
                                            beam_offset = Convert.ToDouble(CognexCameras.ResultItemsArr[0].Result);
                                            WeldedFromCamera = CognexCameras.ResultItemsArr[1].Result.Contains("True") ? true : false;
                                            VisionResult = Convert.ToInt32(CognexCameras.ResultItemsArr[2].Result.Contains("True") ? 1 : 0);
                                            stepProcess = 30;
                                        }
                                        catch (Exception ex)
                                        {
                                            ErrorEvent("Камерата вдигна ексепшън " + ex.ToString());
                                            return true;
                                        }
                                        break;

                                    case 30:
                                        if (VisionResult == 0)
                                        {
                                            ui.Log(eStr + " [" + Name + "]");
                                            LogDuration(logFileName, ref stopWatch, "Камерата не намира обекта");
                                            ErrorEvent("Камерата не намира обекта");
                                            return true;
                                        }
                                        else
                                        {
                                            stepProcess = 40;
                                        }
                                        break;

                                    case 40:
                                        //получава низ с три флага: релативно отместване на главата с Festo, съществуваща заварка и годен резултат
                                        //обработка на резултата от камерата
                                        VisionOK = true;
                                        NestInfo.VisionOK = VisionOK;
                                        NestInfo.WeldedFromCam = WeldedFromCamera;
                                        UpdateIO.ModifyOutputBit(new IObitDetails("", "", VisionOK, "", 100, UpdateIO.bitFlagsC100_VisionOK));
                                        UpdateIO.ModifyOutputBit(new IObitDetails("", "", WeldedFromCamera, "", 100, UpdateIO.bitFlagsC100_WeldedFromCam));

                                        if (!WeldedFromCamera)
                                        {
                                            //изч. laser_offset
                                            int laser_offset = (int)(10000 * Math.Round(beam_offset, 2) + Properties.Settings.Default.CameraOffset);
                                            //запис на Laser_offset в PLC
                                            UpdateIO.ModifyOutputDoubleWord(mcOMRON.MemoryArea.CIO, 160, (uint)laser_offset);
                                        }
                                        //изчистване на флага, за да покаже, че камерата е приключила
                                        UpdateIO.ModifyOutputBit(new IObitDetails("", "", false, "", 100, UpdateIO.bitFlagsC100_LaserGetOffset));
                                        stepProcess = 50;
                                        break;

                                    case 50:
                                        if (!updIO.F_LaserGetOffset)
                                        {
                                            stepProcess = 0;
                                        }
                                        else
                                        {
                                            ChangeStatus("Изчаква обработка на камера офсетите...", Brushes.Yellow);
                                        }
                                        break;

                                    case 60:
                                        if (updIO.StateW == 1 && updIO.iCommonRotTableInPos)
                                        {
                                            if (!GetNestInfo(2, out posIndex, ref NestInfo)) // Get Nest Info
                                            {
                                                ErrorEvent("Не изтегля данните за позицията на масата");
                                                Initialized = false;
                                                return true;
                                            }
                                            else
                                            {
                                                posStatus = CreateNestShortStatus(NestInfo);
                                                LogDuration(logFileName, ref stopWatch, "Начало");
                                                UpdateIO.ModifyOutputBit(new IObitDetails("", "", false, "", 100, UpdateIO.bitFlagsC100_Welded), mcOMRON.MemoryArea.CIO_Bit);
                                                stepProcess = 0;
                                            }
                                        }
                                        else if (updIO.StateW == 98 && !updIO.F_EnableWelderEnd)
                                        {
                                            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 109, UpdateIO.bitFlagsC109_EnableWelderEnd), mcOMRON.MemoryArea.CIO_Bit);
                                            stepProcess = 0;
                                        }
                                        break;
                                }
                                Thread.Sleep(1);
                            }
                        }
                    }
                }
            }

            private void WorkThreadMethod()
            {
                if (updIO.ErrorW == 0) ChangeStatus("Изчакване за инициализация");
                Command = 0;
                Error = true;
                while (true)
                {
                    if (Command > 0)
                    {
                        Ready = false;
                        switch (Command)
                        {
                            case CMD_INIT:
                                Command = 0;
                                Error = InitProcedure();
                                Ready = true;
                                break;

                            case CMD_PROCESS:
                                Command = 0;
                                Error = ProcessProcedure();
                                Ready = true;
                                break;
                        }
                    }

                    Thread.Sleep(25);
                }
            }
        }
        #endregion

        #region Station3
        public class Station3 : AProcess
        {

            private System.Threading.Thread thWork;
            private LotInfo LotInfo = new LotInfo();
            private string logFileName = @"D:\Machine\Marker_processFlow.txt";
            private Stopwatch stopWatch = new Stopwatch();
            private TablePositionInfo NestInfo = new TablePositionInfo();
            List<StationError> Errors = new List<StationError>();
            
            public Station3(string name, MainWindow uiInstance)
            {
                Name = name;
                ui = uiInstance;
                ui.lblSt3Status.DataContext = this;
                stopWatch.Start();
                InitErrors();

                thWork = new System.Threading.Thread(new System.Threading.ThreadStart(WorkThreadMethod));
                thWork.Name = Name;
                thWork.IsBackground = true;
                thWork.Start();
            }


            private void InitErrors()
            {
                //Errors in Weld procedure
                Errors.Add(new StationError(1, "Ролките не се затварят"));
                Errors.Add(new StationError(2, "Ротаторът не спира в началото"));
                Errors.Add(new StationError(3, "Ротаторът не започва завъртане за следващ ред"));
                Errors.Add(new StationError(4, "Ротаторът не спира завъртането за следващ ред"));

                //Errors for Welder in Init procedure
                Errors.Add(new StationError(61, "Init: Има сензор, но гриперът не се затваря"));
                Errors.Add(new StationError(62, "Init: Ролките не се затварят"));
                Errors.Add(new StationError(63, "Init: Алайнерът не се отваря"));
                Errors.Add(new StationError(64, "Init: Ротаторът не спира да се върти"));
            }
            private string DisplayStationStatus(uint errorCode, uint stateCode)
            {
                if (errorCode != 0)
                {
                    int index = Errors.FindIndex(eCode => eCode.ErrCode == errorCode);
                    if (index >= 0) return Errors[index].ErrorText;
                    else return "Неизвестен код за грешка " + errorCode.ToString();
                }
                else return stateCode.ToString();
            }
            private bool InitProcedure()
            {
                // Инициализацията се изпълнява от стейт Start до стейт End
                // Ако няма грешки и стейта стане по-голям от End инициализацията приключва
                Paused = false;
                int InitState_Start = 30;
                int InitState_End = 31;

                if (!updIO.iCommonRotTableInPos) { ErrorEvent("Масата не е на позиция"); Initialized = false; return true; }

                while (true)
                {
                    if (updIO.StateI > 0 && updIO.StateI < InitState_Start) ChangeStatus("Изчаква инициализация на станция Welding..", Brushes.Yellow);
                    else if (updIO.StateI >= InitState_Start && updIO.StateI < InitState_End)
                    {
                        ChangeStatus("Инициализира се...", Brushes.Yellow);
                        while (updIO.StateI >= InitState_Start && updIO.StateI < InitState_End
                            && updIO.ErrorI == 0) // Инициализация
                        {
                            if (CheckForErrors()) return true;
                            if (Command == CMD_INIT) return true;
                            Thread.Sleep(5);
                        }
                        if (updIO.ErrorI > 0) // Проверка за грешки
                        {
                            ChangeStatus(DisplayStationStatus((ushort)(30 + updIO.ErrorI), updIO.StateI), Brushes.Red);
                            return true;
                        }
                    }
                    else if (updIO.StateI >= InitState_End) break; // Край на инициализация

                    Thread.Sleep(1);
                }
                lock (LockObjectStatus) { Initialized = true; }
                ChangeStatus("Инициализирана", Brushes.LightGreen);
                return false;
            }
            private bool ProcessProcedure()
            {
                int posIndex;
                string posStatus;
                bool WasMarked;
                string ActL2 = "";
                DateTime StartProcess;
                int stepProcess = 0;

                if (!Initialized)
                {
                    ErrorEvent("Станцията не е инициализирана!");
                    return true;
                }
                else
                {
                    if (!updIO.iCommonRotTableInPos)
                    {
                        ErrorEvent("Масата не е на позиция");
                        Initialized = false;
                        return true;
                    }
                    else
                    {
                        StartProcess = DateTime.Now;
                        stopWatch.Reset();
                        LogDuration(logFileName, ref stopWatch, "Начало");
                        if (!GetNestInfo(3, out posIndex, ref NestInfo))
                        {
                            ui.Log("Не изтегля данните за позицията на масата");
                            ErrorEvent("Не изтегля данните за позицията на масата");
                            Initialized = false;
                            return true;
                        }
                        else
                        {
                            UpdateIO.ModifyOutputBit(new IObitDetails("", "", NestInfo.Marked, "", 100, UpdateIO.bitFlagsC100_Marked), mcOMRON.MemoryArea.CIO_Bit);
                            UpdateIO.ModifyOutputBit(new IObitDetails("", "", NestInfo.GoodForMark, "", 100, UpdateIO.bitFlagsC100_GoodForMark), mcOMRON.MemoryArea.CIO_Bit);
                            ui.Log("Сетване на флагове Marked and Good for mark");
                            WasMarked = NestInfo.Marked;
                            posStatus = CreateNestShortStatus(NestInfo);
                            updIO.StateM = 0; // Reset State
                            ChangeStatus("Стартиране на процеса...", Brushes.Yellow);
                            ui.Log("Start MArk process");
                            while (true) // Process
                            {
                                if (updIO.ErrorM != 0)
                                {
                                    ChangeStatus(DisplayStationStatus(updIO.ErrorM, updIO.StateM), Brushes.Salmon);
                                    return true;
                                }
                                
                                if (CheckForErrors()) return true;
                                if (Command == CMD_INIT) return true;
                                switch (stepProcess)
                                {
                                    case 0:
                                        if (updIO.StateM == 1 && !updIO.F_EnableMarkerStart)
                                        {
                                            ui.Log("Стартиране на маркиране");
                                            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 109, UpdateIO.bitFlagsC109_EnableMarkerStart), mcOMRON.MemoryArea.CIO_Bit);
                                            if (!WasMarked)
                                            {
                                                DoMark(CurrentLot.CurrentSN, out ActL2);
                                            }
                                            else
                                            {
                                                UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 109, UpdateIO.bitFlagsC109_MarkingDone), mcOMRON.MemoryArea.CIO_Bit);
                                                ui.Log("Сетване на флаг MarkingDone");
                                            }
                                            stepProcess = 1;
                                        }
                                        if (updIO.StateM == 98 && !updIO.F_EnableMarkerEnd
                                            && updIO.HF_mark_done)
                                        {
                                            if (updIO.F_Marked)
                                            {
                                                NestInfo.MarkedWith = ActL2;
                                                NestInfo.Result = 0;
                                            }
                                            NestInfo.Marked = updIO.F_Marked;
                                            if (!SetNestInfo(3, NestInfo))
                                            {
                                                // Error
                                                ErrorEvent("Не записа данните за позицията на масата");
                                                return true;
                                            }
                                            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 109, UpdateIO.bitFlagsC109_EnableMarkerEnd), mcOMRON.MemoryArea.CIO_Bit);
                                            ui.Log("край на маркиране");
                                            stepProcess = 2;
                                        }
                                        break;

                                    case 1: // Wait Process Start
                                        if (updIO.F_EnableMarkerStart || updIO.StateM > 1)
                                        {
                                            stepProcess = 0;
                                        }
                                        else
                                        {
                                            ChangeStatus("Изчаква Стартиране на процеса...", Brushes.Yellow);
                                        }
                                        break;

                                    case 2: // Wait Process end
                                        if (updIO.F_EnableMarkerEnd && updIO.StateM == 99)
                                        {
                                            ProcessDuration = DateTime.Now - StartProcess;
                                            using (System.IO.StreamWriter sw = File.AppendText(logFileName))
                                            {
                                                sw.WriteLine(String.Format("{0:00}.{1:000}", ProcessDuration.Seconds, ProcessDuration.Milliseconds) + " Общо време за процеса с нов статус за SN=" + NestInfo.Snumber.ToString() + ": " + posStatus);
                                            }
                                            ChangeStatus("Край на процеса, очаква готовност на всички станции", Brushes.Yellow);

                                           // ui.MarkLabels();
                                            ui.UpdateSensorSt3IntoDatabase(NestInfo, posIndex);
                                            ui.WriteSensorIntoDatagrid(NestInfo, DateTime.Now, posIndex); //запис в таблицата на екрана
                                            stepProcess = 10;
                                        }
                                        else
                                        {
                                            ChangeStatus("Изчаква Край на процеса...", Brushes.Yellow);
                                        }
                                        break;

                                    case 10: // Process end
                                        if (updIO.StateM == 1 && updIO.iCommonRotTableInPos)
                                        {
                                            ui.timerCycleTime.Stop();
                                            //cycleTS = timerCycleTime.Elapsed;
                                            ui.timerCycleTime.Reset();
                                            if (!GetNestInfo(3, out posIndex, ref NestInfo))
                                            {
                                                ErrorEvent("Не изтегля данните за позицията на масата");
                                                Initialized = false;
                                                return true;
                                            }
                                            else
                                            {
                                                UpdateIO.ModifyOutputBit(new IObitDetails("", "", NestInfo.Marked, "", 100, UpdateIO.bitFlagsC100_Marked), mcOMRON.MemoryArea.CIO_Bit);
                                                UpdateIO.ModifyOutputBit(new IObitDetails("", "", NestInfo.GoodForMark, "", 100, UpdateIO.bitFlagsC100_GoodForMark), mcOMRON.MemoryArea.CIO_Bit);
                                                ui.Log(" flags marked and Good for mark  кейс 10");
                                                WasMarked = NestInfo.Marked;
                                                posStatus = CreateNestShortStatus(NestInfo);
                                                stepProcess = 0;
                                            }
                                        }
                                        else if (updIO.StateM == 98 && !updIO.F_EnableMarkerEnd)
                                        {
                                            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 109, UpdateIO.bitFlagsC109_EnableMarkerEnd), mcOMRON.MemoryArea.CIO_Bit);
                                            ui.Log("край на маркиране кейс 10");
                                            stepProcess = 2;
                                        }
                                        break;
                                }

                                Thread.Sleep(1);
                            }
                        }
                    }
                }
            }
            public void BuildLine2(string FormattingInfo, string ChipLot, bool RomanShifts, int SerNo, out string FilledString)
            {
                string testStr = FormattingInfo;
                string shiftString = "";
                int shift = 0;
                int dayWeek = 0;
                int weekNum = 0;
                int partYear = 0;

                FilledString = "";

                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("bg-BG");
                DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
                System.Globalization.Calendar cal = dfi.Calendar;
                DateTime today = DateTime.Now;
                dayWeek = (int)cal.GetDayOfWeek(today);
                weekNum = cal.GetWeekOfYear(today, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                partYear = today.Year % 100;
                int hour = today.TimeOfDay.Hours;
                if (hour >= 22 || hour < 6) shift = 3;
                else if (hour >= 14 && hour < 22) shift = 2;
                else if (hour >= 6 && hour < 14) shift = 1;
                else shift = 0;
                if (RomanShifts)
                {
                    if (shift == 1) shiftString = "I";
                    else if (shift == 2) shiftString = "II";
                    else shiftString = "III";
                }
                else shiftString = shift.ToString();

                // проверява за единично 'D', 'S', '@', 'YY', 'WW', 'BBBB' или 'BBBBB', 'N..." (колкото е дълго),
                // замества единично 'S' с броя на символите в смяната,
                // замества единично '@' с 'W'
                // замества поредиците от 'B' със символите в chiplot-а, като изравнява дължината към по-дългата
                // замества поредиците от 'N' със серийния номер, като попълва с водещи 0-и
                // останалите символи ги запазва
                // при повторение на форматираща поредица замества само първата й поява

                //проверява за повторения на типовете допустими форматиращи символи

                testStr = FormattingInfo;
                List<StringReps> ListSeq = AllSequencesofChar(testStr, 'D');
                ReplaceWithData(testStr, ListSeq, "D", dayWeek.ToString(), out FilledString);

                ListSeq = AllSequencesofChar(FilledString, 'S');
                ReplaceWithData(FilledString, ListSeq, "S", shiftString, out FilledString);

                ListSeq = AllSequencesofChar(FilledString, '@');
                ReplaceWithData(FilledString, ListSeq, "@", "W", out FilledString);

                ListSeq = AllSequencesofChar(FilledString, 'Y');
                ReplaceWithData(FilledString, ListSeq, "YY", partYear.ToString("D2"), out FilledString);

                ListSeq = AllSequencesofChar(FilledString, 'W');
                ReplaceWithData(FilledString, ListSeq, "WW", weekNum.ToString("D2"), out FilledString);

                ListSeq = AllSequencesofChar(FilledString, 'N');
                ReplaceWithData(FilledString, ListSeq, "NNNNN", SerNo.ToString("D5"), out FilledString);

                //последно се замества ChipLot, защото в този низ може да има някой от форматиращите символи
                ListSeq = AllSequencesofChar(FilledString, 'B');
                ReplaceWithData(FilledString, ListSeq, "BBBB", ChipLot, out FilledString);
            }

            public static bool ReplaceWithData(string currentStr, List<StringReps> listOfSeq, string formatStr, string actData, out string targetStr)
            {
                targetStr = currentStr;
                var myItem = listOfSeq.Find(Item => Item.SeqLen.Equals(formatStr.Length));
                if (myItem == null) return false;

                var aStringBuilder = new StringBuilder(targetStr);
                aStringBuilder.Remove(myItem.SeqFirst, formatStr.Length);
                aStringBuilder.Insert(myItem.SeqFirst, actData);
                targetStr = aStringBuilder.ToString();
                return true;
            }

            public List<StringReps> AllSequencesofChar(string SearchIn, char c)
            {
                string myStr = SearchIn;
                List<StringReps> indexes = new List<StringReps>();
                Debug.WriteLine("Test string " + SearchIn);
                while (true)
                {
                    for (int i = myStr.Length; ; i--)
                    {
                        if (i == 0) break;
                        string searchSeq = new string(c, i);
                        if (myStr.IndexOf(searchSeq, 0) != -1)
                        {
                            StringReps sRep = new StringReps(i, 0);
                            while (myStr.IndexOf(searchSeq, 0) != -1)
                            {
                                if (sRep.SeqFirst == -1) sRep.SeqFirst = myStr.IndexOf(searchSeq, 0);
                                sRep.SeqRep++;
                                var aStringBuilder = new StringBuilder(myStr);
                                aStringBuilder.Remove(myStr.IndexOf(searchSeq, 0), searchSeq.Length);
                                aStringBuilder.Insert(myStr.IndexOf(searchSeq, 0), new String(' ', searchSeq.Length));
                                myStr = aStringBuilder.ToString();
                            }
                            indexes.Add(sRep);
                        }
                    }
                    return indexes;
                }
            }
            public bool LoadMarkingFile(string mfn = "")
            {
                //return true;

                ////с версии?

                DateTime tStart;
                TimeSpan tSpent;
                string myMFname = "";
                if (mfn == "") myMFname = "MarkHousing";
                else myMFname = mfn;

                if (myMFname == markingFileLoaded) return true;

                if (mLaser == null)
                {
                    ErrorEvent("Няма обект на лазера");
                    return false;
                }

                if (!mLaser.Connected) mLaser.ChangePort(Properties.Settings.Default.PortMarkLaser);
                if (!mLaser.Connected)
                {
                    ErrorEvent("Не отваря порта на лазера");
                    return false;
                }
                mLaser.MarkingFileName = myMFname;
                mLaser.Command = MarkingLaserWithCom.LCMD_LDMARKFILE;
                tStart = DateTime.Now;
                while (mLaser.Command != 0)
                {
                    Thread.Sleep(10);
                    tSpent = DateTime.Now - tStart;
                    if (tSpent.TotalMilliseconds > 10000)
                    {
                        ErrorEvent("Таймаут за команда LOADMARKFILE");
                        markingFileLoadedOK = false;
                        mLaser.MarkingFileName = "";
                        return false;
                    }
                }
                if (mLaser.Error)
                {
                    ErrorEvent("Лазерът е в грешка. Вижте в 'Диагностика'");
                    markingFileLoadedOK = false;
                    mLaser.MarkingFileName = "";
                    return false;
                }
                else if (!mLaser.Connected)
                {
                    ErrorEvent("Връзката с лазера е прекъсната");
                    markingFileLoadedOK = false;
                    mLaser.MarkingFileName = "";
                    return false;
                }
                else return true;
            }

            public bool DoMark(int SerNo, out string ActualL2)
            {
                //return true;
                DateTime tStart;
                TimeSpan tSpent;
                ActualL2 = "Немаркиран";

                string markfn_default = @"C:\TRUMARK\VLF\MarkHousing";
                string markfiatfn = @"C:\TRUMARK\VLF\MarkHousingFiat";
                string markdaimler = @"C:\TRUMARK\VLF\MarkHousingLogoDaimler0";
                string markmoparfn = @"C:\TRUMARK\VLF\MarkHousingMopar0";
                string markfusofn = @"C:\TRUMARK\VLF\MarkHousingItalic";
                string markdetroitfn = @"C:\TRUMARK\VLF\MarkHousingDetroit";
                string markdecfn = @"C:\TRUMARK\VLF\DEC";

                fiat_sensor = CurrentProduct.L3.ToUpper().StartsWith("FPT");
                daimler_sensor = CurrentProduct.L3.ToUpper().StartsWith("DAIMLER");
                mopar_sensor = CurrentProduct.L3.ToUpper().StartsWith("MOPAR");
                fuso_sensor = CurrentProduct.L3.ToUpper().StartsWith("FUSO");
                detroit_sensor = CurrentProduct.L3.ToUpper().StartsWith("DETROIT");
                dec_sensor = CurrentProduct.L3.ToUpper().StartsWith("DEC");

                string[] LinesToMark = new string[]
                    {
                    CurrentProduct.L1,
                    CurrentProduct.L2,
                    CurrentProduct.L3,
                    CurrentProduct.L4
                    };

                if (fiat_sensor) LinesToMark[2] = LinesToMark[2].Substring(4);
                else if (detroit_sensor) LinesToMark[2] = LinesToMark[2].Substring(8);
                else if (mopar_sensor) LinesToMark[2] = LinesToMark[2].Substring(5);
                else if (daimler_sensor) LinesToMark[2] = LinesToMark[2].Substring(8);
                else if (dec_sensor) LinesToMark[2] = " "; //не е празен низ, за да маркира логото (без нищо друго) на този ред

                if (!LoadMarkingFile(CurrentProduct.MarkFN)) return false;
                if (!updIO.MLI_LaserReady) { ChangeStatus("Лазерът няма готовност", Brushes.Red); return true; }

                //започни маркиране
                for (int i = 0; i < LinesToMark.Length; i++)
                {
                    while (!updIO.F_MarkLine)
                    {
                        if (CheckForErrors()) return true;
                        if (Command == CMD_INIT) return true;
                        Thread.Sleep(10);
                    }
                    UpdateIO.ModifyOutputBit(new IObitDetails("", "", false, "", 109, UpdateIO.bitFlagsC109_MarkLine), mcOMRON.MemoryArea.CIO_Bit);

                    if (i == 2)
                    {
                        if (fiat_sensor)
                        {
                            if (!LoadMarkingFile(markfiatfn)) return false;
                        }
                        if (daimler_sensor)
                        {
                            if (!LoadMarkingFile(markdaimler)) return false;
                        }
                        if (mopar_sensor)
                        {
                            if (!LoadMarkingFile(markmoparfn)) return false;
                        }
                        else if (fuso_sensor)
                        {
                            if (!LoadMarkingFile(markfusofn)) return false;
                        }
                        else if (detroit_sensor)
                        {
                            if (!LoadMarkingFile(markdetroitfn)) return false;
                        }
                        else if (dec_sensor)
                        {
                            if (!LoadMarkingFile(markdecfn)) return false;
                        }
                    }

                    //изпращане на променливата към маркиращия файл
                    if (!mLaser.Connected)
                    {
                        mLaser.ChangePort(Properties.Settings.Default.PortMarkLaser);
                        if (!mLaser.Connected)
                        {
                            ErrorEvent("Не отваря порта на лазера");
                            return false;
                        }
                    }
                    mLaser.NameVar = "V01";
                    if (i == 2)
                    {
                        BuildLine2(LinesToMark[i], CurrentLot.ChipLot, Properties.Settings.Default.RomanShifts, SerNo, out ActualL2);
                        mLaser.ValueVar = ActualL2;
                    }
                    else mLaser.ValueVar = LinesToMark[i];
                    mLaser.Command = MarkingLaserWithCom.LCMD_SETVAR;
                    tStart = DateTime.Now;
                    while (mLaser.Command != 0)
                    {
                        Thread.Sleep(10);
                        tSpent = DateTime.Now - tStart;
                        if (tSpent.TotalMilliseconds > 5000)
                        {
                            ErrorEvent("Таймаут за команда SETVAR " + mLaser.NameVar + "...");
                            return false;
                        }
                    }
                    if (mLaser.Error)
                    {
                        ErrorEvent("Лазерът е в грешка");
                        return false;
                    }
                    else if (!mLaser.Connected)
                    {
                        ErrorEvent("Връзката с лазера се разпада");
                        return false;
                    }

                    //маркиране
                    if (!mLaser.Connected)
                    {
                        mLaser.ChangePort(Properties.Settings.Default.PortMarkLaser);
                        if (!mLaser.Connected)
                        {
                            ErrorEvent("Не отваря порта на лазера");
                            return false;
                        }
                    }
                    mLaser.Command = MarkingLaserWithCom.LCMD_MARKING;
                    tStart = DateTime.Now;
                    while (mLaser.Command != 0)
                    {
                        Thread.Sleep(10);
                        tSpent = DateTime.Now - tStart;
                        if (tSpent.Milliseconds > 10000)
                        {
                            ErrorEvent("Таймаут за команда MARKING");
                            return false;
                        }
                    }
                    if (mLaser.Error)
                    {
                        ErrorEvent("Лазерът е в грешка");
                        return false;
                    }
                    else if (!mLaser.Connected)
                    {
                        ErrorEvent("Връзката с лазера се разпада");
                        return false;
                    }

                    if (i < LinesToMark.Length - 1 && LinesToMark[i + 1] != "")
                        UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 109, UpdateIO.bitFlagsC109_MarkingRotate), mcOMRON.MemoryArea.CIO_Bit);
                    else
                    {
                        UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 109, UpdateIO.bitFlagsC109_MarkingDone), mcOMRON.MemoryArea.CIO_Bit);
                        break;
                    }

                    //презареждане на основния маркиращ файл, ако е имало маркиране на лого
                    if ((i == 2) &&
                            (fiat_sensor ||
                            daimler_sensor ||
                            mopar_sensor ||
                            fuso_sensor ||
                            detroit_sensor ||
                            dec_sensor)) {
                        if (!LoadMarkingFile(CurrentProduct.MarkFN)) return true; }
                }

                return true;
            }

            private void WorkThreadMethod()
            {
                if (updIO.ErrorW == 0) ChangeStatus("Изчакване за инициализация");
                Command = 0;
                Error = true;
                while (true)
                {
                    if (Command > 0)
                    {
                        Ready = false;
                        switch (Command)
                        {
                            case CMD_INIT:
                                Command = 0;
                                Error = InitProcedure();
                                Ready = true;
                                break;

                            case CMD_PROCESS:
                                Command = 0;
                                Error = ProcessProcedure();
                                Ready = true;
                                break;
                        }
                    }

                    Thread.Sleep(25);
                }
            }
        }
        #endregion


        #region ButtonEvents
        private void btnUnload_Click(object sender, RoutedEventArgs e)
        {
            //bUnload = true;
            UpdateIO.ModifyOutputBit(new IObitDetails("", "", true, "", 100, UpdateIO.bitFlagsC100_Unload));
        }


        private void lblSt3Status_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bCanContinue = true;
        }
        private void buttonProcessDowntime_Click(object sender, RoutedEventArgs e)
        {
            string operatorID = Operator;
            var downtimeWindow = new DownTimeWindow(Properties.Settings.Default.MachineID, operatorID);
            if (downtimeWindow.ShowDialog().Value)
            {
                operatorID = "";
            }
        }
        #endregion
    }
    #region CapCleanOutcome
    public class CapCleanOutcome
    {
        // следва всичко, с което искаме да съвместим горното
        // и тук идва моментът да използваме резултата от таска
        //в противен случай продължаваме
        public string ErrString { get; set; }
        public bool FinishedOK { get; set; }
    }
    #endregion

    #region TablePositionInfo
    public class TablePositionInfo
    {
        public string Lot { get; set; }
        public long LotIdent { get; set; }
        public string Product { get; set; }
        public string LotOrder { get; set; }
        public string Operator { get; set; }
        public short Snumber { get; set; }
        public short Quantity { get; set; }
        public bool Loaded { get; set; }
        public bool Welded { get; set; }
        public bool Marked { get; set; }
        public bool VisionOK { get; set; }
        public bool WeldedFromCam { get; set; }
        public string MarkedWith { get; set; }
        public short Result { get; set; }
        public bool GoodForMark { get; set; }
        public int SensorID { get; set; }

        public TablePositionInfo()
        {
            Lot = "";
            LotIdent = 0;
            Product = "";
            LotOrder = "";
            Operator = "";
            Snumber = 0;
            Quantity = 0;
            Loaded = false;
            Welded = false;
            Marked = false;
            VisionOK = false;
            WeldedFromCam = false;
            GoodForMark = false;
            MarkedWith = "";
            Result = 0;
            SensorID = 0;
        }
    }
    #endregion

    public class StationError
    {
        public uint ErrCode { get; set; }
        public string ErrorText { get; set; }
        public StationError(uint eCode, string eText)
        {
            ErrCode = eCode;
            ErrorText = eText;
        }
    }
    public class StringReps
    {
        public int SeqLen { get; set; }
        public int SeqRep { get; set; }
        public int SeqFirst { get; set; }
        public StringReps(int l, int r)
        {
            SeqLen = l;
            SeqRep = r;
            SeqFirst = -1;
        }
    }

}
