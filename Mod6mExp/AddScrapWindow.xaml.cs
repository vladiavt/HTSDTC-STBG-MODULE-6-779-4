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
using System.Windows.Media.Effects;
using DataTracking.Models;

namespace Machine
{
	/// <summary>
	/// Interaction logic for AddScrapWindow.xaml
	/// </summary>
	public partial class AddScrapWindow : Window
	{
		bool WindowMoving = false;
		Point WindowMovingStartPoint = new Point();

		public int Result = 0;
		private int[] ErrorCodesArray;
		string ErrorString = "";
		private int[] ExcludedErrorCodes = new int[0];
		string[,] ErrorCodesText;

		public AddScrapWindow()
		{
			InitializeComponent();

			int res = LoadErrorCodesFromDB();
			if (res != 0)
			{
				MessageBox.Show("Грешка при извличане на кодовете от сървъра! \n\n" + res.ToString(), "Auto Chip Assembly", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// за да може да премества прозорец без рамка
			MouseDown += new MouseButtonEventHandler(TittleBar_MouseDown);
			MouseUp += new MouseButtonEventHandler(TittleBar_MouseUp);
			MouseMove += new MouseEventHandler(TittleBar_MouseMove);
			MouseLeave += new MouseEventHandler(TittleBar_MouseLeave);

			if (ExcludedErrorCodes != null)
			{
				if (ExcludedErrorCodes.Length > 0)
				{
					List<int> error_codes = new List<int>();
					error_codes.AddRange(ErrorCodesArray);
					for (int i = 0; i < ExcludedErrorCodes.Length; i++) error_codes.Remove(ExcludedErrorCodes[i]);
					ErrorCodesArray = error_codes.ToArray();
				}
			}
			ShowButtons();

			//MessageBox.Show(gridButtons.RowDefinitions.Count.ToString());
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
            //exists = false;
            ErrorString = "";

            String ConnectionString = "";
            ConnectionString += "user id=machine;";
            ConnectionString += "password=qipe;";
            ConnectionString += "server=" + Properties.Settings.Default.TraceabilityServerIP + ";";
            ConnectionString += "database=" + Properties.Settings.Default.TraceabilityDBName + ";";
            ConnectionString += "connection timeout=5";
            SqlConnection connectionLocal = new SqlConnection(ConnectionString);

            try
            {
                connectionLocal.Open();
            }
            catch (Exception e)
            {
                ErrorString = e.Message;
                return 1;
            }

            // read cuurent values
            SqlDataReader reader = null;
            SqlCommand commandRead = new SqlCommand(@"SELECT DefectsByOperations.Defect, 
																											 DefectsByOperations.TaskID, 
																											 DefectsByOperations.MachineId, 
																											 Defects.ProbDescr, 
																											 Defects.DefectDescriptionBG
																								FROM DefectsByOperations INNER JOIN Defects ON DefectsByOperations.Defect = Defects.ProbID
																								WHERE (DefectsByOperations.TaskID = 
																												(SELECT TaskId FROM Machines where Machine = '" + Properties.Settings.Default.MachineID + "'))"
                                                                                            , connectionLocal);
            try { reader = commandRead.ExecuteReader(); }
            catch
            {
                try { reader = commandRead.ExecuteReader(); }
                catch (Exception e)
                {
                    ErrorString = e.Message;
                    return 2;
                }
            }

            List<int> error_codes = new List<int>();
            List<string> error_str = new List<string>();

            while (reader.Read())
            {
                try
                {
                    error_codes.Add(Convert.ToInt32(reader["Defect"].ToString()));
                    error_str.Add(reader["DefectDescriptionBG"].ToString());
                }
                catch
                {
                }
            }

            ErrorCodesText = new string[error_codes.Count, 2];
            ErrorCodesArray = new int[error_codes.Count];
            for (int i = 0; i < error_codes.Count; i++)
            {
                ErrorCodesArray[i] = error_codes[i];
                ErrorCodesText[i, 0] = error_codes[i].ToString();
                ErrorCodesText[i, 1] = error_str[i].ToString();
            }

            reader.Close();
            return 0;
        }

        //private void ShowButtons()
        //{
        //  if (ErrorCodesArray.Length < 5)
        //  {
        //    for (int i = 0; i < ErrorCodesArray.Length; i++) gridButtons.RowDefinitions.Add(new RowDefinition());
        //  }
        //  else
        //  {
        //    for (int i = 0; i < (ErrorCodesArray.Length / 2 + ErrorCodesArray.Length % 2); i++) gridButtons.RowDefinitions.Add(new RowDefinition());
        //    gridButtons.ColumnDefinitions.Add(new ColumnDefinition());
        //    gridButtons.ColumnDefinitions.Add(new ColumnDefinition());
        //  }

        //  for (int i = 0; i < ErrorCodesArray.Length; i++)
        //  {
        //    Button b = new Button();
        //    b.Width = 300;
        //    b.Height = 30;
        //    b.Content = "[" + ErrorCodesArray[i].ToString() + "]  " + GetErrorText(ErrorCodesArray[i]);

        //    gridButtons.Children.Add(b);

        //    //Grid.SetColumn(b, 0);
        //    Grid.SetRow(b, i);
        //    if (i % 2 == 0) Grid.SetColumn(b, 0);
        //    else Grid.SetColumn(b, 1);
        //    gridButtons.Children.Add(b);
        //  }
        //}

        private void ShowButtons()
		{
			if (ErrorCodesArray.Length < 5)
			{
				Width = 305;
				Height = 135 + ErrorCodesArray.Length * 50;
			}
			else
			{
				Width = 680;
				Height = 150 + (ErrorCodesArray.Length / 2 + ErrorCodesArray.Length % 2) * 50;
			}

			int x = 15, y = 0;
			bool second_col = false;

			for (int i = 0; i < ErrorCodesArray.Length; i++)
			{
				Button b = new Button();
				b.Name = "buttonScrap" + ErrorCodesArray[i].ToString();
				b.Content = "";
				b.Style = (Style)FindResource("AppDefaultButtonStyle");
				b.Background = Brushes.Red;

				b.HorizontalAlignment = HorizontalAlignment.Left;
				b.VerticalAlignment = VerticalAlignment.Top;
				b.Margin = new Thickness(x, y, 0, 0);
				b.Width = 320;
				b.Height = 45;

				b.Click +=new RoutedEventHandler(onScrapButton_click);
				gridButtons.Children.Add(b);

				// етикет с кода за брак в/у бутона
				Label lb = new Label();
				lb.Content = ErrorCodesArray[i].ToString();
				lb.HorizontalAlignment = HorizontalAlignment.Left;
				lb.VerticalAlignment = VerticalAlignment.Top;
				lb.Width = 65;
				lb.Height = 40;
				lb.Foreground = Brushes.White;
				lb.FontFamily = new FontFamily("Arial");
				lb.FontSize = 24;
				lb.FontWeight = FontWeights.Bold;
				lb.Margin = new Thickness(x + 5, y + 4, 0, 0);
				lb.IsHitTestVisible = false;
				DropShadowEffect dse = new DropShadowEffect();
				dse.Color = Brushes.Black.Color;
				dse.Direction = 320;
				dse.ShadowDepth = 1;
				dse.Opacity = 0.5;
				lb.Effect = dse;
				gridButtons.Children.Add(lb);

				// етикет за текста за брак
				TextBlock txb = new TextBlock();
				txb.Text = GetErrorText(ErrorCodesArray[i]);
				txb.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
				txb.LineHeight = 16;
				txb.Foreground = Brushes.White;
				txb.FontSize = 15;
				txb.FontWeight = FontWeights.Medium;
				txb.TextWrapping = TextWrapping.Wrap;
				txb.TextAlignment = TextAlignment.Left;
				txb.HorizontalAlignment = HorizontalAlignment.Left;
				txb.VerticalAlignment = VerticalAlignment.Center;
				txb.Padding = new Thickness(65, 0, 0, 0);
				txb.IsHitTestVisible = false;
				DropShadowEffect ds = new DropShadowEffect();
				ds.Color = Brushes.Black.Color;
				ds.ShadowDepth = 0.2;
				ds.Opacity = 1;
				txb.Effect = ds;

				Border border = new Border();
				border.HorizontalAlignment = HorizontalAlignment.Left;
				border.VerticalAlignment = VerticalAlignment.Top;
				border.Width = 320;
				border.Height = 45;
				border.Margin = new Thickness(x, y, 0, 0);
				border.Child = txb;
				gridButtons.Children.Add(border);

				y += 50;
				if (ErrorCodesArray.Length > 4 && i + 1 >= (ErrorCodesArray.Length / 2 + ErrorCodesArray.Length % 2) && !second_col)
				{
					x = 350;
					y = 0;
					second_col = true;
				}
			}
		}

		public void ExcludeCodes(params int[] codes)
		{
			ExcludedErrorCodes = codes;
		}

		private string GetErrorText(int error_code)
		{
			for (int i = 0; i < ErrorCodesText.Length / 2; i++) if (error_code.ToString() == ErrorCodesText[i, 0]) return ErrorCodesText[i, 1];
			return "";
		}

		private void onScrapButton_click(object sender, RoutedEventArgs e)
		{
			Result = 0;
			Control b = (Control)sender;
			string str = b.Name.Remove(0, 11);
			try
			{
				Result = Convert.ToInt32(str);
				DialogResult = true;
				Close();
			}
			catch { }
		}

	}
}
