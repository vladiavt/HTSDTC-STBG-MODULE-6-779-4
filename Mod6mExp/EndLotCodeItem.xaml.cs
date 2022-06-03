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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Machine
{
	/// <summary>
	/// Interaction logic for EndLotCodeItem.xaml
	/// </summary>
	public partial class EndLotCodeItem : UserControl
	{
		private int CurrentValue = 0;
		private bool _ChangeEnabled = true;
		public event EventHandler OnPlus;
		public event EventHandler OnMinus;

		public string Description
		{
			get { return this.textBloxkDescription.Text as string; }
			set
			{
				this.textBloxkDescription.Text = value;
			}
		}

		public string Code
		{
			get { return this.labelCode.Content as string; }
			set
			{
				this.labelCode.Content = value;
			}
		}

		public int Value
		{
			get { return this.CurrentValue; }
			set 
			{ 
				CurrentValue = value;
				labelValue.Content = CurrentValue.ToString();
			}
		}

		public double DescriptionFontSize
		{
			get { return textBloxkDescription.FontSize; }
			set { textBloxkDescription.FontSize = value; }
		}

		public bool ChangeEnabled
		{
			get { return _ChangeEnabled; }
			set 
			{
				_ChangeEnabled = value;
				buttonMinus.Visibility = _ChangeEnabled ? Visibility.Visible : Visibility.Collapsed;
				buttonPlus.Visibility = _ChangeEnabled ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public EndLotCodeItem() : this("","")
		{
			
		}

		public EndLotCodeItem(string scrapCode,string scrapDescription)
		{
			InitializeComponent();
			this.Code = scrapCode;
			this.Description = scrapDescription;
		}

		private void buttonPlus_Click(object sender, RoutedEventArgs e)
		{
			if (OnPlus != null) OnPlus(this, new EventArgs());
		}

		private void buttonMinus_Click(object sender, RoutedEventArgs e)
		{
			if (OnMinus != null) OnMinus(this, new EventArgs());
		}
	}
}
