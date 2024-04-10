using System.Net;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using System.Net.Http;
using System.ComponentModel;
using System.Security.Claims;
using System.Windows.Controls;
using RefreshCourseClient.Models;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using RefreshCourseClient.Data.Services;
using RefreshCourseClient.Data.Encryption;
using System.IdentityModel.Tokens.Jwt;

namespace RefreshCourseClient.ViewModels
{
    internal class MainVM : INotifyPropertyChanged
    {
        private string _token;
        private string _serverUrl;
        private string _userEmail;
        private string _privateKey;
        private bool _currentGridVisibility;
        private ObservableCollection<RecordModel> _records;

        public MainVM()
        {
            _token = string.Empty;
            _userEmail = "smv@psu.ru";
            _privateKey = string.Empty;
            _currentGridVisibility = true;
            _serverUrl = "https://localhost:44359";
            _records = new ObservableCollection<RecordModel>();
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

        public ObservableCollection<RecordModel> Records
        {
            get
            {
                return _records;
            }
        }

        private RelayCommand? _loginCommand;
        public RelayCommand LoginCommand
        {
            get
            {
                return _loginCommand ??= new RelayCommand(passwordTB =>
                {
                    // Обмен публичными ключами
                    var password = ((PasswordBox)passwordTB).Password;
                    (string privateKeyClient, string publicKeyClient) = VKOGost.GetKeyPair();

                    string jsonData = JsonConvert.SerializeObject(new
                    {
                        Email = _userEmail,
                        PublicKey = publicKeyClient
                    });

                    string serverPublicKey = SendPostRequest(jsonData, $"{_serverUrl}/api/Auth/SwapKeys");


                    // Авторизация и получение токена
                    if (serverPublicKey != string.Empty)
                    {
                        _privateKey = VKOGost.GetHash(privateKeyClient, serverPublicKey);

                        jsonData = JsonConvert.SerializeObject(new
                        {
                            Email = _userEmail,
                            Password = CipherEngine.EncryptString(password.ToString(), _privateKey)
                        });

                        _token = SendPostRequest(jsonData, $"{_serverUrl}/api/Auth/Login");

                        if (_token != string.Empty)
                        {
                            var handler = new JwtSecurityTokenHandler();
                            var jsonToken = handler.ReadToken(_token) as JwtSecurityToken;

                            var username = jsonToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

                            MessengerService.ShowInfoMessageBox("Вход", $"Добро пожаловать,\n{username}");
                            CurrentGridVisibility = false;

                            UpdateWorkLoad();
                        }
                        else
                        {
                            _privateKey = string.Empty;
                            _token = string.Empty;

                            MessengerService.ShowErrorMessageBox("Ошибка", "Ошибка авторизации");
                        }
                    }
                    else
                        MessengerService.ShowErrorMessageBox("Ошибка", "Ошибка авторизации");
                });
            }
        }

        // Обновление данных о нагрузке
        private RelayCommand? _updateCommand;
        public RelayCommand UpdateCommand
        {
            get
            {
                return _updateCommand ??= new RelayCommand(_ => UpdateWorkLoad());
            }
        }

        // Выход из учетной записи
        private RelayCommand? _logoutCommand;
        public RelayCommand LogoutCommand
        {
            get
            {
                return _logoutCommand ??= new RelayCommand(_ =>
                {
                    MessageBoxResult acceptExit = MessengerService
                        .ShowWarningMessageBox("Выход", "Вы действительно хотите выйти?");

                    if (acceptExit == MessageBoxResult.OK)
                    {
                        string responseData = SendPostRequest(string.Empty, $"{_serverUrl}/api/Auth/Logout");
                        
                        _token = string.Empty;
                        _privateKey = string.Empty;

                        CurrentGridVisibility = true;
                    }
                });
            }
        }

        // Получение текущих данных о нагрузке
        private void UpdateWorkLoad()
        {
            string responseData = SendGetRequest($"{_serverUrl}/api/Home/GetWorkLoad");

            if (responseData != string.Empty)
            {
                try
                {
                    string data = CipherEngine.DecryptString(responseData, _privateKey);
                    _records.Clear();

                    foreach (var record in JsonConvert.DeserializeObject<RecordModel[]>(data)!)
                        _records.Add(record);
                }
                catch
                {
                    MessengerService.ShowErrorMessageBox("Ошибка", "Ошибка при получении данных с сервера");
                }             
            }
        }

        // Отправка Post запроса на сервер
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

        // Отправка Get запроса на сервер
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
