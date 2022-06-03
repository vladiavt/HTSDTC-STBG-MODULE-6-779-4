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
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using Machine.PDL.Base;

namespace Machine
{
	/// <summary>
	/// Interaction logic for NewLotWindow.xaml
	/// </summary>
	public partial class NewLotWindow : Window
	{
        public int Quantity = 0;
        public string Order = "";
        public string ChipLot;
        public int StartSN;
		bool WindowMoving = false;
		Point WindowMovingStartPoint = new Point();
		DispatcherTimer timerRefresh;
		BarcodeReader reader;
        private bool receivedLot = false, receivedComponent = false, receivedOperator = false;
		private string serverName = "", databaseName = "", tableName = "";

        public NewLotWindow(string ServerName, string DatabaseName, string TableName, BarcodeReader barcodeReader = null, bool ShowComponent1 = false, bool ShowComponent2 = false)
		{
			InitializeComponent();

			serverName = ServerName;
			databaseName = DatabaseName;
			tableName = TableName;

			FillComboBoxProducts();

			// за да може да премества прозорец без рамка
			MouseDown += new MouseButtonEventHandler(TittleBar_MouseDown);
			MouseUp += new MouseButtonEventHandler(TittleBar_MouseUp);
			MouseMove += new MouseEventHandler(TittleBar_MouseMove);

			if (!ShowComponent1 && !ShowComponent2)
			{
				GridControls.Height -= 35;
				GridControls.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Pixel);
			}
			else
			{
				GridControls.ColumnDefinitions[0].Width = new GridLength(100, GridUnitType.Pixel);
				GridControls.ColumnDefinitions[4].Width = new GridLength(0, GridUnitType.Pixel);
			}
			if (!ShowComponent1) GridControls.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Pixel);
			if (!ShowComponent2) GridControls.ColumnDefinitions[3].Width = new GridLength(0, GridUnitType.Pixel);

			if (barcodeReader != null)
			{
				reader = barcodeReader;

				timerRefresh = new DispatcherTimer();
				timerRefresh.Interval = TimeSpan.FromMilliseconds(100);
				timerRefresh.Tick += new EventHandler(timerRefresh_Tick);
				timerRefresh.Start();
			}
		}

		public void FillComboBoxProducts()
		{
			using (SqlConnection conn = new SqlConnection("user id=machine;password=qipe;server=" + serverName + ";database=" + databaseName + ";connection timeout=5"))
			using (SqlDataAdapter da = new SqlDataAdapter("select product from " + tableName, conn))
			{
				//da.SelectCommand.Parameters.Add("@product", SqlDbType.NVarChar, 16).Value = "Product";
				using (DataSet ds = new DataSet())
				{
					try
					{
						da.Fill(ds, tableName);
						comboBoxProduct.ItemsSource = ds.Tables[0].DefaultView;
						comboBoxProduct.DisplayMemberPath = ds.Tables[0].Columns["Product"].ToString();
						//comboBoxProduct.SelectedValuePath = ds.Tables[0].Columns["ID"].ToString();
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.ToString());
					}
				}
			}
		}

		public void SetWindowLocation(int left, int top)
		{
			WindowStartupLocation = WindowStartupLocation.Manual;
			Left = left;
			Top = top;
		}

		void timerRefresh_Tick(object sender, EventArgs e)
		{
            string errorMsg;
			if (reader.ReceivedLot)
			{
				reader.ReceivedLot = false;
				receivedLot = true;
				textBoxLot.Text = reader.Lot;
				comboBoxProduct.Text = reader.Product;
				textBoxOrder.Text = reader.Order;
				textBoxQuantity.Text = reader.Quantity.ToString();
                bool res = MainWindow.TraceabilityServer.GetMod6ChipLot(textBoxLot.Text, out ChipLot, out errorMsg);
                if (!res) MainWindow.UpdateLabel(ChipLotMessage, "Грешка при получаване на лота на чипа", Brushes.Red);
                else if (ChipLot == "0" || ChipLot == "") MainWindow.UpdateLabel(ChipLotMessage, "Недефиниран лот на чипа", Brushes.Red);
                else { textBoxChipLot.Text = ChipLot; MainWindow.UpdateLabel(ChipLotMessage, "Сравнете лота на чипа с фиша", Brushes.Red); }
            }

            if (reader.ReceivedComponent)
			{
				reader.ReceivedComponent = false;
                receivedComponent = true;
                if (textBoxComponentLot.Text == "")
				{
					textBoxComponentLot.Text = reader.ComponentLot;
					textBoxComponentProduct.Text = reader.ComponentProduct;
					textBoxComponentQuantity.Text = reader.ComponentQuantity.ToString();
				}
				else if (reader.ComponentProduct != textBoxComponentProduct.Text)
				{
					textBoxComponentLot2.Text = reader.ComponentLot;
					textBoxComponentProduct2.Text = reader.ComponentProduct;
					textBoxComponentQuantity2.Text = reader.ComponentQuantity.ToString();
				}
			}

			if (reader.ReceivedOperator)
			{
				reader.ReceivedOperator = false;
				receivedOperator = true;
				textBoxOperator.Text = reader.Operator;
			}

			//if (receivedLot && receivedOperator)
			//{
			//	try
			//	{
			//		Quantity = Convert.ToInt32(textBoxQuantity.Text);
			//		Order = textBoxOrder.Text;
			//		DialogResult = true;
			//	}
			//	catch
			//	{
			//		MessageBox.Show("Неприемливи стойности!", "Грешка!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			//	}
			//}
		}

		private void buttonOK_Click(object sender, RoutedEventArgs e)
		{
			try
			{
                if (textBoxChipLot.Text.Length == 0 || textBoxChipLot.Text.Length > 5)
                { MainWindow.UpdateLabel(ChipLotMessage, "Лотът на чипа е от 0 до 5 символа", Brushes.Red); return; }
                else if (textBoxChipLot.Text.Contains(" ")) { MainWindow.UpdateLabel(ChipLotMessage, "Лотът на чипа трябва да е без интервали", Brushes.Red); return; }
                Quantity = Convert.ToInt32(textBoxQuantity.Text);
                Order = textBoxOrder.Text;
                ChipLot = textBoxChipLot.Text;
                DialogResult = true;
			}
			catch
			{
				MessageBox.Show("Неприемлива стойност за количество!", "Грешка!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
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

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (timerRefresh != null) timerRefresh.Stop();
		}
	}
}
