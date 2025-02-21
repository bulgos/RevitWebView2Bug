using IdentityModel.OidcClient.Browser;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Auth0WPF.Core
{
    public class Auth0WebViewBrowser : IBrowser
    {
        private readonly Func<Window> _windowFactory;

        private readonly bool _shouldCloseWindow;

        public Auth0WebViewBrowser(Func<Window> windowFactory, bool shouldCloseWindow = true)
        {
            _windowFactory = windowFactory;
            _shouldCloseWindow = shouldCloseWindow;
        }

        public Auth0WebViewBrowser(string title = "Authenticating...", string? userDataFolder = null, int width = 1024, int height = 768)
            : this(() => new Window
            {
                Name = "WebAuthentication",
                Title = title,
                Width = width,
                Height = height
            })
        {
            if (userDataFolder != null && Directory.Exists(userDataFolder))
            {
                UserDataFolder = userDataFolder;
            }
        }

        public string UserDataFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            TaskCompletionSource<BrowserResult> tcs = new TaskCompletionSource<BrowserResult>();
            Window window = _windowFactory();
            WebView2 webView = new WebView2();
            window.Content = webView;
            webView.NavigationStarting += delegate (object? sender, CoreWebView2NavigationStartingEventArgs e)
            {
                if (e.Uri.StartsWith(options.EndUrl))
                {
                    tcs.SetResult(new BrowserResult
                    {
                        ResultType = BrowserResultType.Success,
                        Response = e.Uri.ToString()
                    });
                    if (_shouldCloseWindow)
                    {
                        window.Close();
                    }
                    else
                    {
                        window.Content = null;
                    }
                }
            };
            window.Closing += delegate
            {
                webView.Dispose();
                if (!tcs.Task.IsCompleted)
                {
                    tcs.SetResult(new BrowserResult
                    {
                        ResultType = BrowserResultType.UserCancel
                    });
                }
            };
            window.Show();

            var webView2Environment = await CoreWebView2Environment.CreateAsync(null, UserDataFolder);
            await webView.EnsureCoreWebView2Async(webView2Environment);
            webView.CoreWebView2.Navigate(options.StartUrl);
            return await tcs.Task;
        }
    }
}
