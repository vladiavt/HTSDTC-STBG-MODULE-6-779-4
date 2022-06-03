using System;
using System.Drawing;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Machine.Common;
using System.Windows.Threading;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace Machine
{
    public class LedInfo
    {
        public string LedName { get; set; }
        public bool LedState { get; set; }
        public LedInfo (string lName, bool lState)
        {
            LedName = lName;
            LedState = lState;
        }
    }
    public class MarkingLaserWithCom
    {
        public const int LCMD_SETVAR = 20421;
        public const int LCMD_MARKING = 20204;
        public const int LCMD_GETSTATUS = 20261;
        public const int LCMD_LDMARKFILE = 20401;
        public const int LCMD_LASSHUTTER = 20201;
        public const int LCMD_RESETLASER = 20209;

        public string MarkingFileName { get; set; }

        static private SerialPort Port;
        private Thread thread;

        private string myNameVar = "";
        private string myValueVar = "";
        public bool Connected = false;
        public bool Error = false;
        public int Command = 0;
        public ushort binStatus = 0;
        private string oldStatus = String.Empty;
        private MainWindow ui;

        public MarkingLaserWithCom(string portName, MainWindow userInterface)
        {
            ui = userInterface;
            thread = new Thread(new ThreadStart(threadMethod));
            Connected = false;

            Port = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            Port.ReadTimeout = 1000;
            try
            {
                Port.Open();
                Port.DiscardInBuffer();
                Connected = true;
            }
            catch (UnauthorizedAccessException) { Connected = false; }
            catch (IOException) { Connected = false; }
            catch (ArgumentException) { Connected = false; }

            if (Connected)
            {
                MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Портът е отворен успешно", System.Windows.Media.Brushes.Green));
            }
            else
            {
                MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Портът не се отваря", System.Windows.Media.Brushes.Red));
            }

            thread.IsBackground = true;
            thread.Start();
        }

        public string NameVar
        {
            get { return myNameVar; }
            set { myNameVar = value; }
        }

        public string ValueVar
        {
            get { return myValueVar; }
            set { myValueVar = value; }
        }

        public void ChangePort(string portName)
        {
            try
            {
                Port.Close();
                Port.PortName = portName;
                Port.Open();
                Port.DiscardInBuffer();
                Connected = true;
                MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Портът е отворен успешно", System.Windows.Media.Brushes.Green));
            }
            catch
            {
                Connected = false;
                MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Портът не се отваря", System.Windows.Media.Brushes.Red));
            }
        }

        private byte[] CreatePacket(int commandCode, params string[] values)
        {
            int totlen = 0;
            byte checkSum = 0x00;
            var myByteArray = new ArrayList();

            myByteArray.Add((byte)0x02); //start packet with 'STX'

            myByteArray.AddRange(BitConverter.GetBytes(commandCode)); //command code first

            foreach (string str in values)
            {
                totlen = totlen + str.Length + 1;
            }
            myByteArray.AddRange(BitConverter.GetBytes(totlen)); //packet length next (command code is not included)

            foreach (string str in values)
            {
                myByteArray.AddRange(Encoding.ASCII.GetBytes(str));
                myByteArray.Add((byte)0x00); //terminate with 'null'
            }

            for (int i = 1; i < myByteArray.Count; i++)
            {
                checkSum ^= (byte)myByteArray[i];
            }
            if (checkSum < 0x20) checkSum += 0x20;
            myByteArray.Add(checkSum);
            
            myByteArray.Add((byte)0x03); //end packet with 'ETX'

            //4 bytes for 'commandCode' + 4 bytes for 'Length' + 1 byte for 'STX' + n * (string lengths + 1 byte for null terminator) + 1 byte for BCC + 1 byte for 'ETX'
            //byte[] fba = new byte[totlen + 11];
            byte[] fba = new byte[myByteArray.Count];
            fba = (byte[])myByteArray.ToArray(typeof(byte));
            return fba;
        }

        private int SendReceive(byte[] command, byte[] received)
        //returns   0: receved telegram is OK
        //          1: laser does not accept the telegram - probably wrong format/data/BCC
        //          2: laser does not respond to a command with neither ACK nor NAK
        //          3: exception during port read for ACK/NAK
        //          4: exception during port read for body of telegram
        //          5: telegram is less than 8 bytes long
        //          6: telegram with bad BCC was received

        {
            int numberTries;
            int idxReceived = 0;
            byte byteReceived = 0;
            byte checkSum;
            byte[] respCode = { 0x06, 0x15 }; //ACK and NAK codes to complete receiving of data
            bool inTelegram = false;
            DateTime timeoutStart;
            TimeSpan timeSpent;

            numberTries = 0;
        Retry:
            Port.DiscardInBuffer();
            Port.Write(command, 0, command.Length);
            timeoutStart = DateTime.Now;

            try
            {
                do
                {
                    if (Port.BytesToRead > 0)
                    {
                        byteReceived = (byte)Port.ReadByte();
                        if (byteReceived == respCode[0]) break; //ACK
                        if (byteReceived == respCode[1]) //NAK
                        {
                            return 1; //laser does still not accept the telegram
                        }
                    }

                    Thread.Sleep(10);
                    timeSpent = DateTime.Now - timeoutStart;
                    if (timeSpent.TotalMilliseconds > 20000)
                    {
                        numberTries++;
                        if (numberTries < 2) goto Retry;
                        else
                        {
                            Connected = false;
                            return 2; //laser does not respond
                        }
                    }
                }
                while (true);
            }
            catch
            {
                return 3;  //exception during port read for ACK/NAK
            }

            try
            {
                while (true)
                {
                    if (Port.BytesToRead > 0)
                    {
                        byteReceived = (byte)Port.ReadByte();
                        if (!inTelegram)
                        {
                            if (byteReceived == (byte)0x02)
                            {
                                idxReceived = 0;
                                inTelegram = true;
                            }
                        }
                        else
                        {
                            if (byteReceived == (byte)0x03)
                            {
                                break;
                            }
                            else
                            {
                                received[idxReceived++] = byteReceived;
                            }
                        }
                    }

                    Thread.Sleep(10);
                    timeSpent = DateTime.Now - timeoutStart;
                    if (timeSpent.TotalMilliseconds > 20000)
                    {
                        numberTries++;
                        if (numberTries < 2) goto Retry;
                        else
                        {
                            Connected = false;
                            return 2; //laser does not respond
                        }
                    }
                }
            }
            catch (TimeoutException)
            {
                Connected = false;
                return 4;  //exception during port read for body of telegram
            }

            if (idxReceived < 8)
            {
                return 5;   //telegeram length < 8
            }

            //check for BCC:
            checkSum = 0;
            for (int i = 0; i < idxReceived-2; i++)
            {
                checkSum ^= received[i];
            }

            if (checkSum == received[idxReceived - 1])
            {
                Port.Write(respCode, 0, 1);
                return 0;
            }
            else
            {
                Port.Write(respCode, 1, 1);
                return 6;   //telegeram with bad BCC was received
            }
        }

        private void GetMarkingStatus()
        {
            byte[] response = new byte[1024];
            byte[] staCommand = new byte[11]; //1 byte for STX + 4 bytes for the command code + 4 bytes for the packet length + 1 byte for the BCC + 1 byte for ETX
            int returnCode;
            if (!Connected) return;
            staCommand = CreatePacket(LCMD_GETSTATUS);
            returnCode = SendReceive(staCommand, response);
            switch (returnCode)
            //          1: laser does not accept the telegram - probably wrong format/data/BCC
            //          2: laser does not respond to a command with neither ACK nor NAK
            //          3: exception during port read for ACK/NAK
            //          4: exception during port read for body of telegram
            //          5: telegram is less than 8 bytes long
            //          6: telegram with bad BCC was received
            {
                case 0:
                    break;
                case 1:
                    MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Лазерът връща NAK: грешна програма", System.Windows.Media.Brushes.Red));
                    return;
                case 2:
                    MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Лазерът не отговаря...", System.Windows.Media.Brushes.Red));
                    return;
                case 3:
                    MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Exception докато чака ACK/NAK", System.Windows.Media.Brushes.Red));
                    return;
                case 4:
                    MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Exception докато чака тялото на програмата", System.Windows.Media.Brushes.Red));
                    return;
                case 5:
                    MainWindow.ListMLmess.Add(new MainWindow.MLcomms("програмата е по-къса от 8 байта", System.Windows.Media.Brushes.Red));
                    return;
                case 6:
                    MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Получена е програма с грешна контролна сума", System.Windows.Media.Brushes.Red));
                    return;
                default:
                    MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Неизвестен код за грешка по време на комуникация", System.Windows.Media.Brushes.Red));
                    return;
            }
            // process the communicated status:
            int cmdValue = (int)BitConverter.ToUInt32(response, 0);
            if (cmdValue != LCMD_GETSTATUS)
            { //"Get status" command was sent but Marking laser responds with a diffrement command code
                MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Отговаря на команда <> Статус", System.Windows.Media.Brushes.Red));
                return;
            }
            int lenValue = (int)BitConverter.ToUInt32(response, 4);
            if ((lenValue != 20) && (lenValue != 22))
            {
                //Error for length of response
                MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Отговаря на статус команда с " + lenValue + "байта, вместо 20/22", System.Windows.Media.Brushes.Red));
                return;
            }
            if (response[9] != (byte)0x00)
            {
                MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Отговаря на статус команда с лош формат за код на грешка G1", System.Windows.Media.Brushes.Red));
                return;
            }

            char errValue = (char)BitConverter.ToChar(response, 8);
            if (errValue != '0')
            {
                switch (errValue)
                {
                    case '1':
                        if (lenValue != 22)
                        {
                            MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Липсва грешка G2 в отговора на заявката за статус", System.Windows.Media.Brushes.Red));
                            return;
                        }
                        else
                        {
                            errValue = (char)BitConverter.ToChar(response, 28);
                            if (errValue == '0')
                            {
                                MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Вътрешна грешка при отговора на заявката за статус", System.Windows.Media.Brushes.Red));
                                return;
                            }
                            else
                            {
                                MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Неизвестен код за G2 при отговора на заявката за статус", System.Windows.Media.Brushes.Red));
                                return;
                            }
                        }

                    case '3':
                        MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Невалидна КОМАНДА-ДЪЛЖИНА на заявката за статус", System.Windows.Media.Brushes.Red));
                        return;
                    case '4':
                        MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Невалидна КОМАНДА-ДЪЛЖИНА на отговора за статус", System.Windows.Media.Brushes.Red));
                        return;

                    default:
                        MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Грешен код за грешка G1 в отговора на заявката за статус", System.Windows.Media.Brushes.Red));
                        return;
                }
            }
            // here when marking laser return status with no errors:
            string status = Encoding.ASCII.GetString(response, 19, 8) + Encoding.ASCII.GetString(response, 10, 8);
            MainWindow.updIO.MLI_Status = Convert.ToUInt16(status, 2);
        }

        private bool CommandExec(int command, params string[] values)
        {
            byte[] response = new byte[1024];
            int lenArray = 0;
            int returnCode;

            //check for the right number of params:
            switch (command)
            {
                case LCMD_SETVAR:
                    if (values.Length != 2)
                    {
                        MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Грешен брой параметри на команда SETVAR", System.Windows.Media.Brushes.Red));
                        return true;
                    }
                    else MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Sent: SETVAR " + values[0] + " " + values[1], System.Windows.Media.Brushes.Green));
                    lenArray = 13 + values[0].Length + values[1].Length;
                    break;

                case LCMD_MARKING:
                    if (values.Length > 0)
                    {
                        MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Грешен брой параметри на команда MARKING", System.Windows.Media.Brushes.Red));
                        return true;
                    }
                    else MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Sent: MARK", System.Windows.Media.Brushes.Green));
                    lenArray = 11;
                    break;

                case LCMD_LDMARKFILE:
                    if (values.Length != 1)
                    {
                        MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Грешен брой параметри на команда LOAD_MARKING_FILE", System.Windows.Media.Brushes.Red));
                        return true;
                    }
                    else MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Sent: LDMARKFILE " + values[0], System.Windows.Media.Brushes.Green));
                    lenArray = 12 + values[0].Length;
                    break;

                case LCMD_LASSHUTTER:
                    if (values.Length != 1)
                    {
                        MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Грешен брой параметри на команда SHUTTER", System.Windows.Media.Brushes.Red));
                        return true;
                    }
                    else
                    {
                        if ((values[0] != "0") && (values[0] != "1"))
                        {
                            MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Грешен параметър (<> 0/1) на команда SHUTTER", System.Windows.Media.Brushes.Red));
                            return true;
                        }
                        else MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Sent: SHUTTER" + values[0], System.Windows.Media.Brushes.Green));
                        lenArray = 12 + values[0].Length;
                        break;
                    }

                case LCMD_RESETLASER:
                    if (values.Length > 0)
                    {
                        MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Грешен брой параметри на команда RESET", System.Windows.Media.Brushes.Red));
                        return true;
                    }
                    else MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Sent: RESET", System.Windows.Media.Brushes.Green));
                    lenArray = 11;
                    break;
            }

            //here if the parameters for the command are ok:

            byte[] staCommand = new byte[lenArray];
            switch (command)
            {
                case LCMD_SETVAR:
                    staCommand = CreatePacket(command, values[0], values[1]);
                    break;

                case LCMD_MARKING:
                    staCommand = CreatePacket(command);
                    break;

                case LCMD_LDMARKFILE:
                    staCommand = CreatePacket(command, values[0]);
                    break;

                case LCMD_LASSHUTTER:
                    staCommand = CreatePacket(command, values[0]);
                    break;

                case LCMD_RESETLASER:
                    staCommand = CreatePacket(command);
                    break;
            }

            returnCode = SendReceive(staCommand, response);
            switch (returnCode)
            //          1: laser does not accept the telegram - probably wrong format/data/BCC
            //          2: laser does not respond to a command with neither ACK nor NAK
            //          3: exception during port read for ACK/NAK
            //          4: exception during port read for body of telegram
            //          5: telegram is less than 8 bytes long
            //          6: telegram with bad BCC was received
            {
                case 0:
                    break;
                case 1:
                    MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Лазерът връща NAK: грешна телеграма", System.Windows.Media.Brushes.Red));
                    return true;
                case 2:
                    MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Лазерът не отговаря...", System.Windows.Media.Brushes.Red));
                    return true;
                case 3:
                    MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Exception докато чака ACK/NAK", System.Windows.Media.Brushes.Red));
                    return true;
                case 4:
                    MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Exception докато чака тялото на телеграмата", System.Windows.Media.Brushes.Red));
                    return true;
                case 5:
                    MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Телеграмата е по-къса от 8 байта", System.Windows.Media.Brushes.Red));
                    return true;
                case 6:
                    MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Получена е телеграма с грешна контролна сума", System.Windows.Media.Brushes.Red));
                    return true;
                default:
                    MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Неизвестен код за грешка по време на комуникация", System.Windows.Media.Brushes.Red));
                    return true;
            }

            // process the communicated status:
            int intValue = (int)BitConverter.ToUInt32(response, 0);
            if (intValue != command)
            { //some command was sent but Marking laser responds with a diffrement command code
                MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Отговаря на различна команда", System.Windows.Media.Brushes.Red));
                return true;
            }

            int lenValue = (int)BitConverter.ToUInt32(response, 4);
            if ((lenValue != 2) && (lenValue != 4))
            {
                MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Отговаря с грешна дължина", System.Windows.Media.Brushes.Red));
                return true;
            }

            char G1 = (char)0x00;
            string G2 = "";
            G1 = (char)BitConverter.ToChar(response, 8);
            if (((G1 == '1') && (lenValue < 4)) || ((G1 == '0') && (lenValue != 2)))
            {
                MainWindow.ListMLmess.Add(new MainWindow.MLcomms("Има разминаване между грешки G1 и G2", System.Windows.Media.Brushes.Red));
                return true;
            }

            if (G1 == '1')
                G2 = Encoding.ASCII.GetString(response, 10, lenValue - 3);

            string errMessage = "";
            switch (command)
            {
                case LCMD_SETVAR:
                    if (G1 == '0') errMessage = "OK";
                    else if (G1 == '1')
                    {
                        if (G2 == "1") errMessage = "Няма такава променлива";
                        else if (G2 == "2") errMessage = "Не е зареден маркиращ файл";
                        else errMessage = "Вътрешна грешка";
                    }
                    else if (G1 == '2') errMessage = "Неправилен формат за променливата";
                    else if (G1 == '3') errMessage = "Грешна дължина на заявката";
                    break;

                case LCMD_MARKING:
                    if (G1 == '0') errMessage = "OK";
                    else if (G1 == '1')
                    {
                        if (G2 == "1") errMessage = "Няма наличен конвертиран маркиращ файл";
                        else if (G2 == "2") errMessage = "Не е зареден маркиращ файл";
                        else if (G2 == "3") errMessage = "Лазерът е изключен";
                        else if (G2 == "4") errMessage = "Маркирането вече е стартирано";
                        else if (G2 == "5") errMessage = "Маркирането е прекъснато ръчно";
                        else if (G2 == "10") errMessage = "Обща грешка при конвертиране";
                        else if (G2 == "11") errMessage = "Няма конвертиране, празна променлива?";
                        else if (G2 == "12") errMessage = "Лазерният шрифт не съществува";
                        else if (G2 == "13") errMessage = "Променлива от текстов файл: данните са изчерпани";
                        else if (G2 == "14") errMessage = "Сериен номер: достигната е крайната стойност";
                        else errMessage = "Вътрешна грешка";
                    }
                    else if (G1 == '3') errMessage = "Грешна дължина на заявката";
                    break;

                case LCMD_LDMARKFILE:
                    if (G1 == '0')
                    {
                        errMessage = "OK";
                        MainWindow.markingFileLoadedOK = true;
                        MainWindow.markingFileLoaded = MarkingFileName;
                        break;
                    }
                    else if (G1 == '1')
                    {
                        MainWindow.markingFileLoadedOK = false;
                        MainWindow.markingFileLoaded = "";
                        if (G2 == "1") errMessage = "Няма наличен конвертиран маркиращ файл";
                        else if (G2 == "10") errMessage = "Обща грешка при конвертиране";
                        else if (G2 == "11") errMessage = "Няма конвертиране, празна променлива?";
                        else if (G2 == "12") errMessage = "Лазерният шрифт не съществува";
                        else if (G2 == "13") errMessage = "Променлива от текстов файл: данните са изчерпани";
                        else if (G2 == "14") errMessage = "Сериен номер: достигната е крайната стойност";
                        else errMessage = "Вътрешна грешка";
                    }
                    else if (G1 == '2') errMessage = "Неправилен формат за променливата";
                    else if (G1 == '3') errMessage = "Грешна дължина на заявката";
                    break;

                case LCMD_LASSHUTTER:
                    if (G1 == '0') errMessage = "OK";
                    else if (G1 == '1') { if (Convert.ToInt32(G2) > 0) errMessage = "Вътрешна грешка"; }
                    else if (G1 == '2') errMessage = "Неправилен формат за променливата";
                    else if (G1 == '3') errMessage = "Грешна дължина на заявката";
                    break;

                case LCMD_RESETLASER:
                    if (G1 == '0') errMessage = "OK";
                    else if (G1 == '1') { if (Convert.ToInt32(G2) > 0) errMessage = "Вътрешна грешка"; }
                    else if (G1 == '3') errMessage = "Грешна дължина на заявката";
                    break;
            }

            if (errMessage.Equals("OK"))
            {
                MainWindow.ListMLmess.Add(new MainWindow.MLcomms(errMessage, System.Windows.Media.Brushes.Green));
                return false;
            }
            else
            {
                MainWindow.ListMLmess.Add(new MainWindow.MLcomms(errMessage, System.Windows.Media.Brushes.Red));
                return true;
            }
        }

        public void ResetLaser()
        {
            Command = LCMD_RESETLASER;
        }

        public void LaserOn()
        {
            Command = LCMD_LASSHUTTER;
        }

        private void threadMethod()
        {
            while (true)
            {
                try
                {
                    if (Port.IsOpen)
                    {
                        if (Command > 0)
                        {
                            switch (Command)
                            {
                                case LCMD_SETVAR:
                                    Error = CommandExec(Command, myNameVar, myValueVar);
                                    Command = 0;
                                    break;

                                case LCMD_MARKING:
                                    Error = CommandExec(Command);
                                    Command = 0;
                                    break;

                                case LCMD_LDMARKFILE:
                                    Error = CommandExec(Command, MarkingFileName);
                                    Command = 0;
                                    break;

                                case LCMD_LASSHUTTER:
                                    if (MainWindow.updIO.MLI_ShutterOpen) Error = CommandExec(Command, "0");
                                    else Error = CommandExec(Command, "1");
                                    Command = 0;
                                    break;

                                case LCMD_RESETLASER:
                                    Error = CommandExec(Command);
                                    Command = 0;
                                    break;
                            }
                            Thread.Sleep(100);                    // TODO: Validate this.
                        }
                        else
                        {
                            GetMarkingStatus();
                        }
                        Thread.Sleep(1);                    // TODO: Validate this.
                    }
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                    Connected = false;
                }
            }
        }
    }
}
