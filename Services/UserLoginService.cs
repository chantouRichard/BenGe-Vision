using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using picture_backend.Models;
using picture_backend.Repositories.IRepositories;
using picture_backend.Services.IServices;
using picture_backend.Utils;

namespace picture_backend.Services
{
    public class UserLoginService : IUserLoginService
    {
        private readonly IUserRepository _userLoginRepository;
        private readonly IConfiguration _configuration;

        public UserLoginService(IUserRepository userLoginRepository, IConfiguration configuration)
        {
            _userLoginRepository = userLoginRepository;
            _configuration = configuration;
        }

        public async Task<string?> LoginAsync(string username, string plainPassword)
        {
            // 验证用户凭据
            var user = await _userLoginRepository.GetUserByUsernameAsync(username);
            if (user == null || !PasswordHelper.VerifyPassword(user.PasswordHash, plainPassword))
            {
                return null; // 登录失败
            }

            // 生成JWT令牌
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.Now.AddHours(24),

                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token); // 返回令牌
        }
    }
}
