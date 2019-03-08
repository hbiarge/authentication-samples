using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(MvcLocalUsers.Areas.Identity.IdentityHostingStartup))]
namespace MvcLocalUsers.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => { });
        }
    }
}