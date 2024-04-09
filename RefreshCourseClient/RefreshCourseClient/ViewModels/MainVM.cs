using RefreshCourseClient.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Windows;
using Newtonsoft.Json;
using RefreshCourseClient.Data.Encryption;
using RefreshCourseClient.Data.Services;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Data;
using RefreshCourseClient.Models;

namespace RefreshCourseClient.ViewModels
{
    internal class MainVM : INotifyPropertyChanged
    {
        private bool _currentGridVisibility;
        private string _serverUrl;
        private string _userEmail;
        private string _userPassword;
        private string _token;
        private string _privateKey;
        private DataTable? _dataTable;
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

        public DataTable? DataTable
        {
            get => _dataTable;
            set => _dataTable = value;
        }

        public MainVM()
        {
            _currentGridVisibility = true;
            _serverUrl = "https://localhost:44359";
            _userEmail = "smv@psu.ru";
            _userPassword = "User1!smv";
            //_userPassword = new NetworkCredential("", "User1!smv").SecurePassword;
            _token = string.Empty;
            _dataTable = null; //new DataTable();
        }

        private RelayCommand? _loginCommand;
        public RelayCommand LoginCommand
        {
            get
            {
                return _loginCommand ??= new RelayCommand(_ =>
                {
                    // Этап №1 (обмен ключами)
                    // Генерация пары ключей
                    (string privateKeyClient, string publicKeyClient) = VKOGost.GetKeyPair();

                    string jsonData = JsonConvert.SerializeObject(new
                    {
                        Email = _userEmail,
                        PublicKey = publicKeyClient
                    });

                    // Запрос на сервер и ответ (публичный ключ сервера)
                    string serverPublicKey = SendPostRequest(jsonData, $"{_serverUrl}/api/Auth/SwapKeys");

                    if (serverPublicKey == string.Empty)
                        return;

                    // Получаем общий приватный ключ
                    _privateKey = VKOGost.GetHash(privateKeyClient, serverPublicKey);

                    // Этап №2 (авторизация)
                    jsonData = JsonConvert.SerializeObject(new
                    {
                        Email = _userEmail,
                        Password = CipherEngine.EncryptString(_userPassword, _privateKey)
                    });

                    // Запрос на сервер и ответ (токен для доступа)
                    _token = SendPostRequest(jsonData, $"{_serverUrl}/api/Auth/Login");

                    if (_token != string.Empty)
                    {
                        MessengerService.ShowInfoMessageBox("Добро пожаловать,\nПетров Петр Петрович");
                        CurrentGridVisibility = false;
                        this.DataTable = UpdateWorkLoad();
                        //TODO: clean user email and password

                    }
                    else
                    {
                        _privateKey = string.Empty;
                        _token = string.Empty;
                        this.DataTable = null;
                        MessengerService.ShowInfoMessageBox("Ошибка авторизации");
                    }
                });
            }
        }

        private RelayCommand? _updateCommand;

        public RelayCommand UpdateCommand
        {
            get
            {
                return _updateCommand ??= new RelayCommand(_ =>
                {
                    this.DataTable = UpdateWorkLoad();
                });
            }
        }


        private RelayCommand? _logoutCommand;
        public RelayCommand LogoutCommand
        {
            get
            {
                return _logoutCommand ??= new RelayCommand(_ =>
                {
                    MessageBoxResult acceptExit = MessengerService.ShowWarningMessageBox("Выход", "Вы действительно хотите выйти?");
                    if (acceptExit == MessageBoxResult.OK)
                    {
                        // Выход из системы
                        string responseData = SendPostRequest(string.Empty, $"{_serverUrl}/api/Auth/Logout");
                        
                        _token = string.Empty;
                        _privateKey = string.Empty;
                        CurrentGridVisibility = true;
                    }
                });
            }
        }



        private DataTable? UpdateWorkLoad()
        {
            string data = string.Empty;

            // Получение информации о нагрузке и т.д.
            string responseData = SendGetRequest($"{_serverUrl}/api/Home/GetWorkLoad");

            if (responseData == string.Empty)
                return null;

            // Расшифровка и вывод полученных данных
            data = CipherEngine.DecryptString(responseData, _privateKey);
            RecordModel[] records = JsonConvert.DeserializeObject<RecordModel[]>(data)!;

            // Преобразование JSON to DataTable
            return (DataTable)JsonConvert.DeserializeObject(data, (typeof(DataTable)))!;
        }

        private string SendPostRequest(string jsonData, string api)
        {
            StringContent httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            string responseResult = string.Empty;

            try
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
                HttpResponseMessage response = httpClient.PostAsync(api, httpContent).Result;

                if (response.StatusCode == HttpStatusCode.OK)
                    responseResult = response.Content.ReadAsStringAsync().Result;
                else
                    return string.Empty;
            }
            catch
            {
                return string.Empty;
            }

            return responseResult;
        }

        private string SendGetRequest(string api)
        {
            string responseResult = string.Empty;

            try
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
                HttpResponseMessage response = httpClient.GetAsync(api).Result;

                if (response.StatusCode == HttpStatusCode.OK)
                    responseResult = response.Content.ReadAsStringAsync().Result;
                else
                    return string.Empty;
            }
            catch
            {
                return string.Empty;
            }

            return responseResult;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
