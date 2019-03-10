using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MvcCorporateAdal.Infrastructure;
using Newtonsoft.Json.Linq;

namespace MvcCorporateAdal.Pages
{
    public class ApiModel : PageModel
    {
        private readonly AzureAdOptions _azureAdOptions;
        private readonly IMemoryCache _memoryCache;

        public ApiModel(IOptions<AzureAdOptions> azureAdOptions, IMemoryCache memoryCache)
        {
            _azureAdOptions = azureAdOptions.Value;
            _memoryCache = memoryCache;
        }

        public string Json { get; set; }

        public async Task OnGet()
        {
            // Because we signed-in already in the WebApp, the userObjectId is know
            string userObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

            // Using ADAL.Net, get a bearer token to access the TodoListService
            AuthenticationContext authContext = new AuthenticationContext(
                authority: _azureAdOptions.Authority,
                tokenCache: new NaiveSessionCache(
                    userId: userObjectID,
                    cache: _memoryCache));
            ClientCredential credential = new ClientCredential(
                clientId: _azureAdOptions.ClientId,
                clientSecret: _azureAdOptions.ClientSecret);
            AuthenticationResult result = await authContext.AcquireTokenSilentAsync(
                resource: _azureAdOptions.ApiResourceId,
                clientCredential: credential,
                userId: new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));


            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            var content = await client.GetStringAsync("https://localhost:5001/api/values");

            Json = JArray.Parse(content).ToString();
        }
    }
}