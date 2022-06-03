using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Diagnostics;
using System.Windows;

namespace Machine
{
    //To connect to the PLC use IP Address 192.168.250.2, Subnet Mask 255.255.255.0, Default Gateway 192.168.0.1

    //TCPcom извършва обмена с периферията:

    //Чете входовете от CIO адреси (всичко е в UINT формат): 
    //  0х0000 -> CommonInputs1 
    //  0х3301 -> CommonInputs2 
    //  0х3302 -> Position1Inputs 
    //  0х3303 -> Position2Inputs
    //  0х3304 -> Position3Inputs
    //Записва изходите на CIO адреси
    //  CommonOutputs -> 0х0001
    //  Pos1Outputs -> 0x3202
    //  Pos2Outputs -> 0x3203 
    //  Pos3Outputs -> 0x3204

    //Обработва информацията за FESTO:
    //  Към мотора:
    //      16 бита на FestoControl1 -> 0x3206 (UINT)
    //      TargetVelocity + FestoControl2 -> 0x3207 (high byte is for % of nominal velocity) (UINT)
    //      FestoTargetPosition -> 0x3208+0x3209 (DINT)
    //  От мотора:
    //      0x3306 -> FestoStatus1 (UINT)
    //      0x3307 -> CurrentVelocity + FestoStatus2 (high byte is for % of nominal velocity) (UINT)
    //      0x3308+0x3309 (32 bits) -> FestoCurrentPosition (DINT)

    //Обработва информацията за LinMot:
    //  Към мотора:
    //      16 бита на LinMotControl -> 0x3215 (UINT)
    //      LinModCmdHeader -> 0x3216 (UINT)
    //      LinMotTargetPosition -> 0x3217+0x3218 (DINT)
    //      LinMotVelocity -> 0x3219+0x3220 (UDINT)
    //      LinMotAcc -> 0x3221+0x3222 (UDINT)
    //      LinMotDec -> 0x3223+0x3224 (UDINT)
    //  От мотора:
    //      0x3315 -> LinMotStatus (UINT)
    //      0x3316 -> LinMonStateVar (UINT)
    //      0x3317 -> LinMotWarningCode (UINT)
    //      0x3318+0x3319 -> LinMotActualPosition (DINT)
    //      0x3322+0x3323 -> LinMotDemandPosition (DINT)

    //Обработва информацията за лазера
    //  Към лазера:
    //      LaserControl -> 0x3210 (UINT)
    //      LaserProgram -> 0x3211 (UINT)
    //  От лазера:
    //      0x3310 -> LaserStatus (UINT)

    //Входовете CommonInputs1 и CommonInputs2, от моторите и от лазера се четат независимо от положението на масата
    //Входовете от позициите на масата се четат само при спряла маса (iCommonRotTableInPos == true) 
    // и се мапират в Pos1Inputs, Pos2Inputs и Pos3Inputs според iCommonStation1/2/3

    //Изходите към CommonOutpuтs, към моторите и лазера се записват независимо от положението на масата
    //Изходите към позициите на масата се вземат от LoadStationOutputs, WeldStationOutputs и MarkStationOutputs 
    // и се мапират към Position1/2/3 само при спряла маса според iCommonStation1/2/3

    //Мапирането става само при спряла маса (когато iCommonStation1/2/3 са валидни)
    //Схема на мапирането (то е валидно само ако е валиден САМО ЕДИН от сигналите iCommonStation1/2/3):
    //  при iCommonStation1 == true, iCommonStation2 == false, iCommonStation3 == false
    //      LoadStation <-> Position1, WeldStation <-> Position3, MarkStation <-> Position2
    //  при iCommonStation0 == false, iCommonStation2 == true, iCommonStation3 == false
    //      LoadStation <-> Position2, WeldStation <-> Position1, MarkStation <-> Position3
    //  при iCommonStation0 == false, iCommonStation2 == false, iCommonStation3 == true
    //      LoadStation <-> Position3, WeldStation <-> Position2, MarkStation <-> Position1

    //Всички команди към изходите по позиции се записват в опашка и изпълняват с приоритет при първа възможност (спряла маса)
    //Командите в опашката имат структурата (OutRegisterID, OutRegisterVal) - CIO адресът, отговарящ на OutRegisterID,
    //  се определя в момента на изпълнение на командата (за да е сигурно, че е валиден при спряла маса)

