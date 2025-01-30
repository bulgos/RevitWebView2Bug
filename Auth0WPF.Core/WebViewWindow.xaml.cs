using Microsoft.Web.WebView2.Core;
using System;
using System.Windows;

namespace Auth0WPF.Core
{
    /// <summary>
    /// Interaction logic for WebControl.xaml
    /// </summary>
    public partial class WebViewWindow : Window
    {
        public WebViewWindow(string source)
        {
            InitializeComponent();

            InitializeAsync();
            webView.Source = new Uri(source);
        }

        // called from Window Constructor after InitializeComponent()
        // note: the `async void` signature is required for environment init
        async void InitializeAsync()
        {
            string appDataFolderACAD = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            System.IO.Directory.CreateDirectory(appDataFolderACAD);

            // must create a data folder if running out of a secured folder that can't write like Program Files
            var env = await CoreWebView2Environment.CreateAsync(null, appDataFolderACAD);

            // NOTE: this waits until the first page is navigated - then continues
            //       executing the next line of code!
            await webView.EnsureCoreWebView2Async(env);
        }

    }
}
