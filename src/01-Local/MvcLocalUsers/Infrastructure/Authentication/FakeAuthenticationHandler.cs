using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MvcLocalUsers.Infrastructure.Authentication
{
    public class FakeAuthenticationHandler : AuthenticationHandler<FakeAuthenticationOptions>
    {
        public FakeAuthenticationHandler(
            IOptionsMonitor<FakeAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "Hugo"),
                new Claim(ClaimTypes.Surname, "Biarge"),
                new Claim(ClaimTypes.AuthenticationInstant, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Fake"));

            var result = AuthenticateResult.Success(
                new AuthenticationTicket(
                    principal,
                    new AuthenticationProperties(),
                    Scheme.Name));

            return Task.FromResult(result);
        }
    }
}
