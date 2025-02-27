using Auth0.OidcClient;
using Auth0WPF.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.Data.Entities;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rewe.Revit.API
{
    internal class Client : ObservableObject
    {
        //const string API_BASE_URL = "https://api.greenbuilding.d2p.ch/";
        const string API_BASE_URL = "http://localhost:5000/";

        const string DOMAIN = "rewe-auth.eu.auth0.com";
        const string CLIENT_ID = "YNJY613qnUrVCvV3fAofPmgO2JR75iB7";
        const string REDIRECT_URI = "http://localhost:3003";
        const string POST_LOGOUT_REDIRECT_URI = "http://localhost:3003/logout";
        const string AUDIENCE_PARAMETER = "audience";
        const string AUDIENCE = "https://rewe/api";
        const string PROMPT_PARAMETER = "prompt";
        const string PROMPT_PARAMETER_VALUE = "login";

        private Auth0Client Auth0Client;

        static Client()
        {
            CurrentInstance = new Client();
        }

        private Client()
        {
            var auth0ClientOptions = new Auth0ClientOptions
            {
                Domain = DOMAIN,
                ClientId = CLIENT_ID,
                RedirectUri = REDIRECT_URI,
                PostLogoutRedirectUri = POST_LOGOUT_REDIRECT_URI,
                Browser = new Auth0WebViewBrowser("Login"),
            };

#if DEBUG
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            auth0ClientOptions.EnableTelemetry = true;
            auth0ClientOptions.LoggerFactory = loggerFactory;
#endif

            Auth0Client = new Auth0Client(auth0ClientOptions);
            Api = new ReweClient(API_BASE_URL, HttpClient);
        }

        public static Client CurrentInstance { get; private set; }
        
        public HttpClient HttpClient { get; } = new HttpClient();

        public ReweClient Api { get; }

        public UserEntity? User { get; private set; } = null;

        public bool LoggedIn => User != null;

        private bool ActivelyLoggedOut = false;

        public async Task<(bool, LoginResult?)> LoginInteractively()
        {
            LoginResult loginResult = null;

            try
            {
                var extraParameters = new Dictionary<string, string>
                {
                    { AUDIENCE_PARAMETER, AUDIENCE }
                };
                // force new login window after explicit logout, even if cookies persisted
                // TODO: Instead of this, logout should clear cookies from the embedded web view.
                if (ActivelyLoggedOut) extraParameters.Add(PROMPT_PARAMETER, PROMPT_PARAMETER_VALUE);

                loginResult = await Auth0Client.LoginAsync(extraParameters);
                if (loginResult.IsError) return (false, loginResult);
                HttpClient.SetBearerToken(loginResult.AccessToken);

                User = await Api.AuthenticateAsync();

                OnPropertyChanged(nameof(LoggedIn));
                OnPropertyChanged(nameof(User));
                ActivelyLoggedOut = false;
            }
            catch (Exception e)
            {
                HttpClient.SetBearerToken(null);
            }

            return (User != null, loginResult);
        }

        public async Task<(bool, LoginResult?)> EnsureLogin()
        {
            if (LoggedIn) return (true, null);
            return await LoginInteractively();
        }

        public async Task<bool> Logout()
        {
            try
            {
                await Api.SignoutAsync();
                User = null;
                OnPropertyChanged(nameof(LoggedIn));
                OnPropertyChanged(nameof(User));
                ActivelyLoggedOut = true;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
