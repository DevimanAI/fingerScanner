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

namespace fingerScannerWPF
{
    public partial class student : Form
    {
        public student()
        {
            InitializeComponent();
        }

        private void student_Load(object sender, EventArgs e)
        {
            // Load student data or initialize components here
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Save student information
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            // Browse for an image file
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            // Handle name text change
        }

        private void txtID_TextChanged(object sender, EventArgs e)
        {
            // Handle ID text change
        }

        private void txtFatherName_TextChanged(object sender, EventArgs e)
        {
            // Handle father's name text change
        }

        private void txtPhone_TextChanged(object sender, EventArgs e)
        {
            // Handle phone text change
        }

        private void txtAddress_TextChanged(object sender, EventArgs e)
        {
            // Handle address text change
        }

        private void picStudent_Click(object sender, EventArgs e)
        {
            // Handle student picture click
        }
    }
}