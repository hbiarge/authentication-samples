using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace ApiCorporate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<ClaimDto>> Get()
        {
            var claims = User.Claims
                .Select(c => new ClaimDto { 
                    Type = c.Type, 
                    Value = c.Value, 
                    Issuer = c.Issuer });

            return Ok(claims);
        }

        public class ClaimDto
        {
            public string Type { get; set; }
            public string Value { get; set; }
            public string Issuer { get; set; }
        }
    }
}
