using System.Windows;

namespace RefreshCourseClient.Data.Services
{
    internal interface IMessengerService
    {
        static abstract void ShowErrorMessageBox(string message);
    }

    internal class MessengerService : IMessengerService
    {
        public static void ShowErrorMessageBox(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
