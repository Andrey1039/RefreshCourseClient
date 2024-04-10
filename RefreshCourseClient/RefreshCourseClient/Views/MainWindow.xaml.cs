using System.Windows;
using System.Windows.Input;
using RefreshCourseClient.ViewModels;

namespace RefreshCourseClient.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainVM();
        }
    }
}