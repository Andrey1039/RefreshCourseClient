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
using System.Security.Cryptography;
using System.Windows.Documents;
using System.IO;
using System.Diagnostics;
using System.Windows.Threading;

namespace RefreshCourseClient.ViewModels
{
    internal class MainVM : INotifyPropertyChanged
    {
        private string _token;
        private string _serverUrl;
        private string _userEmail;
        private string _privateKey;
        private bool _currentGridVisibility;
        private bool _progressBar;
        private string _progressText;
        private ObservableCollection<RecordModel> _records;
        private IMessengerService _messenger;

        public MainVM()
        {
            _token = string.Empty;
            _userEmail = Properties.Settings.Default.Login;
            _privateKey = string.Empty;
            _currentGridVisibility = true;
            _progressBar = false;
            _progressText = string.Empty;
            _serverUrl = Properties.Settings.Default.ServerIP;
            _records = new ObservableCollection<RecordModel>();
            _messenger = new MessengerService();
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

        public bool ProgressBar
        {
            get => _progressBar;
            set
            {
                _progressBar = value;
                OnPropertyChanged("ProgressBar");
            }
        }

        public string ProgressText
        {
            get => _progressText;
            set
            {
                _progressText = value;
                OnPropertyChanged("ProgressText");
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

        // Авторизация
        private RelayCommand? _loginCommand;
        public RelayCommand LoginCommand
        {
            get
            {
                return _loginCommand ??= new RelayCommand(passwordTB =>
                {
                    ProgressBar = true;
                    ProgressText = "Выполняется авторизация...";

                    Task.Factory.StartNew(() =>
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

                                var username = jsonToken.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;

                                _messenger.ShowInfoMessageBox("Вход", $"Добро пожаловать,\n{username}");
                                CurrentGridVisibility = false;

                                ProgressText = "Обновление данных...";
                                string result = UpdateWorkLoad(false);

                                if (result == string.Empty)
                                    CurrentGridVisibility = true;
                            }
                            else
                            {
                                _privateKey = string.Empty;
                                _token = string.Empty;

                                //_messenger.ShowErrorMessageBox("Ошибка", "Ошибка авторизации");
                            }
                        }
                        else
                            _messenger.ShowErrorMessageBox("Ошибка", "Ошибка авторизации");

                        ProgressBar = false;
                    });               
                });
            }
        }

        // Сохранение отчетов
        private RelayCommand? _getReportCommand;
        public RelayCommand GetReportCommand
        {
            get
            {
                return _getReportCommand ??= new RelayCommand(records =>
                {
                    StringBuilder csvData = new StringBuilder();

                    csvData.Append("№;Группа;Предмет;Тип занятия;Кол-во часов;Оплата за час;Сумма\n");

                    foreach (var record in (IList<RecordModel>)records)
                        csvData.Append($"{record.Id};{record.GroupName};{record.SubjectName};{record.LessonType};" +
                            $"{record.HoursCount};{record.PayHour};{record.Money}\n");

                    var result = Encoding.UTF8.GetString(
                                    Encoding.UTF8.GetPreamble()
                                        .Concat(Encoding.UTF8.GetBytes(csvData.ToString())).ToArray());

                    IDialogService dialog = new DialogService();
                    string filename = dialog.SaveFileDialog("CSV files (*.csv)|*.csv");

                    try
                    {
                        using (var writer = new StreamWriter(filename))
                        {
                            writer.Write(result);
                        }

                        _messenger.ShowInfoMessageBox("Отчет", "Ваш отчет успешно сохранен");
                    }
                    catch
                    {
                        _messenger.ShowErrorMessageBox("Отчет", "Ошибка при сохранении отчета");
                    }
                });
            }
        }

        // Обновление данных о нагрузке
        private RelayCommand? _updateCommand;
        public RelayCommand UpdateCommand
        {
            get
            {
                return _updateCommand ??= new RelayCommand(_ =>
                {

                    Task.Factory.StartNew(() =>
                    {
                        ProgressBar = true;
                        ProgressText = "Обновление данных...";

                        string result = UpdateWorkLoad(true);

                        ProgressBar = false;

                        if (result == string.Empty)
                            CurrentGridVisibility = true;
                    });
                });
                
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
                    MessageBoxResult acceptExit = _messenger
                        .ShowWarningMessageBox("Выход", "Вы действительно хотите выйти?");

                    if (acceptExit == MessageBoxResult.OK)
                    {
                        ProgressBar = true;
                        ProgressText = "Выполняется выход...";

                        Task.Factory.StartNew(() =>
                        {
                            string responseData = SendPostRequest(string.Empty, $"{_serverUrl}/api/Auth/Logout");
                            ProgressBar = false;

                            _token = string.Empty;
                            _privateKey = string.Empty;
                        });
                        
                        CurrentGridVisibility = true;
                    }
                });
            }
        }

        // Выход из программы
        private RelayCommand? _closingCommand;
        public RelayCommand ClosingCommand
        {
            get
            {
                return _closingCommand ??= new RelayCommand(_ =>
                {

                    Properties.Settings.Default.Login = _userEmail;
                    Properties.Settings.Default.ServerIP = _serverUrl;
                    Properties.Settings.Default.Save();
                });
            }
        }

        // Получение текущих данных о нагрузке
        private string UpdateWorkLoad(bool mode)
        {
            string responseData = SendGetRequest($"{_serverUrl}/api/Home/GetWorkLoad");

            if (responseData != string.Empty)
            {
                try
                {
                    string data = CipherEngine.DecryptString(responseData, _privateKey);
                    App.Current.Dispatcher.Invoke(new Action(_records.Clear));

                    var records = JsonConvert.DeserializeObject<RecordModel[]>(data)!;

                    for (int i = 0; i < records.Length; i++)
                    {
                        records[i].Id = i + 1;
                        App.Current.Dispatcher.Invoke(new Action(() => _records.Add(records[i])));
                    }

                    if (mode)
                        _messenger.ShowInfoMessageBox("Данные", "Данные успешно обновлены");

                    return "Ok";
                }
                catch (CryptographicException e)
                {
                    _messenger.ShowErrorMessageBox("Ошибка", "Произошла ошибка получения данных о нагрузке.");
                }
                catch (JsonReaderException e)
                {
                    _messenger.ShowErrorMessageBox("Ошибка", "Произошла ошибка получения данных о нагрузке.");
                }
                catch
                {
                    _messenger.ShowErrorMessageBox("Ошибка", responseData);
                }             
            }

            return string.Empty;
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
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _messenger.ShowErrorMessageBox("Авторизация",
                            response.Content.ReadAsStringAsync().Result);
                    }

                    return string.Empty;
                }                   
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
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _messenger.ShowErrorMessageBox("Авторизация",
                            "Время сессии истекло.\nПожалуйста, выполните повторую авторизацию.");                     
                    }

                    return string.Empty;
                }
                    
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