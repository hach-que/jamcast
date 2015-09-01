#if PLATFORM_WINDOWS

using System;
using System.Windows.Forms;

namespace Client
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

            Manager m = new Manager();
			m.Run();

			// Start the main application loop.
			Application.Run();
        }
    }
}

#endif