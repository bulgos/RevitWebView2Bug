using Auth0.OidcClient;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using Newtonsoft.Json;
using Rewe.Revit.API;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Windows;

namespace Auth0WPF.Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            (bool success, LoginResult? loginResult) = await Client.CurrentInstance.EnsureLogin();
            if (!success || loginResult == null) return;
            
            DisplayResult(loginResult);
        }

        private void DisplayResult(LoginResult loginResult)
        {
            if (loginResult == null)
            {
                resultTextBox.Text = "Login result is null.";
                return;
            }

            // Display error
            if (loginResult.IsError)
            {
                resultTextBox.Text = loginResult.Error;
                return;
            }

            // Display result
            StringBuilder sb = new StringBuilder();

            //sb.AppendLine("Tokens");
            //sb.AppendLine("------");
            //sb.AppendLine($"id_token: {loginResult.IdentityToken}");
            //sb.AppendLine();
            //sb.AppendLine($"access_token: {loginResult.AccessToken}");
            //sb.AppendLine();
            //sb.AppendLine($"refresh_token: {loginResult.RefreshToken}");
            //sb.AppendLine();

            sb.AppendLine("{");
            foreach (var requestHeader in Client.CurrentInstance.HttpClient.DefaultRequestHeaders)
            {
                string headerValues = string.Join(", ", requestHeader.Value);
                sb.AppendLine($"\t{requestHeader.Key} = \"{headerValues}\"");
                sb.AppendLine();
            }
            sb.AppendLine("}");

            sb.AppendLine("Claims");
            sb.AppendLine("------");
            foreach (var claim in loginResult.User.Claims)
            {
                sb.AppendLine($"{claim.Type}: {claim.Value}");
            }

            resultTextBox.Text = sb.ToString();
        }

        private async void Projects_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var projects = await Client.CurrentInstance.Api.ProjectsGETAsync();
                resultTextBox.Text = JsonConvert.SerializeObject(projects, Formatting.Indented);
            }
            catch (Exception ex)
            {
                resultTextBox.Text = ex.Message;
            }
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            bool success = await Client.CurrentInstance.Logout();

            if (success)
                resultTextBox.Text = "Logged out successfully";
        }
    }
}