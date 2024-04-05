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

namespace RefreshCourseClient.Views
{
    /// <summary>
    /// Логика взаимодействия для TablePage.xaml
    /// </summary>
    public partial class TablePage : Page
    {
        public TablePage()
        {
            InitializeComponent();
        }

        private void LogOutClicked(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AuthPage());
        }

        private void UpdateClicked(object sender, RoutedEventArgs e)
        {
            // post request to getting new data from DB
        }
        private void CreateReportClicked(object sender, RoutedEventArgs e)
        {
            // create report from current data and save this shit with extention .xls
        }
    }
}
