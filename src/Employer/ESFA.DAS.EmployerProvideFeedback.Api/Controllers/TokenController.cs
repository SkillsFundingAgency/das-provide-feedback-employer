using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ESFA.DAS.EmployerProvideFeedback.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : Controller
    {
        private readonly IOptions<JwtOptions> _jwtOptions;

        public TokenController(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult CreateToken([FromBody]LoginModel login)
        {
            IActionResult response = Unauthorized();
            var user = Authenticate(login);

            if (user == null) return response;

            var tokenString = BuildToken(user);
            response = Ok(new { token = tokenString });

            return response;
        }

        private string BuildToken(UserModel user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Value.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_jwtOptions.Value.Issuer,
                _jwtOptions.Value.Issuer,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserModel Authenticate(LoginModel login)
        {
            UserModel user = null;
            // TODO: verify API key with list of active keys
            if (login.ApiKey == "15ENjjhvbdnnlbubvchklnfvkelvutnt")
            {
                user = new UserModel { Name = "test", Email = "test@mkten.com" };
            }
            return user;
        }

        [Serializable]
        public class LoginModel
        {
            public string ApiKey { get; set; }
        }

        [Serializable]
        private class UserModel
        {
            public string Name { get; set; }
            public string Email { get; set; }
        }
    }
}