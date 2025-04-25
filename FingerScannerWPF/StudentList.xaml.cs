using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;

namespace FingerScannerWPF
{
    public partial class StudentList : Window
    {
        private HttpClient _httpClient;
        private List<student> listStudent;

        Student _student;
        public StudentList(Student student)
        {
            InitializeComponent();
            _student = student;

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:4000/");
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "reports/student/");
                request.Headers.Add("Authorization", $"Bearer {LoginWindow.LoginRes.Token}");
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                JsonNode jn = JsonNode.Parse(responseBody);
                listStudent = new List<student>();
                if (jn is JsonArray jsonArray)
                {
                    foreach (var item in jsonArray)
                    {
                        var student = new student()
                        {
                            id = item["id"].GetValue<string>(),
                            firstname = item["firstname"].GetValue<string>(),
                            lastname = item["lastname"].GetValue<string>(),
                            nationCode = item["nationCode"].GetValue<string>(),
                            fingerprint = item["fingerprint"] == null ? "ثبت نشده" : "ثبت شده"
                        };
                        listStudent.Add(student);
                    }
                }
                RecordCountTextBlock.Text = $"تعداد موارد: {listStudent.Count}";
                StudentDataGrid.ItemsSource = listStudent;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            var filteredList = listStudent.Where(p =>
                (string.IsNullOrEmpty(FilterIdTextBox.Text) || p.id.Contains(FilterIdTextBox.Text)) &&
                (FingerprintComboBox.SelectedItem == null || p.fingerprint.Contains(FingerprintComboBox.SelectedItem.ToString())) &&
                (string.IsNullOrEmpty(FilterNationCodeTextBox.Text) || p.nationCode.Contains(FilterNationCodeTextBox.Text))
            ).ToList();

            RecordCountTextBlock.Text = $"تعداد موارد: {filteredList.Count()}";
            StudentDataGrid.ItemsSource = filteredList;
        }

        private void StudentDataGrid_CellDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (StudentDataGrid.SelectedItem is student selectedStudent)
            {
                _student.getStudent(selectedStudent.id);
                this.Close();
            }
        }
    }
}
