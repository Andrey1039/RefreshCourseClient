using System.Windows;

namespace RefreshCourseClient.Data.Services
{
    internal interface IMessengerService
    {
        abstract MessageBoxResult ShowErrorMessageBox(string title, string message);
        abstract MessageBoxResult ShowInfoMessageBox(string title, string message);
        abstract MessageBoxResult ShowWarningMessageBox(string title, string message);
    }

    internal class MessengerService : IMessengerService
    {
        public MessageBoxResult ShowErrorMessageBox(string title, string message)
        {
            return MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public MessageBoxResult ShowInfoMessageBox(string title, string message)
        {
             return MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public MessageBoxResult ShowWarningMessageBox(string title, string message)
        {
            return MessageBox.Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
        }

    }
}