    public class TCPcom2
    {
        private Thread thWorkWrite;
        private Thread thWorkRead;

        public static ushort[] PlcInternal = new ushort[2];
        public static ushort[] PlcControl = new ushort[52];
        public static ushort[] PlcInputs = new ushort[25];
        public static ushort[] PlcOutputs = new ushort[25];
        public static ushort[] hMemory = new ushort[16];
        public bool ReadHmemory { get; set; }
        public bool WriteHmemory { get; set; }
        public bool ValidHvalues { get; set; }

        public Queue<SetOutCommand> SetOutCommands = new Queue<SetOutCommand>();

        private mcOMRON.OmronPLC plc;

        public TCPcom2(ref mcOMRON.OmronPLC plcInstance)
        {
            IPAddress IP = IPAddress.Parse("192.168.250.1");
            plc = plcInstance;
            mcOMRON.tcpFINSCommand tcpCommand = ((mcOMRON.tcpFINSCommand)plc.FinsCommand);
            if (IPAddress.TryParse(Properties.Settings.Default.PlcIP, out IP))
            {
                tcpCommand.SetTCPParams(IP, 9600);

                if (!plc.Connect())
                {
                    throw new Exception(plc.LastError);
                }
            }

            ReadHmemory = false;
            WriteHmemory = false;
            ValidHvalues = false;

            thWorkWrite = new Thread(new ThreadStart(WorkThreadMethodWrite));
            thWorkWrite.Name = "TCPcomWrite";
            thWorkWrite.IsBackground = true;
            thWorkWrite.Start();
            thWorkRead = new Thread(new ThreadStart(WorkThreadMethodRead));
            thWorkRead.Name = "TCPcomRead";
            thWorkRead.IsBackground = true;
            thWorkRead.Start();
        }

        public bool ValidTableState()
        {
            bool state;
            state = MainWindow.updIO.iCommonRotTableInPos &&
                ((MainWindow.updIO.iCommonStation1 && !MainWindow.updIO.iCommonStation2 && !MainWindow.updIO.iCommonStation3) ||
                (!MainWindow.updIO.iCommonStation1 && MainWindow.updIO.iCommonStation2 && !MainWindow.updIO.iCommonStation3) ||
                (!MainWindow.updIO.iCommonStation1 && !MainWindow.updIO.iCommonStation2 && MainWindow.updIO.iCommonStation3));
            return state;
        }

        private bool locked;

        public bool Locked
        {
            get { return locked; }
            set { locked = value; }
        }



        private void WorkThreadMethodWrite()
        {
            ushort[] AtOffset0 = new ushort[160];
            ushort[] AtOffset3200 = new ushort[130];
            SetOutCommand cmd = new SetOutCommand();
            while (true)
            {
                if (MainWindow.plc.Connected)
                {
                    if(!Locked)
                    {
                        Locked = true;
                        while (SetOutCommands.Count > 0)
                        {
                            cmd = SetOutCommands.Dequeue();
                            switch (cmd.cmdID)
                            {
                                case 'B':
                                    plc.WriteBit(cmd.memArea, cmd.RegAddress, cmd.BitPosition, cmd.BitValue);
                                    break;
                                case 'W':
                                    plc.WriteWord(cmd.memArea, cmd.RegAddress, cmd.WordValue);
                                    break;
                                case 'D':
                                    plc.WriteDoubleWord(cmd.memArea, cmd.RegAddress, cmd.DWordValue);
                                    break;
                            }
                            Thread.Sleep(10);
                        }
                        Locked = false;
                    }
                }
                Thread.Sleep(5);
            }
        }

