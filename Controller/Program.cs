using System;
using System.Windows.Forms;

namespace Controller
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var m = new MainForm();
            Application.Run(m);
        }
    }
}