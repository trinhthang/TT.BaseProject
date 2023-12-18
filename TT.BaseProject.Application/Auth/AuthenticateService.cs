using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TT.BaseProject.Application.Base;
using TT.BaseProject.Application.Contracts.Auth;
using TT.BaseProject.Domain.Authen;
using TT.BaseProject.Domain.Business;
using TT.BaseProject.Domain.Config;
using TT.BaseProject.Domain.Constant.Authen;
using BCrypt.Net;

namespace TT.BaseProject.Application.Business
{
    public class AuthenticateService : BaseService<UserEntity, IAuthenticateRepo>, IAuthenticateService
    {
        protected readonly AuthConfig _config;

        public AuthenticateService(IAuthenticateRepo repo, IServiceProvider serviceProvider, IOptions<AuthConfig> config) : base(repo, serviceProvider)
        {
            _config = config.Value;
        }

        /// <summary>
        /// Lấy user theo user_id
        /// </summary>
        /// <param name="id"></param>
        public async Task<UserEntity> GetUserByUserIdAsync(Guid id)
        {
            var user = await GetUserByUserIdAsync(id);

            return user;
        }

        /// <summary>
        /// Đăng ký
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> Register(AuthenticateRequest registerModel)
        {
            // Lấy người dùng theo username trong database
            var users = await _repo.GetAsync<UserEntity>(nameof(UserEntity.user_name), registerModel.UserName);

            //Người dùng đã tồn tại
            if (users != null && users.Any()) return new AuthenticateResponse(success: false, AuthenResponseCode.EXIST_USER);

            //Check rule tên người dùng và mật khẩu
            if (String.IsNullOrWhiteSpace(registerModel.UserName) || String.IsNullOrWhiteSpace(registerModel.Password))
            {
                return new AuthenticateResponse(success: false, AuthenResponseCode.INVALIDUSERORPASSWORD);
            }

            //Generate new password
            var salt = GenerateSalt();
            var passwordHash = GeneratePassword(registerModel.Password, salt);
            var user = new UserEntity()
            {
                user_id = Guid.NewGuid(),
                user_name = registerModel.UserName,
                salt = salt,
                password = passwordHash
            };

            //Lưu người dùng vào database
            await _repo.InsertAsync(user);

            // Register successful so generate jwt token
            var token = GenerateJwtToken(user);

            return new AuthenticateResponse(success: true, string.Empty, user, token);
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> Login(AuthenticateRequest loginModel)
        {
            // Lấy người dùng theo username trong database
            var users = await _repo.GetAsync<UserEntity>(nameof(UserEntity.user_name), loginModel.UserName);

            // return null if user not found
            if (users == null || !users.Any())
            {
                return new AuthenticateResponse(success: false, AuthenResponseCode.NOTEXIST_USER);
            }

            var user = users.FirstOrDefault();
            //Compare password login with passwordHash
            if (string.IsNullOrEmpty(loginModel.UserName)
                || string.IsNullOrEmpty(loginModel.Password)
                || !ValidatePassword(loginModel.Password, user.password, user.salt))
            {
                return new AuthenticateResponse(success: false, AuthenResponseCode.WRONGUSERORPASSWORD);
            }

            // authentication successful so generate jwt token
            var token = GenerateJwtToken(user);

            return new AuthenticateResponse(success: true, string.Empty, user, token);
        }


        #region Private

        /// <summary>
        /// Sinh chuỗi token cho User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string GenerateJwtToken(UserEntity user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.user_id.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(_config.ExpiredMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private string GenerateSalt()
        {
            return BCrypt.Net.BCrypt.GenerateSalt();
        }

        private string GeneratePassword(string password, string salt)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }

        private bool ValidatePassword(string password, string passwordHash, string salt)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        #endregion
    }
}
