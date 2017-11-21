using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WpfCorporateAdal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Config Values

        //
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        // The Tenant is the name of the Azure AD tenant in which this application is registered.
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Redirect URI is the URI where Azure AD will return OAuth responses.
        // The Authority is the sign-in URL of the tenant.
        //
        private static readonly string AadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static readonly string Tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static readonly string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static readonly string Authority = String.Format(CultureInfo.InvariantCulture, AadInstance, Tenant);

        private static readonly string ApiResourceId = ConfigurationManager.AppSettings["ida:ApiResourceId"];
        private static readonly string GraphResourceId = ConfigurationManager.AppSettings["ida:GraphResourceId"];
        private static readonly string GraphApiVersion = ConfigurationManager.AppSettings["ida:GraphApiVersion"];
        private static readonly string GraphApiEndpoint = ConfigurationManager.AppSettings["ida:GraphEndpoint"];

        private readonly Uri _redirectUri = new Uri(ConfigurationManager.AppSettings["ida:RedirectUri"]);

        #endregion

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly AuthenticationContext _authContext;

        public MainWindow()
        {
            InitializeComponent();

            _authContext = new AuthenticationContext(Authority, new FileCache());

            CheckForCachedToken();
        }

        public async void CheckForCachedToken()
        {
            // As the application starts, try to get an access token without prompting the user.  
            // If one exists, show the user as signed in.
            AuthenticationResult result;
            try
            {
                result = await _authContext.AcquireTokenAsync(
                    GraphResourceId,
                    ClientId,
                    _redirectUri,
                    new PlatformParameters(PromptBehavior.Never));
            }
            catch (AdalException ex)
            {
                if (ex.ErrorCode != "user_interaction_required")
                {
                    // An unexpected error occurred.
                    MessageBox.Show(this, ex.Message);
                }

                // If user interaction is required, proceed to main page without singing the user in.
                return;
            }

            // A valid token is in the cache
            SignOutButton.Visibility = Visibility.Visible;
            UserNameLabel.Content = result.UserInfo.DisplayableId;
        }

        private void SignOut(object sender = null, RoutedEventArgs args = null)
        {
            // Clear the token cache
            _authContext.TokenCache.Clear();

            // Clear cookies from the browser control.
            ClearCookies();

            // Reset the UI
            SearchResults.ItemsSource = string.Empty;
            SignOutButton.Visibility = Visibility.Hidden;
            UserNameLabel.Content = string.Empty;
            SearchText.Text = string.Empty;
        }

        private async void Search(object sender, RoutedEventArgs e)
        {
            // Validate the Input String
            if (string.IsNullOrEmpty(SearchText.Text))
            {
                MessageBox.Show(this, "Please enter a value for the To Do item name");
                return;
            }

            // Get an Access Token for the Graph API
            AuthenticationResult result;
            try
            {
                result = await _authContext.AcquireTokenAsync(
                    GraphResourceId,
                    ClientId,
                    _redirectUri,
                    new PlatformParameters(PromptBehavior.Auto));
                UserNameLabel.Content = result.UserInfo.DisplayableId;
                SignOutButton.Visibility = Visibility.Visible;
            }
            catch (AdalException ex)
            {
                // An unexpected error occurred, or user canceled the sign in.
                if (ex.ErrorCode != "access_denied")
                {
                    MessageBox.Show(this, ex.Message);
                }

                return;
            }

            // Once we have an access_token, search for users.
            try
            {
                var graphRequest = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}{1}/users?api-version={2}&$filter=startswith(userPrincipalName, '{3}')",
                    GraphApiEndpoint,
                    Tenant,
                    GraphApiVersion,
                    SearchText.Text);
                var request = new HttpRequestMessage(HttpMethod.Get, graphRequest);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                var response = _httpClient.SendAsync(request).Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new WebException(response.StatusCode + ": " + response.ReasonPhrase);
                }

                var content = await response.Content.ReadAsStringAsync();
                var jResult = JObject.Parse(content);

                if (jResult["odata.error"] != null)
                {
                    throw new Exception((string)jResult["odata.error"]["message"]["value"]);
                }

                // Show Search Results
                SearchResults.ItemsSource = jResult["value"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error: " + ex.Message);
            }
        }

        private async void CallApi(object sender, RoutedEventArgs e)
        {
            // Get an Access Token for the API
            AuthenticationResult result;
            try
            {
                result = await _authContext.AcquireTokenAsync(
                    ApiResourceId,
                    ClientId,
                    _redirectUri,
                    new PlatformParameters(PromptBehavior.Auto));
                UserNameLabel.Content = result.UserInfo.DisplayableId;
                SignOutButton.Visibility = Visibility.Visible;
            }
            catch (AdalException ex)
            {
                // An unexpected error occurred, or user canceled the sign in.
                if (ex.ErrorCode != "access_denied")
                {
                    MessageBox.Show(this, ex.Message);
                }

                return;
            }

            // Once we have an access_token, call api.
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12415/api/values");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                var response = _httpClient.SendAsync(request).Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new WebException(response.StatusCode + ": " + response.ReasonPhrase);
                }

                var content = await response.Content.ReadAsStringAsync();
                var data = JArray.Parse(content).ToString();

                MessageBox.Show(this, data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error: " + ex.Message);
            }
        }

        #region Cookie Management

        // This function clears cookies from the browser control used by ADAL.
        private void ClearCookies()
        {
            const int INTERNET_OPTION_END_BROWSER_SESSION = 42;
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_END_BROWSER_SESSION, IntPtr.Zero, 0);
        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);

        #endregion
    }
}
