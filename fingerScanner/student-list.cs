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
using System.Text.Json;
using System.Text.Json.Nodes;

namespace fingerScanner
{
    public partial class student_list : Form
    {
        private HttpClient _httpClient;
        private List<student> liststudent;

        public student_list(main m)
        {
            InitializeComponent();
            mainform= m;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:4000/");
        }

        private main mainform;
        private async void student_list_Load(object sender, EventArgs e)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "reports/student/");
                request.Headers.Add("Authorization", $"Bearer {Form1.LoginRes.Token}");
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                JsonNode jn = JsonNode.Parse(responseBody);
                liststudent = new List<student>();
                if (jn is JsonArray jsonArray)
                {
                    foreach (var item in jsonArray)
                    {
                        student s = new student()
                        {
                            id = item["id"].GetValue<string>(),
                            firstname = item["firstname"].GetValue<string>(),
                            lastname = item["lastname"].GetValue<string>(),
                            nationCode = item["nationCode"].GetValue<string>()
                        };
                        if (item["fingerprint"] == null)
                        {
                            s.fingerprint = "ثبت نشده";
                        }
                        else
                        {
                            s.fingerprint = "ثبت شده";
                        }
                        liststudent.Add(s);
                    }
                }
                numberlable.Text = liststudent.Count.ToString();
                dataGridView1.AutoGenerateColumns = false;
                dataGridView1.DataSource = liststudent;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"HTTP Error: {ex.Message}");
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"JSON Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var filteredList = liststudent.Where(p =>
            (string.IsNullOrEmpty(filterId.Text) || p.id.ToString().Contains(filterId.Text)) &&
            (string.IsNullOrEmpty(comboBox1.SelectedItem.ToString()) || p.fingerprint.ToString().Contains(comboBox1.SelectedItem.ToString())) &&
            (string.IsNullOrEmpty(filternationCode.Text) || p.nationCode.ToString().Contains(filternationCode.Text))
            ).ToList();
            numberlable.Text = filteredList.Count.ToString();
            dataGridView1.DataSource = filteredList;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string id = dataGridView1.Rows[e.RowIndex].Cells["id"].Value.ToString();
            mainform.getStudent(id);
            this.Close();
        }
    }
}
