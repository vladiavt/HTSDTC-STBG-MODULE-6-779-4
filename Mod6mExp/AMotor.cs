// Base class for Motorcontrol
using System;

namespace Machine
{
    public abstract class AMotor
    {
        public string Name = "";
        public string Type = "";
        private float workSpeed_ = 10;
        private float workAccel_ = 1;
        public static float safetyDivider = 3;
        public static bool safety = false;
        public Func<float,bool> CheckSafeToMove = null;

        public float WorkSpeed
        {
            get { if (safety) { return workSpeed_ / safetyDivider; } else return workSpeed_; }   
            set { workSpeed_ = value; }    
        }
        public float WorkAccel
        {
            get { if (safety) { return workAccel_ / safetyDivider; } else return workAccel_; }
            set { workAccel_ = value; }
        }
        public abstract bool OnTarget();
        public abstract float ActualPos();
        public abstract float ActualVel();
        public abstract float TargetPos();
        public abstract int CurrentCommand();
        public abstract UInt16 GetStatus();
        public abstract bool GetError();
        public abstract bool GetEnabled();
        public abstract bool GetReferenced();
        public abstract bool GetCommunicationOK();
        public virtual void Home() {}
        public virtual void MoveAbsolute(float Position, float Velocity, float Acceleration) {}
        public virtual void MoveAbsolute(float Position) {}
        public virtual void MoveAbsoluteVelocity(float Velocity) {}
        public virtual void MoveAbsoluteVelocity(float Velocity, float Acceleration) {}
        public virtual void SetMaxCurrent(int current) { }
        public virtual bool GetNegativeLimit()
        {
            return false;
        }
        public virtual bool GetPositiveLimit()
        {
            return false;
        }
        public virtual void FaultReset() {}
        public virtual void Abort() { }
        public virtual void DriveEnable() {}
        public virtual void DriveDisable() {}
        public virtual float ActualFrc() { return 0; }
        public virtual int ActualMaxCur() { return 0; }
    }
}