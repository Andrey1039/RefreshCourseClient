using System.Windows;

namespace RefreshCourseClient.Data.Services
{
    internal interface IMessengerService
    {
        static abstract MessageBoxResult ShowErrorMessageBox(string title, string message);
        static abstract MessageBoxResult ShowInfoMessageBox(string title, string message);
        static abstract MessageBoxResult ShowWarningMessageBox(string title, string message);
    }

    internal class MessengerService : IMessengerService
    {
        public static MessageBoxResult ShowErrorMessageBox(string title, string message)
        {
            return MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static MessageBoxResult ShowInfoMessageBox(string title, string message)
        {
             return MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static MessageBoxResult ShowWarningMessageBox(string title, string message)
        {
            return MessageBox.Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
        }

    }
}