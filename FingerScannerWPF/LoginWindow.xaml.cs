using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FingerScannerWPF
{
    public partial class LoginWindow : Window
    {
        private readonly HttpClient _httpClient;

        public static LoginResponse LoginRes { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:4000/");
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = UsernameTextBox.Text.Trim();
                string password = PasswordBox.Password.Trim();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("شناسه و رمز ورود را وارد کنید");
                    return;
                }

                var credentials = new Credentials { id = username, password = password };
                var json = JsonConvert.SerializeObject(credentials);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, "login/")
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                //LoginRes = JsonConvert.DeserializeObject<LoginResponse>(responseBody);

                var studentWin = new Student();
                studentWin.Show();
                this.Close();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"خطا در ارسال درخواست: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطای : {ex.Message}");
            }
        }
    }
}