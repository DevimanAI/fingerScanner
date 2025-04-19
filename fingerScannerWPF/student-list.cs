csharp
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;

namespace fingerScannerWPF
{
    public partial class student_list : Form
    {
        public student_list()
        {
            InitializeComponent();
        }
        string cs = ConfigurationManager.ConnectionStrings["dbcs"].ConnectionString;
        void BindGridView()
        {
            SqlConnection con = new SqlConnection(cs);
            string query = "select * from student_table";
            SqlDataAdapter sda = new SqlDataAdapter(query, con);
            DataTable data = new DataTable();
            sda.Fill(data);
            dataGridView1.DataSource = data;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void student_list_Load(object sender, EventArgs e)
        {
            BindGridView();
        }
        private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                string selectedStudentId = selectedRow.Cells["id"].Value.ToString();
                student std = new student(selectedStudentId);
                std.ShowDialog();
            }
        }
    }
}