using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Media;

namespace Machine
{
    public class UpdateIO : INotifyPropertyChanged
    {
        public static ObservableCollection<IObitDetails> PlcCommonOutputs = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcPos1Outputs = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcPos2Outputs = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcPos3Outputs = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcCommonInputs1 = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcCommonInputs2 = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcPos1Inputs = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcPos2Inputs = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcPos3Inputs = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcFestoOutputs = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcFestoInputs = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcLinMotOutputs = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcLinMotInputs = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcWeldLasOutputs = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcWeldLasInputs = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcMarkLasInputs = new ObservableCollection<IObitDetails>();
        public static ObservableCollection<IObitDetails> PlcHflags = new ObservableCollection<IObitDetails>();

        public const byte ibitHF_UseWelding = 0;
        public const byte ibitHF_UseMarking = 1;
        public const byte ibitHF_in_welding = 4;
        public const byte ibitHF_in_marking = 5;
        public const byte ibitHF_traceability = 6;
        public const byte ibitHF_load_done= 7;
        public const byte ibitHF_weld_done = 8;
        public const byte ibitHF_mark_done = 9;
        public const byte ibitHF_UseStation1 = 13;
        public const byte ibitHF_UseStation2 = 14;
        public const byte ibitHF_UseStation3 = 15;

        private const byte ibitCommonPowerOK = 0;
        private const byte ibitCommonPedal = 1;
        private const byte ibitCommonAirOK = 2;
        private const byte ibitCommonArgonOK = 3;
        public const byte ibitCommonStation1 = 4;
        public const byte ibitCommonStation2 = 5;
        public const byte ibitCommonStation3 = 6;
        private const byte ibitCommonPusherInHome = 7;
        private const byte ibitCommonArgonNozzleOut = 8;
        private const byte ibitCommonArgonNozzleIn = 9;
        private const byte ibitCommonRotTableReady = 10;
        private const byte ibitCommonRotTableInPos = 11;
        private const byte ibitCommonRotTableMotorOK = 12;
        private const byte ibitCommonPushCylDown = 13;

        private const byte ibitSt1NutOK = 0;
        private const byte ibitSt1BucketOp = 1;
        private const byte ibitSt1BucketCl = 2;
        private const byte ibitSt2NutOK = 3;
        private const byte ibitSt2BucketOp = 4;
        private const byte ibitSt2BucketCl = 5;
        private const byte ibitSt3NutOK = 6;
        private const byte ibitSt3BucketOp = 7;
        private const byte ibitSt3BucketCl = 8;

        private const byte ibitStAlignerOp = 0;
        private const byte ibitStAlignerCl = 1;
        private const byte ibitStSupportRollsCl = 2;
        private const byte ibitStSensorPresent = 3;
        private const byte ibitStClampGripperOp = 4;
        private const byte ibitStClampGripperCl = 5;
        private const byte ibitStFaulhaberStatus = 6;
        private const byte ibitStFaulhaberHome = 7;
        private const byte ibitStBucketOp = 8;
        private const byte ibitStBucketCl = 9;
        private const byte ibitStNutOK = 10;

        private const byte obitCommonOpLampRed = 0;
        private const byte obitCommonOpLampGreen = 1;
        private const byte obitCommonMuteA = 2;
        private const byte obitCommonMuteB = 3;
        private const byte obitCommonAirOn = 4;
        private const byte obitCommonGreen = 5;
        private const byte obitCommonYellow = 6;
        private const byte obitCommonRed = 7;
        private const byte obitCommonBuzzer = 8;
        private const byte obitCommonBrakeOff = 9;
        private const byte obitCommonRotTableErrorReset = 10;
        private const byte obitCommonRotTableStart = 11;
        private const byte obitCommonMoveArgonIn = 12;
        private const byte obitCommonArgonOn = 13;
        private const byte obitCommonCameraLightOn = 14;
        private const byte obitCommonLaserStationVacuum = 15;

        private const byte оbitStOpenSupportRolls = 0;
        private const byte obitStOpenAligner = 1;
        private const byte obitStClampGripper = 3;
        private const byte obitStOnBlow = 4;
        private const byte obitStOpenBucket = 5;
        private const byte obitStStartFaulhaber = 6;

        private const byte obitFEO_EnableDrive = 0;
        private const byte obitFEO_EnableOperation = 1;
        private const byte obitFEO_ReleaseBrake = 2;
        private const byte obitFEO_ResetFault = 3;
        private const byte obitFEO_OpMode1 = 6;
        private const byte obitFEO_OpMode2 = 7;
        private const byte obitFEO_ReleaseHalt = 8;
        private const byte obitFEO_StartPositioning = 9;
        private const byte obitFEO_StartHoming = 10;
        private const byte obitFEO_JogPositive = 11;
        private const byte obitFEO_JogNegative = 12;
        private const byte obitFEO_ClearRemaining = 14;
        private const byte obitFEO_Relative = 0;

        private const byte ibitFEI_DriveEnabled = 0;
        private const byte ibitFEI_OperationEnabled = 1;
        private const byte ibitFEI_Warning = 2;
        private const byte ibitFEI_Fault = 3;
        private const byte ibitFEI_VoltageApplied = 4;
        private const byte ibitFEI_OpMode1 = 6;
        private const byte ibitFEI_OpMode2 = 7;
        private const byte ibitFEI_HaltNotActive = 8;
        private const byte ibitFEI_AckStart = 9;
        private const byte ibitFEI_MotionComplete = 10;
        private const byte ibitFEI_Moving = 12;
        private const byte ibitFEI_FollowingError = 13;
        private const byte ibitFEI_AxisReferenced = 15;
        private const byte ibitFEI_MotionIsRelative = 0;

        private const byte obitLMO_SwitchOn = 0;
        private const byte obitLMO_VoltageEnable = 1;
        private const byte obitLMO_QuickStop = 2;
        private const byte obitLMO_EnableOperation = 3;
        private const byte obitLMO_Abort = 4;
        private const byte obitLMO_Freeze = 5;
        private const byte obitLMO_GoToPosition = 6;
        private const byte obitLMO_ResetError = 7;
        private const byte obitLMO_JogPlus = 8;
        private const byte obitLMO_JogMinus = 9;
        private const byte obitLMO_SpecialMode = 10;
        private const byte obitLMO_Home = 11;
        private const byte obitLMO_ClearanceCheck = 12;
        private const byte obitLMO_GoToInitial = 13;
        private const byte obitLMO_PhaseSearch = 15;

        private const byte ibitLMI_Enabled = 0;
        private const byte ibitLMI_SwitchedOn = 1;
        private const byte ibitLMI_OperationEnabled = 2;
        private const byte ibitLMI_Error = 3;
        private const byte ibitLMI_VoltageEnabled = 4;
        private const byte ibitLMI_QuickStopped = 5;
        private const byte ibitLMI_SwitchOnLocked = 6;
        private const byte ibitLMI_Warning = 7;
        private const byte ibitLMI_EventHandlerActive = 8;
        private const byte ibitLMI_SpecialMotionActive = 9;
        private const byte ibitLMI_InTargetPosition = 10;
        private const byte ibitLMI_Homed = 11;
        private const byte ibitLMI_FatalError = 12;
        private const byte ibitLMI_MotionActive = 13;
        private const byte ibitLMI_RangeIndicator1 = 14;
        private const byte ibitLMI_RangeIndicator2 = 15;

        private const byte obitWLO_Request = 0;
        private const byte obitWLO_Reset = 8;
        private const byte obitWLO_StartDyn = 10;
        private const byte obitWLO_StandBy = 12;
        private const byte obitWLO_On = 13;
        private const byte obitWLO_ExtAct = 15;

        private const byte ibitWLI_Assigned = 3;
        private const byte ibitWLI_Fault = 4;
        private const byte ibitWLI_PrgCompleted = 11;
        private const byte ibitWLI_ProgActive = 12;
        private const byte ibitWLI_Ready = 13;
        private const byte ibitWLI_On = 14;
        private const byte ibitWLI_ExtAct = 15;

        private const byte ibitMLI_BeamSourceOn = 0;
        private const byte ibitMLI_LaserReady = 1;
        private const byte ibitMLI_SafetyClosed = 2;
        private const byte ibitMLI_ShutterOpen = 3;
        private const byte ibitMLI_BeamExpInPos = 4;
        private const byte ibitMLI_LaserStable = 5;
        private const byte ibitMLI_LaserMonitoring = 6;
        private const byte ibitMLI_Malfunction = 7;
        private const byte ibitMLI_PilotLaserOn = 8;
        private const byte ibitMLI_WarningLampOn = 9;
        private const byte ibitMLI_QswitchOn = 10;
        private const byte ibitMLI_AutoDeactivation = 11;
        private const byte ibitMLI_KeySwitchLock = 12;
        private const byte ibitMLI_EmgrCircClosed = 13;
        private const byte ibitMLI_HeatingPhaseOn = 14;
        private const byte ibitMLI_ControlIsOn = 15;

