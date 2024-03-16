using System.Windows;
using RefreshCourseClient.ViewModels;

namespace RefreshCourseClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainVM();
        }
    }
}