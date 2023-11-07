using Microsoft.AspNetCore.Mvc;
using TT.BaseProject.Application.Contracts.Auth;
using TT.BaseProject.Domain.Authen;
using TT.BaseProject.HostBase.Controller;

namespace TT.BaseProject.AuthApi.Controllers
{
    public class AuthController : BaseApi
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthenticateService _service;

        public AuthController(ILogger<AuthController> logger, IAuthenticateService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet("health")]
        public IActionResult Get()
        {
            return Ok("Auth");
        }

        [HttpPost("register")]
        public virtual async Task<IActionResult> Register([FromBody] AuthenticateRequest param)
        {
            var res = await _service.Register(param);
            return Ok(res);
        }

        [HttpPost("login")]
        public virtual async Task<IActionResult> Login([FromBody] AuthenticateRequest param)
        {
            var res = await _service.Login(param);
            return Ok(res);
        }
    }
}