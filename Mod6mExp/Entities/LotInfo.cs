using System;

namespace Machine.Entities
{
    public class LotInfo
    {
        public string Lot { get; set; }
        public string Product { get; set; }
        public string Order { get; set; }
        public short Quantity { get; set; }
        public string Operator { get; set; }
        public string ShiftLeader { get; set; }
        public string MachineID { get; set; }
        public long LotIdent { get; set; }
        public int PiecesLeft { get; set; }
        public int PiecesOK { get; set; }
        public int PiecesNOK { get; set; }
        public string ChipLot { get; set; }
        public int StartSN { get; set; }
        public int CurrentSN { get; set; }

        public LotInfo()
        {
            MachineID = Properties.Settings.Default.MachineID;
        }
    }
}
