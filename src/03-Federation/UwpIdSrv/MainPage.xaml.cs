using System.Net.Http;
using System.Text;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;

namespace UwpIdSrv
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static OidcClient _oidcClient;
        private static readonly HttpClient ApiClient = new HttpClient();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void CallApi(object sender, RoutedEventArgs e)
        {
            var options = new OidcClientOptions
            {
                Authority = "https://arosbi-identity.azurewebsites.net/",
                ClientId = "public.uwp.hybrid.pkce",
                Scope = "openid profile api1 offline_access",
                RedirectUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().AbsoluteUri,

                Browser = new WabBrowser(),
            };

            _oidcClient = new OidcClient(options);
            var result = await _oidcClient.LoginAsync(new LoginRequest());

            if (!string.IsNullOrEmpty(result.Error))
            {
                TbStatus.Text = result.Error;
                return;
            }

            var message = new StringBuilder();

            foreach (var claim in result.User.Claims)
            {
                message.AppendLine($"{claim.Type}: {claim.Value}");
            }

            message.AppendLine(string.Empty);
            message.AppendLine("ACCESS TOKEN");
            message.AppendLine(result.AccessToken);

            TbResut.Text = message.ToString();
        }
    }
}
