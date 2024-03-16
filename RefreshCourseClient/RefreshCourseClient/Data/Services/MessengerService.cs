using System.Windows;

namespace RefreshCourseClient.Data.Services
{
    internal interface IMessengerService
    {
        void ShowErrorMessageBox(string message);
    }

    internal class MessengerService : IMessengerService
    {
        public void ShowErrorMessageBox(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
