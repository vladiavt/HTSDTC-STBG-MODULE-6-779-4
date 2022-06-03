using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;

namespace Machine
{
	public enum MyMessageBoxResult { OK, Retry, Cancel, Yes, No }
	public enum MessageBoxButtons { OK, OKCancel, OKRetryCancel, RetryCancel, YesNo, YesNoCancel }

	class MyMessageBox
	{
		private static Window wnd;
		private static Button buttonOK;
		private static Button buttonCancel;
		private static Button buttonRetry;
		private static Button buttonYes;
		private static Button buttonNo;
		
		private static MyMessageBoxResult Result = MyMessageBoxResult.Cancel;

		public static MyMessageBoxResult Show(string Message)
		{
			wnd = new Window() { MinWidth = 200 };
			wnd.SizeToContent = SizeToContent.WidthAndHeight;

			Grid WindowGrid = new Grid();
			WindowGrid.RowDefinitions.Add(new RowDefinition());
			WindowGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });

			TextBlock labelMessage = new TextBlock()
			{
				Text = Message,
				TextWrapping = TextWrapping.Wrap,
				MaxWidth = 500,
				Margin = new Thickness(20, 20, 30, 20),
				HorizontalAlignment = HorizontalAlignment.Center
			};
			Grid.SetRow(labelMessage, 0);
			WindowGrid.Children.Add(labelMessage);

			BrushConverter bc = new BrushConverter();
			StackPanel stackPanelButtons = new StackPanel() { Orientation = Orientation.Horizontal, FlowDirection = FlowDirection.RightToLeft, Background = (Brush)bc.ConvertFrom("#EEEEEE") };
			Grid.SetRow(stackPanelButtons, 1);
			WindowGrid.Children.Add(stackPanelButtons);

			SetButtons(stackPanelButtons);

