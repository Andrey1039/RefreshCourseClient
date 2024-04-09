using System.Windows;

namespace RefreshCourseClient.Data.Services
{
    internal interface IMessengerService
    {
        static abstract void ShowErrorMessageBox(string message);
        static abstract void ShowInfoMessageBox(string message);
        static abstract void ShowWarningMessageBox(string title, string message);
    }

    internal class MessengerService : IMessengerService
    {
        public static void ShowErrorMessageBox(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowInfoMessageBox(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowWarningMessageBox(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
        }

    }
}
