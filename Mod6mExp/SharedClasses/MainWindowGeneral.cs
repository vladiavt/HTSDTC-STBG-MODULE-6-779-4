using System;
using System.Collections.Generic;
using System.Threading;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Configuration;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Data;
using System.Windows.Threading;
using Machine.PDL.Base;
using Machine.Common;
using System.Drawing;

namespace Machine
{
    public partial class MainWindow
    {
        public const string FormatTimeToString = "yyyy.MM.dd HH:mm:ss";

        public class LogItem
        {
            public string RecDateTime { get; set; }
            public string Log { get; set; }
            public LogItem(string date_time, string log)
            {
                RecDateTime = date_time;
                Log = log;
            }
        }
        public class MLcomms
        {
            public string RecDateTime { get; set; }
            public string Msg { get; set; }
            public System.Windows.Media.Brush MsgForeground { get; set; }
            public MLcomms(string msg, System.Windows.Media.Brush mColor)
            {
                RecDateTime = DateTime.Now.ToString("HH:mm:ss");
                Msg = msg;
                MsgForeground = mColor;
            }
        }

        public class LotItem
        {
            public string Lot { get; set; }
            public string Product { get; set; }
            public string Order { get; set; }
            public string OperatorNumber { get; set; }
            public string Quantity { get; set; }
            public string RecDateTime { get; set; }
            public LotItem(string lot, string product, string order, string quantity, string operator_number, string rec_date_time)
            {
                Lot = lot;
                Product = product;
                Order = order;
                Quantity = quantity;
                OperatorNumber = operator_number;
                RecDateTime = rec_date_time;
            }
        }

        public static ObservableCollection<LotItem> ListLot = new ObservableCollection<LotItem>();
        public static ObservableCollection<LogItem> ListLog = new ObservableCollection<LogItem>();
        public static ObservableCollection<MLcomms> ListMLmess { get; set; }
        public object MyMLcommsLock = new object();
        public static object LockObjectPieces = new object();
        public static object LockObjectCycleTime = new object();
        public static object LockObjectDatagridSensors = new object();
        public static object LockObjectTraceability = new object();
        public static int MachineState_ = 0, oldMachineState = 0xFFFF;
        protected static int PiecesOK_ = 0, PiecesNOK_ = 0;
        protected static int Pieces_ = 0;
        public static string Lot = "", Product = "", Operator = "", OrderStr = "";
        public static string Order = "";
        public static short Quantity = 0;
        public bool ShowDiagnostic = false, ShowProducts = false, ShowProcessParameters = false, bShowEndLotWindow = false, DoInitialization = false;
        public bool ManualDiagnostic = false, ManualQuit = false, ManualProducts = false, ManualOn = false, ManualProcessParameters = false, ManualInitialization = false;
        private static bool bStartFinishTraceabilityReport_ = false;
        public static bool bStartFinishTraceabilityReport
        {
            get { bool ret; lock (LockObjectTraceability) ret = bStartFinishTraceabilityReport_; return ret; }
            set { lock (LockObjectTraceability) bStartFinishTraceabilityReport_ = value; }
        }
        public static string LocalDatabaseName = "";
        private DataGrid DataGridLots, DataGridLogs;
        public static double[] Counters, CountersTemporal;
        public static string ErrorString = "";
        static string[] oldSettings = new string[Properties.Settings.Default.Properties.Count];
        public static Stopwatch timerUpTime;
        public Stopwatch timerLot;
        public Stopwatch timerCycleTime;
        public RfidReader ReaderRFID;
        public static BarcodeReader ReaderBarcode;
        public int LastLotIdent = 0;
        public static float MachineTactTime = -1, LastCycleTime = 0;
        public string ResultString = "";
        public long TimeStartLotOffset = 0;
        public static bool NewSoftwareAvailable = false;
        //private RemoteStatusServer StatusServer;
        public static bool bLotFinished = true;
        public static DateTime LastLotBeginTime = new DateTime();
        public string[] SensorsTableColumns;
        private static object LockObjectMochineState = new object();
        public static bool bInit = false;
        private static RemoteStatusServer StatusServer;

        public static int MachineState
        {
            get
            {
                int ret = 0;
                lock (LockObjectMochineState) ret = MachineState_;
                return ret;
            }
            set
            {
                lock (LockObjectMochineState) MachineState_ = value;
            }
        }
        public static int Pieces
        {
            //get	{	int ret = 0; lock (LockObjectPieces) ret = Pieces_;	return ret; }
            get { return (Quantity - (PiecesOK + PiecesNOK)); }
            set { lock (LockObjectPieces) Pieces_ = value; }
        }
        public static int PiecesOK
        {
            //get { int ret = 0; lock (LockObjectPieces) ret = PiecesOK_; return ret; }
            get { return GetPiecesOK(); }
            set { lock (LockObjectPieces) PiecesOK_ = value; }
        }
        public static int PiecesNOK
        {
            //get { int ret = 0; lock (LockObjectPieces) ret = PiecesNOK_; return ret; }
            get { return GetPiecesNOK(); }
            set { lock (LockObjectPieces) PiecesNOK_ = value; }
        }

        public static int GetPiecesNOK()
        {
            int result = -10;

            try
            {
                result = Database.CommonDB.CountRecords("127.0.0.1",
                "Mod6MX",
                "Sensors",
                "[Lot]='" + MainWindow.Lot + "' AND [Result]>99");
            }
            catch (Exception ex)
            {
                result = -5;
            }

            return result;
        }

        public static int GetPiecesOK()
        {
            int result = -10;

            try
            {
                result = Database.CommonDB.CountRecords("127.0.0.1",
                "Mod6MX",
                "Sensors",
                "[Lot]='" + MainWindow.Lot + "' AND [Result]=0");
            }
            catch (Exception ex)
            {
                result = -5;
            }

            return result;
        }

