using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using DataTracking.Models;

namespace Machine
{
	/// <summary>
	/// Interaction logic for WindowEndLot.xaml
	/// </summary>
	public partial class WindowEndLot : Window
	{
		bool WindowMoving = false;
		Point WindowMovingStartPoint = new Point();

		class ScrapItem
		{
			public int Code = 0;
			public string ScrapText = "";
		}
		List<ScrapItem> ScrapList = new List<ScrapItem>();

		public double SettingsMaxItemsPerColumn = 16;
		public double SettingsSrapItemHeight = 38;
		public double SettingsSrapItemWidth = 380;

		string ErrorString = "";
		//private int[] ErrorCodesArray;
		//string[,] ErrorCodesText;
		//public int PiecesOK = 0;
		//public int[] ScrapCodesArray = new int[0];
		//public int[] ScrapPieces = new int[0];
		public List<int[]> LotResults;
		private List<EndLotCodeItem> ListCodeControls = new List<EndLotCodeItem>();
		private int[] ExcludedErrorCodes = new int[0];
		private int iQuantity = 0;

		public WindowEndLot(int quantity, List<int[]> results, bool TotalPiecesChangable = false)
		{
			InitializeComponent();

			iQuantity = quantity;
			itemPiecesOK.ChangeEnabled = TotalPiecesChangable;
			LotResults = results;
			itemPiecesOK.Value = results[0][1];
		}

