using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace Machine.DAL.Local
{
    public class DBClass
    {
        private string LocalTableName = "Sensors";
        private string ConnectionString = "";

        public DBClass(string LocalDatabaseName)
        {
            ConnectionString = "user id=machine;password=qipe;server=127.0.0.1;database=" + LocalDatabaseName + ";connection timeout=5";
        }

        public bool IsTableEmpty()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "SELECT COUNT(*) FROM TableInfo WHERE Loaded=1";
                        int countLoadedPositions = (int)cmd.ExecuteScalar();
                        return countLoadedPositions == 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Грешка при проверка за празна маса!" + ex.ToString());
                    return false;
                }
            }
        }

    }
}
