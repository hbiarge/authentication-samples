using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace ApiIdSrv.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            var claims = User.Claims
                .Select(c => new { c.Type, c.Value, c.Issuer });

            return Ok(claims);
        }
    }
}
