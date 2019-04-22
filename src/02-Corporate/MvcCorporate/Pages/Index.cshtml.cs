using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MvcCorporate.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IAuthenticationSchemeProvider schemes;

        public IndexModel(IAuthenticationSchemeProvider schemes)
        {
            this.schemes = schemes;
        }

        public IEnumerable<AuthenticationScheme> Schemes { get; private set; }

        public IEnumerable<AuthenticationScheme> RequestHandlerSchemes { get; private set; }

        public AuthenticationScheme DefaultAuthenticateScheme { get; private set; }
        public AuthenticationScheme DefaultChallengeScheme { get; private set; }
        public AuthenticationScheme DefaultForbidScheme { get; private set; }
        public AuthenticationScheme DefaultSignInScheme { get; private set; }
        public AuthenticationScheme DefaultSignOutScheme { get; private set; }

        public async Task OnGet()
        {
            Schemes = await this.schemes.GetAllSchemesAsync();
            RequestHandlerSchemes = await this.schemes.GetRequestHandlerSchemesAsync();

            DefaultAuthenticateScheme = await this.schemes.GetDefaultAuthenticateSchemeAsync();
            DefaultChallengeScheme = await this.schemes.GetDefaultChallengeSchemeAsync();
            DefaultForbidScheme = await this.schemes.GetDefaultForbidSchemeAsync();
            DefaultSignInScheme = await this.schemes.GetDefaultSignInSchemeAsync();
            DefaultSignOutScheme = await this.schemes.GetDefaultSignOutSchemeAsync();
        }
    }
}
