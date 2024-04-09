using RefreshCourseClient.ViewModels;
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
using Accessibility;

namespace RefreshCourseClient.Views
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
            this.DataContext = new AuthVM();
        }

        private void AuthButtonClicked(object sender, RoutedEventArgs e)
        {
            bool successAuth = true;
            if (successAuth)
            {
                MessageBox.Show("Добро пожаловать,\nПетров Петр Петрович", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new TablePage());
            }
            else
            {
                MessageBox.Show("Ошибка авторизации", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
