using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Controls;
using System.Windows;
using System.IO;

namespace Machine
{

	public static class Gen
	{
		public const string TEMPORAL_FILE_EXE = "update.tmp";
		public static string pass = "caselogic";
		static public bool bPasswordReady = false;
		public static Queue<string> MachineStatusQueue = new Queue<string>();
	

		/// <summary>
		/// Зареждане на DataGridView с таблица от база данни
		/// </summary>
		/// <param name="ColumnsList">Списък с колоните от таблицата разделени със запетая. Пр. "Product,Lot,Order" или "*"</param>
		public static void FillDataGrid(DataGrid dg, string ServerName, string DatabaseName, string TableName, string ColumnsList)
		{
			string ConnectionString = "user id=machine;password=qipe;server=" + ServerName + ";database=" + DatabaseName + ";connection timeout=5";
			string CmdString = string.Empty;
			using (SqlConnection con = new SqlConnection(ConnectionString))
			{
				using (SqlCommand cmd = new SqlCommand("select " + ColumnsList + " from " + TableName, con))
				{
					SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
					DataSet dataSet = new DataSet();
					dataAdapter.Fill(dataSet, TableName);
					dg.ItemsSource = dataSet.Tables[TableName].DefaultView;
				}
			}  
		}

		/// <summary>
		/// Зареждане на DataGridView с таблица от база данни
		/// </summary>
		/// <param name="ColumnsList">Списък с колоните от таблицата разделени със запетая. Пр. "Product,Lot,Order" или "*"</param>
		public static void FillDataGrid(DataGrid dg, string ServerName, string DatabaseName, string TableName, string Username, string Password, string ColumnsList)
		{
			string ConnectionString = "user id=" + Username + ";password=" + Password + ";server=" + ServerName + ";database=" + DatabaseName + ";connection timeout=5";
			string CmdString = string.Empty;
			using (SqlConnection con = new SqlConnection(ConnectionString))
			{
				using (SqlCommand cmd = new SqlCommand("select " + ColumnsList + " from " + TableName, con))
				{
					try
					{
						SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
						DataSet dataSet = new DataSet();
						dataAdapter.Fill(dataSet, TableName);
						dg.ItemsSource = dataSet.Tables[TableName].DefaultView;
					}
					catch (Exception ex)
					{
						MessageBox.Show("Грешка при запълване на таблицата от сървъра!\n\n" + ex.ToString());
					}
				}
			}
		}

		/// <summary>
		/// Зареждане на DataGrid с таблица от база данни
		/// </summary>
		/// <param name="ColumnsList">Списък с колоните от таблицата разделени със запетая. Пр. "Product,Lot,Order" или "*"</param>
		public static void FillDataGrid(DataGrid dg, ref SqlConnection sqlConnection, ref SqlDataAdapter dataAdapter, ref DataSet dataSet, string ServerName, string DatabaseName, string TableName, string ColumnsList, string WhereClause = "")
		{
			string ConnectionString = "user id=machine;password=qipe;server=" + ServerName + ";database=" + DatabaseName + ";connection timeout=5";
			string CmdString = string.Empty;
			sqlConnection = new SqlConnection(ConnectionString);
			if (WhereClause != "" && !WhereClause.Contains("where")) WhereClause = "where " + WhereClause;
			using (SqlCommand cmd = new SqlCommand("select " + ColumnsList + " from " + TableName + " " + WhereClause, sqlConnection))
			{
				try
				{
					dataAdapter = new SqlDataAdapter(cmd);
					dataSet = new DataSet();
					dataAdapter.Fill(dataSet, TableName);
					dg.ItemsSource = dataSet.Tables[TableName].DefaultView;
				}
				catch (Exception ex)
				{
					MessageBox.Show("Грешка при запълване на таблицата от сървъра!\n\n" + ex.ToString());
				}
			}
		}

		public static void SetupDataGridColumnsWidth(DataGrid dg, string[] HeaderTexts, int[] ColumnsWidth)
		{
			if (dg.Columns.Count == 0) return;
			int index = 0;
			for (int i = 0; i < dg.Columns.Count; i++)
			{
				if (dg.Columns[i].Visibility == System.Windows.Visibility.Hidden) continue;
				if (HeaderTexts.Length < (index + 1)) break;
				else dg.Columns[i].Header = HeaderTexts[index];

				try
				{
					if (ColumnsWidth.Length < (index + 1)) break;
					else dg.Columns[i].Width = ColumnsWidth[index];
				}
				catch { }
				index++;
			}

			if (dg.Columns[dg.Columns.Count - 1].Visibility == System.Windows.Visibility.Visible)
				dg.Columns[dg.Columns.Count - 1].Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);
			else
			{
				try
				{
					dg.Columns[dg.Columns.Count - 2].Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);
				}
				catch { }
			}
		}

		//public static void FillComboBoxProducts(ComboBox comboBox, string ServerName, string DatabaseName, string TableName)
		//{
		//	String ConnectionString = "user id=rch;password=rchrch!;server=" + ServerName + ";database=" + DatabaseName + ";connection timeout=5";
		//	SqlDataAdapter da;
		//	DataSet ds;
		//	try
		//	{
		//		using (SqlConnection con = new SqlConnection(ConnectionString))
		//		{
		//			using (SqlCommand cmd = con.CreateCommand())
		//			{
		//				cmd.CommandType = CommandType.Text;
		//				cmd.CommandText = "SELECT @product FROM " + TableName;
		//				cmd.Parameters.Add("@product", SqlDbType.NVarChar, 16).Value = "Product1";
		//				da = new SqlDataAdapter(cmd);
		//				ds = new DataSet();
		//				da.Fill(ds, TableName);
		//				comboBox.DataContext = ds.Tables[TableName];
		//				comboBox.DisplayMemberPath = ds.Tables[TableName].Columns[0].ToString();
		//			}
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		MessageBox.Show(ex.ToString());
		//	}
		//}
		
		public static int GetCRC16(byte[] message)
		{
			ushort CRCFull = 0xFFFF;
			char CRCLSB;

			for (int i = 0; i < (message.Length) - 2; i++)
			{
				CRCFull = (ushort)(CRCFull ^ message[i]);

				for (int j = 0; j < 8; j++)
				{
					CRCLSB = (char)(CRCFull & 0x0001);
					CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

					if (CRCLSB == 1) CRCFull = (ushort)(CRCFull ^ 0xA001);
				}
			}
			return CRCFull;
		}

		public static void LogTextFile(string log)
		{
			StreamWriter sw = File.AppendText("log.txt");
			sw.WriteLine(DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss") + "     " + log);
			sw.Close();
		}
		

	}
}