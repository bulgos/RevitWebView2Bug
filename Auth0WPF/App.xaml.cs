using Auth0WPF.Core;
using System.Windows;

namespace Auth0WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        { }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LaunchWindows();
        }

        void LaunchWindows()
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();

            var webView = new WebViewWindow("https://www.google.com");
            webView.Show();
        }
    }
}
