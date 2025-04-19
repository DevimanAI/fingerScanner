csharp
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace fingerScannerWPF
{
    public partial class MainWindow : Window
    {
        private List<Student> students = new List<Student>();
        private string connectionString;
        private SqlConnection connection;

        public MainWindow()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["fingerScanner.Properties.Settings.fingerPrintDBConnectionString"].ConnectionString;
            LoadStudents();
        }

        private void LoadStudents()
        {
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                string query = "SELECT * FROM Students";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Student student = new Student
                    {
                        Id = (int)reader["id"],
                        Name = reader["name"].ToString(),
                        LastName = reader["lastName"].ToString(),
                        StudentId = reader["studentID"].ToString(),
                        Template = reader["template"] as byte[]
                    };
                    students.Add(student);
                }

                reader.Close();
                connection.Close();
                StudentsDataGrid.ItemsSource = students;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading students: " + ex.Message);
            }
        }

        private void AddStudentButton_Click(object sender, RoutedEventArgs e)
        {
            AddStudent();
        }

        private void RemoveStudentButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveStudent();
        }

        private void SearchStudentButton_Click(object sender, RoutedEventArgs e)
        {
            SearchStudent();
        }

        private void AddStudent()
        {
           
            StudentWindow studentWindow = new StudentWindow();

            if (studentWindow.ShowDialog() == true)
            {

                Student newStudent = studentWindow.Student;
                try
                {
                    connection = new SqlConnection(connectionString);
                    connection.Open();
                    string query = "INSERT INTO Students (name, lastName, studentID, template) VALUES (@name, @lastName, @studentID, @template)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@name", newStudent.Name);
                    command.Parameters.AddWithValue("@lastName", newStudent.LastName);
                    command.Parameters.AddWithValue("@studentID", newStudent.StudentId);
                    command.Parameters.AddWithValue("@template", newStudent.Template);
                    command.ExecuteNonQuery();

                    connection.Close();

                    LoadStudents();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding student: " + ex.Message);
                }
            }
        }


        private void RemoveStudent()
        {
            if (StudentsDataGrid.SelectedItem != null)
            {
                Student selectedStudent = (Student)StudentsDataGrid.SelectedItem;

                try
                {
                    connection = new SqlConnection(connectionString);
                    connection.Open();
                    string query = "DELETE FROM Students WHERE id = @id";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", selectedStudent.Id);
                    command.ExecuteNonQuery();

                    connection.Close();
                    LoadStudents();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error removing student: " + ex.Message);
                }
            }
        }

        private void SearchStudent()
        {
            string searchTerm = SearchTextBox.Text.ToLower();
            var filteredStudents = students.Where(s =>
                s.Name.ToLower().Contains(searchTerm) ||
                s.LastName.ToLower().Contains(searchTerm) ||
                s.StudentId.ToLower().Contains(searchTerm)).ToList();
            StudentsDataGrid.ItemsSource = filteredStudents;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadStudents();
            SearchTextBox.Text = "";
        }

        public class Student
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string LastName { get; set; }
            public string StudentId { get; set; }
            public byte[] Template { get; set; }
        }

        private void StudentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudentsDataGrid.SelectedItem != null)
            {
                Student selectedStudent = (Student)StudentsDataGrid.SelectedItem;

                if (selectedStudent.Template != null && selectedStudent.Template.Length > 0)
                {
                    
                }
            }
        }
    }
}