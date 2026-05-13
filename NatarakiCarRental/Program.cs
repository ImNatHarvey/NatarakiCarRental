using NatarakiCarRental.Data;
using NatarakiCarRental.Forms.Auth;
using NatarakiCarRental.Helpers;

namespace NatarakiCarRental
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            try
            {
                DatabaseInitializer.Initialize();
            }
            catch (Exception exception)
            {
                MessageBoxHelper.ShowDatabaseError(exception);
                return;
            }

            Application.Run(new LoginForm());
        }
    }
}
