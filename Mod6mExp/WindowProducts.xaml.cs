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
using System.Data;
using System.Data.SqlClient;
using System.Windows.Threading;

namespace Machine
{

	/// <summary>
	/// Interaction logic for WindowProducts.xaml
	/// </summary>
	public partial class WindowProducts : Window
	{
		SqlConnection sqlConnectionProducts;
		SqlDataAdapter dataAdapterProducts;
		DataSet dataSetProducts;
		DispatcherTimer timerUpdateProducts;


		public WindowProducts()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// за да може да премества прозорец без рамка
			MouseDown += new MouseButtonEventHandler(Window_MouseDown);
			MouseUp += new MouseButtonEventHandler(Window_MouseUp);
			MouseMove += new MouseEventHandler(Window_MouseMove);

			timerUpdateProducts = new DispatcherTimer();
			timerUpdateProducts.Tick += new EventHandler(timerUpdateProducts_Tick);
			timerUpdateProducts.Interval = TimeSpan.FromMilliseconds(500);

			Gen.FillDataGrid(dataGridProducts, ref sqlConnectionProducts, ref dataAdapterProducts, ref dataSetProducts, Properties.Settings.Default.TraceabilityServerIP,
				"MachinesTrace", Properties.Settings.Default.TableProductsName, "Product,L1,L2,L3,L4,MARKFN,ProductLength,CheckNut,ProductType");
		}

		private void dataGridProducts_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
		{
			ContentPresenter cp = (ContentPresenter)e.EditingElement;

			UIElement destinationTextBox = VisualTreeHelper.GetChild(cp, 0) as UIElement;

			if (destinationTextBox != null)
			{
				if (destinationTextBox is TextBox)
				{
					destinationTextBox.Focus();
					TextBox tb = destinationTextBox as TextBox;
					tb.SelectAll();
				}
				else if (destinationTextBox is ComboBox)
				{
					ComboBox cb = destinationTextBox as ComboBox;
					cb.IsDropDownOpen = true;
				}
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			timerUpdateProducts.Stop();

			try
			{
				SqlCommandBuilder builder = new SqlCommandBuilder(dataAdapterProducts);
				dataAdapterProducts.UpdateCommand = builder.GetUpdateCommand();
				dataAdapterProducts.Update(dataSetProducts, Properties.Settings.Default.TableProductsName);
				sqlConnectionProducts.Close();
			}
			catch 
			{
				MessageBox.Show("Грешка при обновяване на базата данни!");
			}

			Properties.Settings.Default.Save();
		}

		private void dataGridProducts_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			timerUpdateProducts.Start();
		}

		void timerUpdateProducts_Tick(object sender, EventArgs e)
		{
			timerUpdateProducts.Stop();
			try
			{
				SqlCommandBuilder builderProducts = new SqlCommandBuilder(dataAdapterProducts);
				dataAdapterProducts.UpdateCommand = builderProducts.GetUpdateCommand();
				dataAdapterProducts.Update(dataSetProducts, Properties.Settings.Default.TableProductsName);
			}
			catch
			{
				MessageBox.Show("Грешка при обновяване на базата данни!");
			}
		}

		private void dataGridProducts_PreviewDeleteCommandHandler(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Command == DataGrid.DeleteCommand)
			{
				if (!(MessageBox.Show("Сигурни ли сте че искате да изтриете продукта?", "Изтриване на продукт", MessageBoxButton.YesNo) == MessageBoxResult.Yes))
				{
					e.Handled = true;	// отмяна
				}
				else timerUpdateProducts.Start();
			}
		}

		#region Преместване на прозореца
		bool WindowMoving = false;
		Point WindowMovingStartPoint = new Point();
		void Window_MouseMove(object sender, MouseEventArgs e)
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
		void Window_MouseUp(object sender, MouseButtonEventArgs e)
		{
			WindowMoving = false;
		}
		void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.GetPosition(this).Y <= 60)
			{
				WindowMoving = true;
				WindowMovingStartPoint = e.GetPosition(this);
			}
		}
		#endregion
		
	}
}
