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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Machine
{
    /// <summary>
    /// Interaction logic for DownTimeWebBrowser.xaml
    /// </summary>
    public partial class DownTimeWebBrowser : Page
    {
        private string machineID;
        private string operatorID;
        Window parentWindow;

        public DownTimeWebBrowser(string MachineID,string OperatorID,Window ParentWindow)
        {
            InitializeComponent();
            machineID = MachineID;
            operatorID = OperatorID;
            parentWindow = ParentWindow;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("http://msn.corp.sensata.com/MaintView/DownTimeEvent.aspx?P4="+ machineID +"&P2="+ operatorID + "&MachineEvent=");
            try
            {

                MyWebBrowser.Navigate(uri);
            }
            catch (Exception ex)
            {
                //LoggingService.Log(ex.Message);
            }
        }

        private void MyWebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var navigatedURL = e.Uri.ToString();
            if (navigatedURL.Contains("P3=OK"))
            {
                parentWindow.DialogResult = true;
            }

            //dynamic doc = MyWebBrowser.Document;
            //var htmlText = doc.documentElement.InnerHtml;
            //if (htmlText.Contains("P3=OK"))
            //{
            //    parentWindow.DialogResult = true;
            //}
        }
    }
}
