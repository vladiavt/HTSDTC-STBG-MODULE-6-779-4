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

namespace Machine
{
	/// <summary>
	/// Interaction logic for WindowDocumentation.xaml
	/// </summary>
	public partial class WindowDocumentation : Window
	{

		public WindowDocumentation()
		{
			InitializeComponent();

			WindowStyle = WindowStyle.None;
			WindowState = WindowState.Maximized;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// за да може да премества прозорец без рамка
			MouseDown += new MouseButtonEventHandler(TittleBar_MouseDown);
			MouseUp += new MouseButtonEventHandler(TittleBar_MouseUp);
			MouseMove += new MouseEventHandler(TittleBar_MouseMove);
			MouseLeave += new MouseEventHandler(TittleBar_MouseLeave);
		}

		#region Преместване на прозереца
		bool WindowMoving = false;
		Point WindowMovingStartPoint = new Point();

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

		private void buttonFrd_Click(object sender, RoutedEventArgs e)
		{
			webBrowser.GoForward();
		}

		private void buttonBck_Click(object sender, RoutedEventArgs e)
		{
			webBrowser.GoBack();
		}

		private void buttonClose_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
