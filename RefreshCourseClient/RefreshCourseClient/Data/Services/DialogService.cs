using Microsoft.Win32;

namespace RefreshCourseClient.Data.Services
{
    internal interface IDialogService
    {
        string[] OpenFileDialog(string filter, bool multiSelect);
        string SaveFileDialog(string filter);
    }

    internal class DialogService : IDialogService
    {
        public string[] OpenFileDialog(string filter, bool multiSelect)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Выберите файлы";
            openFileDialog.Filter = filter;
            openFileDialog.Multiselect = multiSelect;

            if (openFileDialog.ShowDialog() == true)
                return openFileDialog.FileNames;

            return Array.Empty<string>();
        }

        public string SaveFileDialog(string filter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Title = "Сохранение отчета";
            saveFileDialog.Filter = filter;

            if (saveFileDialog.ShowDialog() == true)
                return saveFileDialog.FileName;

            return string.Empty;
        }
    }
}