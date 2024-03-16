using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RefreshCourseClient.ViewModels
{
    internal class MainVM : INotifyPropertyChanged
    {
        private string _text;
        public MainVM()
        {
            _text = "Hello World!";
        }

        private RelayCommand? textCommand;
        public RelayCommand TextCommand
        {
            get
            {
                return textCommand ??= new RelayCommand(_ => Text = "Привет Мир!");
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
