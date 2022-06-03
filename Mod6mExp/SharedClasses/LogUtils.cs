using System;
using System.Windows;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.ComponentModel; 

namespace Machine
{
    public class LoopsCounter
    {
        int secLast;
        int mainTickCnt = 0;
        public int maintickLoops = 0;

        public int tick()
        {
            int sec = DateTime.Now.Second;
            if (sec == secLast)
            {
                mainTickCnt++;
                return 0;
            }
            else
            {
                secLast = sec;
                maintickLoops = mainTickCnt;
                mainTickCnt = 0;
                return 1;
            }
        }
    }

    public class LogTextInfo
    {
        private static Object logLock = new Object();  // lock for file logging from threads
        public bool enableLog = false; 
        public string logFileName = "Machine.log";
        public string errorString = "";

        public LogTextInfo(string fname, bool en=false)
        {
           logFileName=fname;
           if (en) StartLog();
           else enableLog = false;
        }

        public void StartLog()
        {
            if (enableLog == false)
            {
                enableLog = true;
                lock (logLock)
                {
                    StreamWriter log_file = null;
                    try
                    {
                        log_file = new StreamWriter(logFileName, true);
                    }
                    catch (Exception)
                    {
                        //if (e == System.IO.DirectoryNotFoundException){};
                        Directory.CreateDirectory(new FileInfo(logFileName).Directory.FullName);
                        try
                        {
                            log_file = new StreamWriter(logFileName, true);
                        }
                        catch (Exception e)
                        {
                            errorString = e.Message;
                            return;   // 
                        }
                    }
                    using (log_file)
                    {
                        DateTime dt1 = DateTime.Now;
                        string ls = dt1.ToString("O");
                        ls = ls.Remove(10);
                        ls += ",Process,C1,C2,C3,C4,C5,C6,C7";

                        if (log_file != null)
                        {
                            try
                            {
                                log_file.WriteLine(ls);
                            }
                            catch (Exception e)
                            {
                                errorString = e.Message; // MessageBox("Can not write to log file !!!");  
                            }
                        }
                    }
                }
            }

        }
        public void Add(string s)
        {
            if (enableLog)
            {
                lock (logLock)
                {
                    StreamWriter log_file=null;
                    try
                    {
                        log_file= new StreamWriter(logFileName, true);
                    }
                    catch (Exception)
                    {
                        //if (e == System.IO.DirectoryNotFoundException){};
                        Directory.CreateDirectory(new FileInfo(logFileName).Directory.FullName);
                        try
                        {
                            log_file= new StreamWriter(logFileName, true);
                        }
                        catch (Exception e)
                        {
                            errorString = e.Message;
                            return;   // 
                        }
                    }
                    using ( log_file )
                    {
                        DateTime dt1 = DateTime.Now;
                        string ls = dt1.ToString("O");
                        ls=ls.Remove(25).Remove(0, 11);
                        ls+= "," + s;

                        if (log_file != null)
                        {
                            try
                            {
                                log_file.WriteLine(ls);
                            }
                            catch (Exception)
                            {// MessageBox("Can not write to log file !!!");  
                            }
                        }
                    }
                }
            }
        }

    }

    internal class HiTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);

        private long startTime, stopTime;
        private long freq;

        // Constructor
        public HiTimer()
        {
            startTime = 0;
            stopTime = 0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                // high-performance counter not supported
                throw new Win32Exception();
            }
        }

        // Start the timer - let other threads run before
        public void Start()
        {
            // lets do the waiting threads there work
            Thread.Sleep(0);

            QueryPerformanceCounter(out startTime);
        }

        // Start the timer immediatelly
        public void StartI()
        {
            QueryPerformanceCounter(out startTime);
        }

        // Stop the timer
        public void Stop()
        {
            QueryPerformanceCounter(out stopTime);
        }

        // Returns the duration of the timer (in seconds)
        public double Duration
        {
            get
            {
                return (double)(stopTime - startTime) / (double)freq;
            }
        }
        // Returns the duration of the timer (in seconds)
        public double Duration_ms
        {
            get
            {
                return (double)(stopTime - startTime)*1000 / (double)freq;
            }
        }
    }

}