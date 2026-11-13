using System;
using System.Windows.Forms;
using RestaurantPOS.Data;
using RestaurantPOS.Forms;

namespace RestaurantPOS
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create the local SQLite database (if it doesn't exist yet)
            // and seed it with default users + 240+ menu items.
            DatabaseHelper.Initialize();

            Application.Run(new LoginForm());
        }
    }
}
