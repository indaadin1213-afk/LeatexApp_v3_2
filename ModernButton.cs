
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using LeatexApp.Services;

namespace LeatexApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Init storage and settings
            StorageService.Init();
            var settings = SettingsService.Load();
            ThemeService.ApplyLightTheme();

            using (var splash = new LeatexApp.Forms.SplashForm())
            {
                splash.Show();
                splash.Refresh();
                Task.Delay(1200).Wait();
            }

            using var login = new LeatexApp.Forms.LoginForm();
            if (login.ShowDialog() == DialogResult.OK)
            {
                var user = login.LoggedInUser;
                Application.Run(new LeatexApp.Forms.MainForm(user));
            }
        }
    }
}
