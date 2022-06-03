using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;
using System.Data;
using System.Data.SqlClient;
using Machine.Common;

namespace Machine
{
    public class RemoteStatusServer
    {
        private Thread listenThread;
        Label StatusLabel;
        public int CpuFrequency = 0;
        public int CpuCount = 1;
        public string CpuFrequencyText = "";
        public string DateModified = "";
        MachineWorkloadPieDiagram WorkLoadControl;
        private object LockObjectForceUpdate = new object();
        private bool _bForceUpdate = false;
        private string LocalDatabaseName = "";
        public string ErrorString = "";

        private bool bForceUpdate
        {
            get
            {
                bool b;
                lock (LockObjectForceUpdate) b = _bForceUpdate;
                return b;
            }
            set
            {
                lock (LockObjectForceUpdate) _bForceUpdate = value;
            }
        }

        public RemoteStatusServer(Label statusLabel, MachineWorkloadPieDiagram WorkLoadControlReference, string localDatabaseName)
        {
            CpuFrequency = GetCpuFrequency();
            CpuCount = GetNumberOfCpu();
            if (CpuCount > 1) CpuFrequencyText = CpuCount.ToString() + " x ";
            CpuFrequencyText += CpuFrequency.ToString();
            DateModified = File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString("dd.MM.yy HH:mm");

            StatusLabel = statusLabel;
            WorkLoadControl = WorkLoadControlReference;
            LocalDatabaseName = localDatabaseName;
            listenThread = new Thread(new ThreadStart(ThreadMethod));
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        public void ForceUpdate()
        {
            bForceUpdate = true;
        }

        public void Close()
        {
        }

        private void ThreadMethod()
        {
            int period = 600;
            int counter = period - 5;   // 5 секунди след старт на приложението се обновява статуса
            while (true)
            {
                if (bForceUpdate)
                {
                    bForceUpdate = false;
                    counter = 0;
                    UpdateCurrentState();
                }

                Thread.Sleep(1000);
                counter++;
                if (counter >= period)  // 10 min.
                {
                    counter = 0;
                    UpdateCurrentState();
                }
            }
        }

        private int UpdateCurrentState()
        {
            int result = 0;
            try
            {
                string ShiftsDataString = GetShiftsData();

                using (SqlConnection connection = new SqlConnection())
                using (SqlCommand command = new SqlCommand())
                {
                    connection.ConnectionString = "user id=machine;password=qipe;server=" + Properties.Settings.Default.MonitoringServerIP + ";database=MachinesTrace;connection timeout=5";
                    command.Connection = connection;

                    string cmd = "update StatusServer_Machines set " +
                        "[Status]=@status," +
                        "[StatusColor]=@statuscolor," +
                        "[CurrentLot]=@currentlot," +
                        "[CurrentProduct]=@currentproduct," +
                        "[Info]=@info," +
                        "[Counters]=@counters," +
                        "[Statistics]=@statistics," +
                        "[LoadDistributionPieChartData]=@piechart," +
                        "[ShiftsChartData]=@shiftschartdata" +
                        " where [MachineID]=@machineid";
                    command.CommandText = cmd;

                    //+ "Cycle Time: " + (MainWindow.MachineTactTime == -1 ? MainWindow.LastCycleTime.ToString("F1") : MainWindow.MachineTactTime.ToString("F1")) + ";"

                    // status, status color
                    string status = "", status_color = "";

                    if (StatusLabel.Dispatcher.CheckAccess())
                    {
                        if (StatusLabel.Content != null)
                        {
                            status = StatusLabel.Content.ToString();
                            status_color = StatusLabel.Foreground.ToString() + "," + StatusLabel.Background.ToString();
                        }
                    }
                    else
                    {
                        StatusLabel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            if (StatusLabel.Content != null)
                            {
                                status = StatusLabel.Content.ToString();
                                status_color = StatusLabel.Foreground.ToString() + "," + StatusLabel.Background.ToString();
                            }
                        }));
                    }


                    // uptime 
                    long l = MainWindow.timerUpTime.ElapsedMilliseconds / 1000;
                    long days = l / 86400;
                    l = l % 86400;
                    int hours = (int)(l / 3600);
                    l = l % 3600;
                    int minutes = (int)(l / 60);
                    int seconds = (int)(l % 60);
                    string uptime = String.Format((days == 0 ? "" : " {0} d") + " {1:00}:{2:00}:{3:00}", days, hours, minutes, seconds);

                    // pie chart
                    string piechart = "";
                    WorkLoadControl.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        for (int i = 0; i < WorkLoadControl.PiesCount; i++) piechart += WorkLoadControl.GetPieValue(i).ToString() + "," + WorkLoadControl.GetPieColor(i) + ";";
                    }));
                    piechart = piechart.TrimEnd(';');

                    // counters
                    string counters = "";
                    //for (int i = 0; i < CommonData.Counters.Length; i++) counters += (CommonData.Counters[i] + CommonData.CountersTemporal[i]).ToString() + ",";
                    counters = counters.TrimEnd(',');

                    // statistics
                    string statistics = "";

                    command.Parameters.Add("@status", SqlDbType.NVarChar).Value = status;
                    command.Parameters.Add("@statuscolor", SqlDbType.NVarChar).Value = status_color;
                    command.Parameters.Add("@currentlot", SqlDbType.NVarChar, 20).Value = MainWindow.Lot;
                    command.Parameters.Add("@currentproduct", SqlDbType.NVarChar, 20).Value = MainWindow.Product;
                    command.Parameters.Add("@machineid", SqlDbType.NVarChar, 5).Value = Properties.Settings.Default.MachineID;
                    command.Parameters.Add("@info", SqlDbType.NVarChar, 1000).Value = "Uptime= " + uptime + ";" + "SW ver.= " + DateModified + ";" + "CPU=" + CpuFrequencyText;
                    command.Parameters.Add("@counters", SqlDbType.NVarChar, 200).Value = counters;
                    command.Parameters.Add("@statistics", SqlDbType.NVarChar, 200).Value = statistics;
                    command.Parameters.Add("@piechart", SqlDbType.NVarChar, 200).Value = piechart;
                    command.Parameters.Add("@shiftschartdata", SqlDbType.NVarChar, 1000).Value = ShiftsDataString;

                    try
                    {
                        result = -1;
                        connection.Open();
                        result = -2;
                        command.ExecuteNonQuery();
                        result = 0;
                        //ErrorString = "";
                    }
                    catch (Exception e)
                    {
                        ErrorString = e.Message;
                        //MessageBox.Show("Грешка при обновяване на статуса в сървъра за мониторинг!\n\n" + e.Message + "\n\n" + cmd);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        private int GetCpuFrequency()
        {
            int ret = 0;
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey("Hardware\\DESCRIPTION\\System\\CentralProcessor\\0", false);
                ret = (int)key.GetValue("~MHz");
                key.Close();
            }
            catch { }
            return ret;
        }

        private int GetNumberOfCpu()
        {
            int ret = 1;
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey("Hardware\\DESCRIPTION\\System\\CentralProcessor", false);
                string[] s = (string[])key.GetSubKeyNames();
                ret = s.Length;
                key.Close();
            }
            catch { }
            return ret;
        }

        private string GetShiftsData()
        {
            string result = "";
            using (SqlConnection connection = new SqlConnection())
            using (SqlCommand command = new SqlCommand())
            {
                connection.ConnectionString = "user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5";
                command.Connection = connection;
                command.CommandText = @"select Dates,Shifts,Count(*) as Counts from 
					(SELECT 
					(case 
					when datepart(HH,[RecDateTime]) between 0 and 5 then CAST(FLOOR(CAST(DATEADD(Day,-1,[RecDateTime]) AS FLOAT)) AS DATETIME) 
					else CAST(FLOOR(CAST([RecDateTime] AS FLOAT)) AS DATETIME) 
					end 
					) as Dates 
					, 
					(case 
					when datepart(HH,[RecDateTime]) between 6 and 13 then 1 
					when datepart(HH,[RecDateTime]) between 14 and 21 then 2 
					when datepart(HH,[RecDateTime]) between 22 and 23 then 3 
					when datepart(HH,[RecDateTime]) between 0 and 5 then 3 
					end) as Shifts 
					FROM Sensors where [RecDateTime] >= dateadd(HH,-200,getdate())) as g 
					group by Dates,Shifts 
					order by Dates desc, Shifts desc";

                try
                {
                    connection.Open();
                    List<string[]> list = new List<string[]>();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new string[3]);
                            list[list.Count - 1][0] = ((DateTime)reader[0]).ToString("yyyy-MM-dd");
                            list[list.Count - 1][1] = reader[1].ToString();
                            list[list.Count - 1][2] = reader[2].ToString();
                            //result += ((DateTime)reader[0]).ToString("yyyy-MM-dd") + "," + reader[1].ToString() + "," + reader[2].ToString() + ";";
                        }
                        //result = result.TrimEnd(';');
                    }

                    // допълване на липсващи дати (без обработки)
                    DateTime dt = DateTime.Now;
                    if (dt.Hour > 21) dt = new DateTime(dt.Year, dt.Month, dt.Day, 22, 0, 0);
                    else if (dt.Hour > 13) dt = new DateTime(dt.Year, dt.Month, dt.Day, 14, 0, 0);
                    else if (dt.Hour > 5) dt = new DateTime(dt.Year, dt.Month, dt.Day, 6, 0, 0);
                    else
                    {
                        dt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
                        dt = dt.AddHours(-2);       // return to a previous date
                    }
                    int pos = 0, current_shift = 0;
                    if (dt.Hour > 5 && dt.Hour < 14) current_shift = 1;
                    else if (dt.Hour > 13 && dt.Hour < 22) current_shift = 2;
                    else current_shift = 3;

                    //MessageBox.Show(list[0].Length.ToString());

                    result = "";
                    for (int i = 0; i < 21; i++)
                    {
                        if (list.Count > pos && dt.ToString("yyyy-MM-dd") == list[pos][0] && current_shift.ToString() == list[pos][1])
                        {
                            int pieces = Convert.ToInt32(list[pos][2]);
                            int lots = pieces / 100;
                            if (pieces % 100 > 50) lots++;
                            result += list[pos][0] + "," + list[pos][1] + "," + lots.ToString() + ";";
                            pos++;
                        }
                        else result += dt.ToString("yyyy-MM-dd") + "," + current_shift.ToString() + ",0;";

                        current_shift--;
                        if (current_shift == 0) current_shift = 3;
                        dt = dt.AddHours(-8);
                    }

                    //MessageBox.Show(result.Replace(';', '\n').Replace(',', '\t'));
                }
                catch (Exception e)
                {
                    ErrorString = e.ToString();
                    result = "";
                    //MessageBox.Show(e.ToString());
                }
            }

            return result;
        }


    }
}