        private void WorkThreadMethodRead()
        {
            ushort[] AtOffset0 = new ushort[160];
            ushort[] AtOffset3200 = new ushort[130];
            SetOutCommand cmd = new SetOutCommand();
            while (true)
            {
                if (MainWindow.plc.Connected)
                {
                    if(!Locked)
                    {
                        Locked = true;
                        plc.ReadWordRange(mcOMRON.MemoryArea.CIO, 0, ref AtOffset0, (ushort)AtOffset0.Length);
                        plc.ReadWordRange(mcOMRON.MemoryArea.CIO, 3200, ref AtOffset3200, (ushort)AtOffset3200.Length);
                        //reading all relevant PLC data takes about 0.35 sec

                        if (MainWindow.plc.LastError != "")
                        {
                            MessageBox.Show(MainWindow.plc.LastError, "PLC communication error", MessageBoxButton.OK);
                        }
                        else MainWindow.PLCdataRefreshed = true;

                        Array.Copy(AtOffset0, 0, PlcInternal, 0, 2);
                        Array.Copy(AtOffset0, 100, PlcControl, 0, 52);
                        Array.Copy(AtOffset3200, 0, PlcOutputs, 0, 25);
                        Array.Copy(AtOffset3200, 100, PlcInputs, 0, 25);

                        if (ReadHmemory)
                        {
                            ReadHmemory = false;
                            ValidHvalues = plc.ReadWordRange(mcOMRON.MemoryArea.HR, 0, ref hMemory, (ushort)hMemory.Length);
                        }
                        if (WriteHmemory)
                        {
                            WriteHmemory = false;
                            plc.WriteWordRange(mcOMRON.MemoryArea.HR, 0, ref hMemory);
                        }
                        Locked = false;
                        MainWindow.updIO.iCommonInputs1Port = PlcInternal[0];
                        MainWindow.updIO.oCommonOutputsPort = PlcInternal[1];

                        MainWindow.updIO.iPos1Port = PlcControl[27];
                        MainWindow.updIO.iPos2Port = PlcControl[28];
                        MainWindow.updIO.iPos3Port = PlcControl[29];
                        MainWindow.updIO.oPos1Port = PlcControl[30];
                        MainWindow.updIO.oPos2Port = PlcControl[31];
                        MainWindow.updIO.oPos3Port = PlcControl[32];

                        MainWindow.updIO.oFestoOutputs1Port = PlcOutputs[6];
                        MainWindow.updIO.oFestoOutputs2Port = PlcOutputs[7];
                        MainWindow.updIO.FEI_Inputs1Port = PlcInputs[6];
                        MainWindow.updIO.FEI_Inputs2Port = PlcInputs[7];
                        MainWindow.updIO.FEI_CurrentPosition = PlcInputs[8] + (PlcInputs[9] << 16);

                        MainWindow.updIO.LMO_ControlPort = PlcOutputs[15];
                        MainWindow.updIO.LMO_CmdHeader = PlcOutputs[16];
                        MainWindow.updIO.LMI_Status = PlcInputs[15];
                        MainWindow.updIO.LMI_StateVar = PlcInputs[16];
                        MainWindow.updIO.LMI_WarningCode = PlcInputs[17];
                        MainWindow.updIO.LMI_CurrentPosition = PlcInputs[18] + (PlcInputs[19] << 16);

                        MainWindow.updIO.WLO_Control = PlcOutputs[10];
                        MainWindow.updIO.WLO_LaserProgram = PlcOutputs[11];
                        MainWindow.updIO.WLI_Status = PlcInputs[10];

                        MainWindow.updIO.H_Flags = PlcControl[33];

                        MainWindow.updIO.C100_Flags = PlcControl[0];
                        MainWindow.updIO.C109_Flags = PlcControl[9];

                        MainWindow.updIO.StateL = PlcControl[1];
                        MainWindow.updIO.StateF = PlcControl[2];
                        MainWindow.updIO.StateR = PlcControl[3];
                        MainWindow.updIO.StateW = PlcControl[4];
                        MainWindow.updIO.StateS = PlcControl[5];
                        MainWindow.updIO.StateM = PlcControl[6];
                        MainWindow.updIO.StateT = PlcControl[7];
                        MainWindow.updIO.StateI = PlcControl[17];

                        MainWindow.updIO.ErrorI = PlcControl[12];
                        MainWindow.updIO.ErrorL = PlcControl[13];
                        MainWindow.updIO.ErrorW = PlcControl[14];
                        MainWindow.updIO.ErrorM = PlcControl[15];

                        if ((PlcOutputs[15] & (ushort)0xFF00) == 0x0400) MainWindow.updIO.ErrorT = (ushort)(PlcOutputs[15] & 0x00FF);

                        MainWindow.updIO.Prompts = PlcControl[11];
                    }
                }

                Thread.Sleep(5);
            }
        }

        private void Exit()
        {
            plc.Close();
        }
    }
}
