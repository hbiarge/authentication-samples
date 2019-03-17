using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace MvcIdSrv.Infrastructure
{
    public class CustomOpenIdConnectEvents : LogOpenIdConnectEvents
    {
        public CustomOpenIdConnectEvents(ILogger<LogOpenIdConnectEvents> logger)
            : base(logger)
        {
        }

        public override async Task UserInformationReceived(UserInformationReceivedContext context)
        {
            await base.UserInformationReceived(context);

            // AAD fixup
            if (context.User["name"] is JArray values)
            {
                context.User["name"] = values[0];
            }
        }
    }
}