        public delegate bool CheckForErrorsDelegate();
        public delegate string GetStatusStringDelegate(int status);
        CheckForErrorsDelegate CheckForErrorsFunction = null;
        GetStatusStringDelegate GetStatusStringFunction = null;

        public void InitGeneral(string basename, Object labelStatus, DataGrid dataGridLots,
                                                        DataGrid dataGridLogs, int CountersNumber, MachineWorkloadPieDiagram WorkLoadControlReference)
        {
            LocalDatabaseName = basename;
            DataGridLots = dataGridLots;
            DataGridLogs = dataGridLogs;

            if (CountersNumber > 0)
            {
                Counters = new double[CountersNumber];
                CountersTemporal = new double[CountersNumber];
            }

            string dummy = Properties.Settings.Default.MachineID;       // ??? настройките са празни докато не се изплолзват
            Properties.Settings.Default.SettingsSaving += new SettingsSavingEventHandler(Default_SettingsSaving);
            ImportSettings(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location) + ".set");

            if (WorkLoadControlReference != null) StatusServer = new RemoteStatusServer((Label)labelStatus, WorkLoadControlReference, LocalDatabaseName);

            if (CreateDatabaseTables() != 0)
            {
                Log("Грешка при обръщение към локалната база данни!");
                MessageBox.Show("Грешка при обръщение към локалната база данни!" + "\n\n" + ErrorString, "Грешка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            timerUpTime = new Stopwatch();
            timerUpTime.Start();

            timerLot = new Stopwatch();
            timerCycleTime = new Stopwatch();
        }

        private bool MoveCyl(int Output, bool OutputState, int Input, bool InputState, int Timeout, int ErrorState)
        {
            int t = 0;
            DioOut(Output, OutputState);
            while (true)
            {
                if (CheckForErrorsFunction != null) if (CheckForErrorsFunction()) return true;
                if (DioIn(Input) == InputState) break;

                Thread.Sleep(1);
                t += 1;
                if (t > Timeout)
                {
                    MachineState = ErrorState;
                    if (GetStatusStringFunction != null) Log(GetStatusStringFunction(ErrorState));
                    return true;
                }
            }
            return false;
        }

        private bool MoveDCyl(int Output, bool OutputState, int Input, bool InputState, int Timeout, int ErrorState)
        {
            int t = 0;
            DioOut(Output, OutputState);
            DioOut(Output + 1, !OutputState);
            while (true)
            {
                if (CheckForErrorsFunction != null) if (CheckForErrorsFunction()) return true;
                if (DioIn(Input) == InputState) break;

                Thread.Sleep(1);
                t += 1;
                if (t > Timeout)
                {
                    MachineState = ErrorState;
                    if (GetStatusStringFunction != null) Log(GetStatusStringFunction(ErrorState));
                    return true;
                }
            }
            return false;
        }

        public bool CheckCylinder(int first_input, int state_error)
        {
            if ((MainWindow.DioIn(first_input) && MainWindow.DioIn(first_input + 1)) || (!MainWindow.DioIn(first_input) && !MainWindow.DioIn(first_input + 1)))
            {
                MachineState = state_error;
                return true;
            }
            else return false;
        }

        public bool CheckCylinder(int first_input, int second_input, int state_error)
        {
            if ((MainWindow.DioIn(first_input) && MainWindow.DioIn(second_input)) || (!MainWindow.DioIn(first_input) && !MainWindow.DioIn(second_input)))
            {
                MachineState = state_error;
                return true;
            }
            else return false;
        }

        public bool CheckAndInitDCylinder(int first_input, int first_output, int state_error)
        {
            if ((MainWindow.DioIn(first_input) && MainWindow.DioIn(first_input + 1)) || (!MainWindow.DioIn(first_input) && !MainWindow.DioIn(first_input + 1)))
            {
                MachineState = state_error;
                return true;
            }
            else if (MainWindow.DioIn(first_input))
            {
                MainWindow.DioOut(first_output, true);
                MainWindow.DioOut(first_output + 1, false);
                return false;
            }
            else if (MainWindow.DioIn(first_input + 1))
            {
                MainWindow.DioOut(first_output, false);
                MainWindow.DioOut(first_output + 1, true);
                return false;
            }
            else
            {
                MachineState = state_error;
                return true;
            }
        }

        public bool CheckAndInitDCylinder(int first_input, int second_input, int first_output, int second_output, int state_error)
        {
            if (MainWindow.DioIn(first_input))
            {
                MainWindow.DioOut(first_output, true);
                MainWindow.DioOut(second_output, false);
                return false;
            }
            else if (MainWindow.DioIn(second_input))
            {
                MainWindow.DioOut(first_output, false);
                MainWindow.DioOut(second_output, true);
                return false;
            }
            else
            {
                MachineState = state_error;
                return true;
            }
        }

        public bool CheckTimeout(ref int counter, int timeout, int ErrorState)
        {
            Thread.Sleep(5);
            counter += 5;
            if (CheckForErrorsFunction()) return true;
            if (counter >= timeout)
            {
                MachineState = ErrorState;
                Log(GetStatusStringFunction(ErrorState));
                return true;
            }
            else return false;
        }

        public void RestoreLastLots()
        {
            tbL1.Text = CurrentProduct.L1;
            tbL2.Text = CurrentProduct.L2;
            tbL3.Text = CurrentProduct.L3;
            tbL4.Text = CurrentProduct.L4;
            DateTime rec_date_time;
            bool sent = false;
            Lot = "";

            using (SqlConnection connection = new SqlConnection("user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5"))
            {
                try
                {
                    connection.Open();

                    // извличане на данните за серията
                    using (SqlCommand command = new SqlCommand("SELECT * FROM (SELECT TOP 20 * FROM Lots ORDER BY RecDateTime DESC) T ORDER BY RecDateTime", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                rec_date_time = (DateTime)reader["RecDateTime"];

                                ListLot.Add(new LotItem(reader["Lot"].ToString(),
                                                            reader["Product"].ToString(),
                                                            reader["LotOrder"].ToString(),
                                                            reader["Quantity"].ToString(),
                                                            reader["Operator"].ToString(),
                                                            rec_date_time.ToString("yyyy.MM.dd  HH:mm")));
                                sent = reader["Sent"].ToString() == "True";
                                if (!sent)
                                {
                                    try { LastLotIdent = Convert.ToInt32(reader["IdentLot"].ToString()); }
                                    catch { LastLotIdent = 0; }
                                    try { Quantity = Convert.ToInt16(reader["Quantity"].ToString()); }
                                    catch { Quantity = 0; }

                                    Lot = reader["Lot"].ToString();
                                    Product = reader["Product"].ToString();
                                    Order = reader["LotOrder"].ToString();
                                    Operator = reader["Operator"].ToString();
                                }
                                else
                                {
                                    Lot = "";
                                    Product = "";
                                    Order = "";
                                    Operator = "";
                                    LastLotIdent = 0;
                                    Quantity = 0;
                                }
                                TimeStartLotOffset = (long)DateTime.Now.Subtract(rec_date_time).TotalMilliseconds;
                                LastLotBeginTime = rec_date_time;
                            }
                        }
                        ErrorString = "";
                    }

                    if (!sent && Lot != "")
                    {// последната серия не е изпратена
                        Pieces = Quantity;
                        PiecesOK = 0;
                        PiecesNOK = 0;
                        using (SqlCommand command = new SqlCommand("SELECT Result FROM Sensors WHERE Lot=@lot", connection))
                        {
                            command.Parameters.Add("@lot", SqlDbType.NVarChar, 12).Value = Lot;
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (reader["Result"].ToString() == "0") PiecesOK++;
                                    else PiecesNOK++;
                                    Pieces--;
                                }
                            }
                        }
                        bLotFinished = Pieces <= 0;
                    }
                }
                catch (Exception e)
                {
                    ErrorString = e.Message;
                    MessageBox.Show("Грешка при извличане на данни за серията от локалната база данни!\n\n" + e.Message);
                }
            }

