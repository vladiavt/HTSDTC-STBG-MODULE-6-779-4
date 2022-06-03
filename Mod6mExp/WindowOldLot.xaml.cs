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

namespace Machine
{
	/// <summary>
	/// Interaction logic for WindowOldLot.xaml
	/// </summary>
	public partial class WindowOldLot : Window
	{
		string LocalLot = "";
		int LocalPieces = 0;
		int LocalPiecesNOK = 0;
		int LocalQuantity = 0;
		DateTime LocalDate;

		public WindowOldLot(string lot, int pieces, int pieces_nok, int quantity, DateTime date)
		{
			InitializeComponent();
			LocalLot = lot;
			LocalPieces = pieces;
			LocalPiecesNOK = pieces_nok;
			LocalQuantity = quantity;
			LocalDate = date;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			ShowText();
		}

		private void ShowText()
		{
			Text1.Text = "";
			Text1.Inlines.Add("Серията ");
			Text1.Inlines.Add(new Bold(new Run(LocalLot) { Foreground = Brushes.Yellow }));
			Text1.Inlines.Add(" вече е въвеждана в машината на \n" + LocalDate.ToLongDateString() + "  " + LocalDate.ToLongTimeString());

			if (LocalPieces < LocalQuantity)
			{
				Text1.Inlines.Add(" и са обработени ");
				Text1.Inlines.Add(new Bold(new Run(LocalPieces.ToString()) { Foreground = Brushes.Yellow }));
				Text1.Inlines.Add(" модула.");
				Text1.Inlines.Add("\n   Годни:\t");
				Text1.Inlines.Add(new Bold(new Run((LocalPieces - LocalPiecesNOK).ToString())));
				Text1.Inlines.Add("\n   Брак:\t");
				Text1.Inlines.Add(new Bold(new Run(LocalPiecesNOK.ToString())));
				buttonStartLot.Visibility = Visibility.Visible;
				buttonCancelLot.Visibility = Visibility.Visible;
				buttonOK.Visibility = Visibility.Collapsed;
			}
			else
			{
				Text1.Inlines.Add(new Bold(new Run(" количеството е изразходвано.") { Foreground = Brushes.Pink }));
				buttonStartLot.Visibility = Visibility.Collapsed;
				buttonCancelLot.Visibility = Visibility.Collapsed;
				buttonOK.Visibility = Visibility.Visible;
			}
		}

		private void buttonStartLot_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}

		private void buttonCancelLot_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void buttonOK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		
	}
}