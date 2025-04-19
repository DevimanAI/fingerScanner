using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net.Http;
using Newtonsoft.Json;

namespace fingerScanner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:4000/");
        }

        private HttpClient _httpClient;

        public main m { get; set; }

        public static LoginResponse LoginRes { get; private set; }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string username = textBox1.Text.Trim();
                string password = textBox2.Text.Trim();
                if (string.IsNullOrEmpty(username)||string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("شناسه و رمز ورود را وارد کنید");
                }
                var credentials = new Credentials { id = username, password = password };
                var json = JsonConvert.SerializeObject(credentials);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                //var response = await _httpClient.PostAsync("login/", content);
                var request = new HttpRequestMessage(HttpMethod.Post, "login/")
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                // پردازش پاسخ
                LoginRes = JsonConvert.DeserializeObject<LoginResponse>(responseBody);

                m = new main();
                //m.MdiParent= this;
                m.FormClosed += (s, args) => this.Close();
                m.Show();
                this.Hide();
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
