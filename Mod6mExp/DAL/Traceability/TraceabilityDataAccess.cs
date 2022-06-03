using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTracking;
using Machine.Entities;
using DataTracking.Models;
using System.Data;
using System.Data.SqlClient;

namespace Machine.DAL.Traceability
{
    public class TraceabilityDataAccess
    {
        private string UserID;
        private string Password;
        public string TraceabilityServer;
        public string TraceabilityDBName;
        public string ApplicationName;
         

        DataTracking.DAL.ITraceAbility traceabilityOp;


        public TraceabilityDataAccess( string TraceabilityServer,string TraceabilityDBName,string ApplicationName)
        {
            this.UserID = "Machine";
            this.Password = "qipe";
            this.TraceabilityServer = TraceabilityServer;
            this.TraceabilityDBName = TraceabilityDBName;
            this.ApplicationName = ApplicationName;
            InitConnection();
        }

        public void InitConnection()
        {
            var connectionStringInfo = new DataTracking.Models.ConnectionStringInfo();
            connectionStringInfo = new DataTracking.Models.ConnectionStringInfo();
            connectionStringInfo.UserID = UserID;
            connectionStringInfo.Password = Password;
            connectionStringInfo.TraceabilityServer = TraceabilityServer;
            connectionStringInfo.TraceabilityDBName = TraceabilityDBName;
            connectionStringInfo.ApplicationName = ApplicationName;
            traceabilityOp = new DataTracking.DAL.TraceAbility(connectionStringInfo, false);
        }

        public void StartLot(DataTracking.Models.LotInfo lot, out int outputParamStatus, out string statusErrorDescription)
        {

            outputParamStatus = -1;
            statusErrorDescription = "Define";

            var result = traceabilityOp.StartLot(lot);
            if (!result.Success)
            {

                foreach (var item in result.MessagesList)
                {
                    outputParamStatus = result.ErrorCode;
                    statusErrorDescription = item;
                }
            }
            else
            {
                outputParamStatus = 0;
                statusErrorDescription = "";

            }
        }

        public void GetMachineDefects(DataTracking.Models.LotInfo lot, List<ScrapInfo> listDefects,out int outputParamStatus, out string statusErrorDescription)
        {
            outputParamStatus = -1;
            statusErrorDescription = "Define";


            var result = traceabilityOp.GetMachineDefects(lot);


            if (!result.Success)
            {

                foreach (var item in result.MessagesList)
                {
                    outputParamStatus = result.ErrorCode;
                    statusErrorDescription = item;
                }
            }
            else
            {
                listDefects = result.ResultObject;
                outputParamStatus = 0;
                statusErrorDescription = "";

            }
        }

        public void EndLotComponent(ComponentInfo cInfo, int usedComp, out int outputParamStatus, out string statusErrorDescription)
        {

            //var componentInfo = new DataTracking.Models.ComponentInfo();



            outputParamStatus = -1;
            statusErrorDescription = "Define";

            var result = traceabilityOp.EndLotComponent(cInfo, usedComp, false);

            if (!result.Success)
            {

                foreach (var item in result.MessagesList)
                {
                    outputParamStatus = result.ErrorCode;
                    statusErrorDescription = item;
                }
            }
            else
            {
                outputParamStatus = 0;
                statusErrorDescription = "";
            }
        }

        public  bool GetMod6ChipLot(string lot, out string chipLot, out string eMsg)
        {
            chipLot = "";
            eMsg = "";
            SqlConnection connectionRemote;

            String ConnectionString = "";
            ConnectionString += "user id=machine;";
            ConnectionString += "password=qipe;";
            ConnectionString += "server=" + Properties.Settings.Default.TraceabilityServerIP + ";";
            ConnectionString += "database=" + Properties.Settings.Default.TraceabilityDBName + ";";
            ConnectionString += "connection timeout=5";
            connectionRemote = new SqlConnection(ConnectionString);

            using (connectionRemote)
            using (SqlCommand cmd = new SqlCommand("[dbo].[GheckMod6Lot]", connectionRemote))
            {
                cmd.Parameters.Add("@ProdLot", SqlDbType.NVarChar, 10).Value = lot;
                cmd.Parameters.Add("@Lot", SqlDbType.NVarChar, 10).Direction = ParameterDirection.Output;
                try
                {
                    connectionRemote.Open();
                    cmd.ExecuteNonQuery();
                    if (cmd.Parameters["@Lot"].Value != DBNull.Value)
                        chipLot = (string)cmd.Parameters["@Lot"].Value;
                    return true;
                }
                catch (Exception ee)
                {
                    eMsg = ee.Message;
                    return false;
                }
            }
        }
        public bool GetMod6StartSN(string chipLot, int qty, out int startSN, out string eMsg)
        {
            startSN = 0;
            eMsg = "";
            SqlConnection connectionRemote;

            String ConnectionString = "";
            ConnectionString += "user id=machine;";
            ConnectionString += "password=qipe;";
            ConnectionString += "server=" + Properties.Settings.Default.TraceabilityServerIP + ";";
            ConnectionString += "database=MachinesTrace;";
            ConnectionString += "connection timeout=5";
            connectionRemote = new SqlConnection(ConnectionString);
            using (connectionRemote)
            using (SqlCommand cmd = new SqlCommand("[dbo].[GetMod6SNumber]", connectionRemote))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Lot", SqlDbType.NVarChar, 5).Value = chipLot;
                cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = qty;
                cmd.Parameters.Add("@IsCreate", SqlDbType.Bit).Value = true;
                cmd.Parameters.Add("@RetSNumber", SqlDbType.Int).Direction = ParameterDirection.Output;
                try
                {
                    connectionRemote.Open();
                    cmd.ExecuteNonQuery();
                    if (cmd.Parameters["@RetSNumber"].Value != DBNull.Value)
                        startSN = (int)cmd.Parameters["@RetSNumber"].Value;
                    return true;
                }
                catch (Exception ee)
                {
                    eMsg = ee.Message;
                    return false;
                }
            }
        }


    }
}
