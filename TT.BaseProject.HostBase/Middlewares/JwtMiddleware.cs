using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using TT.BaseProject.Application.Contracts.Auth;
using TT.BaseProject.Domain.Config;
using TT.BaseProject.Domain.Context;

namespace TT.BaseProject.HostBase.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly AuthConfig _authConfig;

        public JwtMiddleware(RequestDelegate next, IOptions<AuthConfig> authConfig)
        {
            _next = next;
            _authConfig = authConfig.Value;
        }

        public async Task Invoke(HttpContext context, IAuthenticateService authService, IContextService contextService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                AttachUserToContext(context, authService, contextService, token);

            await _next(context);
        }

        private async void AttachUserToContext(HttpContext context, IAuthenticateService authService, IContextService contextService, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_authConfig.Secret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
                var user = await authService.GetUserByUserIdAsync(userId);
                // set context data
                var contextData = new ContextData()
                {
                    UserId = user.user_id,
                    UserName = user.user_name
                    //TODO truyền thêm những thông tin nếu cần thiết vào context
                };
                contextService.Set(contextData);
                // attach user to context on successful jwt validation
                context.Items["User"] = user;
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}