        public static readonly object onLock = new object();

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
        protected bool SetFieldEx<T>(ObservableCollection<IObitDetails> oc, string bg, string bn, ref T field, T value,
        [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            var o = oc.FirstOrDefault(x => (x.BitGroup == bg) & (x.BitText == bn));
            if (o != null && typeof(T) == typeof(bool)) o.BitValue = Convert.ToBoolean(value);
            return true;
        }

        //For Common Inputs 1
        private ushort _iCommonInputs1Port;
        private bool _iCommonPowerOK;
        private bool _iCommonPedal;
        private bool _iCommonAirOK;
        private bool _iCommonArgonOK;
        private bool _iCommonStation1;
        private bool _iCommonStation2;
        private bool _iCommonStation3;
        private bool _iCommonPusherInHome;
        private bool _iCommonArgonNozzleOut;
        private bool _iCommonArgonNozzleIn;
        private bool _iCommonRotTableReady;
        private bool _iCommonRotTableInPos;
        private bool _iCommonRotTableMotorOK;
        private bool _iCommonPushCylDown;

        public ushort iCommonInputs1Port
        {
            get { return _iCommonInputs1Port; }
            set
            {
                SetField(ref _iCommonInputs1Port, value);
                iCommonPowerOK = (iCommonInputs1Port & (1 << ibitCommonPowerOK)) != 0;
                iCommonPedal = (iCommonInputs1Port & (1 << ibitCommonPedal)) != 0;
                iCommonAirOK = (iCommonInputs1Port & (1 << ibitCommonAirOK)) != 0;
                iCommonArgonOK = (iCommonInputs1Port & (1 << ibitCommonArgonOK)) != 0;
                iCommonStation1 = (iCommonInputs1Port & (1 << ibitCommonStation1)) != 0;
                iCommonStation2 = (iCommonInputs1Port & (1 << ibitCommonStation2)) != 0;
                iCommonStation3 = (iCommonInputs1Port & (1 << ibitCommonStation3)) != 0;
                iCommonPusherInHome = (iCommonInputs1Port & (1 << ibitCommonPusherInHome)) != 0;
                iCommonArgonNozzleOut = (iCommonInputs1Port & (1 << ibitCommonArgonNozzleOut)) != 0;
                iCommonArgonNozzleIn = (iCommonInputs1Port & (1 << ibitCommonArgonNozzleIn)) != 0;
                iCommonRotTableReady = (iCommonInputs1Port & (1 << ibitCommonRotTableReady)) != 0;
                iCommonRotTableInPos = (iCommonInputs1Port & (1 << ibitCommonRotTableInPos)) != 0;
                iCommonRotTableMotorOK = (iCommonInputs1Port & (1 << ibitCommonRotTableMotorOK)) != 0;
                iCommonPushCylDown = (iCommonInputs1Port & (1 << ibitCommonPushCylDown)) != 0;
            }
        }
        public bool iCommonPowerOK
        {
            get { return _iCommonPowerOK; }
            set { SetFieldEx(PlcCommonInputs1, "CI1", "Power OK", ref _iCommonPowerOK, value); }
        }
        public bool iCommonPedal
        {
            get { return _iCommonPedal; }
            set { SetFieldEx(PlcCommonInputs1, "CI1", "Pedal", ref _iCommonPedal, value); }
        }
        public bool iCommonAirOK
        {
            get { return _iCommonAirOK; }
            set { SetFieldEx(PlcCommonInputs1, "CI1", "Air OK", ref _iCommonAirOK, value); }
        }
        public bool iCommonArgonOK
        {
            get { return _iCommonArgonOK; }
            set { SetFieldEx(PlcCommonInputs1, "CI1", "Argon OK", ref _iCommonArgonOK, value); }
        }
        public bool iCommonStation1
        {
            get { return _iCommonStation1; }
            set { SetFieldEx(PlcCommonInputs1, "CI1", "Station 1", ref _iCommonStation1, value); }
        }
        public bool iCommonStation2
        {
            get { return _iCommonStation2; }
            set { SetFieldEx(PlcCommonInputs1, "CI1", "Station 2", ref _iCommonStation2, value); }
        }
        public bool iCommonStation3
        {
            get { return _iCommonStation3; }
            set { SetFieldEx(PlcCommonInputs1, "CI1", "Station 3", ref _iCommonStation3, value); }
        }
        public bool iCommonPusherInHome
        {
            get { return _iCommonPusherInHome; }
            set { SetFieldEx(PlcCommonInputs1, "CI1", "Pusher Home", ref _iCommonPusherInHome, value); }
        }
        public bool iCommonArgonNozzleOut
        {
            get { return _iCommonArgonNozzleOut; }
            set { SetFieldEx(PlcCommonInputs1, "CI1", "Argon out", ref _iCommonArgonNozzleOut, value); }
        }
        public bool iCommonArgonNozzleIn
        {
            get { return _iCommonArgonNozzleIn; }
            set { SetFieldEx(PlcCommonInputs1, "CI1", "Argon in", ref _iCommonArgonNozzleIn, value); }
        }
        public bool iCommonRotTableReady
        {
            get { return _iCommonRotTableReady; }
            set { SetFieldEx(PlcCommonInputs1, "CI1", "RT ready", ref _iCommonRotTableReady, value); }
        }
        public bool iCommonRotTableInPos
        {
            get { return _iCommonRotTableInPos; }
            set { SetFieldEx(PlcCommonInputs1, "CI1", "RT in pos.", ref _iCommonRotTableInPos, value); }
        }
        public bool iCommonRotTableMotorOK
        {
            get { return _iCommonRotTableMotorOK; }
            set { SetFieldEx(PlcCommonInputs1, "CI1", "RT motor OK", ref _iCommonRotTableMotorOK, value); }
        }
        public bool iCommonPushCylDown
        {
            get { return _iCommonPushCylDown; }
            set { SetFieldEx(PlcCommonInputs1, "CI1", "Pusher Dn", ref _iCommonPushCylDown, value); }
        }

        //For pos. 1 inputs
        private ushort _iPos1Port;
        private bool _iPos1AlignerOp;
        private bool _iPos1AlignerCl;
        private bool _iPos1SupportRollsCl;
        private bool _iPos1SensorPresent;
        private bool _iPos1ClampGripperOp;
        private bool _iPos1ClampGripperCl;
        private bool _iPos1FaulhaberStatus;
        private bool _iPos1FaulhaberHome;
        private bool _iPos1BucketOp;
        private bool _iPos1BucketCl;
        private bool _iPos1NutOK;

        public ushort iPos1Port
        {
            get { return _iPos1Port; }
            set
            {
                SetField(ref _iPos1Port, value);
                iPos1AlignerOp = (iPos1Port & (1 << ibitStAlignerOp)) != 0;
                iPos1AlignerCl = (iPos1Port & (1 << ibitStAlignerCl)) != 0;
                iPos1SupportRollsCl = (iPos1Port & (1 << ibitStSupportRollsCl)) != 0;
                iPos1SensorPresent = (iPos1Port & (1 << ibitStSensorPresent)) != 0;
                iPos1ClampGripperOp = (iPos1Port & (1 << ibitStClampGripperOp)) != 0;
                iPos1ClampGripperCl = (iPos1Port & (1 << ibitStClampGripperCl)) != 0;
                iPos1FaulhaberStatus = (iPos1Port & (1 << ibitStFaulhaberStatus)) != 0;
                iPos1FaulhaberHome = (iPos1Port & (1 << ibitStFaulhaberHome)) != 0;
                iPos1BucketOp = (iPos1Port & (1 << ibitStBucketOp)) != 0;
                iPos1BucketCl = (iPos1Port & (1 << ibitStBucketCl)) != 0;
                iPos1NutOK = (iPos1Port & (1 << ibitStNutOK)) != 0;
            }
        }
        public bool iPos1AlignerOp
        {
            get { return _iPos1AlignerOp; }
            set { SetFieldEx(PlcPos1Inputs, "iP1", "Aligner Op", ref _iPos1AlignerOp, value); }
        }
        public bool iPos1AlignerCl
        {
            get { return _iPos1AlignerCl; }
            set { SetFieldEx(PlcPos1Inputs, "iP1", "Aligner Cl", ref _iPos1AlignerCl, value); }
        }
        public bool iPos1SupportRollsCl
        {
            get { return _iPos1SupportRollsCl; }
            set { SetFieldEx(PlcPos1Inputs, "iP1", "Rolls Cl", ref _iPos1SupportRollsCl, value); }
        }
        public bool iPos1SensorPresent
        {
            get { return _iPos1SensorPresent; }
            set { SetFieldEx(PlcPos1Inputs, "iP1", "Present", ref _iPos1SensorPresent, value); }
        }
        public bool iPos1ClampGripperOp
        {
            get { return _iPos1ClampGripperOp; }
            set { SetFieldEx(PlcPos1Inputs, "iP1", "Gripper Op", ref _iPos1ClampGripperOp, value); }
        }
        public bool iPos1ClampGripperCl
        {
            get { return _iPos1ClampGripperCl; }
            set { SetFieldEx(PlcPos1Inputs, "iP1", "Gripper Cl", ref _iPos1ClampGripperCl, value); }
        }
        public bool iPos1FaulhaberStatus
        {
            get { return _iPos1FaulhaberStatus; }
            set { SetFieldEx(PlcPos1Inputs, "iP1", "FH Status", ref _iPos1FaulhaberStatus, value); }
        }
        public bool iPos1FaulhaberHome
        {
            get { return _iPos1FaulhaberHome; }
            set { SetFieldEx(PlcPos1Inputs, "iP1", "FH Home", ref _iPos1FaulhaberHome, value); }
        }
        public bool iPos1BucketOp
        {
            get { return _iPos1BucketOp; }
            set { SetFieldEx(PlcPos1Inputs, "iP1", "Bucket Op", ref _iPos1BucketOp, value); }
        }
        public bool iPos1BucketCl
        {
            get { return _iPos1BucketCl; }
            set { SetFieldEx(PlcPos1Inputs, "iP1", "Bucket Cl", ref _iPos1BucketCl, value); }
        }
        public bool iPos1NutOK
        {
            get { return _iPos1NutOK; }
            set { SetFieldEx(PlcPos1Inputs, "iP1", "Nut OK", ref _iPos1NutOK, value); }
        }

        //For pos. 2 inputs
        private ushort _iPos2Port;
        private bool _iPos2AlignerOp;
        private bool _iPos2AlignerCl;
        private bool _iPos2SupportRollsCl;
        private bool _iPos2SensorPresent;
        private bool _iPos2ClampGripperOp;
        private bool _iPos2ClampGripperCl;
        private bool _iPos2FaulhaberStatus;
        private bool _iPos2FaulhaberHome;
        private bool _iPos2BucketOp;
        private bool _iPos2BucketCl;
        private bool _iPos2NutOK;

        public ushort iPos2Port
        {
            get { return _iPos2Port; }
            set
            {
                SetField(ref _iPos2Port, value);
                iPos2AlignerOp = (iPos2Port & (1 << ibitStAlignerOp)) != 0;
                iPos2AlignerCl = (iPos2Port & (1 << ibitStAlignerCl)) != 0;
                iPos2SupportRollsCl = (iPos2Port & (1 << ibitStSupportRollsCl)) != 0;
                iPos2SensorPresent = (iPos2Port & (1 << ibitStSensorPresent)) != 0;
                iPos2ClampGripperOp = (iPos2Port & (1 << ibitStClampGripperOp)) != 0;
                iPos2ClampGripperCl = (iPos2Port & (1 << ibitStClampGripperCl)) != 0;
                iPos2FaulhaberStatus = (iPos2Port & (1 << ibitStFaulhaberStatus)) != 0;
                iPos2FaulhaberHome = (iPos2Port & (1 << ibitStFaulhaberHome)) != 0;
                iPos2BucketOp = (iPos2Port & (1 << ibitStBucketOp)) != 0;
                iPos2BucketCl = (iPos2Port & (1 << ibitStBucketCl)) != 0;
                iPos2NutOK = (iPos2Port & (1 << ibitStNutOK)) != 0;
            }
        }
        public bool iPos2AlignerOp
        {
            get { return _iPos2AlignerOp; }
            set { SetFieldEx(PlcPos2Inputs, "iP2", "Aligner Op", ref _iPos2AlignerOp, value); }
        }
        public bool iPos2AlignerCl
        {
            get { return _iPos2AlignerCl; }
            set { SetFieldEx(PlcPos2Inputs, "iP2", "Aligner Cl", ref _iPos2AlignerCl, value); }
        }
        public bool iPos2SupportRollsCl
        {
            get { return _iPos2SupportRollsCl; }
            set { SetFieldEx(PlcPos2Inputs, "iP2", "Rolls Cl", ref _iPos2SupportRollsCl, value); }
        }
        public bool iPos2SensorPresent
        {
            get { return _iPos2SensorPresent; }
            set { SetFieldEx(PlcPos2Inputs, "iP2", "Present", ref _iPos2SensorPresent, value); }
        }
        public bool iPos2ClampGripperOp
        {
            get { return _iPos2ClampGripperOp; }
            set { SetFieldEx(PlcPos2Inputs, "iP2", "Gripper Op", ref _iPos2ClampGripperOp, value); }
        }
        public bool iPos2ClampGripperCl
        {
            get { return _iPos2ClampGripperCl; }
            set { SetFieldEx(PlcPos2Inputs, "iP2", "Gripper Cl", ref _iPos2ClampGripperCl, value); }
        }
        public bool iPos2FaulhaberStatus
        {
            get { return _iPos2FaulhaberStatus; }
            set { SetFieldEx(PlcPos2Inputs, "iP2", "FH Status", ref _iPos2FaulhaberStatus, value); }
        }
        public bool iPos2FaulhaberHome
        {
            get { return _iPos2FaulhaberHome; }
            set { SetFieldEx(PlcPos2Inputs, "iP2", "FH Home", ref _iPos2FaulhaberHome, value); }
        }
        public bool iPos2BucketOp
        {
            get { return _iPos2BucketOp; }
            set { SetFieldEx(PlcPos2Inputs, "iP2", "Bucket Op", ref _iPos2BucketOp, value); }
        }
        public bool iPos2BucketCl
        {
            get { return _iPos2BucketCl; }
            set { SetFieldEx(PlcPos2Inputs, "iP2", "Bucket Cl", ref _iPos2BucketCl, value); }
        }
        public bool iPos2NutOK
        {
            get { return _iPos2NutOK; }
            set { SetFieldEx(PlcPos2Inputs, "iP2", "Nut OK", ref _iPos2NutOK, value); }
        }

        //For pos. 3 inputs
        private ushort _iPos3Port;
        private bool _iPos3AlignerOp;
        private bool _iPos3AlignerCl;
        private bool _iPos3SupportRollsCl;
        private bool _iPos3SensorPresent;
        private bool _iPos3ClampGripperOp;
        private bool _iPos3ClampGripperCl;
        private bool _iPos3FaulhaberStatus;
        private bool _iPos3FaulhaberHome;
        private bool _iPos3BucketOp;
        private bool _iPos3BucketCl;
        private bool _iPos3NutOK;

        public ushort iPos3Port
        {
            get { return _iPos3Port; }
            set
            {
                SetField(ref _iPos3Port, value);
                iPos3AlignerOp = (iPos3Port & (1 << ibitStAlignerOp)) != 0;
                iPos3AlignerCl = (iPos3Port & (1 << ibitStAlignerCl)) != 0;
                iPos3SupportRollsCl = (iPos3Port & (1 << ibitStSupportRollsCl)) != 0;
                iPos3SensorPresent = (iPos3Port & (1 << ibitStSensorPresent)) != 0;
                iPos3ClampGripperOp = (iPos3Port & (1 << ibitStClampGripperOp)) != 0;
                iPos3ClampGripperCl = (iPos3Port & (1 << ibitStClampGripperCl)) != 0;
                iPos3FaulhaberStatus = (iPos3Port & (1 << ibitStFaulhaberStatus)) != 0;
                iPos3FaulhaberHome = (iPos3Port & (1 << ibitStFaulhaberHome)) != 0;
                iPos3BucketOp = (iPos3Port & (1 << ibitStBucketOp)) != 0;
                iPos3BucketCl = (iPos3Port & (1 << ibitStBucketCl)) != 0;
                iPos3NutOK = (iPos3Port & (1 << ibitStNutOK)) != 0;
            }
        }
        public bool iPos3AlignerOp
        {
            get { return _iPos3AlignerOp; }
            set { SetFieldEx(PlcPos3Inputs, "iP3", "Aligner Op", ref _iPos3AlignerOp, value); }
        }
        public bool iPos3AlignerCl
        {
            get { return _iPos3AlignerCl; }
            set { SetFieldEx(PlcPos3Inputs, "iP3", "Aligner Cl", ref _iPos3AlignerCl, value); }
        }
        public bool iPos3SupportRollsCl
        {
            get { return _iPos3SupportRollsCl; }
            set { SetFieldEx(PlcPos3Inputs, "iP3", "Rolls Cl", ref _iPos3SupportRollsCl, value); }
        }
        public bool iPos3SensorPresent
        {
            get { return _iPos3SensorPresent; }
            set { SetFieldEx(PlcPos3Inputs, "iP3", "Present", ref _iPos3SensorPresent, value); }
        }
        public bool iPos3ClampGripperOp
        {
            get { return _iPos3ClampGripperOp; }
            set { SetFieldEx(PlcPos3Inputs, "iP3", "Gripper Op", ref _iPos3ClampGripperOp, value); }
        }
        public bool iPos3ClampGripperCl
        {
            get { return _iPos3ClampGripperCl; }
            set { SetFieldEx(PlcPos3Inputs, "iP3", "Gripper Cl", ref _iPos3ClampGripperCl, value); }
        }
        public bool iPos3FaulhaberStatus
        {
            get { return _iPos3FaulhaberStatus; }
            set { SetFieldEx(PlcPos3Inputs, "iP3", "FH Status", ref _iPos3FaulhaberStatus, value); }
        }
        public bool iPos3FaulhaberHome
        {
            get { return _iPos3FaulhaberHome; }
            set { SetFieldEx(PlcPos3Inputs, "iP3", "FH Home", ref _iPos3FaulhaberHome, value); }
        }
        public bool iPos3BucketOp
        {
            get { return _iPos3BucketOp; }
            set { SetFieldEx(PlcPos3Inputs, "iP3", "Bucket Op", ref _iPos3BucketOp, value); }
        }
        public bool iPos3BucketCl
        {
            get { return _iPos3BucketCl; }
            set { SetFieldEx(PlcPos3Inputs, "iP3", "Bucket Cl", ref _iPos3BucketCl, value); }
        }
        public bool iPos3NutOK
        {
            get { return _iPos3NutOK; }
            set { SetFieldEx(PlcPos3Inputs, "iP3", "Nut OK", ref _iPos3NutOK, value); }
        }

        //For common outputs
        private ushort _oCommonOutputsPort;
        private bool _oCommonOpLampRed;
        private bool _oCommonOpLampGreen;
        private bool _oCommonMuteA;
        private bool _oCommonMuteB;
        private bool _oCommonAirOn;
        private bool _oCommonGreen;
        private bool _oCommonYellow;
        private bool _oCommonRed;
        private bool _oCommonBuzzer;
        private bool _oCommonBrakeOff;
        private bool _oCommonRotTableErrorReset;
        private bool _oCommonRotTableStart;
        private bool _oCommonMoveArgonIn;
        private bool _oCommonArgonOn;
        private bool _oCommonCameraLightOn;
        private bool _oCommonLaserStationVacuum;

        public ushort oCommonOutputsPort
        {
            get { return _oCommonOutputsPort; }
            set
            {
                SetField(ref _oCommonOutputsPort, value);
                oCommonOpLampRed = (oCommonOutputsPort & (1 << obitCommonOpLampRed)) != 0;
                oCommonOpLampGreen = (oCommonOutputsPort & (1 << obitCommonOpLampGreen)) != 0;
                oCommonMuteA = (oCommonOutputsPort & (1 << obitCommonMuteA)) != 0;
                oCommonMuteB = (oCommonOutputsPort & (1 << obitCommonMuteB)) != 0;
                oCommonAirOn = (oCommonOutputsPort & (1 << obitCommonAirOn)) != 0;
                oCommonGreen = (oCommonOutputsPort & (1 << obitCommonGreen)) != 0;
                oCommonYellow = (oCommonOutputsPort & (1 << obitCommonYellow)) != 0;
                oCommonRed = (oCommonOutputsPort & (1 << obitCommonRed)) != 0;
                oCommonBuzzer = (oCommonOutputsPort & (1 << obitCommonBuzzer)) != 0;
                oCommonBrakeOff = (oCommonOutputsPort & (1 << obitCommonBrakeOff)) != 0;
                oCommonRotTableErrorReset = (oCommonOutputsPort & (1 << obitCommonRotTableErrorReset)) != 0;
                oCommonRotTableStart = (oCommonOutputsPort & (1 << obitCommonRotTableStart)) != 0;
                oCommonMoveArgonIn = (oCommonOutputsPort & (1 << obitCommonMoveArgonIn)) != 0;
                oCommonArgonOn = (oCommonOutputsPort & (1 << obitCommonArgonOn)) != 0;
                oCommonCameraLightOn = (oCommonOutputsPort & (1 << obitCommonCameraLightOn)) != 0;
                oCommonLaserStationVacuum = (oCommonOutputsPort & (1 << obitCommonLaserStationVacuum)) != 0;
            }
        }

        public bool oCommonOpLampRed
        {
            get { return _oCommonOpLampRed; }
            private set
            {
                SetFieldEx(PlcCommonOutputs, "CO", "Op. Red", ref _oCommonOpLampRed, value);
                if (_oCommonOpLampRed != value)
                {
                    if (value) oCommonOutputsPort |= (ushort)(1 << obitCommonOpLampRed);
                    else oCommonOutputsPort &= (0xFFFF) ^ (ushort)(1 << obitCommonOpLampRed);
                }
            }
        }
        public bool oCommonOpLampGreen
        {
            get { return _oCommonOpLampGreen; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "Op. Green", ref _oCommonOpLampGreen, value); }
        }
        public bool oCommonMuteA
        {
            get { return _oCommonMuteA; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "MuteA", ref _oCommonMuteA, value); }
        }
        public bool oCommonMuteB
        {
            get { return _oCommonMuteB; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "MuteB", ref _oCommonMuteB, value); }
        }
        public bool oCommonAirOn
        {
            get { return _oCommonAirOn; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "OnAir", ref _oCommonAirOn, value); }
        }
        public bool oCommonGreen
        {
            get { return _oCommonGreen; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "Green", ref _oCommonGreen, value); }
        }
        public bool oCommonYellow
        {
            get { return _oCommonYellow; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "Yellow", ref _oCommonYellow, value); }
        }
        public bool oCommonRed
        {
            get { return _oCommonRed; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "Red", ref _oCommonRed, value); }
        }
        public bool oCommonBuzzer
        {
            get { return _oCommonBuzzer; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "Buzzer", ref _oCommonBuzzer, value); }
        }
        public bool oCommonBrakeOff
        {
            get { return _oCommonBrakeOff; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "Off Brake", ref _oCommonBrakeOff, value); }
        }
        public bool oCommonRotTableErrorReset
        {
            get { return _oCommonRotTableErrorReset; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "RT Reset", ref _oCommonRotTableErrorReset, value); }
        }
        public bool oCommonRotTableStart
        {
            get { return _oCommonRotTableStart; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "RT Start", ref _oCommonRotTableStart, value); }
        }
        public bool oCommonMoveArgonIn
        {
            get { return _oCommonMoveArgonIn; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "In Argon", ref _oCommonMoveArgonIn, value); }
        }
        public bool oCommonArgonOn
        {
            get { return _oCommonArgonOn; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "On Argon", ref _oCommonArgonOn, value); }
        }
        public bool oCommonCameraLightOn
        {
            get { return _oCommonCameraLightOn; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "On Camera Light", ref _oCommonCameraLightOn, value); }
        }
        public bool oCommonLaserStationVacuum
        {
            get { return _oCommonLaserStationVacuum; }
            private set { SetFieldEx(PlcCommonOutputs, "CO", "On Vacuum", ref _oCommonLaserStationVacuum, value); }
        }

        //For pos. 1 outputs
        private ushort _oPos1Port;
        private bool _оPos1OpenSupportRolls;
        private bool _oPos1OpenAligner;
        private bool _oPos1ClampGripper;
        private bool _oPos1OnBlow;
        private bool _oPos1OpenBucket;
        private bool _oPos1StartFaulhaber;

        public ushort oPos1Port
        {
            get { return _oPos1Port; }
            set
            {
                SetField(ref _oPos1Port, value);
                oPos1OpenSupportRolls = (oPos1Port & (1 << оbitStOpenSupportRolls)) != 0;
                oPos1OpenAligner = (oPos1Port & (1 << obitStOpenAligner)) != 0;
                oPos1ClampGripper = (oPos1Port & (1 << obitStClampGripper)) != 0;
                oPos1OnBlow = (oPos1Port & (1 << obitStOnBlow)) != 0;
                oPos1OpenBucket = (oPos1Port & (1 << obitStOpenBucket)) != 0;
                oPos1StartFaulhaber = (oPos1Port & (1 << obitStStartFaulhaber)) != 0;
            }
        }
        public bool oPos1OpenSupportRolls
        {
            get { return _оPos1OpenSupportRolls; }
            private set { SetFieldEx(PlcPos1Outputs, "oP1", "Op Rolls", ref _оPos1OpenSupportRolls, value); }
        }
        public bool oPos1OpenAligner
        {
            get { return _oPos1OpenAligner; }
            private set { SetFieldEx(PlcPos1Outputs, "oP1", "Op Aligner", ref _oPos1OpenAligner, value); }
        }
        public bool oPos1ClampGripper
        {
            get { return _oPos1ClampGripper; }
            private set { SetFieldEx(PlcPos1Outputs, "oP1", "Cl Gripper", ref _oPos1ClampGripper, value); }
        }
        public bool oPos1OnBlow
        {
            get { return _oPos1OnBlow; }
            private set { SetFieldEx(PlcPos1Outputs, "oP1", "On Blow", ref _oPos1OnBlow, value); }
        }
        public bool oPos1OpenBucket
        {
            get { return _oPos1OpenBucket; }
            private set { SetFieldEx(PlcPos1Outputs, "oP1", "Op Bucket", ref _oPos1OpenBucket, value); }
        }
        public bool oPos1StartFaulhaber
        {
            get { return _oPos1StartFaulhaber; }
            private set { SetFieldEx(PlcPos1Outputs, "oP1", "FH Home", ref _oPos1StartFaulhaber, value); }
        }

        //For pos.2 outputs
        private ushort _oPos2Port;
        private bool _oPos2OpenSupportRolls;
        private bool _oPos2OpenAligner;
        private bool _oPos2ClampGripper;
        private bool _oPos2OnBlow;
        private bool _oPos2OpenBucket;
        private bool _oPos2StartFaulhaber;

        public ushort oPos2Port
        {
            get { return _oPos2Port; }
            set
            {
                SetField(ref _oPos2Port, value);
                oPos2OpenSupportRolls = (oPos2Port & (1 << оbitStOpenSupportRolls)) != 0;
                oPos2OpenAligner = (oPos2Port & (1 << obitStOpenAligner)) != 0;
                oPos2ClampGripper = (oPos2Port & (1 << obitStClampGripper)) != 0;
                oPos2OnBlow = (oPos2Port & (1 << obitStOnBlow)) != 0;
                oPos2OpenBucket = (oPos2Port & (1 << obitStOpenBucket)) != 0;
                oPos2StartFaulhaber = (oPos2Port & (1 << obitStStartFaulhaber)) != 0;
            }
        }
        public bool oPos2OpenSupportRolls
        {
            get { return _oPos2OpenSupportRolls; }
            private set { SetFieldEx(PlcPos2Outputs, "oP2", "Op Rolls", ref _oPos2OpenSupportRolls, value); }
        }
        public bool oPos2OpenAligner
        {
            get { return _oPos2OpenAligner; }
            private set { SetFieldEx(PlcPos2Outputs, "oP2", "Op Aligner", ref _oPos2OpenAligner, value); }
        }
        public bool oPos2ClampGripper
        {
            get { return _oPos2ClampGripper; }
            private set { SetFieldEx(PlcPos2Outputs, "oP2", "Cl Gripper", ref _oPos2ClampGripper, value); }
        }
        public bool oPos2OnBlow
        {
            get { return _oPos2OnBlow; }
            private set { SetFieldEx(PlcPos2Outputs, "oP2", "On Blow", ref _oPos2OnBlow, value); }
        }
        public bool oPos2OpenBucket
        {
            get { return _oPos2OpenBucket; }
            private set { SetFieldEx(PlcPos2Outputs, "oP2", "Op Bucket", ref _oPos2OpenBucket, value); }
        }
        public bool oPos2StartFaulhaber
        {
            get { return _oPos2StartFaulhaber; }
            private set { SetFieldEx(PlcPos2Outputs, "oP2", "FH Fast", ref _oPos2StartFaulhaber, value); }
        }

        //For pos.3 outputs
        private ushort _oPos3Port;
        private bool _oPos3OpenSupportRolls;
        private bool _oPos3OpenAligner;
        private bool _oPos3ClampGripper;
        private bool _oPos3OnBlow;
        private bool _oPos3OpenBucket;
        private bool _oPos3StartFaulhaber;

        public ushort oPos3Port
        {
            get { return _oPos3Port; }
            set
            {
                SetField(ref _oPos3Port, value);
                oPos3OpenSupportRolls = (oPos3Port & (1 << оbitStOpenSupportRolls)) != 0;
                oPos3OpenAligner = (oPos3Port & (1 << obitStOpenAligner)) != 0;
                oPos3ClampGripper = (oPos3Port & (1 << obitStClampGripper)) != 0;
                oPos3OnBlow = (oPos3Port & (1 << obitStOnBlow)) != 0;
                oPos3OpenBucket = (oPos3Port & (1 << obitStOpenBucket)) != 0;
                oPos3StartFaulhaber = (oPos3Port & (1 << obitStStartFaulhaber)) != 0;
            }
        }
        public bool oPos3OpenSupportRolls
        {
            get { return _oPos3OpenSupportRolls; }
            private set { SetFieldEx(PlcPos3Outputs, "oP3", "Op Rolls", ref _oPos3OpenSupportRolls, value); }
        }
        public bool oPos3OpenAligner
        {
            get { return _oPos3OpenAligner; }
            private set { SetFieldEx(PlcPos3Outputs, "oP3", "Op Aligner", ref _oPos3OpenAligner, value); }
        }
        public bool oPos3ClampGripper
        {
            get { return _oPos3ClampGripper; }
            private set { SetFieldEx(PlcPos3Outputs, "oP3", "Cl Gripper", ref _oPos3ClampGripper, value); }
        }
        public bool oPos3OnBlow
        {
            get { return _oPos3OnBlow; }
            private set { SetFieldEx(PlcPos3Outputs, "oP3", "On Blow", ref _oPos3OnBlow, value); }
        }
        public bool oPos3OpenBucket
        {
            get { return _oPos3OpenBucket; }
            private set { SetFieldEx(PlcPos3Outputs, "oP3", "Op Bucket", ref _oPos3OpenBucket, value); }
        }
        public bool oPos3StartFaulhaber
        {
            get { return _oPos3StartFaulhaber; }
            private set { SetFieldEx(PlcPos3Outputs, "oP3", "FH step", ref _oPos3StartFaulhaber, value); }
        }

        //Festo outputs 1 & 2

        private int _FEO_TargetPosition;
        public int FEO_TargetPosition
        {
            get { return _FEO_TargetPosition; }
            set
            {
                SetField(ref _FEO_TargetPosition, value);
            }
        }

        private ushort _oFestoOutputs1Port;
        private bool _FEO_EnableDrive;
        private bool _FEO_EnableOperation;
        private bool _FEO_ReleaseBrake;
        private bool _FEO_ResetFault;
        private bool _FEO_OpMode1;
        private bool _FEO_OpMode2;
        private bool _FEO_ReleaseHalt;
        private bool _FEO_StartPositioning;
        private bool _FEO_StartHoming;
        private bool _FEO_JogPositive;
        private bool _FEO_JogNegative;
        private bool _FEO_ClearRemaining;

        public ushort oFestoOutputs1Port
        {
            get { return _oFestoOutputs1Port; }
            set
            {
                SetField(ref _oFestoOutputs1Port, value);
                FEO_EnableDrive = (oFestoOutputs1Port & (1 << obitFEO_EnableDrive)) != 0;
                FEO_EnableOperation = (oFestoOutputs1Port & (1 << obitFEO_EnableOperation)) != 0;
                FEO_ReleaseBrake = (oFestoOutputs1Port & (1 << obitFEO_ReleaseBrake)) != 0;
                FEO_ResetFault = (oFestoOutputs1Port & (1 << obitFEO_ResetFault)) != 0;
                FEO_OpMode1 = (oFestoOutputs1Port & (1 << obitFEO_OpMode1)) != 0;
                FEO_OpMode2 = (oFestoOutputs1Port & (1 << obitFEO_OpMode2)) != 0;
                FEO_ReleaseHalt = (oFestoOutputs1Port & (1 << obitFEO_ReleaseHalt)) != 0;
                FEO_StartPositioning = (oFestoOutputs1Port & (1 << obitFEO_StartPositioning)) != 0;
                FEO_StartHoming = (oFestoOutputs1Port & (1 << obitFEO_StartHoming)) != 0;
                FEO_JogPositive = (oFestoOutputs1Port & (1 << obitFEO_JogPositive)) != 0;
                FEO_JogNegative = (oFestoOutputs1Port & (1 << obitFEO_JogNegative)) != 0;
                FEO_ClearRemaining = (oFestoOutputs1Port & (1 << obitFEO_ClearRemaining)) != 0;
            }
        }
        public bool FEO_EnableDrive
        {
            get { return _FEO_EnableDrive; }
            private set { SetFieldEx(PlcFestoOutputs, "oFE", "Enable drive", ref _FEO_EnableDrive, value); }
        }
        public bool FEO_EnableOperation
        {
            get { return _FEO_EnableOperation; }
            private set { SetFieldEx(PlcFestoOutputs, "oFE", "Enable operation", ref _FEO_EnableOperation, value); }
        }
        public bool FEO_ReleaseBrake
        {
            get { return _FEO_ReleaseBrake; }
            private set { SetFieldEx(PlcFestoOutputs, "oFE", "Release brake", ref _FEO_ReleaseBrake, value); }
        }
        public bool FEO_ResetFault
        {
            get { return _FEO_ResetFault; }
            private set { SetFieldEx(PlcFestoOutputs, "oFE", "Reset fault", ref _FEO_ResetFault, value); }
        }
        public bool FEO_OpMode1
        {
            get { return _FEO_OpMode1; }
            private set { SetFieldEx(PlcFestoOutputs, "oFE", "Operation mode 1", ref _FEO_OpMode1, value); }
        }
        public bool FEO_OpMode2
        {
            get { return _FEO_OpMode2; }
            private set { SetFieldEx(PlcFestoOutputs, "oFE", "Operation mode 2", ref _FEO_OpMode2, value); }
        }
        public bool FEO_ReleaseHalt
        {
            get { return _FEO_ReleaseHalt; }
            private set { SetFieldEx(PlcFestoOutputs, "oFE", "Release Halt", ref _FEO_ReleaseHalt, value); }
        }
        public bool FEO_StartPositioning
        {
            get { return _FEO_StartPositioning; }
            private set { SetFieldEx(PlcFestoOutputs, "oFE", "Start Positioning", ref _FEO_StartPositioning, value); }
        }
        public bool FEO_StartHoming
        {
            get { return _FEO_StartHoming; }
            private set { SetFieldEx(PlcFestoOutputs, "oFE", "Start Homing", ref _FEO_StartHoming, value); }
        }
        public bool FEO_JogPositive
        {
            get { return _FEO_JogPositive; }
            private set { SetFieldEx(PlcFestoOutputs, "oFE", "Jog Positive", ref _FEO_JogPositive, value); }
        }
        public bool FEO_JogNegative
        {
            get { return _FEO_JogNegative; }
            private set { SetFieldEx(PlcFestoOutputs, "oFE", "Jog Negative", ref _FEO_JogNegative, value); }
        }
        public bool FEO_ClearRemaining
        {
            get { return _FEO_ClearRemaining; }
            private set { SetFieldEx(PlcFestoOutputs, "oFE", "Clear Remaining", ref _FEO_ClearRemaining, value); }
        }

        private ushort _oFestoOutputs2Port;
        private bool _FEO_Relative;
        private ushort _FEO_TargetVelocity;

        public ushort oFestoOutputs2Port
        {
            get { return _oFestoOutputs2Port; }
            set
            {
                SetField(ref _oFestoOutputs2Port, value);
                FEO_Relative = (oFestoOutputs2Port & (1 << obitFEO_Relative)) != 0;
                FEO_TargetVelocity = (ushort)((oFestoOutputs2Port & 0xFF00) >> 8);
            }
        }
        public ushort FEO_TargetVelocity
        {
            get { return _FEO_TargetVelocity; }
            set { SetField(ref _FEO_TargetVelocity, value); }
        }
        public bool FEO_Relative
        {
            get { return _FEO_Relative; }
            private set { SetFieldEx(PlcFestoOutputs, "oFE", "Relative move", ref _FEO_Relative, value); }
        }

        //Fest inputs 1 & 2

        private ushort _FEI_CurrentVelocity;
        public ushort FEI_CurrentVelocity
        {
            get { return _FEI_CurrentVelocity; }
            set { SetField(ref _FEI_CurrentVelocity, (ushort)(value / 1000)); }
        }


        private float _FEI_CurrentPosition;
        public float FEI_CurrentPosition
        {
            get { return _FEI_CurrentPosition; }
            set { SetField(ref _FEI_CurrentPosition, value / 10000); }
        }

        private ushort _FEI_Inputs1Port;
        private bool _FEI_DriveEnabled;
        private bool _FEI_OperationEnabled;
        private bool _FEI_Warning;
        private bool _FEI_Fault;
        private bool _FEI_VoltageApplied;
        private bool _FEI_OpMode1;
        private bool _FEI_OpMode2;
        private bool _FEI_HaltNotActive;
        private bool _FEI_AckStart;
        private bool _FEI_MotionComplete;
        private bool _FEI_Moving;
        private bool _FEI_FollowingError;
        private bool _FEI_AxisReferenced;

        public ushort FEI_Inputs1Port
        {
            get { return _FEI_Inputs1Port; }
            set
            {
                SetField(ref _FEI_Inputs1Port, value);
                FEI_DriveEnabled = (FEI_Inputs1Port & (1 << ibitFEI_DriveEnabled)) != 0;
                FEI_OperationEnabled = (FEI_Inputs1Port & (1 << ibitFEI_OperationEnabled)) != 0;
                FEI_Warning = (FEI_Inputs1Port & (1 << ibitFEI_Warning)) != 0;
                FEI_Fault = (FEI_Inputs1Port & (1 << ibitFEI_Fault)) != 0;
                FEI_VoltageApplied = (FEI_Inputs1Port & (1 << ibitFEI_VoltageApplied)) != 0;
                FEI_OpMode1 = (FEI_Inputs1Port & (1 << ibitFEI_OpMode1)) != 0;
                FEI_OpMode2 = (FEI_Inputs1Port & (1 << ibitFEI_OpMode2)) != 0;
                FEI_HaltNotActive = (FEI_Inputs1Port & (1 << ibitFEI_HaltNotActive)) != 0;
                FEI_AckStart = (FEI_Inputs1Port & (1 << ibitFEI_AckStart)) != 0;
                FEI_MotionComplete = (FEI_Inputs1Port & (1 << ibitFEI_MotionComplete)) != 0;
                FEI_Moving = (FEI_Inputs1Port & (1 << ibitFEI_Moving)) != 0;
                FEI_FollowingError = (FEI_Inputs1Port & (1 << ibitFEI_FollowingError)) != 0;
                FEI_AxisReferenced = (FEI_Inputs1Port & (1 << ibitFEI_AxisReferenced)) != 0;
            }
        }
        public bool FEI_DriveEnabled
        {
            get { return _FEI_AxisReferenced; }
            set { SetFieldEx(PlcFestoInputs, "iFE", "Drive Enabled", ref _FEI_DriveEnabled, value); }
        }
        public bool FEI_OperationEnabled
        {
            get { return _FEI_OperationEnabled; }
            set { SetFieldEx(PlcFestoInputs, "iFE", "Operation Enabled", ref _FEI_OperationEnabled, value); }
        }
        public bool FEI_Warning
        {
            get { return _FEI_Warning; }
            set { SetFieldEx(PlcFestoInputs, "iFE", "Warning", ref _FEI_Warning, value); }
        }
        public bool FEI_Fault
        {
            get { return _FEI_Fault; }
            set { SetFieldEx(PlcFestoInputs, "iFE", "Fault", ref _FEI_Fault, value); }
        }
        public bool FEI_VoltageApplied
        {
            get { return _FEI_VoltageApplied; }
            set { SetFieldEx(PlcFestoInputs, "iFE", "Voltage Applied", ref _FEI_VoltageApplied, value); }
        }
        public bool FEI_OpMode1
        {
            get { return _FEI_OpMode1; }
            set { SetFieldEx(PlcFestoInputs, "iFE", "Operation Mode1", ref _FEI_OpMode1, value); }
        }
        public bool FEI_OpMode2
        {
            get { return _FEI_OpMode2; }
            set { SetFieldEx(PlcFestoInputs, "iFE", "Operation Mode2", ref _FEI_OpMode2, value); }
        }
        public bool FEI_HaltNotActive
        {
            get { return _FEI_HaltNotActive; }
            set { SetFieldEx(PlcFestoInputs, "iFE", "Halt Not Active", ref _FEI_HaltNotActive, value); }
        }
        public bool FEI_AckStart
        {
            get { return _FEI_AckStart; }
            set { SetFieldEx(PlcFestoInputs, "iFE", "Acknowledge Start", ref _FEI_AckStart, value); }
        }
        public bool FEI_MotionComplete
        {
            get { return _FEI_MotionComplete; }
            set { SetFieldEx(PlcFestoInputs, "iFE", "Motion Complete", ref _FEI_MotionComplete, value); }
        }
        public bool FEI_Moving
        {
            get { return _FEI_Moving; }
            set { SetFieldEx(PlcFestoInputs, "iFE", "Moving", ref _FEI_Moving, value); }
        }
        public bool FEI_FollowingError
        {
            get { return _FEI_FollowingError; }
            set { SetFieldEx(PlcFestoInputs, "iFE", "Following Error", ref _FEI_FollowingError, value); }
        }
        public bool FEI_AxisReferenced
        {
            get { return _FEI_AxisReferenced; }
            set { SetFieldEx(PlcCommonInputs1, "iFE", "Axis Referenced", ref _FEI_AxisReferenced, value); }
        }

        private ushort _FEI_Inputs2Port;
        private bool _FEI_MotionIsRelative;

        public ushort FEI_Inputs2Port
        {
            get { return _FEI_Inputs2Port; }
            set
            {
                SetField(ref _FEI_Inputs2Port, value);
                FEI_MotionIsRelative = (FEI_Inputs2Port & (1 << ibitFEI_MotionIsRelative)) != 0;
            }
        }
        public bool FEI_MotionIsRelative
        {
            get { return _FEI_MotionIsRelative; }
            set
            {
                SetFieldEx(PlcFestoInputs, "iFE", "Motion Is Relative", ref _FEI_MotionIsRelative, value);
                FEI_CurrentVelocity = (ushort)(FEI_Inputs2Port & 0xFF00 >> 8);
            }
        }

        //For LinMot outputs 1 & 2 

        private int _LMO_TargetPositon;
        private uint _LMO_TargetVelocity;
        private uint _LMO_TargetAcc;
        private uint _LMO_TargetDec;
        public int LMO_TargetPosition
        {
            get { return _LMO_TargetPositon; }
            set { SetField(ref _LMO_TargetPositon, value); }
        }

        public uint LMO_TargetVelocity
        {
            get { return _LMO_TargetVelocity; }
            set { SetField(ref _LMO_TargetVelocity, value); }
        }
        public uint LMO_TargetAcc
        {
            get { return _LMO_TargetAcc; }
            set { SetField(ref _LMO_TargetAcc, value); }
        }
        public uint LMO_TargetDec
        {
            get { return _LMO_TargetDec; }
            set { SetField(ref _LMO_TargetDec, value); }
        }

        private ushort _LMO_CmdHeader;
        private ushort _LMO_ControlPort;
        private bool _LMO_SwitchOn;
        private bool _LMO_VoltageEnable;
        private bool _LMO_QuickStop;
        private bool _LMO_EnableOperation;
        private bool _LMO_Abort;
        private bool _LMO_Freeze;
        private bool _LMO_GoToPosition;
        private bool _LMO_ResetError;
        private bool _LMO_JogPlus;
        private bool _LMO_JogMinus;
        private bool _LMO_SpecialMode;
        private bool _LMO_Home;
        private bool _LMO_ClearanceCheck;
        private bool _LMO_GoToInitial;
        private bool _LMO_PhaseSearch;

        public ushort LMO_CmdHeader
        {
            get { return _LMO_CmdHeader; }
            set { SetField(ref _LMO_CmdHeader, value); }
        }
        public ushort LMO_ControlPort
        {
            get { return _LMO_ControlPort; }
            set
            {
                SetField(ref _LMO_ControlPort, value);
                LMO_SwitchOn = (LMO_ControlPort & (1 << obitLMO_SwitchOn)) != 0;
                LMO_VoltageEnable = (LMO_ControlPort & (1 << obitLMO_VoltageEnable)) != 0;
                LMO_QuickStop = (LMO_ControlPort & (1 << obitLMO_QuickStop)) != 0;
                LMO_EnableOperation = (LMO_ControlPort & (1 << obitLMO_EnableOperation)) != 0;
                LMO_Abort = (LMO_ControlPort & (1 << obitLMO_Abort)) != 0;
                LMO_Freeze = (LMO_ControlPort & (1 << obitLMO_Freeze)) != 0;
                LMO_GoToPosition = (LMO_ControlPort & (1 << obitLMO_GoToPosition)) != 0;
                LMO_ResetError = (LMO_ControlPort & (1 << obitLMO_ResetError)) != 0;
                LMO_JogPlus = (LMO_ControlPort & (1 << obitLMO_JogPlus)) != 0;
                LMO_JogMinus = (LMO_ControlPort & (1 << obitLMO_JogMinus)) != 0;
                LMO_SpecialMode = (LMO_ControlPort & (1 << obitLMO_SpecialMode)) != 0;
                LMO_Home = (LMO_ControlPort & (1 << obitLMO_Home)) != 0;
                LMO_ClearanceCheck = (LMO_ControlPort & (1 << obitLMO_ClearanceCheck)) != 0;
                LMO_GoToInitial = (LMO_ControlPort & (1 << obitLMO_GoToInitial)) != 0;
                LMO_PhaseSearch = (LMO_ControlPort & (1 << obitLMO_PhaseSearch)) != 0; ;
            }
        }
        public bool LMO_SwitchOn
        {
            get { return _LMO_SwitchOn; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Switch On", ref _LMO_SwitchOn, value); }
        }
        public bool LMO_VoltageEnable
        {
            get { return _LMO_VoltageEnable; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Voltage Enable", ref _LMO_VoltageEnable, value); }
        }
        public bool LMO_QuickStop
        {
            get { return _LMO_QuickStop; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Quick Stop", ref _LMO_QuickStop, value); }
        }
        public bool LMO_EnableOperation
        {
            get { return _LMO_EnableOperation; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Enable Operation", ref _LMO_EnableOperation, value); }
        }
        public bool LMO_Abort
        {
            get { return _LMO_Abort; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Abort", ref _LMO_Abort, value); }
        }
        public bool LMO_Freeze
        {
            get { return _LMO_Freeze; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Freeze", ref _LMO_Freeze, value); }
        }
        public bool LMO_GoToPosition
        {
            get { return _LMO_GoToPosition; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Goto Position", ref _LMO_GoToPosition, value); }
        }
        public bool LMO_ResetError
        {
            get { return _LMO_ResetError; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Reset Error", ref _LMO_ResetError, value); }
        }
        public bool LMO_JogPlus
        {
            get { return _LMO_JogPlus; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Jog Plus", ref _LMO_JogPlus, value); }
        }
        public bool LMO_JogMinus
        {
            get { return _LMO_JogMinus; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Jog Minus", ref _LMO_JogMinus, value); }
        }
        public bool LMO_SpecialMode
        {
            get { return _LMO_SpecialMode; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Special Mode", ref _LMO_SpecialMode, value); }
        }
        public bool LMO_Home
        {
            get { return _LMO_Home; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Home", ref _LMO_Home, value); }
        }
        public bool LMO_ClearanceCheck
        {
            get { return _LMO_ClearanceCheck; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Clearance Check", ref _LMO_ClearanceCheck, value); }
        }
        public bool LMO_GoToInitial
        {
            get { return _LMO_GoToInitial; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Goto Initial", ref _LMO_GoToInitial, value); }
        }
        public bool LMO_PhaseSearch
        {
            get { return _LMO_PhaseSearch; }
            set { SetFieldEx(PlcLinMotOutputs, "oLM", "Phase Search", ref _LMO_PhaseSearch, value); }
        }

        //For LinMot inputs 1 & 2 

        private float _LMI_CurrentPosition;
        private ushort _LMI_StateVar;
        private ushort _LMI_WarningCode;
        public float LMI_CurrentPosition
        {
            get { return _LMI_CurrentPosition; }
            set { SetField(ref _LMI_CurrentPosition, value / 10000); }
        }
        public ushort LMI_StateVar
        {
            get { return _LMI_StateVar; }
            set { SetField(ref _LMI_StateVar, value); }
        }
        public ushort LMI_WarningCode
        {
            get { return _LMI_WarningCode; }
            set { SetField(ref _LMI_WarningCode, value); }
        }

        private ushort _LMI_Status;
        private bool _LMI_Enabled;
        private bool _LMI_SwitchedOn;
        private bool _LMI_OperationEnabled;
        private bool _LMI_Error;
        private bool _LMI_VoltageEnabled;
        private bool _LMI_QuickStopped;
        private bool _LMI_SwitchOnLocked;
        private bool _LMI_Warning;
        private bool _LMI_EventHandlerActive;
        private bool _LMI_SpecialMotionActive;
        private bool _LMI_InTargetPosition;
        private bool _LMI_Homed;
        private bool _LMI_FatalError;
        private bool _LMI_MotionActive;
        private bool _LMI_RangeIndicator1;
        private bool _LMI_RangeIndicator2;

        public ushort LMI_Status
        {
            get { return _LMI_Status; }
            set
            {
                SetField(ref _LMI_Status, value);
                LMI_Enabled = (LMI_Status & (1 << ibitLMI_Enabled)) != 0;
                LMI_SwitchedOn = (LMI_Status & (1 << ibitLMI_SwitchedOn)) != 0;
                LMI_OperationEnabled = (LMI_Status & (1 << ibitLMI_OperationEnabled)) != 0;
                LMI_Error = (LMI_Status & (1 << ibitLMI_Error)) != 0;
                LMI_VoltageEnabled = (LMI_Status & (1 << ibitLMI_VoltageEnabled)) != 0;
                LMI_QuickStopped = (LMI_Status & (1 << ibitLMI_QuickStopped)) != 0;
                LMI_SwitchOnLocked = (LMI_Status & (1 << ibitLMI_SwitchOnLocked)) != 0;
                LMI_Warning = (LMI_Status & (1 << ibitLMI_Warning)) != 0;
                LMI_EventHandlerActive = (LMI_Status & (1 << ibitLMI_EventHandlerActive)) != 0;
                LMI_SpecialMotionActive = (LMI_Status & (1 << ibitLMI_SpecialMotionActive)) != 0;
                LMI_InTargetPosition = (LMI_Status & (1 << ibitLMI_InTargetPosition)) != 0;
                LMI_Homed = (LMI_Status & (1 << ibitLMI_Homed)) != 0;
                LMI_FatalError = (LMI_Status & (1 << ibitLMI_FatalError)) != 0;
                LMI_MotionActive = (LMI_Status & (1 << ibitLMI_MotionActive)) != 0;
                LMI_RangeIndicator1 = (LMI_Status & (1 << ibitLMI_RangeIndicator1)) != 0;
                LMI_RangeIndicator2 = (LMI_Status & (1 << ibitLMI_RangeIndicator2)) != 0;
            }
        }
        public bool LMI_Enabled
        {
            get { return _LMI_Enabled; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Enabled", ref _LMI_Enabled, value); }
        }
        public bool LMI_SwitchedOn
        {
            get { return _LMI_SwitchedOn; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Switched On", ref _LMI_SwitchedOn, value); }
        }
        public bool LMI_OperationEnabled
        {
            get { return _LMI_OperationEnabled; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Operation Enabled", ref _LMI_OperationEnabled, value); }
        }
        public bool LMI_Error
        {
            get { return _LMI_Error; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Error", ref _LMI_Error, value); }
        }
        public bool LMI_VoltageEnabled
        {
            get { return _LMI_VoltageEnabled; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Voltage Enabled", ref _LMI_VoltageEnabled, value); }
        }
        public bool LMI_QuickStopped
        {
            get { return _LMI_QuickStopped; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Quick Stopped", ref _LMI_QuickStopped, value); }
        }
        public bool LMI_SwitchOnLocked
        {
            get { return _LMI_SwitchOnLocked; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Switch On Locked", ref _LMI_SwitchOnLocked, value); }
        }
        public bool LMI_Warning
        {
            get { return _LMI_Warning; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Warning", ref _LMI_Warning, value); }
        }
        public bool LMI_EventHandlerActive
        {
            get { return _LMI_EventHandlerActive; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Event Handled Active", ref _LMI_EventHandlerActive, value); }
        }
        public bool LMI_SpecialMotionActive
        {
            get { return _LMI_SpecialMotionActive; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Special Motion Active", ref _LMI_SpecialMotionActive, value); }
        }
        public bool LMI_InTargetPosition
        {
            get { return _LMI_InTargetPosition; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "In Target Position", ref _LMI_InTargetPosition, value); }
        }
        public bool LMI_Homed
        {
            get { return _LMI_Homed; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Homed", ref _LMI_Homed, value); }
        }
        public bool LMI_FatalError
        {
            get { return _LMI_FatalError; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Fatal Error", ref _LMI_FatalError, value); }
        }
        public bool LMI_MotionActive
        {
            get { return _LMI_MotionActive; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Motion Active", ref _LMI_MotionActive, value); }
        }
        public bool LMI_RangeIndicator1
        {
            get { return _LMI_RangeIndicator1; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Range Indicator1", ref _LMI_RangeIndicator1, value); }
        }
        public bool LMI_RangeIndicator2
        {
            get { return _LMI_RangeIndicator2; }
            set { SetFieldEx(PlcLinMotInputs, "iLM", "Range Indicator2", ref _LMI_RangeIndicator2, value); }
        }

        // For Weld Laser outputs

        private ushort _WLO_LaserProgram;
        public ushort WLO_LaserProgram
        {
            get { return _WLO_LaserProgram; }
            set { SetField(ref _WLO_LaserProgram, value); }
        }

        private ushort _WLO_Control;
        private bool _WLO_Request;
        private bool _WLO_Reset;
        private bool _WLO_StartDyn;
        private bool _WLO_StandBy;
        private bool _WLO_On;
        private bool _WLO_ExtAct;

        public ushort WLO_Control
        {
            get { return _WLO_Control; }
            set
            {
                SetField(ref _WLO_Control, value);
                WLO_Request = (WLO_Control & (1 << obitWLO_Request)) != 0;
                WLO_Reset = (WLO_Control & (1 << obitWLO_Reset)) != 0;
                WLO_StartDyn = (WLO_Control & (1 << obitWLO_StartDyn)) != 0;
                WLO_StandBy = (WLO_Control & (1 << obitWLO_StandBy)) != 0;
                WLO_On = (WLO_Control & (1 << obitWLO_On)) != 0;
                WLO_ExtAct = (WLO_Control & (1 << obitWLO_ExtAct)) != 0;
            }
        }
        public bool WLO_Request
        {
            get { return _WLO_Request; }
            set { SetFieldEx(PlcWeldLasOutputs, "oWL", "Request", ref _WLO_Request, value); }
        }
        public bool WLO_Reset
        {
            get { return _WLO_Reset; }
            set { SetFieldEx(PlcWeldLasOutputs, "oWL", "Reset", ref _WLO_Reset, value); }
        }
        public bool WLO_StartDyn
        {
            get { return _WLO_StartDyn; }
            set { SetFieldEx(PlcWeldLasOutputs, "oWL", "Start Dyn", ref _WLO_StartDyn, value); }
        }
        public bool WLO_StandBy
        {
            get { return _WLO_StandBy; }
            set { SetFieldEx(PlcWeldLasOutputs, "oWL", "StandBy", ref _WLO_StandBy, value); }
        }
        public bool WLO_On
        {
            get { return _WLO_On; }
            set { SetFieldEx(PlcWeldLasOutputs, "oWL", "On", ref _WLO_On, value); }
        }
        public bool WLO_ExtAct
        {
            get { return _WLO_ExtAct; }
            set { SetFieldEx(PlcWeldLasOutputs, "oWL", "Ext. Act.", ref _WLO_ExtAct, value); }
        }

        // For Weld Laser inputs
        private ushort _WLI_Status;
        private bool _WLI_Assigned;
        private bool _WLI_Fault;
        private bool _WLI_PrgCompleted;
        private bool _WLI_ProgActive;
        private bool _WLI_Ready;
        private bool _WLI_On;
        private bool _WLI_ExtAct;

        public ushort WLI_Status
        {
            get { return _WLI_Status; }
            set
            {
                SetField(ref _WLI_Status, value);
                WLI_Assigned = (WLI_Status & (1 << ibitWLI_Assigned)) != 0;
                WLI_Fault = (WLI_Status & (1 << ibitWLI_Fault)) != 0;
                WLI_PrgCompleted = (WLI_Status & (1 << ibitWLI_PrgCompleted)) != 0;
                WLI_ProgActive = (WLI_Status & (1 << ibitWLI_ProgActive)) != 0;
                WLI_Ready = (WLI_Status & (1 << ibitWLI_Ready)) != 0;
                WLI_On = (WLI_Status & (1 << ibitWLI_On)) != 0;
                WLI_ExtAct = (WLI_Status & (1 << ibitWLI_ExtAct)) != 0;
            }
        }
        public bool WLI_Assigned
        {
            get { return _WLI_Assigned; }
            set { SetFieldEx(PlcWeldLasInputs, "iWL", "Assigned", ref _WLI_Assigned, value); }
        }
        public bool WLI_Fault
        {
            get { return _WLI_Fault; }
            set { SetFieldEx(PlcWeldLasInputs, "iWL", "Fault", ref _WLI_Fault, value); }
        }
        public bool WLI_PrgCompleted
        {
            get { return _WLI_PrgCompleted; }
            set { SetFieldEx(PlcWeldLasInputs, "iWL", "Prg. Completed", ref _WLI_PrgCompleted, value); }
        }
        public bool WLI_ProgActive
        {
            get { return _WLI_ProgActive; }
            set { SetFieldEx(PlcWeldLasInputs, "iWL", "Prg. Active", ref _WLI_ProgActive, value); }
        }
        public bool WLI_Ready
        {
            get { return _WLI_Ready; }
            set { SetFieldEx(PlcWeldLasInputs, "iWL", "Ready", ref _WLI_Ready, value); }
        }
        public bool WLI_On
        {
            get { return _WLI_On; }
            set { SetFieldEx(PlcWeldLasInputs, "iWL", "On", ref _WLI_On, value); }
        }
        public bool WLI_ExtAct
        {
            get { return _WLI_ExtAct; }
            set { SetFieldEx(PlcWeldLasInputs, "iWL", "Ext. Act.", ref _WLI_ExtAct, value); }
        }

        // for Mark laser inputs

        // For Weld Laser inputs
        private ushort _MLI_Status;
        private bool _MLI_BeamSourceOn;
        private bool _MLI_LaserReady;
        private bool _MLI_SafetyClosed;
        private bool _MLI_ShutterOpen;
        private bool _MLI_BeamExpInPos;
        private bool _MLI_LaserStable;
        private bool _MLI_LaserMonitoring;
        private bool _MLI_Malfunction;
        private bool _MLI_PilotLaserOn;
        private bool _MLI_WarningLampOn;
        private bool _MLI_QswitchOn;
        private bool _MLI_AutoDeactivation;
        private bool _MLI_KeySwitchLock;
        private bool _MLI_EmgrCircClosed;
        private bool _MLI_HeatingPhaseOn;
        private bool _MLI_ControlIsOn;
        System.Windows.Media.Brush[] brushes = { System.Windows.Media.Brushes.Green, System.Windows.Media.Brushes.Brown, System.Windows.Media.Brushes.Red };

        public ushort MLI_Status
        {
            get { return _MLI_Status; }
            set
            {
                if (MainWindow.ListMLmess.Count > 1800) MainWindow.ListMLmess.Clear();
                System.Windows.Media.Brush brush = brushes[MLI_Status % 3];
                if (MLI_Status != value) MainWindow.ListMLmess.Add(new MainWindow.MLcomms(">Status=0x" + value.ToString("X4"), brush));
                SetField(ref _MLI_Status, value);
                MLI_BeamSourceOn = (MLI_Status & (1 << ibitMLI_BeamSourceOn)) != 0;
                MLI_LaserReady = (MLI_Status & (1 << ibitMLI_LaserReady)) != 0;
                MLI_SafetyClosed = (MLI_Status & (1 << ibitMLI_SafetyClosed)) != 0;
                MLI_ShutterOpen = (MLI_Status & (1 << ibitMLI_ShutterOpen)) != 0;
                MLI_BeamExpInPos = (MLI_Status & (1 << ibitMLI_BeamExpInPos)) != 0;
                MLI_LaserStable = (MLI_Status & (1 << ibitMLI_LaserStable)) != 0;
                MLI_LaserMonitoring = (MLI_Status & (1 << ibitMLI_LaserMonitoring)) != 0;
                MLI_Malfunction = (MLI_Status & (1 << ibitMLI_Malfunction)) != 0;
                MLI_PilotLaserOn = (MLI_Status & (1 << ibitMLI_PilotLaserOn)) != 0;
                MLI_WarningLampOn = (MLI_Status & (1 << ibitMLI_WarningLampOn)) != 0;
                MLI_QswitchOn = (MLI_Status & (1 << ibitMLI_QswitchOn)) != 0;
                MLI_AutoDeactivation = (MLI_Status & (1 << ibitMLI_AutoDeactivation)) != 0;
                MLI_KeySwitchLock = (MLI_Status & (1 << ibitMLI_KeySwitchLock)) != 0;
                MLI_EmgrCircClosed = (MLI_Status & (1 << ibitMLI_EmgrCircClosed)) != 0;
                MLI_HeatingPhaseOn = (MLI_Status & (1 << ibitMLI_HeatingPhaseOn)) != 0;
                MLI_ControlIsOn = (MLI_Status & (1 << ibitMLI_ControlIsOn)) != 0;
            }
        }
        public bool MLI_BeamSourceOn
        {
            get { return _MLI_BeamSourceOn; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Beam Source is On", ref _MLI_BeamSourceOn, value); }
        }
        public bool MLI_LaserReady
        {
            get { return _MLI_LaserReady; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Laser is Ready", ref _MLI_LaserReady, value); }
        }
        public bool MLI_SafetyClosed
        {
            get { return _MLI_SafetyClosed; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Emgr. Circuit is Closed", ref _MLI_SafetyClosed, value); }
        }
        public bool MLI_ShutterOpen
        {
            get { return _MLI_ShutterOpen; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Shutter is Open", ref _MLI_ShutterOpen, value); }
        }
        public bool MLI_BeamExpInPos
        {
            get { return _MLI_BeamExpInPos; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Beam Expander in Position", ref _MLI_BeamExpInPos, value); }
        }
        public bool MLI_LaserStable
        {
            get { return _MLI_LaserStable; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Laser is Stable", ref _MLI_LaserStable, value); }
        }
        public bool MLI_LaserMonitoring
        {
            get { return _MLI_LaserMonitoring; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Laser Monitoring Reacted", ref _MLI_LaserMonitoring, value); }
        }
        public bool MLI_Malfunction
        {
            get { return _MLI_Malfunction; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Malfunction", ref _MLI_Malfunction, value); }
        }
        public bool MLI_PilotLaserOn
        {
            get { return _MLI_PilotLaserOn; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Pilot Laser is On", ref _MLI_PilotLaserOn, value); }
        }
        public bool MLI_WarningLampOn
        {
            get { return _MLI_WarningLampOn; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Warning Lamp is On", ref _MLI_WarningLampOn, value); }
        }
        public bool MLI_QswitchOn
        {
            get { return _MLI_QswitchOn; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Q-switch Trigger is On", ref _MLI_QswitchOn, value); }
        }
        public bool MLI_AutoDeactivation
        {
            get { return _MLI_AutoDeactivation; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Autodeactivation activated", ref _MLI_AutoDeactivation, value); }
        }
        public bool MLI_KeySwitchLock
        {
            get { return _MLI_KeySwitchLock; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Key Switch Locking is Closed", ref _MLI_KeySwitchLock, value); }
        }
        public bool MLI_EmgrCircClosed
        {
            get { return _MLI_EmgrCircClosed; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Emergency off Circuit is Closed", ref _MLI_EmgrCircClosed, value); }
        }
        public bool MLI_HeatingPhaseOn
        {
            get { return _MLI_HeatingPhaseOn; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Heating Phase is On", ref _MLI_HeatingPhaseOn, value); }
        }
        public bool MLI_ControlIsOn
        {
            get { return _MLI_ControlIsOn; }
            set { SetFieldEx(PlcMarkLasInputs, "iML", "Control is On", ref _MLI_ControlIsOn, value); }
        }

        //error variables for PLC process
        private ushort _ErrorI; //Global initialization
        private ushort _ErrorL; //loading station
        private ushort _ErrorW; //marking station
        private ushort _ErrorM; //welding station
        private ushort _Prompts; //loading station
        public ushort ErrorI
        {
            get { return _ErrorI; }
            set { SetField(ref _ErrorI, value); }
        }
        public ushort ErrorL
        {
            get { return _ErrorL; }
            set { SetField(ref _ErrorL, value); }
        }
        public ushort ErrorW
        {
            get { return _ErrorW; }
            set { SetField(ref _ErrorW, value); }
        }
        public ushort ErrorM
        {
            get { return _ErrorM; }
            set { SetField(ref _ErrorM, value); }
        }

        //state variables for PLC processes
        private ushort _StateI; //Global initialization
        private ushort _StateL; //loading station
        private ushort _StateW; //marking station
        private ushort _StateM; //welding station
        private ushort _StateF; //Festo motor
        private ushort _StateR; //rotation table
        private ushort _StateT; //LinMot motor
        private ushort _StateS; //laser
        public ushort StateI
        {
            get { return _StateI; }
            set { SetField(ref _StateI, value); }
        }
        public ushort StateL
        {
            get { return _StateL; }
            set { SetField(ref _StateL, value); }
        }
        public ushort StateW
        {
            get { return _StateW; }
            set { SetField(ref _StateW, value); }
        }
        public ushort StateM
        {
            get { return _StateM; }
            set { SetField(ref _StateM, value); }
        }
        public ushort StateR
        {
            get { return _StateR; }
            set { SetField(ref _StateR, value); }
        }
        public ushort StateF
        {
            get { return _StateF; }
            set { SetField(ref _StateF, value); }
        }
        public ushort StateT
        {
            get { return _StateT; }
            set { SetField(ref _StateT, value); }
        }
        public ushort StateS
        {
            get { return _StateS; }
            set { SetField(ref _StateS, value); }
        }

        //Error variables for PLC processes
        private ushort _ErrorT;
        public ushort ErrorT
        {
            get { return _ErrorT; }
            set { SetField(ref _ErrorT, value); }
        }
        public ushort Prompts
        {
            get { return _Prompts; }
            set { SetField(ref _Prompts, value); }
        }

        public const byte bitFlagsC100_LaserWeld = 0;
        public const byte bitFlagsC100_LaserGetOffset = 1;
        public const byte bitFlagsC100_CheckNut = 2;
        public const byte bitFlagsC100_VisionOK = 3;
        public const byte bitFlagsC100_WeldedFromCam = 4;
        public const byte bitFlagsC100_WeldedByPLC = 5;
        public const byte bitFlagsC100_MarkedByPLC = 6;
        public const byte bitFlagsC100_Next = 9;
        public const byte bitFlagsC100_Unload = 10;
        public const byte bitFlagsC100_Reset = 11;
        public const byte bitFlagsC100_Welded = 12;
        public const byte bitFlagsC100_Marked = 13;
        public const byte bitFlagsC100_GoodForMark = 14;
        public const byte bitFlagsC100_LaserWait = 15;

        private ushort _C100_Flags;
        private bool _F_LaserWeld;
        private bool _F_LaserGetOffset;
        private bool _F_VisionOK;
        private bool _F_WeldedFromCam;
        private bool _F_WeldedByPLC;
        private bool _F_MarkedByPLC;
        private bool _F_CheckNut;
        private bool _F_Next;
        private bool _F_Unload;
        private bool _F_Reset;
        private bool _F_Welded;
        private bool _F_Marked;
        private bool _F_GoodForMark;
        private bool _F_LaserWait;
        public ushort C100_Flags
        {
            get { return _C100_Flags; }
            set
            {
                SetField(ref _C100_Flags, value);
                F_LaserWeld = (C100_Flags & (1 << bitFlagsC100_LaserWeld)) != 0;
                F_LaserGetOffset = (C100_Flags & (1 << bitFlagsC100_LaserGetOffset)) != 0;
                F_CheckNut = (C100_Flags & (1 << bitFlagsC100_CheckNut)) != 0;
                F_VisionOK = (C100_Flags & (1 << bitFlagsC100_VisionOK)) != 0;
                F_WeldedFromCam = (C100_Flags & (1 << bitFlagsC100_WeldedFromCam)) != 0;
                F_WeldedByPLC = (C100_Flags & (1 << bitFlagsC100_WeldedByPLC)) != 0;
                F_MarkedByPLC = (C100_Flags & (1 << bitFlagsC100_MarkedByPLC)) != 0;
                F_Next = (C100_Flags & (1 << bitFlagsC100_Next)) != 0;
                F_Unload = (C100_Flags & (1 << bitFlagsC100_Unload)) != 0;
                F_Reset = (C100_Flags & (1 << bitFlagsC100_Reset)) != 0;
                F_Welded = (C100_Flags & (1 << bitFlagsC100_Welded)) != 0;
                F_Marked = (C100_Flags & (1 << bitFlagsC100_Marked)) != 0;
                F_GoodForMark = (C100_Flags & (1 << bitFlagsC100_GoodForMark)) != 0;
                F_LaserWait = (C100_Flags & (1 << bitFlagsC100_LaserWait)) != 0;
            }
        }
        public bool F_LaserWeld
        {
            get { return _F_LaserWeld; }
            set { SetField(ref _F_LaserWeld, value); }
        }
        public bool F_LaserGetOffset
        {
            get { return _F_LaserGetOffset; }
            set { SetField(ref _F_LaserGetOffset, value); }
        }
        public bool F_VisionOK
        {
            get { return _F_VisionOK; }
            set { SetField(ref _F_VisionOK, value); }
        }
        public bool F_WeldedFromCam
        {
            get { return _F_WeldedFromCam; }
            set { SetField(ref _F_WeldedFromCam, value); }
        }
        public bool F_WeldedByPLC
        {
            get { return _F_WeldedByPLC; }
            set { SetField(ref _F_WeldedByPLC, value); }
        }
        public bool F_MarkedByPLC
        {
            get { return _F_MarkedByPLC; }
            set { SetField(ref _F_MarkedByPLC, value); }
        }
        public bool F_CheckNut
        {
            get { return _F_CheckNut; }
            set { SetField(ref _F_CheckNut, value); }
        }
        public bool F_Next
        {
            get { return _F_Next; }
            set { SetField(ref _F_Next, value); }
        }
        public bool F_Unload
        {
            get { return _F_Unload; }
            set { SetField(ref _F_Unload, value); }
        }
        public bool F_Reset
        {
            get { return _F_Reset; }
            set { SetField(ref _F_Reset, value); }
        }
        public bool F_Welded
        {
            get { return _F_Welded; }
            set { SetField(ref _F_Welded, value); }
        }
        public bool F_Marked
        {
            get { return _F_Marked; }
            set { SetField(ref _F_Marked, value); }
        }
        public bool F_GoodForMark
        {
            get { return _F_GoodForMark; }
            set { SetField(ref _F_GoodForMark, value); }
        }
        public bool F_LaserWait
        {
            get { return _F_LaserWait; }
            set { SetField(ref _F_LaserWait, value); }
        }

        public const byte bitFlagsC109_ClearError = 0;
        public const byte bitFlagsC109_MarkingRotate = 1;
        public const byte bitFlagsC109_MarkingDone = 2;
        public const byte bitFlagsC109_EnableLoaderStart = 3;
        public const byte bitFlagsC109_MarkLine = 4;
        public const byte bitFlagsC109_Trotating = 5;
        public const byte bitFlagsC109_EnableLoaderEnd = 6;
        public const byte bitFlagsC109_MarkingPossible = 7;
        public const byte bitFlagsC109_SensorLoaded = 8;
        public const byte bitFlagsC109_EnableWelderStart = 9;
        public const byte bitFlagsC109_EnableWelderEnd = 10;
        public const byte bitFlagsC109_EnableMarkerStart = 11;
        public const byte bitFlagsC109_EnableMarkerEnd = 12;

        private ushort _C109_Flags;
        private bool _F_ClearError;
        private bool _F_MarkingRotate;
        private bool _F_MarkingDone;
        private bool _F_EnableLoaderStart;
        private bool _F_MarkLine;
        private bool _F_Trotating;
        private bool _F_EnableLoaderEnd;
        private bool _F_MarkingPossible;
        private bool _F_SensorLoaded;
        private bool _F_EnableWelderStart;
        private bool _F_EnableWelderEnd;
        private bool _F_EnableMarkerStart;
        private bool _F_EnableMarkerEnd;

        public ushort C109_Flags
        {
            get { return _C109_Flags; }
            set
            {
                SetField(ref _C109_Flags, value);
                F_ClearError = (C109_Flags & (1 << bitFlagsC109_ClearError)) != 0;
                F_MarkingRotate = (C109_Flags & (1 << bitFlagsC109_MarkingRotate)) != 0;
                F_MarkingDone = (C109_Flags & (1 << bitFlagsC109_MarkingDone)) != 0;
                F_EnableLoaderStart = (C109_Flags & (1 << bitFlagsC109_EnableLoaderStart)) != 0;
                F_MarkLine = (C109_Flags & (1 << bitFlagsC109_MarkLine)) != 0;
                F_Trotating = (C109_Flags & (1 << bitFlagsC109_Trotating)) != 0;
                F_EnableLoaderEnd = (C109_Flags & (1 << bitFlagsC109_EnableLoaderEnd)) != 0;
                F_MarkingPossible = (C109_Flags & (1 << bitFlagsC109_MarkingPossible)) != 0;
                F_SensorLoaded = (C109_Flags & (1 << bitFlagsC109_SensorLoaded)) != 0;
                F_EnableWelderStart = (C109_Flags & (1 << bitFlagsC109_EnableWelderStart)) != 0;
                F_EnableWelderEnd = (C109_Flags & (1 << bitFlagsC109_EnableWelderEnd)) != 0;
                F_EnableMarkerStart = (C109_Flags & (1 << bitFlagsC109_EnableMarkerStart)) != 0;
                F_EnableMarkerEnd = (C109_Flags & (1 << bitFlagsC109_EnableMarkerEnd)) != 0;
            }
        }
        public bool F_ClearError
        {
            get { return _HF_UseWelding; }
            set { SetField(ref _F_ClearError, value); }
        }
        public bool F_MarkingRotate
        {
            get { return _F_MarkingRotate; }
            set { SetField(ref _F_MarkingRotate, value); }
        }
        public bool F_MarkingDone
        {
            get { return _F_MarkingDone; }
            set { SetField(ref _F_MarkingDone, value); }
        }
        public bool F_EnableLoaderStart
        {
            get { return _F_EnableLoaderStart; }
            set { SetField(ref _F_EnableLoaderStart, value); }
        }
        public bool F_MarkLine
        {
            get { return _F_MarkLine; }
            set { SetField(ref _F_MarkLine, value); }
        }
        public bool F_Trotating
        {
            get { return _F_Trotating; }
            set { SetField(ref _F_Trotating, value); }
        }
        public bool F_EnableLoaderEnd
        {
            get { return _F_EnableLoaderEnd; }
            set { SetField(ref _F_EnableLoaderEnd, value); }
        }
        public bool F_MarkingPossible
        {
            get { return _F_MarkingPossible; }
            set { SetField(ref _F_MarkingPossible, value); }
        }
        public bool F_SensorLoaded
        {
            get { return _F_SensorLoaded; }
            set { SetField(ref _F_SensorLoaded, value); }
        }
        public bool F_EnableWelderStart
        {
            get { return _F_EnableWelderStart; }
            set { SetField(ref _F_EnableWelderStart, value); }
        }
        public bool F_EnableWelderEnd
        {
            get { return _F_EnableWelderEnd; }
            set { SetField(ref _F_EnableWelderEnd, value); }
        }
        public bool F_EnableMarkerStart
        {
            get { return _F_EnableMarkerStart; }
            set { SetField(ref _F_EnableMarkerStart, value); }
        }
        public bool F_EnableMarkerEnd
        {
            get { return _F_EnableMarkerEnd; }
            set { SetField(ref _F_EnableMarkerEnd, value); }
        }

        private ushort _H_Flags;
        private bool _HF_UseWelding;
        private bool _HF_UseMarking;
        private bool _HF_in_welding;
        private bool _HF_in_marking;
        private bool _HF_traceability;
        private bool _HF_load_done;
        private bool _HF_weld_done;
        private bool _HF_mark_done;
        private bool _HF_UseStation1;
        private bool _HF_UseStation2;
        private bool _HF_UseStation3;
        public bool HF_UseWelding
        {
            get { return _HF_UseWelding; }
            set { SetFieldEx(PlcHflags, "HFL", "Use Welding", ref _HF_UseWelding, value); }
        }
        public bool HF_UseMarking
        {
            get { return _HF_UseMarking; }
            set { SetFieldEx(PlcHflags, "HFL", "Use Marking", ref _HF_UseMarking, value); }
        }
        public bool HF_in_welding
        {
            get { return _HF_in_welding; }
            set { SetFieldEx(PlcHflags, "HFL", "in welding", ref _HF_in_welding, value); }
        }
        public bool HF_in_marking
        {
            get { return _HF_in_marking; }
            set { SetFieldEx(PlcHflags, "HFL", "In Marking", ref _HF_in_marking, value); }
        }
        public bool HF_traceability
        {
            get { return _HF_traceability; }
            set { SetFieldEx(PlcHflags, "HFL", "Traceability", ref _HF_traceability, value); }
        }

        public bool HF_load_done
        {
            get { return _HF_load_done; }
            set { SetFieldEx(PlcHflags, "HFL", "Load Done", ref _HF_load_done, value); }
        }

        public bool HF_weld_done
        {
            get { return _HF_weld_done; }
            set { SetFieldEx(PlcHflags, "HFL", "Weld Done", ref _HF_weld_done, value); }
        }

        public bool HF_mark_done
        {
            get { return _HF_mark_done; }
            set { SetFieldEx(PlcHflags, "HFL", "Mark Done", ref _HF_mark_done, value); }
        }

        public bool HF_UseStation1
        {
            get { return _HF_UseStation1; }
            set { SetFieldEx(PlcHflags, "HFL", "Use St.1", ref _HF_UseStation1, value); }
        }
        public bool HF_UseStation2
        {
            get { return _HF_UseStation2; }
            set { SetFieldEx(PlcHflags, "HFL", "Use St.2", ref _HF_UseStation2, value); }
        }
        public bool HF_UseStation3
        {
            get { return _HF_UseStation3; }
            set { SetFieldEx(PlcHflags, "HFL", "Use St.3", ref _HF_UseStation3, value); }
        }
        public ushort H_Flags
        {
            get { return _H_Flags; }
            set
            {
                SetField(ref _H_Flags, value);
                HF_UseWelding = (H_Flags & (1 << ibitHF_UseWelding)) != 0;
                HF_UseMarking = (H_Flags & (1 << ibitHF_UseMarking)) != 0;
                HF_in_welding = (H_Flags & (1 << ibitHF_in_welding)) != 0;
                HF_in_marking = (H_Flags & (1 << ibitHF_in_marking)) != 0;
                HF_traceability = (H_Flags & (1 << ibitHF_traceability)) != 0;
                HF_load_done = (H_Flags & (1 << ibitHF_load_done)) != 0;
                HF_weld_done = (H_Flags & (1 << ibitHF_weld_done)) != 0;
                HF_mark_done = (H_Flags & (1 << ibitHF_mark_done)) != 0;
                HF_UseStation1 = (H_Flags & (1 << ibitHF_UseStation1)) != 0;
                HF_UseStation2 = (H_Flags & (1 << ibitHF_UseStation2)) != 0;
                HF_UseStation3 = (H_Flags & (1 << ibitHF_UseStation3)) != 0;
            }
        }

        public UpdateIO()
        {
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonOpLampRed), oCommonOpLampRed, "Op. Red", 0x0001, obitCommonOpLampRed));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonOpLampGreen), oCommonOpLampGreen, "Op. Green", 0x0001, obitCommonOpLampGreen));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonMuteA), oCommonMuteA, "MuteA", 0x0001, obitCommonMuteA));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonMuteB), oCommonMuteB, "MuteB", 0x0001, obitCommonMuteB));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonAirOn), oCommonAirOn, "OnAir", 0x0001, obitCommonAirOn));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonGreen), oCommonGreen, "Green", 0x0001, obitCommonGreen));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonYellow), oCommonYellow, "Yellow", 0x0001, obitCommonYellow));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonRed), oCommonRed, "Red", 0x0001, obitCommonRed));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonBuzzer), oCommonBuzzer, "Buzzer", 0x0001, obitCommonBuzzer));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonBrakeOff), oCommonBuzzer, "Off Brake", 0x0001, obitCommonBrakeOff));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonRotTableErrorReset), oCommonRotTableErrorReset, "RT Reset", 0x0001, obitCommonRotTableErrorReset));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonRotTableStart), oCommonRotTableStart, "RT Start", 0x0001, obitCommonRotTableStart));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonMoveArgonIn), oCommonMoveArgonIn, "In Argon", 0x0001, obitCommonMoveArgonIn));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonArgonOn), oCommonArgonOn, "On Argon", 0x0001, obitCommonArgonOn));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonCameraLightOn), oCommonCameraLightOn, "On Camera Light", 0x0001, obitCommonCameraLightOn));
            PlcCommonOutputs.Add(new IObitDetails("CO", nameof(oCommonLaserStationVacuum), oCommonLaserStationVacuum, "On Vacuum", 0x0001, obitCommonLaserStationVacuum));

            PlcPos1Outputs.Add(new IObitDetails("oP1", nameof(oPos1OpenSupportRolls), oPos1OpenSupportRolls, "Op Rolls", 130, оbitStOpenSupportRolls));
            PlcPos1Outputs.Add(new IObitDetails("oP1", nameof(oPos1OpenAligner), oPos1OpenAligner, "Op Aligner", 130, obitStOpenAligner));
            PlcPos1Outputs.Add(new IObitDetails("oP1", nameof(oPos1ClampGripper), oPos1ClampGripper, "Cl Gripper", 130, obitStClampGripper));
            PlcPos1Outputs.Add(new IObitDetails("oP1", nameof(oPos1OnBlow), oPos1OnBlow, "On Blow", 130, obitStOnBlow));
            PlcPos1Outputs.Add(new IObitDetails("oP1", nameof(oPos1OpenBucket), oPos1OpenBucket, "Op Bucket", 130, obitStOpenBucket));
            PlcPos1Outputs.Add(new IObitDetails("oP1", nameof(oPos1StartFaulhaber), oPos1StartFaulhaber, "FH Home", 130, obitStStartFaulhaber));
            PlcPos1Outputs.Add(new IObitDetails("oP1", nameof(oPos1StartFaulhaber), oPos1StartFaulhaber, "FH Fast", 3202, obitStStartFaulhaber));

            PlcPos2Outputs.Add(new IObitDetails("oP2", nameof(oPos2OpenSupportRolls), oPos2OpenSupportRolls, "Op Rolls", 131, оbitStOpenSupportRolls));
            PlcPos2Outputs.Add(new IObitDetails("oP2", nameof(oPos2OpenAligner), oPos2OpenAligner, "Op Aligner", 131, obitStOpenAligner));
            PlcPos2Outputs.Add(new IObitDetails("oP2", nameof(oPos2ClampGripper), oPos2ClampGripper, "Cl Gripper", 131, obitStClampGripper));
            PlcPos2Outputs.Add(new IObitDetails("oP2", nameof(oPos2OnBlow), oPos2OnBlow, "On Blow", 131, obitStOnBlow));
            PlcPos2Outputs.Add(new IObitDetails("oP2", nameof(oPos2OpenBucket), oPos2OpenBucket, "Op Bucket", 131, obitStOpenBucket));
            PlcPos2Outputs.Add(new IObitDetails("oP2", nameof(oPos2StartFaulhaber), oPos2StartFaulhaber, "FH Fast", 131, obitStStartFaulhaber));

            PlcPos3Outputs.Add(new IObitDetails("oP3", nameof(oPos3OpenSupportRolls), oPos3OpenSupportRolls, "Op Rolls", 132, оbitStOpenSupportRolls));
            PlcPos3Outputs.Add(new IObitDetails("oP3", nameof(oPos3OpenAligner), oPos3OpenAligner, "Op Aligner", 132, obitStOpenAligner));
            PlcPos3Outputs.Add(new IObitDetails("oP3", nameof(oPos3ClampGripper), oPos3ClampGripper, "Cl Gripper", 132, obitStClampGripper));
            PlcPos3Outputs.Add(new IObitDetails("oP3", nameof(oPos3OnBlow), oPos3OnBlow, "On Blow", 132, obitStOnBlow));
            PlcPos3Outputs.Add(new IObitDetails("oP3", nameof(oPos3OpenBucket), oPos3OpenBucket, "Op Bucket", 132, obitStOpenBucket));
            PlcPos3Outputs.Add(new IObitDetails("oP3", nameof(oPos3StartFaulhaber), oPos3StartFaulhaber, "FH Step", 132, obitStStartFaulhaber));
            PlcPos3Outputs.Add(new IObitDetails("oP3", nameof(oPos3StartFaulhaber), oPos3StartFaulhaber, "FH Fast", 3204, obitStStartFaulhaber));

            PlcCommonInputs1.Add(new IObitDetails("CI1", nameof(iCommonPowerOK), iCommonPowerOK, "Power OK", 0x0000, ibitCommonPowerOK));
            PlcCommonInputs1.Add(new IObitDetails("CI1", nameof(iCommonPedal), iCommonPedal, "Pedal", 0x0000, ibitCommonPedal));
            PlcCommonInputs1.Add(new IObitDetails("CI1", nameof(iCommonAirOK), iCommonAirOK, "Air OK", 0x0000, ibitCommonAirOK));
            PlcCommonInputs1.Add(new IObitDetails("CI1", nameof(iCommonArgonOK), iCommonArgonOK, "Argon OK", 0x0000, ibitCommonArgonOK));
            PlcCommonInputs1.Add(new IObitDetails("CI1", nameof(iCommonStation1), iCommonStation1, "Station 1", 0x0000, ibitCommonStation1));
            PlcCommonInputs1.Add(new IObitDetails("CI1", nameof(iCommonStation2), iCommonStation2, "Station 2", 0x0000, ibitCommonStation2));
            PlcCommonInputs1.Add(new IObitDetails("CI1", nameof(iCommonStation3), iCommonStation3, "Station 3", 0x0000, ibitCommonStation3));
            PlcCommonInputs1.Add(new IObitDetails("CI1", nameof(iCommonPusherInHome), iCommonPusherInHome, "Pusher Home", 0x0000, ibitCommonPusherInHome));
            PlcCommonInputs1.Add(new IObitDetails("CI1", nameof(iCommonArgonNozzleOut), iCommonArgonNozzleOut, "Argon out", 0x0000, ibitCommonArgonNozzleOut));
            PlcCommonInputs1.Add(new IObitDetails("CI1", nameof(iCommonArgonNozzleIn), iCommonArgonNozzleIn, "Argon in", 0x0000, ibitCommonArgonNozzleIn));
            PlcCommonInputs1.Add(new IObitDetails("CI1", nameof(iCommonRotTableReady), iCommonRotTableReady, "RT ready", 0x0000, ibitCommonRotTableReady));
            PlcCommonInputs1.Add(new IObitDetails("CI1", nameof(iCommonRotTableInPos), iCommonRotTableInPos, "RT in pos.", 0x0000, ibitCommonRotTableInPos));
            PlcCommonInputs1.Add(new IObitDetails("CI1", nameof(iCommonRotTableMotorOK), iCommonRotTableMotorOK, "RT motor OK", 0x0000, ibitCommonRotTableMotorOK));
            PlcCommonInputs1.Add(new IObitDetails("CI1", nameof(iCommonPushCylDown), iCommonPushCylDown, "Pusher Dn", 0x0000, ibitCommonPushCylDown));

            PlcPos1Inputs.Add(new IObitDetails("iP1", nameof(iPos1AlignerOp), iPos1AlignerOp, "Aligner Op", 127, ibitStAlignerOp));
            PlcPos1Inputs.Add(new IObitDetails("iP1", nameof(iPos1AlignerCl), iPos1AlignerCl, "Aligner Cl", 127, ibitStAlignerCl));
            PlcPos1Inputs.Add(new IObitDetails("iP1", nameof(iPos1SupportRollsCl), iPos1SupportRollsCl, "Rolls Cl", 127, ibitStSupportRollsCl));
            PlcPos1Inputs.Add(new IObitDetails("iP1", nameof(iPos1SensorPresent), iPos1SensorPresent, "Present", 127, ibitStSensorPresent));
            PlcPos1Inputs.Add(new IObitDetails("iP1", nameof(iPos1ClampGripperOp), iPos1ClampGripperOp, "Gripper Op", 127, ibitStClampGripperOp));
            PlcPos1Inputs.Add(new IObitDetails("iP1", nameof(iPos1ClampGripperCl), iPos1ClampGripperCl, "Gripper Cl", 127, ibitStClampGripperCl));
            PlcPos1Inputs.Add(new IObitDetails("iP1", nameof(iPos1FaulhaberStatus), iPos1FaulhaberStatus, "FH Status", 127, ibitStFaulhaberStatus));
            PlcPos1Inputs.Add(new IObitDetails("iP1", nameof(iPos1FaulhaberHome), iPos1FaulhaberHome, "FH Home", 127, ibitStFaulhaberHome));
            PlcPos1Inputs.Add(new IObitDetails("iP1", nameof(iPos1BucketOp), iPos1BucketOp, "Bucket Op", 127, ibitStBucketOp));
            PlcPos1Inputs.Add(new IObitDetails("iP1", nameof(iPos1BucketCl), iPos1BucketCl, "Bucket Cl", 127, ibitStBucketCl));
            PlcPos1Inputs.Add(new IObitDetails("iP1", nameof(iPos1NutOK), iPos1NutOK, "Nut OK", 127, ibitStNutOK));

            PlcPos2Inputs.Add(new IObitDetails("iP2", nameof(iPos2AlignerOp), iPos2AlignerOp, "Aligner Op", 128, ibitStAlignerOp));
            PlcPos2Inputs.Add(new IObitDetails("iP2", nameof(iPos2AlignerCl), iPos2AlignerCl, "Aligner Cl", 128, ibitStAlignerCl));
            PlcPos2Inputs.Add(new IObitDetails("iP2", nameof(iPos2SupportRollsCl), iPos2SupportRollsCl, "Rolls Cl", 128, ibitStSupportRollsCl));
            PlcPos2Inputs.Add(new IObitDetails("iP2", nameof(iPos2SensorPresent), iPos2SensorPresent, "Present", 128, ibitStSensorPresent));
            PlcPos2Inputs.Add(new IObitDetails("iP2", nameof(iPos2ClampGripperOp), iPos2ClampGripperOp, "Gripper Op", 128, ibitStClampGripperOp));
            PlcPos2Inputs.Add(new IObitDetails("iP2", nameof(iPos2ClampGripperCl), iPos2ClampGripperCl, "Gripper Cl", 128, ibitStClampGripperCl));
            PlcPos2Inputs.Add(new IObitDetails("iP2", nameof(iPos2FaulhaberStatus), iPos2FaulhaberStatus, "FH Status", 128, ibitStFaulhaberStatus));
            PlcPos2Inputs.Add(new IObitDetails("iP2", nameof(iPos2FaulhaberHome), iPos2FaulhaberHome, "FH Home", 128, ibitStFaulhaberHome));
            PlcPos2Inputs.Add(new IObitDetails("iP2", nameof(iPos2BucketOp), iPos2BucketOp, "Bucket Op", 128, ibitStBucketOp));
            PlcPos2Inputs.Add(new IObitDetails("iP2", nameof(iPos2BucketCl), iPos2BucketCl, "Bucket Cl", 128, ibitStBucketCl));
            PlcPos2Inputs.Add(new IObitDetails("iP2", nameof(iPos2NutOK), iPos2NutOK, "Nut OK", 128, ibitStNutOK));

            PlcPos3Inputs.Add(new IObitDetails("iP3", nameof(iPos3AlignerOp), iPos3AlignerOp, "Aligner Op", 129, ibitStAlignerOp));
            PlcPos3Inputs.Add(new IObitDetails("iP3", nameof(iPos3AlignerCl), iPos3AlignerCl, "Aligner Cl", 129, ibitStAlignerCl));
            PlcPos3Inputs.Add(new IObitDetails("iP3", nameof(iPos3SupportRollsCl), iPos3SupportRollsCl, "Rolls Cl", 129, ibitStSupportRollsCl));
            PlcPos3Inputs.Add(new IObitDetails("iP3", nameof(iPos3SensorPresent), iPos3SensorPresent, "Present", 129, ibitStSensorPresent));
            PlcPos3Inputs.Add(new IObitDetails("iP3", nameof(iPos3ClampGripperOp), iPos3ClampGripperOp, "Gripper Op", 129, ibitStClampGripperOp));
            PlcPos3Inputs.Add(new IObitDetails("iP3", nameof(iPos3ClampGripperCl), iPos3ClampGripperCl, "Gripper Cl", 129, ibitStClampGripperCl));
            PlcPos3Inputs.Add(new IObitDetails("iP3", nameof(iPos3FaulhaberStatus), iPos3FaulhaberStatus, "FH Status", 129, ibitStFaulhaberStatus));
            PlcPos3Inputs.Add(new IObitDetails("iP3", nameof(iPos3FaulhaberHome), iPos3FaulhaberHome, "FH Home", 129, ibitStFaulhaberHome));
            PlcPos3Inputs.Add(new IObitDetails("iP3", nameof(iPos3BucketOp), iPos3BucketOp, "Bucket Op", 129, ibitStBucketOp));
            PlcPos3Inputs.Add(new IObitDetails("iP3", nameof(iPos3BucketCl), iPos3BucketCl, "Bucket Cl", 129, ibitStBucketCl));
            PlcPos3Inputs.Add(new IObitDetails("iP3", nameof(iPos3NutOK), iPos3NutOK, "Nut OK", 129, ibitStNutOK));

            PlcFestoOutputs.Add(new IObitDetails("oFE", nameof(FEO_EnableDrive), FEO_EnableDrive, "Enable drive", 3206, obitFEO_EnableDrive));
            PlcFestoOutputs.Add(new IObitDetails("oFE", nameof(FEO_EnableOperation), FEO_EnableOperation, "Enable operation", 3206, obitFEO_EnableOperation));
            PlcFestoOutputs.Add(new IObitDetails("oFE", nameof(FEO_ReleaseBrake), FEO_ReleaseBrake, "Release brake", 3206, obitFEO_ReleaseBrake));
            PlcFestoOutputs.Add(new IObitDetails("oFE", nameof(FEO_ResetFault), FEO_ResetFault, "Reset fault", 3206, obitFEO_ResetFault));
            PlcFestoOutputs.Add(new IObitDetails("oFE", nameof(FEO_OpMode1), FEO_OpMode1, "Operation mode 1", 3206, obitFEO_OpMode1));
            PlcFestoOutputs.Add(new IObitDetails("oFE", nameof(FEO_OpMode2), FEO_OpMode2, "Operation mode 2", 3206, obitFEO_OpMode2));
            PlcFestoOutputs.Add(new IObitDetails("oFE", nameof(FEO_ReleaseHalt), FEO_ReleaseHalt, "Release Halt", 3206, obitFEO_ReleaseHalt));
            PlcFestoOutputs.Add(new IObitDetails("oFE", nameof(FEO_StartPositioning), FEO_StartPositioning, "Start Positioning", 3206, obitFEO_StartPositioning));
            PlcFestoOutputs.Add(new IObitDetails("oFE", nameof(FEO_StartHoming), FEO_StartHoming, "Start Homing", 3206, obitFEO_StartHoming));
            PlcFestoOutputs.Add(new IObitDetails("oFE", nameof(FEO_JogPositive), FEO_JogPositive, "Jog Positive", 3206, obitFEO_JogPositive));
            PlcFestoOutputs.Add(new IObitDetails("oFE", nameof(FEO_JogNegative), FEO_JogNegative, "Jog Negative", 3206, obitFEO_JogNegative));
            PlcFestoOutputs.Add(new IObitDetails("oFE", nameof(FEO_ClearRemaining), FEO_ClearRemaining, "Clear Remaining", 3206, obitFEO_ClearRemaining));
            PlcFestoOutputs.Add(new IObitDetails("oFE", nameof(FEO_Relative), FEO_Relative, "Relative move", 3207, obitFEO_Relative));

            PlcFestoInputs.Add(new IObitDetails("iFE", nameof(FEI_DriveEnabled), FEI_DriveEnabled, "Drive Enabled", 3306, ibitFEI_DriveEnabled));
            PlcFestoInputs.Add(new IObitDetails("iFE", nameof(FEI_OperationEnabled), FEI_OperationEnabled, "Operation Enabled", 3306, ibitFEI_OperationEnabled));
            PlcFestoInputs.Add(new IObitDetails("iFE", nameof(FEI_Warning), FEI_Warning, "Warning", 3306, ibitFEI_Warning));
            PlcFestoInputs.Add(new IObitDetails("iFE", nameof(FEI_Fault), FEI_Fault, "Fault", 3306, ibitFEI_Fault));
            PlcFestoInputs.Add(new IObitDetails("iFE", nameof(FEI_VoltageApplied), FEI_VoltageApplied, "Voltage Applied", 3306, ibitFEI_VoltageApplied));
            PlcFestoInputs.Add(new IObitDetails("iFE", nameof(FEI_OpMode1), FEI_OpMode1, "Operation Mode1", 3306, ibitFEI_OpMode1));
            PlcFestoInputs.Add(new IObitDetails("iFE", nameof(FEI_OpMode2), FEI_OpMode2, "Operation Mode2", 3306, ibitFEI_OpMode2));
            PlcFestoInputs.Add(new IObitDetails("iFE", nameof(FEI_HaltNotActive), FEI_HaltNotActive, "Halt Not Active", 3306, ibitFEI_HaltNotActive));
            PlcFestoInputs.Add(new IObitDetails("iFE", nameof(FEI_AckStart), FEI_AckStart, "Acknowledge Start", 3306, ibitFEI_AckStart));
            PlcFestoInputs.Add(new IObitDetails("iFE", nameof(FEI_MotionComplete), FEI_MotionComplete, "Motion Complete", 3306, ibitFEI_MotionComplete));
            PlcFestoInputs.Add(new IObitDetails("iFE", nameof(FEI_Moving), FEI_Moving, "Moving", 3306, ibitFEI_Moving));
            PlcFestoInputs.Add(new IObitDetails("iFE", nameof(FEI_FollowingError), FEI_FollowingError, "Following Error", 3306, ibitFEI_FollowingError));
            PlcFestoInputs.Add(new IObitDetails("iFE", nameof(FEI_AxisReferenced), FEI_AxisReferenced, "Axis Referenced", 3306, ibitFEI_AxisReferenced));
            PlcFestoInputs.Add(new IObitDetails("iFE", nameof(FEI_MotionIsRelative), FEI_MotionIsRelative, "Motion Is Relative", 3307, ibitFEI_MotionIsRelative));

            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_SwitchOn), LMO_SwitchOn, "Switch On", 3215, obitLMO_SwitchOn));
            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_VoltageEnable), LMO_VoltageEnable, "Voltage Enable", 3215, obitLMO_VoltageEnable));
            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_QuickStop), LMO_QuickStop, "Quick Stop", 3215, obitLMO_QuickStop));
            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_EnableOperation), LMO_EnableOperation, "Enable Operation", 3215, obitLMO_EnableOperation));
            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_Abort), LMO_Abort, "Abort", 3215, obitLMO_Abort));
            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_Freeze), LMO_Freeze, "Freeze", 3215, obitLMO_Freeze));
            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_GoToPosition), LMO_GoToPosition, "Goto Position", 3215, obitLMO_GoToPosition));
            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_ResetError), LMO_ResetError, "Reset Error", 3215, obitLMO_ResetError));
            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_JogPlus), LMO_JogPlus, "Jog Plus", 3215, obitLMO_JogPlus));
            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_JogMinus), LMO_JogMinus, "Jog Minus", 3215, obitLMO_JogMinus));
            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_SpecialMode), LMO_SpecialMode, "Special Mode", 3215, obitLMO_SpecialMode));
            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_Home), LMO_Home, "Home", 3215, obitLMO_Home));
            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_ClearanceCheck), LMO_ClearanceCheck, "Clearance Check", 3215, obitLMO_ClearanceCheck));
            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_GoToInitial), LMO_GoToInitial, "Goto Initial", 3215, obitLMO_GoToInitial));
            PlcLinMotOutputs.Add(new IObitDetails("oLM", nameof(LMO_PhaseSearch), LMO_PhaseSearch, "Phase Search", 3215, obitLMO_PhaseSearch));

            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_Enabled), LMI_Enabled, "Enabled", 3315, ibitLMI_Enabled));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_SwitchedOn), LMI_SwitchedOn, "Switched On", 3315, ibitLMI_SwitchedOn));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_OperationEnabled), LMI_OperationEnabled, "Operation Enabled", 3315, ibitLMI_OperationEnabled));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_Error), LMI_Error, "Error", 3315, ibitLMI_Error));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_VoltageEnabled), LMI_VoltageEnabled, "Voltage Enabled", 3315, ibitLMI_VoltageEnabled));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_QuickStopped), LMI_QuickStopped, "Quick Stopped", 3315, ibitLMI_QuickStopped));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_SwitchOnLocked), LMI_SwitchOnLocked, "Switch On Locked", 3315, ibitLMI_SwitchOnLocked));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_Warning), LMI_Warning, "Warning", 3315, ibitLMI_Warning));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_EventHandlerActive), LMI_EventHandlerActive, "Event Handled Active", 3315, ibitLMI_EventHandlerActive));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_SpecialMotionActive), LMI_SpecialMotionActive, "Special Motion Active", 3315, ibitLMI_SpecialMotionActive));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_InTargetPosition), LMI_InTargetPosition, "In Target Position", 3315, ibitLMI_InTargetPosition));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_Homed), LMI_Homed, "Homed", 3315, ibitLMI_Homed));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_FatalError), LMI_FatalError, "Fatal Error", 3315, ibitLMI_FatalError));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_MotionActive), LMI_MotionActive, "Motion Active", 3315, ibitLMI_MotionActive));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_RangeIndicator1), LMI_RangeIndicator1, "Range Indicator1", 3315, ibitLMI_RangeIndicator1));
            PlcLinMotInputs.Add(new IObitDetails("iLM", nameof(LMI_RangeIndicator2), LMI_RangeIndicator2, "Range Indicator2", 3315, ibitLMI_RangeIndicator2));

            PlcWeldLasOutputs.Add(new IObitDetails("oWL", nameof(WLO_Request), WLO_Request, "Request", 3210, obitWLO_Request));
            PlcWeldLasOutputs.Add(new IObitDetails("oWL", nameof(WLO_Reset), WLO_Reset, "Reset", 3210, obitWLO_Reset));
            PlcWeldLasOutputs.Add(new IObitDetails("oWL", nameof(WLO_StartDyn), WLO_StartDyn, "Start Dyn", 3210, obitWLO_StartDyn));
            PlcWeldLasOutputs.Add(new IObitDetails("oWL", nameof(WLO_StandBy), WLO_StandBy, "StandBy", 3210, obitWLO_StandBy));
            PlcWeldLasOutputs.Add(new IObitDetails("oWL", nameof(WLO_On), WLO_On, "On", 3210, obitWLO_On));
            PlcWeldLasOutputs.Add(new IObitDetails("oWL", nameof(WLO_ExtAct), WLO_ExtAct, "Ext. Act.", 3210, obitWLO_ExtAct));

            PlcWeldLasInputs.Add(new IObitDetails("iWL", nameof(WLI_Assigned), WLI_Assigned, "Assigned", 3310, ibitWLI_Assigned));
            PlcWeldLasInputs.Add(new IObitDetails("iWL", nameof(WLI_Fault), WLI_Fault, "Fault", 3310, ibitWLI_Fault));
            PlcWeldLasInputs.Add(new IObitDetails("iWL", nameof(WLI_PrgCompleted), WLI_PrgCompleted, "Prg. Completed", 3310, ibitWLI_PrgCompleted));
            PlcWeldLasInputs.Add(new IObitDetails("iWL", nameof(WLI_ProgActive), WLI_ProgActive, "Prg. Active", 3310, ibitWLI_ProgActive)); ;
            PlcWeldLasInputs.Add(new IObitDetails("iWL", nameof(WLI_Ready), WLI_Ready, "Ready", 3310, ibitWLI_Ready));
            PlcWeldLasInputs.Add(new IObitDetails("iWL", nameof(WLI_On), WLI_On, "On", 3310, ibitWLI_On));
            PlcWeldLasInputs.Add(new IObitDetails("iWL", nameof(WLI_ExtAct), WLI_ExtAct, "Ext. Act.", 3310, ibitWLI_ExtAct));

            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_BeamSourceOn), MLI_BeamSourceOn, "Beam Source is On", 3400, ibitMLI_BeamSourceOn));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_LaserReady), MLI_LaserReady, "Laser is Ready", 3400, ibitMLI_LaserReady));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_SafetyClosed), MLI_SafetyClosed, "Emgr. Circuit is Closed", 3400, ibitMLI_SafetyClosed));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_ShutterOpen), MLI_ShutterOpen, "Shutter is Open", 3400, ibitMLI_ShutterOpen));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_BeamExpInPos), MLI_BeamExpInPos, "Beam Expander in Position", 3400, ibitMLI_BeamExpInPos));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_LaserStable), MLI_LaserStable, "Laser is Stable", 3400, ibitMLI_LaserStable));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_LaserMonitoring), MLI_LaserMonitoring, "Laser Monitoring Reacted", 3400, ibitMLI_LaserMonitoring));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_Malfunction), MLI_Malfunction, "Malfunction", 3400, ibitMLI_Malfunction));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_PilotLaserOn), MLI_PilotLaserOn, "Pilot Laser is On", 3400, ibitMLI_PilotLaserOn));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_WarningLampOn), MLI_WarningLampOn, "Warning Lamp is On", 3400, ibitMLI_WarningLampOn));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_QswitchOn), MLI_QswitchOn, "Q-switch Trigger is On", 3400, ibitMLI_QswitchOn));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_AutoDeactivation), MLI_AutoDeactivation, "Autodeactivation activated", 3400, ibitMLI_AutoDeactivation));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_KeySwitchLock), MLI_KeySwitchLock, "Key Switch Locking is Closed", 3400, ibitMLI_KeySwitchLock));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_EmgrCircClosed), MLI_EmgrCircClosed, "Emergency off Circuit is Closed", 3400, ibitMLI_EmgrCircClosed));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_HeatingPhaseOn), MLI_HeatingPhaseOn, "Heating Phase is On", 3400, ibitMLI_HeatingPhaseOn));
            PlcMarkLasInputs.Add(new IObitDetails("iML", nameof(MLI_ControlIsOn), MLI_ControlIsOn, "Control is On", 3400, ibitMLI_ControlIsOn));

            PlcHflags.Add(new IObitDetails("HFL", nameof(HF_UseWelding), HF_UseWelding, "Use Welding", 133, ibitHF_UseWelding));
            PlcHflags.Add(new IObitDetails("HFL", nameof(HF_UseMarking), HF_UseMarking, "Use Marking", 133, ibitHF_UseMarking));
            PlcHflags.Add(new IObitDetails("HFL", nameof(HF_UseStation1), HF_UseStation1, "Use St.1", 133, ibitHF_UseStation1));
            PlcHflags.Add(new IObitDetails("HFL", nameof(HF_UseStation2), HF_UseStation2, "Use St.2", 133, ibitHF_UseStation2));
            PlcHflags.Add(new IObitDetails("HFL", nameof(HF_UseStation3), HF_UseStation3, "Use St.3", 133, ibitHF_UseStation3));
        }

        public static void ModifyOutputBit(IObitDetails bd, mcOMRON.MemoryArea memArea = mcOMRON.MemoryArea.CIO_Bit)
        {
            SetOutCommand tcpCmd = new SetOutCommand();
            tcpCmd.cmdID = 'B';
            tcpCmd.memArea = memArea;
            tcpCmd.RegAddress = bd.CIOaddress;
            tcpCmd.BitPosition = bd.BitPosition;
            tcpCmd.BitValue = bd.BitValue ? (byte)1 : (byte)0;
            lock (onLock)
            {
                MainWindow.TCP.SetOutCommands.Enqueue(tcpCmd);
            }
            while (MainWindow.TCP.SetOutCommands.Count > 0 && MainWindow.TCP.SetOutCommands.Contains(tcpCmd)) Thread.Sleep(1);
            return;
        }
        public static void ModifyOutputWord(mcOMRON.MemoryArea mArea, ushort mAddr, ushort mValue)
        {
            SetOutCommand tcpCmd = new SetOutCommand();
            tcpCmd.cmdID = 'W';
            tcpCmd.memArea = mArea;
            tcpCmd.RegAddress = mAddr;
            tcpCmd.WordValue = mValue;
            lock (onLock)
            {
                MainWindow.TCP.SetOutCommands.Enqueue(tcpCmd);
            }
            return;
        }
        public static void ModifyOutputDoubleWord(mcOMRON.MemoryArea mArea, ushort mAddr, uint mValue)
        {
            SetOutCommand tcpCmd = new SetOutCommand();
            tcpCmd.cmdID = 'D';
            tcpCmd.memArea = mArea;
            tcpCmd.RegAddress = mAddr;
            tcpCmd.DWordValue = mValue;
            lock (onLock)
            {
                MainWindow.TCP.SetOutCommands.Enqueue(tcpCmd);
            }
            return;
        }
        public static void ModifyOutputByName(string bg, string oName, byte bv)
        {
            var o = new IObitDetails("", "", true, "", 0, 0);
            if (bg == "CO") o = PlcCommonOutputs.FirstOrDefault(x => (x.BitName == oName));
            else if (bg == "oP1") o = PlcPos1Outputs.FirstOrDefault(x => (x.BitName == oName));
            else if (bg == "oP2") o = PlcPos2Outputs.FirstOrDefault(x => (x.BitName == oName));
            else if (bg == "oP3") o = PlcPos3Outputs.FirstOrDefault(x => (x.BitName == oName));
            else return;
            if (o != null)
            {
                SetOutCommand tcpCmd = new SetOutCommand();
                tcpCmd.cmdID = 'B';
                tcpCmd.memArea = mcOMRON.MemoryArea.CIO_Bit;
                tcpCmd.RegAddress = o.CIOaddress;
                tcpCmd.BitPosition = o.BitPosition;
                tcpCmd.BitValue = bv;
                MainWindow.TCP.SetOutCommands.Enqueue(tcpCmd);
            }

        }
    }
    public class IObitDetails : INotifyPropertyChanged
    {
        bool _bitValue;
        public string BitGroup { get; set; }
        public string BitName { get; set; }
        public string BitText { get; set; }
        public ushort CIOaddress { get; set; }
        public byte BitPosition { get; set; }
        public bool BitValue
        {
            get { return _bitValue; }
            set { SetField(ref _bitValue, value); }
        }
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
        public IObitDetails(string bg, string bn, bool bv, string bt, ushort adr, byte pos)
        {
            BitGroup = bg;
            BitName = bn;
            BitValue = bv;
            BitText = bt;
            CIOaddress = adr;
            BitPosition = pos;
        }
    }
}
