using System.Windows.Forms;

namespace NatarakiCarRental.Helpers;

public static class MessageBoxHelper
{
    public static void ShowInfo(string message, string title = AppConstants.ApplicationName)
    {
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static void ShowError(string message, string title = AppConstants.ApplicationName)
    {
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static void ShowDatabaseError(Exception exception)
    {
        ShowError($"Database connection failed.\n\n{exception.Message}");
    }

    public static bool Confirm(string message, string title = AppConstants.ApplicationName)
    {
        return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
