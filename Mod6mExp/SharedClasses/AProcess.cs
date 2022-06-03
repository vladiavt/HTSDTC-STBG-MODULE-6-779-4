using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Media;

namespace Machine
{
    public partial class MainWindow
    {
        public class PortManager
        {
            private byte[] InputsArray;
            private byte[] InputsArrayLast;
            private int Size = 0;
            private System.Threading.Thread thWork;

            public PortManager(ref byte[] MachineInputsArray, int sz)
            {
                InputsArray = MachineInputsArray;
                Size = sz;
                InputsArrayLast = new byte[Size];
                thWork = new System.Threading.Thread(new System.Threading.ThreadStart(PortManMethod));
                thWork.IsBackground = true;
                thWork.Name = "PortMan";
                thWork.Start();
            }

            private void PortManMethod()
            {
                Thread.Sleep(100);   // wait other processes to initialize
                for (int i=0;i<Size;i++) InputsArrayLast[i] = InputsArray[i];
                while (true)
                {
                    Thread.Sleep(1);
                    if (TLog.enableLog)
                        for (int i = 0; i < Size; i++) 
                        {
                            if (InputsArray[i] != InputsArrayLast[i])
                            {
                                for (int m = 1, k=0; m < 256; m = m * 2, k++)
                                {
                                    if ((InputsArray[i] & m) != (InputsArrayLast[i] & m))
                                    {
                                        TLog.Add("PM,In" + (i * 8 + k).ToString() + "," + (((InputsArray[i] & m) == 0) ? "0" : "1"));
                                    }
                                }
                                InputsArrayLast[i] = InputsArray[i];
                            }
                        }

                }
            }

        }
        public class AProcess : PropertyChangedNotificator
        {
            public const int CMD_INIT = 1;
            public const int CMD_PROCESS = 2;

            public string Name = "";
            private TimeSpan processDuration_ = TimeSpan.Parse("0:0:0.000");
            private string StatusString_ = "";
            private string OldStatusString_ = "";
            private bool Error_ = false;
            private bool Ready_ = true;
            private bool AutomaticMode_ = false;
            private int Command_ = 0;
            private bool Initialized_ = false;
            private bool StepByStep_ = false;
            private bool bNextStep_ = false;
            private bool Paused_ = false;
            private bool Abort_ = false;
            private bool bStop_ = false;
            private Brush StatusBackground_ = Brushes.White;

            public object LockObjectStatus = new object();
            public object LockObjectCommand = new object();
            public object LockObjectProcess = new object();
            private object LockObjectStep = new object();

            public bool AutomaticMode
            {
                get { bool ret = false; lock (this.LockObjectCommand) { ret = AutomaticMode_; } return ret; }
                set { lock (this.LockObjectCommand) { AutomaticMode_ = value; } }
            }
            public int Command
            {
                get { int ret = 0; lock (this.LockObjectCommand) { ret = Command_; } return ret; }
                set { lock (this.LockObjectCommand) { Command_ = value; } }
            }
            public bool Initialized
            {
                get { bool ret; lock (this.LockObjectStatus) { ret = Initialized_; } return ret; }
                set { lock (this.LockObjectCommand) { Initialized_ = value; } }
            }
            public string OldStatusString
            {
                get { string ret = ""; lock (this.LockObjectStatus) { ret = OldStatusString_; } return ret; }
                set { lock (this.LockObjectStatus) { OldStatusString_ = value; } }
            }
            public string StatusString
            {
                get { string ret = ""; lock (this.LockObjectStatus) { ret = StatusString_; } return ret; }
                set
                {
                    lock (this.LockObjectStatus) { StatusString_ = value; }
                    OnPropertyChanged("StatusString");
                }
            }
            public TimeSpan ProcessDuration
            {
                get { TimeSpan ret; lock (this.LockObjectStatus) { ret = processDuration_; } return ret; }
                set { lock (this.LockObjectCommand) { processDuration_ = value; } }
            }
            public Brush StatusBackground
            {
                get { Brush ret; lock (this.LockObjectStatus) { ret = StatusBackground_; } return ret; }
                set
                {
                    lock (this.LockObjectStatus)
                    { StatusBackground_ = value; }
                    OnPropertyChanged("StatusBackground");
                }
            }
            public bool Ready
            {
                get { bool ret; lock (this.LockObjectStatus) { ret = Ready_; } return ret; }
                set { lock (this.LockObjectStatus) { Ready_ = value; } }
            }
            public bool Error
            {
                get { bool ret; lock (this.LockObjectStatus) { ret = Error_; } return ret; }
                set { lock (this.LockObjectStatus) { Error_ = value; } }
            }
            public bool StepByStep
            {
                get { bool ret; lock (this.LockObjectStep) { ret = StepByStep_; } return ret; }
                set { lock (this.LockObjectStep) { StepByStep_ = value; } }
            }
            private bool bNextStep
            {
                get { bool ret; lock (this.LockObjectStep) { ret = bNextStep_; } return ret; }
                set { lock (this.LockObjectStep) { bNextStep_ = value; } }
            }
            public bool Paused
            {
                get { bool ret; lock (this.LockObjectStep) { ret = Paused_; } return ret; }
                set { lock (this.LockObjectStep) { Paused_ = value; } }
            }
            public bool Abort
            {
                get { bool ret; lock (this.LockObjectCommand) { ret = Abort_; } return ret; }
                set { lock (this.LockObjectCommand) { Abort_ = value; } }
            }
            public bool bStop
            {
                get { bool ret; lock (this.LockObjectCommand) { ret = bStop_; } return ret; }
                set { lock (this.LockObjectCommand) { bStop_ = value; } }
            }
            public int State = 0;
            public MainWindow ui = null;

