using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TokenRefreshToken.Models;

namespace TokenRefreshToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Authentication : Controller
    {
        private readonly JwtOptions _jwtOptions;

        public Authentication(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        [HttpPost("giriş")]
        public IActionResult Giriş([FromBody] ApiUser apiUser)
        {
            var apiuser = AuthenticationUser(apiUser);
            if (apiuser == null) return NotFound("kullanıcı yok");

            var token = TokenService(apiuser);
            apiuser.RefreshToken = CreateRefreshToken();
            apiuser.RefreshTokenEndDate = token.Expiration.AddMinutes(3);

            return Ok(new
            {
                AccessToken = token.AccessToken,
                RefreshToken = apiuser.RefreshToken
            });
        }


        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetUserById(int id)
        {
            var user = ApiUserData.Kullanici.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            return Ok(user);
        }

        private (string AccessToken, DateTime Expiration) TokenService(ApiUser apiuser)
        {
            if (_jwtOptions.Key == null) throw new Exception("jwt ayaralarındaki key null olamaz");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, apiuser.KullaniciAdi!),
                new Claim(ClaimTypes.Role, apiuser.Rol!)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return (AccessToken: accessToken, Expiration: token.ValidTo);
        }

        private string CreateRefreshToken()
        {
            byte[] number = new byte[32];

            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(number);
                return Convert.ToBase64String(number);
            }
        }
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] string refreshToken)
        {
            // Refresh token kontrolü
            var user = ApiUserData.Kullanici.FirstOrDefault(u => u.RefreshToken == refreshToken);
            if (user == null)
            {
                return Unauthorized("Geçersiz refresh token.");
            }

            // Yeni access token oluştur
            var token = TokenService(user);

            // Yeni bir refresh token oluştur
            user.RefreshToken = CreateRefreshToken();
            user.RefreshTokenEndDate = token.Expiration.AddMinutes(3);

            return Ok(new
            {
                AccessToken = token.AccessToken,
                RefreshToken = user.RefreshToken
            });
        }

        private ApiUser AuthenticationUser(ApiUser apiUser)
        {
            return ApiUserData
                  .Kullanici
                  .FirstOrDefault(x =>
                  x.KullaniciAdi.ToLower() == apiUser.KullaniciAdi
                  && x.Sifre == apiUser.Sifre);
        }
    }
}

