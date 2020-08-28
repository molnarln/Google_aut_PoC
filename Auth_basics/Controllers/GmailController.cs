using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Google_Auth_Practice.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class GmailController : Controller
    {
        public IActionResult GmailTest()
        {
            return Ok(new {message = "minden OK" });
        }


    }
}
