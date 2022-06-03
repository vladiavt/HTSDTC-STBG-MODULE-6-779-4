using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for DownTimeWindow.xaml
    /// </summary>
    public partial class DownTimeWindow : Window
    {
        private string machineID;
        private string operatorID;

        public DownTimeWindow(string MachineID, string OperatorID)
        {
            InitializeComponent();
            machineID = MachineID;
            operatorID = OperatorID;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MainFrame.Navigate(new DownTimeWebBrowser(machineID, operatorID,this));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проблем при отварянето на страницата"+ex.ToString());
                //LoggingService.Log("Проблем при изтегляне на данните в WIP." + ex.Message);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
