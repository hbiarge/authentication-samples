using System.Net.Http;
using System.Windows;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using Newtonsoft.Json.Linq;

namespace WpfIdSrv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static OidcClient _oidcClient;
        private static readonly HttpClient ApiClient = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void CallApi(object sender, RoutedEventArgs e)
        {
            //var options = SystemBrowser();
            var options = WpfEmbeddedBrowser();

            _oidcClient = new OidcClient(options);
            var result = await _oidcClient.LoginAsync(new LoginRequest());

            if (result.IsError)
            {
                MessageBox.Show(
                    this,
                    $"Error {result.Error}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            ApiClient.SetBearerToken(result.AccessToken);
            var response = await ApiClient.GetAsync("https://localhost:5001/api/values");

            if (response.IsSuccessStatusCode)
            {
                var json = JArray.Parse(await response.Content.ReadAsStringAsync());
                TbResut.Text = json.ToString();
            }
            else
            {
                MessageBox.Show(
                    this,
                    $"Response {response.StatusCode}-{response.ReasonPhrase}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private OidcClientOptions SystemBrowser()
        {
            // create a redirect URI using an available port on the loopback address.
            // requires the OP to allow random ports on 127.0.0.1 - otherwise set a static port
            var browser = new SystemBrowser();
            var redirectUri = string.Format($"http://127.0.0.1:{browser.Port}");

            var options = new OidcClientOptions
            {
                Authority = "https://localhost:5021",
                ClientId = "public.hybrid.pkce",
                RedirectUri = redirectUri,
                Scope = "openid profile api1",

                Browser = browser
            };

            return options;
        }

        private OidcClientOptions WpfEmbeddedBrowser()
        {
            var options = new OidcClientOptions
            {
                Authority = "https://localhost:5021",
                ClientId = "public.code.pkce",
                Scope = "openid profile api1",
                RedirectUri = "http://127.0.0.1/sample-wpf-app",
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.FormPost,
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                Browser = new WpfEmbeddedBrowser(this)
            };

            return options;
        }
    }
}
