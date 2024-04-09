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
        private bool _currentGridVisibility;
        private string _serverUrl;
        private string _userEmail;
        private string _userPassword;
        private string _token;
        public MainVM()
        {
            _currentGridVisibility = true;
            _serverUrl = "https://localhost:7095";
            _userEmail = "smv@psu.ru";
            _userPassword = "User1!smv";
            //_userPassword = new NetworkCredential("", "User1!smv").SecurePassword;
            _token = string.Empty;
    }

        private RelayCommand? textCommand;
        public RelayCommand TextCommand
        {
            get
            {
                return textCommand ??= new RelayCommand(_ => _token = "Привет Мир!");
            }
        }

        public bool CurrentGridVisibility
        {
            get => _currentGridVisibility;
            set
            {
                _currentGridVisibility = value;
                OnPropertyChanged("CurrentGridVisibility");
            }
        }

        public string ServerUrl
        {
            get => _serverUrl;
            set
            {
                _serverUrl = value;
                OnPropertyChanged("ServerUrl");
            }
        }
        public string Email
        {
            get => _userEmail;
            set => _userEmail = value;
        }
        public string Password
        {
            get => _userPassword;
            set => _userPassword = value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
