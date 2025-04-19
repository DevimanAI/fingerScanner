using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fingerScanner
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            /*Form1 form1 = new Form1();
            form1.FormClosed += (s, args) => Application.Exit(); // بستن برنامه زمانی که فرم بسته می‌شود
            form1.Show();
            Application.Run();*/
            Application.Run(new Form1());
        }
        //private LoginResponse loginRes;

    }
}