			wnd.Content = WindowGrid;
			wnd.ShowDialog();
			return Result;
		}

		public static MyMessageBoxResult Show(string Message, string Title, MessageBoxButtons Buttons, MessageBoxImage CurentImage)
		{
			wnd = new Window() { MaxWidth = 500 };
			wnd.SizeToContent = SizeToContent.WidthAndHeight;

			wnd.Title = Title;

			Grid WindowGrid = new Grid();
			WindowGrid.RowDefinitions.Add(new RowDefinition());
			WindowGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });

			Grid UpperGrid = new Grid();
			UpperGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(70) });
			UpperGrid.ColumnDefinitions.Add(new ColumnDefinition());

			Image messageImage = new Image() { Stretch = Stretch.None, Margin = new Thickness(10), SnapsToDevicePixels = true };

			SetImage(messageImage, CurentImage);
			UpperGrid.Children.Add(messageImage);

			TextBlock labelMessage = new TextBlock() { Text = Message, TextWrapping = TextWrapping.Wrap, MaxWidth = 500, Margin = new Thickness(20, 20, 30, 20) };
			Grid.SetColumn(labelMessage, 1);
			UpperGrid.Children.Add(labelMessage);

			BrushConverter bc = new BrushConverter();
			StackPanel stackPanelButtons = new StackPanel() 
			{ 
				Orientation = Orientation.Horizontal, 
				FlowDirection = FlowDirection.RightToLeft, 
				Background = (Brush)bc.ConvertFrom("#EEEEEE"),
				MinWidth = 265
			};
			Grid.SetRow(stackPanelButtons, 1);

			WindowGrid.Children.Add(UpperGrid);
			WindowGrid.Children.Add(stackPanelButtons);

			SetButtons(stackPanelButtons, Buttons);

			wnd.Content = WindowGrid;
			wnd.ShowDialog();
			return Result;
		}

		static void SetImage(Image ImageControl, MessageBoxImage CurentImage)
		{
			switch (CurentImage)
			{
				case MessageBoxImage.Error:
					ImageControl.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Error.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
					break;
				case MessageBoxImage.Information:
					ImageControl.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Information.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
					break;
				case MessageBoxImage.Question:
					ImageControl.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Question.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
					break;
				case MessageBoxImage.Warning:
					ImageControl.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Warning.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
					break;
			}
		}

		static void SetButtons(StackPanel stackpanel, MessageBoxButtons buttons_set = MessageBoxButtons.OK)
		{
			switch (buttons_set)
			{
				case MessageBoxButtons.OK:
					buttonOK = new Button() { Content = "OK", Width = 75, Height = 26, Margin = new Thickness(10, 0, 0, 0) };
					buttonOK.Click += buttonOK_Click;
					stackpanel.Children.Add(buttonOK);
					break;

				case MessageBoxButtons.OKCancel:
					buttonCancel = new Button() { Content = "Cancel", Width = 75, Height = 26, Margin = new Thickness(10, 0, 0, 0) };
					buttonCancel.Click += buttonCancel_Click;
					stackpanel.Children.Add(buttonCancel);

					buttonOK = new Button() { Content = "OK", Width = 75, Height = 26, Margin = new Thickness(10, 0, 0, 0) };
					buttonOK.Click += buttonOK_Click;
					stackpanel.Children.Add(buttonOK);
					break;

				case MessageBoxButtons.OKRetryCancel:
					buttonCancel = new Button() { Content = "Cancel", Width = 75, Height = 26, Margin = new Thickness(10, 0, 0, 0) };
					buttonCancel.Click += buttonCancel_Click;
					stackpanel.Children.Add(buttonCancel);

					buttonRetry = new Button() { Content = "Retry", Width = 75, Height = 26, Margin = new Thickness(10, 0, 0, 0) };
					buttonRetry.Click += buttonRetry_Click;
					stackpanel.Children.Add(buttonRetry);

					buttonOK = new Button() { Content = "OK", Width = 75, Height = 26, Margin = new Thickness(10, 0, 0, 0) };
					buttonOK.Click += buttonOK_Click;
					stackpanel.Children.Add(buttonOK);
					break;

				case MessageBoxButtons.RetryCancel:
					buttonCancel = new Button() { Content = "Cancel", Width = 75, Height = 26, Margin = new Thickness(10, 0, 0, 0) };
					buttonCancel.Click += buttonCancel_Click;
					stackpanel.Children.Add(buttonCancel);

					buttonRetry = new Button() { Content = "Retry", Width = 75, Height = 26, Margin = new Thickness(10, 0, 0, 0) };
					buttonRetry.Click += buttonRetry_Click;
					stackpanel.Children.Add(buttonRetry);
					break;

				case MessageBoxButtons.YesNo:
					buttonNo = new Button() { Content = "No", Width = 75, Height = 26, Margin = new Thickness(10, 0, 0, 0) };
					buttonNo.Click += buttonNo_Click;
					stackpanel.Children.Add(buttonNo);

					buttonYes = new Button() { Content = "Yes", Width = 75, Height = 26, Margin = new Thickness(10, 0, 0, 0) };
					buttonYes.Click += buttonYes_Click;
					stackpanel.Children.Add(buttonYes);
					break;

				case MessageBoxButtons.YesNoCancel:
					buttonCancel = new Button() { Content = "Cancel", Width = 75, Height = 26, Margin = new Thickness(10, 0, 0, 0) };
					buttonCancel.Click += buttonCancel_Click;
					stackpanel.Children.Add(buttonCancel);

					buttonNo = new Button() { Content = "No", Width = 75, Height = 26, Margin = new Thickness(10, 0, 0, 0) };
					buttonNo.Click += buttonNo_Click;
					stackpanel.Children.Add(buttonNo);

					buttonYes = new Button() { Content = "Yes", Width = 75, Height = 26, Margin = new Thickness(10, 0, 0, 0) };
					buttonYes.Click += buttonYes_Click;
					stackpanel.Children.Add(buttonYes);
					break;


			}
		}

		static void buttonRetry_Click(object sender, RoutedEventArgs e)
		{
			Result = MyMessageBoxResult.Retry;
			wnd.Close();
		}

		static void buttonCancel_Click(object sender, RoutedEventArgs e)
		{
			Result = MyMessageBoxResult.Cancel;
			wnd.Close();
		}

		static void buttonOK_Click(object sender, RoutedEventArgs e)
		{
			Result = MyMessageBoxResult.OK;
			wnd.Close();
		}

		static void buttonYes_Click(object sender, RoutedEventArgs e)
		{
			Result = MyMessageBoxResult.Yes;
			wnd.Close();
		}

		static void buttonNo_Click(object sender, RoutedEventArgs e)
		{
			Result = MyMessageBoxResult.No;
			wnd.Close();
		}


	}
}
