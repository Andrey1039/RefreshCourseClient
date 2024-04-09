using System.Windows;

namespace RefreshCourseClient.Data.Services
{
    internal interface IMessengerService
    {
        static abstract MessageBoxResult ShowErrorMessageBox(string message);
        static abstract MessageBoxResult ShowInfoMessageBox(string message);
        static abstract MessageBoxResult ShowWarningMessageBox(string title, string message);
    }

    internal class MessengerService : IMessengerService
    {
        public static MessageBoxResult ShowErrorMessageBox(string message)
        {
            return MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static MessageBoxResult ShowInfoMessageBox(string message)
        {
             return MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static MessageBoxResult ShowWarningMessageBox(string title, string message)
        {
            return MessageBox.Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
        }

    }
}
