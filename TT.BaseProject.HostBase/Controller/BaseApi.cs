using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.HostBase.Controller
{
    [ApiController]
    [Route("[controller]")]
    public abstract class BaseApi : ControllerBase
    {
        public virtual string ControllerName { get; set; } = "Base";

        [AllowAnonymous]
        [HttpGet("health")]
        public IActionResult Get()
        {
            return Ok(ControllerName + " live!");
        }

        [AllowAnonymous]
        [HttpGet("now")]
        public IActionResult GetNow()
        {
            return Ok(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        }
    }
}
