using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Acheve.Authentication.Events
{
    public class LogTwitterEvents : TwitterEvents
    {
        private readonly ILogger<LogTwitterEvents> _logger;

        public LogTwitterEvents(ILogger<LogTwitterEvents> logger)
        {
            _logger = logger;
        }

        public override Task RedirectToAuthorizationEndpoint(RedirectContext<TwitterOptions> context)
        {
            _logger.LogInformation("Scheme {scheme}: RedirectToAuthorizationEndpoint called...", context.Scheme.Name);
            return base.RedirectToAuthorizationEndpoint(context);
        }

        public override Task TicketReceived(TicketReceivedContext context)
        {
            _logger.LogInformation("Scheme {scheme}: TicketReceived called...", context.Scheme.Name);
            return base.TicketReceived(context);
        }

        public override Task CreatingTicket(TwitterCreatingTicketContext context)
        {
            _logger.LogInformation("Scheme {scheme}: CreatingTicket called...", context.Scheme.Name);
            return base.CreatingTicket(context);
        }

        public override Task RemoteFailure(RemoteFailureContext context)
        {
            _logger.LogInformation("Scheme {scheme}: RemoteFailure called...", context.Scheme.Name);
            return base.RemoteFailure(context);
        }
    }
}