            if (DataGridLots.Items.Count > 0) DataGridLots.ScrollIntoView(DataGridLots.Items[DataGridLots.Items.Count - 1]);
        }

        public void ImportSettings(string filename)
        {
            if (!File.Exists(filename))
            {
                MessageBox.Show("Файлът с настройки не съществува. Ще се създаде нов със стойности по подразбиране.", "", MessageBoxButton.OK, MessageBoxImage.Information);
                ExportSettings("");
                return;
            }

            SettingsPropertyValueCollection settings = Properties.Settings.Default.PropertyValues;
            SettingsPropertyCollection col = Properties.Settings.Default.Properties;

            string[] lines = File.ReadAllLines(filename);
            foreach (string line in lines)
            {
                String[] split = line.Replace(" ", "").Split('=');

                foreach (System.Configuration.SettingsProperty o in col)
                {
                    if (split[0] == o.Name)
                    {
                        try
                        {
                            if (settings[o.Name].Property.PropertyType.ToString() == "System.Single")
                                settings[o.Name].PropertyValue = (float)Convert.ToDouble(split[1]);
                            else if (settings[o.Name].Property.PropertyType.ToString() == "System.Double")
                                settings[o.Name].PropertyValue = Convert.ToDouble(split[1]);
                            else if (settings[o.Name].Property.PropertyType.ToString() == "System.Boolean")
                                settings[o.Name].PropertyValue = split[1] == "True" ? true : false;
                            else if (settings[o.Name].Property.PropertyType.ToString() == "System.String")
                                settings[o.Name].PropertyValue = split[1];
                            else if (settings[o.Name].Property.PropertyType.ToString() == "System.Int32")
                                settings[o.Name].PropertyValue = Convert.ToInt32(split[1]);
                            else if (settings[o.Name].Property.PropertyType.ToString() == "System.UInt32")
                                settings[o.Name].PropertyValue = Convert.ToUInt32(split[1]);
                        }
                        catch
                        {
                            Log("Настройки по подразбиране за " + o.Name);
                            MessageBox.Show("Error converting value!" + "\nName=" + o.Name + "\nValue=" + split[1]);
                        }
                    }
                }
            }

            Properties.Settings.Default.Save();
        }

        public void ImportSettings()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Settings files (*.set)|*.set|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                if (MessageBox.Show("Внимание!!! Всички настройки ще бъдат презаписани, включително калибрациите на машината!\n\nСигурни ли сте?", "Внимание!",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    ImportSettings(dlg.FileName);
                }
            }
        }

        public void ExportSettings(string FileName)
        {
            try
            {
                SettingsPropertyValueCollection settings = Properties.Settings.Default.PropertyValues;
                if (settings.Count > 0)
                {
                    SettingsPropertyCollection col = Properties.Settings.Default.Properties;
                    string file_name = FileName;
                    if (file_name == "") file_name = System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location) + ".set";
                    StreamWriter export_file = File.CreateText(file_name);

                    foreach (System.Configuration.SettingsProperty o in col)
                    {
                        if (!o.Name.Contains("ConnectionString")) export_file.WriteLine(o.Name + " = " + settings[o.Name].PropertyValue.ToString());
                    }
                    export_file.Close();
                }
                else MessageBox.Show("Системна грешка при зареждане на настройките!");
            }
            catch (Exception ex)
            {

            }
        }

        public int ExportSettingsToDatabase()
        {
            // write settings into string
            string settings = "";
            SettingsPropertyValueCollection set = Properties.Settings.Default.PropertyValues;
            SettingsPropertyCollection col = Properties.Settings.Default.Properties;
            foreach (System.Configuration.SettingsProperty o in col)
            {
                if (!o.Name.Contains("ConnectionString")) settings += o.Name + " = " + set[o.Name].PropertyValue.ToString() + "\r\n";
            }

            // записване в базата данни
            int result = 0;
            using (SqlConnection connection = new SqlConnection(
                "user id=rch;password=rchrch!;server=" + Properties.Settings.Default.TraceabilityServerIP + ";database=MachinesTrace;connection timeout=5"))
            using (SqlCommand command = new SqlCommand("INSERT INTO MachineSettings (MachineID, Settings) Values (@machineid,@settings)", connection))
            {
                command.Parameters.Add("@machineid", SqlDbType.NVarChar, 5).Value = Properties.Settings.Default.MachineID;
                command.Parameters.Add("@settings", SqlDbType.NVarChar).Value = settings;
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    ErrorString = "";
                }
                catch (Exception e)
                {
                    ErrorString = e.Message;
                    MessageBox.Show("Грешка при записване на настройките базата данни!\n\n" + e.Message);
                    result = -1;
                }
            }
            return result;
        }

        public void Log(string log)
        {
            WriteLogIntoDatabase(log);
            if (Application.Current.Dispatcher.CheckAccess())
            {
                ListLog.Add(new LogItem(DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss"), RemoveCardNumberFromString(log)));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    ListLog.Add(new LogItem(DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss"), RemoveCardNumberFromString(log)));
                }));
            }
            if (DataGridLogs.Dispatcher.CheckAccess())
            {
                if (DataGridLogs.Items.Count > 0) DataGridLogs.ScrollIntoView(DataGridLogs.Items[DataGridLogs.Items.Count - 1]);
            }
            else
            {
                DataGridLogs.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                { if (DataGridLogs.Items.Count > 0) DataGridLogs.ScrollIntoView(DataGridLogs.Items[DataGridLogs.Items.Count - 1]); }));
            }
        }

        public void WriteLotIntoDatagrid(string lot, string product, string order, string quantity, string operator_number, string rec_date_time)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                ListLot.Add(new LotItem(lot, product, order, quantity, operator_number, rec_date_time));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    ListLot.Add(new LotItem(lot, product, order, quantity, operator_number, rec_date_time));
                }));
            }
            if (DataGridLots.Dispatcher.CheckAccess())
            {
                if (DataGridLots.Items.Count > 0) DataGridLots.ScrollIntoView(DataGridLots.Items[DataGridLots.Items.Count - 1]);
            }
            else
            {
                DataGridLots.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                { if (DataGridLots.Items.Count > 0) DataGridLots.ScrollIntoView(DataGridLots.Items[DataGridLots.Items.Count - 1]); }));
            }
        }

        public static int WriteLogIntoDatabase(string log)
        {
            int result = 0;
            using (SqlConnection con = new SqlConnection("user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5"))
            using (SqlCommand cmd = new SqlCommand("INSERT INTO Logs (Log) Values (@log)", con))
            {
                cmd.Parameters.Add("@log", SqlDbType.NVarChar, 1000).Value = log;
                try
                {
                    result = -1;
                    con.Open();
                    result = -2;
                    cmd.ExecuteNonQuery();
                    result = 0;
                }
                catch (Exception ex)
                {
                    ErrorString = ex.ToString();
                }
            }
            return result;
        }

        public int RestoreLastLogs()
        {
            SqlConnection connectionLocal;

            String ConnectionString = "";
            ConnectionString += "user id=machine;";
            ConnectionString += "password=qipe;";
            ConnectionString += "server=127.0.0.1;";
            ConnectionString += "database=" + LocalDatabaseName + ";";
            ConnectionString += "connection timeout=5";
            connectionLocal = new SqlConnection(ConnectionString);

            try
            {
                connectionLocal.Open();
            }
            catch (Exception e)
            {
                ErrorString = e.Message;
                return 1;
            }
            SqlDataReader reader = null;
            SqlCommand commandRead = new SqlCommand("select RecDateTime,Log from Logs where RecDateTime>dateadd(DAY,-7,getdate()) order by RecDateTime", connectionLocal);
            try
            {
                reader = commandRead.ExecuteReader();
            }
            catch (Exception e)
            {
                ErrorString = e.Message;
                return 2;
            }

            DateTime rec_date_time;
            while (reader.Read())
            {
                rec_date_time = (DateTime)reader["RecDateTime"];
                ListLog.Add(new LogItem(rec_date_time.ToString("yyyy.MM.dd  HH:mm:ss"), RemoveCardNumberFromString(reader["Log"].ToString())));
            }
            reader.Close();
            connectionLocal.Close();

            return 0;
        }

        void Default_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ExportSettings(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location) + ".set");
        }

        public static string RemoveCardNumberFromString(string source)
        {
            string str = source;
            try
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == '(')
                    {
                        if ((i + 12) > str.Length) break;
                        if (str[i + 11] != ')') continue;
                        bool f = true;
                        for (int j = i + 1; j < i + 10; j++)
                        {
                            if (!((str[j] >= '0' && str[j] <= '9') || (str[j] >= 'A' && str[j] <= 'F') || (str[j] >= 'a' && str[j] <= 'f'))) f = false;
                        }
                        if (f)
                        {
                            str = str.Remove(i, 12);
                            str = str.Replace("   ", " ");
                            break;
                        }
                    }
                }
            }
            catch { }

            return str;
        }

        public static void BeforeChangeSettings()
        {
            SettingsPropertyValueCollection settings = Properties.Settings.Default.PropertyValues;
            SettingsPropertyCollection col = Properties.Settings.Default.Properties;
            int i = 0;
            string s = "";
            foreach (System.Configuration.SettingsProperty o in col)
            {
                oldSettings[i++] = settings[o.Name].PropertyValue.ToString();
                s += settings[o.Name].PropertyValue.ToString() + "\r";
            }
        }

        public void AfterChangeSettings(string card_id, string trigram)
        {
            SettingsPropertyValueCollection settings = Properties.Settings.Default.PropertyValues;
            SettingsPropertyCollection col = Properties.Settings.Default.Properties;
            int i = 0;
            bool change = false;
            foreach (System.Configuration.SettingsProperty o in col)
            {
                if (oldSettings[i] != settings[o.Name].PropertyValue.ToString())
                {
                    if (trigram == "---" || trigram == "") Log("Анонимен промени " + o.Name + " = " + settings[o.Name].PropertyValue.ToString());
                    else Log(trigram + " (" + card_id + ") " + " промени " + o.Name + " = " + settings[o.Name].PropertyValue.ToString());
                    change = true;
                }
                i++;
            }
            if (change) ExportSettingsToDatabase();
        }

        public int StartLotLocal(string lot, string product, string order, string quantity, string operator_)
        {
            int result = 0;
            using (SqlConnection connection = new SqlConnection("user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5"))
            using (SqlCommand command = new SqlCommand(
                "IF((select count(*) FROM Lots WHERE Lot=@lot)=0) " +
                "BEGIN INSERT INTO Lots (Lot,Product,LotOrder,Quantity,Operator,IdentLot) VALUES (@lot,@product,@lotorder,@quantity,@operator,@identlot) END " +
                "ELSE BEGIN UPDATE Lots SET Product=@product,LotOrder=@lotorder,Quantity=@quantity,Operator=@operator,IdentLot=@identlot WHERE Lot=@lot END",
                connection))
            {
                try
                {
                    command.Parameters.Add("@lot", SqlDbType.NVarChar, 12).Value = lot;
                    command.Parameters.Add("@product", SqlDbType.NVarChar, 16).Value = product;
                    command.Parameters.Add("@lotorder", SqlDbType.NVarChar, 12).Value = order == "" ? "0" : order;
                    command.Parameters.Add("@quantity", SqlDbType.Int).Value = Convert.ToInt32(quantity);
                    command.Parameters.Add("@operator", SqlDbType.NVarChar, 9).Value = operator_;
                    command.Parameters.Add("@identlot", SqlDbType.Int).Value = LastLotIdent;
                    connection.Open();
                    command.ExecuteNonQuery();
                    ErrorString = "";
                }
                catch (Exception e)
                {
                    ErrorString = e.Message;
                    MessageBox.Show("Неуспешно записване на серията в локалната база данни!\n\n" + ErrorString);
                    result = -1;
                }
            }
            return result;
        }

        private int CreateDatabaseTables()
        {
            string ConnectionString;
            ConnectionString = "user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5";
            SqlConnection connection = new SqlConnection(ConnectionString);

            try
            {
                connection.Open();
                ErrorString = "";
            }
            catch (Exception e)
            {
                ErrorString = e.Message;
                return -1;
            }

            // --- Logs ---
            SqlCommand command = new SqlCommand("if object_id('Logs') is null create table Logs(RecDateTime datetime DEFAULT getdate(),Log nvarchar(1000))", connection);
            try
            {
                command.ExecuteNonQuery();
                ErrorString = "";
            }
            catch (Exception e)
            {
                ErrorString = e.Message;
                return -2;
            }

            // --- Counters ---
            if (Counters != null)
            {
                string str = "if object_id('Counters') is null ";
                str += "create table Counters(";
                for (int i = 0; i < Counters.Length; i++) str += " C" + i.ToString() + " float,";
                str = str.TrimEnd(',');     // remove last comma
                str += ")";
                command = new SqlCommand(str, connection);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    ErrorString = e.Message;
                    return -3;
                }
            }

            connection.Close();

            return 0;
        }

        public static int UpdateCountersLocal()
        {
            int result = 0;
            string str;
            double[] counters = new double[Counters.Length];

            string ConnectionString;
            ConnectionString = "user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5";
            SqlConnection connection = new SqlConnection(ConnectionString);

            try
            {
                connection.Open();
            }
            catch (Exception e)
            {
                ErrorString = e.Message;
                return -1;
            }

            // read cuurent values
            SqlDataReader reader = null;
            SqlCommand commandRead = new SqlCommand("select * from Counters", connection);
            try { reader = commandRead.ExecuteReader(); }
            catch
            {
                try { reader = commandRead.ExecuteReader(); }
                catch (Exception e)
                {
                    ErrorString = e.Message;
                    return -2;
                }
            }

            SqlCommand command;
            if (reader.Read())
            {
                for (int i = 0; i < Counters.Length; i++)
                {
                    try
                    {
                        counters[i] = Convert.ToDouble(reader[i].ToString());
                    }
                    catch { }
                }
                reader.Close();
            }
            else
            {
                reader.Close();
                str = "INSERT INTO Counters (";
                for (int i = 0; i < Counters.Length; i++) str += "C" + i.ToString() + ",";
                str = str.TrimEnd(',');     // remove last comma
                str += ") Values (";
                for (int i = 0; i < Counters.Length; i++) str += "0,";
                str = str.TrimEnd(',');     // remove last comma
                str += ")";
                command = new SqlCommand(str, connection);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    ErrorString = e.Message + "\n\n" + str;
                    return 4;
                }
            }

            // update values
            for (int i = 0; i < counters.Length; i++)
            {
                counters[i] += CountersTemporal[i];
                CountersTemporal[i] = 0;
            }

            // write updated
            str = "UPDATE Counters SET ";
            for (int i = 0; i < Counters.Length; i++) str += "C" + i.ToString() + "=" + counters[i] + ",";
            str = str.TrimEnd(',');
            command = new SqlCommand(str, connection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                ErrorString = e.Message + "\n\n" + str;
                return 5;
            }

            connection.Close();

            for (int i = 0; i < Counters.Length; i++) Counters[i] = counters[i];

            return result;
        }

        public static int ResetCounter(int index)
        {
            String ConnectionString = "";
            ConnectionString += "user id=rch;";
            ConnectionString += "password=rchrch!;";
            ConnectionString += "server=127.0.0.1;";
            ConnectionString += "database=" + LocalDatabaseName + ";";
            ConnectionString += "connection timeout=5";
            SqlConnection connectionLocal = new SqlConnection(ConnectionString);

            try
            {
                connectionLocal.Open();
            }
            catch (Exception e)
            {
                ErrorString = e.Message;
                return 1;
            }

            string str = "UPDATE Counters SET C" + index.ToString() + "=0";
            SqlCommand command = new SqlCommand(str, connectionLocal);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                ErrorString = e.Message + "\n\n" + str;
                return 4;
            }
            connectionLocal.Close();
            connectionLocal.Dispose();
            command.Dispose();
            return 0;
        }

        #region TRACEABILITY Functions

        public int ValidateComponent(string Product, string Lot, int Quantity)
        {
            SqlConnection connectionRemote;

            String ConnectionString = "";
            ConnectionString += "user id=machine;";
            ConnectionString += "password=qipe;";
            ConnectionString += "server=" + Properties.Settings.Default.TraceabilityServerIP + ";";
            ConnectionString += "database=" + Properties.Settings.Default.TraceabilityDBName + ";";
            ConnectionString += "connection timeout=5";
            connectionRemote = new SqlConnection(ConnectionString);

            SqlParameter paramIdent = new SqlParameter("@Ident", SqlDbType.Int);
            SqlParameter paramIdent2 = new SqlParameter("@Ident2", SqlDbType.Int);
            SqlParameter paramItem = new SqlParameter("@Item", SqlDbType.NVarChar);
            paramItem.Size = 16;
            SqlParameter paramSeria = new SqlParameter("@Seria", SqlDbType.NVarChar);
            paramItem.Size = 16;
            SqlParameter paramQuan = new SqlParameter("@Quan", SqlDbType.Int);
            SqlParameter paramResult = new SqlParameter("@Result", SqlDbType.Int);
            paramResult.Direction = ParameterDirection.Output;

            string cmdsql = String.Format(@"
            EXEC  [dbo].[PR_ComponentsValidationNew]
            @Ident = @Ident,
            @Ident2 = @Ident2,
            @Item = @Item,
            @Seria = @Seria,
            @Quan = @Quan,
            @Result = @Result OUTPUT
            ");

            SqlCommand cmdSelectLine = new SqlCommand(cmdsql, connectionRemote);
            cmdSelectLine.Parameters.Add(paramIdent);
            cmdSelectLine.Parameters.Add(paramIdent2);
            cmdSelectLine.Parameters.Add(paramItem);
            cmdSelectLine.Parameters.Add(paramSeria);
            cmdSelectLine.Parameters.Add(paramQuan);
            cmdSelectLine.Parameters.Add(paramResult);

            try
            {
                paramIdent.Value = LastLotIdent;
                paramIdent2.Value = 0;
                paramItem.Value = Product;
                paramSeria.Value = Lot;
                paramQuan.Value = Quantity;
                connectionRemote.Open();
                cmdSelectLine.ExecuteNonQuery();
                LastLotIdent = (int)paramIdent.Value;
                int result = (int)paramResult.Value;
                if (result > 0)
                {
                    switch (result)
                    {
                        case 1: ResultString = "Сгрешен компонент"; break;
                        case 2: ResultString = "Правилен компонент, но се очакват още компоненти"; break;
                        case 3: ResultString = "Превишено количество на компонента"; break;
                        case 4: ResultString = "Компонента не е преминал предишна операция"; break;
                        default:
                            ResultString = "Даните не са валидирани";
                            break;
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                ResultString = "Грешно попълнени полета: " + ex.Message;
                return 10;
            }
            finally
            {
                if (connectionRemote.State == ConnectionState.Open) connectionRemote.Close();
            }

            return 0;
        }

        public int EndLotOK(int numberOK)
        {
            SqlConnection connectionRemote;

            String ConnectionString = "";
            ConnectionString += "user id=machine;";
            ConnectionString += "password=qipe;";
            ConnectionString += "server=" + Properties.Settings.Default.TraceabilityServerIP + ";";
            ConnectionString += "database=" + Properties.Settings.Default.TraceabilityDBName + ";";
            ConnectionString += "connection timeout=5";
            connectionRemote = new SqlConnection(ConnectionString);

            SqlParameter paramIdent = new SqlParameter("@Ident", SqlDbType.Int);
            SqlParameter paramQtyGood = new SqlParameter("@QtyGood", SqlDbType.Int);
            SqlParameter paramResult = new SqlParameter("@Result", SqlDbType.Int);
            paramResult.Direction = ParameterDirection.Output;
            SqlParameter paramResultText = new SqlParameter("@ResultText", SqlDbType.NVarChar, 1000);
            paramResultText.Direction = ParameterDirection.Output;

            string cmdsql = String.Format(@"
            EXEC  [dbo].[InsertRepRecGood]
						@Ident = @Ident,
						@QtyGood = @QtyGood,
                        @Result = @Result OUTPUT,
						@ResultText = @ResultText OUTPUT
            ");

            SqlCommand cmdSelectLine = new SqlCommand(cmdsql, connectionRemote);
            cmdSelectLine.Parameters.Add(paramIdent);
            cmdSelectLine.Parameters.Add(paramQtyGood);
            cmdSelectLine.Parameters.Add(paramResult);
            cmdSelectLine.Parameters.Add(paramResultText);

            try
            {
                paramIdent.Value = LastLotIdent;
                paramQtyGood.Value = numberOK;
                connectionRemote.Open();
                cmdSelectLine.ExecuteNonQuery();
                if (paramResultText.Value != DBNull.Value) ResultString = paramResultText.Value.ToString();
                int result = (int)paramResult.Value;
                if (result > 0) return result;
            }
            catch (Exception ex)
            {
                ResultString = ex.Message;
                return -1;
            }
            finally
            {
                if (connectionRemote.State == ConnectionState.Open) connectionRemote.Close();
            }

            return 0;
        }

        public int EndLotNOK(int typeNOK, int numberNOK)
        {
            SqlConnection connectionRemote;

            String ConnectionString = "";
            ConnectionString += "user id=machine;";
            ConnectionString += "password=qipe;";
            ConnectionString += "server=" + Properties.Settings.Default.TraceabilityServerIP + ";";
            ConnectionString += "database=" + Properties.Settings.Default.TraceabilityDBName + ";";
            ConnectionString += "connection timeout=5";
            connectionRemote = new SqlConnection(ConnectionString);

            SqlParameter paramIdent = new SqlParameter("@Ident", SqlDbType.Int);
            SqlParameter paramProbId = new SqlParameter("@ProbId", SqlDbType.Int);
            SqlParameter paramQtyBad = new SqlParameter("@QtyBad", SqlDbType.Int);
            SqlParameter paramResult = new SqlParameter("@Result", SqlDbType.Int);
            SqlParameter paramResultText = new SqlParameter("@ResultText", SqlDbType.NVarChar, 1000);
            paramResult.Direction = ParameterDirection.Output;
            paramResultText.Direction = ParameterDirection.Output;

            string cmdsql = String.Format(@"
            EXEC  [dbo].[InsertRepRecBad]
						@Ident = @Ident,
						@ProbId = @ProbId,
						@QtyBad = @QtyBad,
                        @Result = @Result OUTPUT,
                        @ResultText = @ResultText OUTPUT
            ");

            SqlCommand cmdSelectLine = new SqlCommand(cmdsql, connectionRemote);
            cmdSelectLine.Parameters.Add(paramIdent);
            cmdSelectLine.Parameters.Add(paramProbId);
            cmdSelectLine.Parameters.Add(paramQtyBad);
            cmdSelectLine.Parameters.Add(paramResult);
            cmdSelectLine.Parameters.Add(paramResultText);

            try
            {
                paramIdent.Value = LastLotIdent;
                paramProbId.Value = typeNOK;
                paramQtyBad.Value = numberNOK;
                connectionRemote.Open();
                cmdSelectLine.ExecuteNonQuery();
                int result = (int)paramResult.Value;
                if (result > 0)
                {
                    ResultString = paramResultText.Value.ToString();
                    return result;
                }
            }
            catch (Exception ex)
            {
                ResultString = ex.Message;
                return 10;
            }
            finally
            {
                if (connectionRemote.State == ConnectionState.Open) connectionRemote.Close();
            }

            return 0;
        }

        #endregion

        private int CheckLot(string lot, ref bool exists, ref DateTime created_time, ref int pieces, ref int pieces_nok)
        {
            int result = 0;
            exists = false;
            using (SqlConnection connection = new SqlConnection("user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5"))
            {
                using (SqlCommand command = new SqlCommand("select [Quantity],[RecDateTime] from Lots where Lot=@lot", connection))
                {
                    command.Parameters.Add("@lot", SqlDbType.NVarChar, 12).Value = lot;
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                exists = true;
                                DateTime.TryParse(reader["RecDateTime"].ToString(), out created_time);
                                int quantity = 0;
                                Int32.TryParse(reader["Quantity"].ToString(), out quantity);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ErrorString = e.Message;
                        MessageBox.Show("Неуспешна проверка на бройките от серията в локалната база данни! (Code = -1)\n\n" + e.Message);
                        result = -1;
                    }
                }

                if (exists)
                {
                    using (SqlCommand command = new SqlCommand("select Result from Sensors where Lot=@lot", connection))
                    {
                        command.Parameters.Add("@lot", SqlDbType.NVarChar, 12).Value = lot;
                        try
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                int n = 0, nok = 0;
                                while (reader.Read())
                                {
                                    if (reader["Result"].ToString() != "0") nok++;
                                    n++;
                                }
                                if (n > 0) pieces = n; else pieces = 0;
                                pieces_nok = nok;
                            }
                        }
                        catch (Exception e)
                        {
                            ErrorString = e.Message;
                            MessageBox.Show("Неуспешна проверка на бройките от серията в локалната база данни! (Code = -2)\n\n" + e.Message);
                            result = -2;
                        }
                    }
                }
            }
            return result;
        }

        public static int MarkLotAsSent(string lot)
        {
            int result = 0;

            using (SqlConnection connection = new SqlConnection("user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5"))
            using (SqlCommand command = new SqlCommand("UPDATE Lots SET Sent=1 where Lot=@lot", connection))
            {
                command.Parameters.Add("@lot", SqlDbType.NVarChar, 12).Value = lot;
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    ErrorString = "";
                }
                catch (Exception e)
                {
                    ErrorString = e.Message;
                    MessageBox.Show("Неуспешно маркиране на серията като изпратена!\n\n" + e.Message);
                    result = -1;
                }
            }
            return result;
        }

        public static int UpdateLastOKPieceAsScrap(string lot, int ScrapCode)
        {
            int result = 0;
            using (SqlConnection connection = new SqlConnection("user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5"))
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = @"if((select COUNT(*) from Sensors where Lot='123RR' and Result=0) > 0)
																begin
																update Sensors set result=@result where IDSensor=(
																select top 1 IDSensor from Sensors where Lot=@lot and Result=0)
																select 1
																end
																else
																begin
																select 0
																end";

                command.Parameters.Add("@lot", SqlDbType.NVarChar, 12).Value = lot;
                command.Parameters.Add("@result", SqlDbType.SmallInt).Value = ScrapCode;

                try
                {
                    result = -1;
                    connection.Open();
                    result = -2;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader[0].ToString() == "0") return 0;
                            else return 1;
                        }
                        else result = -3;
                    }
                }
                catch (Exception e)
                {
                    ErrorString = e.Message;
                    MessageBox.Show("Неуспешно добяване на бракувани бройки в локалната база данни!\n\n" + e.Message);
                }
            }
            return result;
        }

        public bool GetNumberLocal(string lot, ref List<int[]> results)
        {
            int result = 0;
            results.Clear();
            using (SqlConnection connection = new SqlConnection("user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5"))
            using (SqlCommand command = new SqlCommand("SELECT Result, count(*) FROM Sensors WHERE Lot = @lot GROUP BY Result ORDER BY Result", connection))
            {
                command.Parameters.Add("@lot", SqlDbType.NVarChar, 12).Value = lot;
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new int[2] { Convert.ToInt32(reader[0].ToString()), Convert.ToInt32(reader[1].ToString()) });
                        }
                        if (results[0][0] == -1) results.RemoveAt(0);
                        if (results.Count > 0)
                        {
                            if (results[0][0] != 0) results.Insert(0, new int[2] { 0, 0 });     // ако в серията няма годни бройки, се добавя в списъка 0
                        }
                        else results.Insert(0, new int[2] { 0, 0 });        // ако в серията няма бройки, се добавя в списъка 0
                    }
                }
                catch (Exception e)
                {
                    ErrorString = e.Message;
                    MessageBox.Show("Неуспешно извличане на бройките от серията!\n\n" + e.Message);
                    return false;
                }
            }
            return true;
        }

        public int CompFinishedTraceabilityReport(int usedCompQty, long lotIdent, string compProd, string compLot)
        {
            int result = -1;
            string ResultText = "";
            var Trace = new DAL.Traceability.TraceabilityDataAccess(Properties.Settings.Default.TraceabilityServerIP,
                     Properties.Settings.Default.TraceabilityDBName, Properties.Settings.Default.MachineName);

            DataTracking.Models.ComponentInfo componentlot = new DataTracking.Models.ComponentInfo()
            {
                Ident = lotIdent,
                Item = compProd,
                Seria = compLot,
            };

            Trace.EndLotComponent(componentlot, usedCompQty, out result, out ResultText);
            //if (result != 0)
            //{
            //    Log(ResultText);
            //    MessageBox.Show(ResultText, "Грешка", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return result;
            //}
            result = 0;
            return result;
        }

        public void EndLot(bool ShowErrorMessage, bool SendOKPiecesIfTheyAreZero)
        {
            if (!bLotFinished)
            {
                var response = MessageBox.Show("Текущата серия не е завършена.Искате ли да я прекратите?", "Серията не е завършена", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (response == MessageBoxResult.Yes)
                {
                    if (CurrentLot.Lot != "") Log(String.Format("Оператор {0} прекрати серия {1} предварително. Годни: {2}, Брак: {3}", CurrentLot.Operator, CurrentLot.Lot, CurrentLot.PiecesOK, CurrentLot.PiecesNOK));
                }
                else
                {
                    ReaderBarcode.ReceivedLot = false;
                    return;
                }
            }

            List<int[]> ResultList = new List<int[]>();
            if (!GetNumberLocal(Lot, ref ResultList)) return;
            if (Properties.Settings.Default.ShowEndLotWindow)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    WindowEndLot dlg = new WindowEndLot(CurrentLot.Quantity, ResultList);
                    dlg.ShowDialog();
                });
            }

            if (ResultList.Count > 0)   // are there any pieces processed
            {
                // send NOK number
                for (int i = 1; i < ResultList.Count; i++)
                {
                    if (ResultList[i][0] > 0)
                    {
                        int result1 = EndLotNOK(ResultList[i][0], ResultList[i][1]);
                        if (result1 != 0 && Properties.Settings.Default.ShowCloseLotErrors)
                        {
                            MessageBox.Show("Грешка в системата за проследимост при завършване на серията - регистриране на негодни (код брак:" +
                                ResultList[i][0].ToString() + ", брой: " + ResultList[i][1].ToString() +
                                "\nLot ID: " + LastLotIdent.ToString() +
                                "\n\nCode:" + result1.ToString() + "\n" + ResultString, "",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                // send OK number
                if ((ResultList[0][0] == 0 && ResultList[0][1] > 0) || SendOKPiecesIfTheyAreZero) // is there any OK pieces
                {
                    int result2 = EndLotOK(ResultList[0][1]);
                    if (result2 != 0 && Properties.Settings.Default.ShowCloseLotErrors)
                    {
                        MessageBox.Show("Грешка в системата за проследимост при завършване на серията - регистриране на брой годни (" + ResultList[0][1].ToString() +
                                    ")" + "\nLot ID: " + LastLotIdent.ToString() +
                                    "\n\n" + ResultString + "\n\nCode:" + result2.ToString(), "",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            MainWindow.bLotFinished = true;
            return;
        }

        public static void ImportSettingsFromString(string str_settings)
        {
            SettingsPropertyValueCollection settings = Properties.Settings.Default.PropertyValues;
            SettingsPropertyCollection col = Properties.Settings.Default.Properties;
            string[] set = str_settings.Split(new string[] { "\r", "\n", "\r\n", "\n\r" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < set.Length; i++)
            {
                string[] split = set[i].Replace(" ", "").Split('=');
                foreach (System.Configuration.SettingsProperty o in col)
                {
                    if (split[0] == o.Name)
                    {
                        try
                        {
                            if (settings[o.Name].Property.PropertyType.ToString() == "System.Single")
                                settings[o.Name].PropertyValue = (float)Convert.ToDouble(split[1]);
                            else if (settings[o.Name].Property.PropertyType.ToString() == "System.Double")
                                settings[o.Name].PropertyValue = Convert.ToDouble(split[1]);
                            else if (settings[o.Name].Property.PropertyType.ToString() == "System.Boolean")
                                settings[o.Name].PropertyValue = split[1] == "True" ? true : false;
                            else if (settings[o.Name].Property.PropertyType.ToString() == "System.String")
                                settings[o.Name].PropertyValue = split[1];
                            else if (settings[o.Name].Property.PropertyType.ToString() == "System.Int32")
                                settings[o.Name].PropertyValue = Convert.ToInt32(split[1]);
                            else if (settings[o.Name].Property.PropertyType.ToString() == "System.UInt32")
                                settings[o.Name].PropertyValue = Convert.ToUInt32(split[1]);
                        }
                        catch
                        {
                            MessageBox.Show("Error converting value!");
                        }
                    }
                }
            }
            Properties.Settings.Default.Save();
        }
    }
}
