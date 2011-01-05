using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace JamCast
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Set the visual styles.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Start and run the manager.
            Manager m = new Manager();
            m.Run();
        }
    }
}