            public void LogDuration(string fileName, ref Stopwatch stopWatch, string op)
            {
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime;
                if (!Properties.Settings.Default.ProcessFlowLog) return;
                if (op != "Начало") elapsedTime = String.Format("{0:00}.{1:000}", ts.Seconds, ts.Milliseconds);
                else elapsedTime = Environment.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                using (System.IO.StreamWriter sw = File.AppendText(fileName))
                {
                    sw.WriteLine(elapsedTime + " " + op);
                }
                stopWatch.Restart();
            }
            public void ErrorEvent(string ErrorString, int posIndex = -1)
            {
                Error = true;
                if (StatusString != ErrorString)
                {
                    lock (LockObjectStatus)
                    {
                        StatusString = ErrorString;
                        StatusBackground = Brushes.Red;
                    }
                    if (posIndex == -1) ui.Log(ErrorString + " [" + Name + "]");
                    else ui.Log(ErrorString + " [" + Name + "/" + posIndex.ToString() + "]");
                    TLog.Add(Name + ",E," + ErrorString);
                }
            }

            public void ChangeStatus(string Status, Brush statusBackground)
            {
                StatusString = Status;
                StatusBackground = statusBackground;
                if (TLog.enableLog == true)
                {
                    TLog.Add(Name + ",S," + StatusString);
                }
            }

            public void ChangeStatus(string Status)
            {
                StatusString = Status;
                StatusBackground = Brushes.White;
                if (TLog.enableLog == true)
                {
                    TLog.Add(Name + ",S," + StatusString);
                }
            }

            public void NextStep()
            {
                bNextStep = true;
            }

            public bool CheckPoint(string s = "")
            {
                if (StepByStep)
                {
                    Paused = true;
                    bNextStep = false;
                    string old_status = StatusString;
                    Brush old_brush = StatusBackground;
                    ChangeStatus(old_status + "(изчаква стъпка) " + s + "...!", Brushes.LightGreen);
                    while (!bNextStep)
                    {
                        if (CheckForErrors()) { ChangeStatus("Прекъснат!", Brushes.Red); return true; }
                        if (Command == CMD_INIT) return true;
                        Thread.Sleep(100);
                    }
                    Paused = false;
                    ChangeStatus(old_status, old_brush);
                }
                else
                {
                    if (TLog.enableLog == true)
                    {
                        TLog.Add(Name + ",CP," + s);
                    }
                }
                return false;
            }

            public bool WaitIdle( int tsleep)
            {
                Thread.Sleep(tsleep);
                if (CheckForErrors()) { ChangeStatus("Прекъснат!", Brushes.Red); return true; }
                if (Command == CMD_INIT) return true;
                return false;
            }

            public bool CheckTimeoutLocal(ref int counter, int timeout, string ErrorString)
            {
                Thread.Sleep(1);
                counter += 1;
                if (CheckForErrors()) { ChangeStatus("Прекъснат!", Brushes.Red); return true; }
                if (Command == CMD_INIT) return true;
                if (counter >= timeout)
                {
                    if (!bStop)
                    {
                        ErrorEvent(ErrorString); return true;
                    }
                }
                return false;
            }

            public bool WaitInput(int Input, bool InputState, int Timeout, string ErrorState)
            {
                int t = 0;
                while (DioIn(Input) != InputState)
                {
                    if (CheckForErrors() || Abort) { ChangeStatus("Прекъснат!", Brushes.Red); return true; }
                    if (Command == CMD_INIT) return true;
                    if (Timeout > 0)
                    {
                        Thread.Sleep(1);
                        t += 1;
                        if (t > Timeout)
                        {
                            ErrorEvent(ErrorState);
                            return true;
                        }
                    }
                }
                return false;
            }
            public bool WaitCondition(ref bool Condition, bool bState, int Timeout, string ErrorState)
            {
                int t = 0;
                while (Condition != bState)
                {
                    if (CheckForErrors() || Abort) { ChangeStatus("Прекъснат!", Brushes.Red); return true; }
                    if (Command == CMD_INIT) return true;
                    if (Timeout > 0)
                    {
                        Thread.Sleep(1);
                        t += 1;
                        if (t > Timeout)
                        {
                            ErrorEvent(ErrorState);
                            return true;
                        }
                    }
                }
                return false;
            }

            public virtual void Init()
            {
                lock (LockObjectStatus)
                {
                    Initialized_ = false;
                    Error_ = false;
                    Ready_ = false;
                }
                Command = CMD_INIT;
            }

            public virtual void Process()
            {
                lock (LockObjectStatus)
                {
                    Error_ = false;
                    Ready_ = false;
                }
                Command = CMD_PROCESS;
            }

        }
    }
}