		public WindowEndLot(int quantity, List<int[]> results, int[] ExcludedCodes, bool TotalPiecesChangable = false)
		{
			InitializeComponent();

			iQuantity = quantity;
			itemPiecesOK.ChangeEnabled = TotalPiecesChangable;
			LotResults = results;
			int piecesNOK = 0;
			for (int i = 0; i < results.Count; i++)
			{
				if (results[i][0] != 0) piecesNOK += results[i][1];
			}
			itemPiecesOK.Value = iQuantity - piecesNOK;
			ExcludedErrorCodes = ExcludedCodes;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			int sum = 0;
			for (int j = 1; j < LotResults.Count; j++) sum += LotResults[j][1];
			itemPieces.Value = sum + itemPiecesOK.Value;

			// за да може да премества прозорец без рамка
			MouseDown += new MouseButtonEventHandler(TittleBar_MouseDown);
			MouseUp += new MouseButtonEventHandler(TittleBar_MouseUp);
			MouseMove += new MouseEventHandler(TittleBar_MouseMove);
			MouseLeave += new MouseEventHandler(TittleBar_MouseLeave);

			int res = LoadErrorCodesFromDB();
			if (res != 0)
			{
				MessageBox.Show("Грешка при извличане на кодовете от сървъра!\n\n" + res.ToString(), "Auto Chip Assembly", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			if (ExcludedErrorCodes != null)
			{
				for (int i = 0; i < ExcludedErrorCodes.Length; i++)
				{
					for (int j = 0; j < ScrapList.Count; j++)
					{
						if (ScrapList[j].Code == ExcludedErrorCodes[i])
						{
							ScrapList.RemoveAt(j);
							break;
						}
					}
				}
			}

			ShowControls();
		}

		#region Преместване на прозореца
		void TittleBar_MouseLeave(object sender, MouseEventArgs e)
		{
			WindowMoving = false;
		}

		void TittleBar_MouseMove(object sender, MouseEventArgs e)
		{
			if (WindowMoving)
			{
				Point p1 = e.GetPosition(this);
				Point p2 = this.PointToScreen(p1);
				Point p3 = new Point(p2.X - WindowMovingStartPoint.X, p2.Y - WindowMovingStartPoint.Y);
				this.Left = p3.X;
				this.Top = p3.Y;
			}
		}

		void TittleBar_MouseUp(object sender, MouseButtonEventArgs e)
		{
			WindowMoving = false;
		}

		void TittleBar_MouseDown(object sender, MouseButtonEventArgs e)
		{
			WindowMoving = true;
			WindowMovingStartPoint = e.GetPosition(this);
		}
		#endregion

		private int LoadErrorCodesFromDB()
		{
            var Trace = new DAL.Traceability.TraceabilityDataAccess(Properties.Settings.Default.TraceabilityServerIP,
            Properties.Settings.Default.TraceabilityDBName, Properties.Settings.Default.MachineName);
            DataTracking.Models.LotInfo structLot = new DataTracking.Models.LotInfo()
            {
                MachineID = Properties.Settings.Default.MachineID
            };

            int result = -1;
            string ResultText = "";
            List<ScrapInfo> listDefects = new List<ScrapInfo>();
            Trace.GetMachineDefects(structLot, listDefects, out result, out ResultText);

            foreach (var item in listDefects)
            {
                try
                {
                    ScrapList.Add(new ScrapItem() { Code = item.ScrapCode, ScrapText = item.ScrapCodeDescr });
                }
                catch
                {
                    return 10;
                }
            }
            return result;
		}

		private void ShowControls()
		{
			if (ScrapList.Count < 5)
			{
				for (int i = 0; i < ScrapList.Count; i++) gridMain.RowDefinitions.Add(new RowDefinition());
				gridMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(SettingsSrapItemWidth + 20) });
			}
			else
			{
				if (ScrapList.Count <= (SettingsMaxItemsPerColumn * 2))
				{
					for (int i = 0; i < (ScrapList.Count / 2 + ScrapList.Count % 2); i++)
					{
						gridMain.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(SettingsSrapItemHeight + 4) });
					}
					gridMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(SettingsSrapItemWidth + 20) });
					gridMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(SettingsSrapItemWidth + 20) });
				}
				else
				{
					for (int i = 0; i < (ScrapList.Count / 3 + ((ScrapList.Count % 3) > 0 ? 1 : 0)); i++)
					{
						gridMain.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(SettingsSrapItemHeight + 4) });
					}
					gridMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(SettingsSrapItemWidth + 20) });
					gridMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(SettingsSrapItemWidth + 20) });
					gridMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(SettingsSrapItemWidth + 20) });
				}
			}

			ListCodeControls.Clear();
			int col = 0, row = 0;
			for (int i = 0; i < ScrapList.Count; i++)
			{
				EndLotCodeItem code_item = new EndLotCodeItem();
				code_item.Name = "itemCode" + ScrapList[i].Code.ToString();	// ErrorCodesText[i, 0];
				code_item.Code = ScrapList[i].Code.ToString();	// ErrorCodesText[i, 0];
				code_item.Description = ScrapList[i].ScrapText;	// ErrorCodesText[i, 1];
				for (int j = 0; j < LotResults.Count; j++)
					if (LotResults[j][0] == ScrapList[i].Code)	//ErrorCodesText[i, 0])
					{
						code_item.Value = LotResults[j][1];
						var converter = new System.Windows.Media.BrushConverter();
						code_item.labelValue.Background = (Brush)converter.ConvertFromString("#FFFF1A1A");
						code_item.labelValue.Foreground = Brushes.White;
					}
				code_item.OnPlus += OnPlus;
				code_item.OnMinus += OnMinus;
				gridMain.Children.Add(code_item);
				ListCodeControls.Add(code_item);

				Grid.SetColumn(code_item, col);
				Grid.SetRow(code_item, row);
				row++;
				if (row >= gridMain.RowDefinitions.Count)
				{
					row = 0;
					col++;
				}
			}
		}

		void OnMinus(object sender, EventArgs e)
		{
			EndLotCodeItem item = (EndLotCodeItem)sender;
			if (item.Value > 0)
			{
				item.Value--;
				itemPiecesOK.Value++;

				var converter = new System.Windows.Media.BrushConverter();
				if (item.Value == 0)
				{
					item.labelValue.Background = (Brush)converter.ConvertFromString("#FFF1F0C9");
					item.labelValue.Foreground = Brushes.Black;
				}
				else
				{
					item.labelValue.Background = (Brush)converter.ConvertFromString("#FFFF1A1A");
					item.labelValue.Foreground = Brushes.White;
				}
			}
		}

		void OnPlus(object sender, EventArgs e)
		{
			EndLotCodeItem item = (EndLotCodeItem)sender;
			//if (itemPiecesOK.Value < Quantity)
			{
				item.Value++;
				itemPiecesOK.Value--;

				var converter = new System.Windows.Media.BrushConverter();
				if (item.Value == 0)
				{
					item.labelValue.Background = (Brush)converter.ConvertFromString("#FFF1F0C9");
					item.labelValue.Foreground = Brushes.Black;
				}
				else
				{
					item.labelValue.Background = (Brush)converter.ConvertFromString("#FFFF1A1A");
					item.labelValue.Foreground = Brushes.White;
				}
			}
		}

		private void itemPiecesOK_OnMinus(object sender, EventArgs e)
		{
			if (itemPiecesOK.Value > 0)
			{
				itemPiecesOK.Value--;
				itemPieces.Value--;
			}
		}

		private void itemPiecesOK_OnPlus(object sender, EventArgs e)
		{
			if (itemPieces.Value < iQuantity)
			{
				itemPiecesOK.Value++;
				itemPieces.Value++;
			}
		}

		private void buttonApply_Click(object sender, RoutedEventArgs e)
		{
			LotResults.Clear();
		LotResults.Add(new int[] { 0, (int)itemPiecesOK.Value });
			for (int i = 0; i < ListCodeControls.Count; i++)
			{
				if (ListCodeControls[i].Value > 0)
				{
					LotResults.Add(new int[] { int.Parse(ListCodeControls[i].Code), (int)ListCodeControls[i].Value });
				}
			}
			DialogResult = true;
		}
	}
}
