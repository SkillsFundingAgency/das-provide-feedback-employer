namespace ESFA.DAS.EmployerProvideFeedback.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text;
    using System.Threading.Tasks;

    using ESFA.DAS.EmployerProvideFeedback.Api.Configuration;
    using ESFA.DAS.EmployerProvideFeedback.Api.Dto;
    using ESFA.DAS.EmployerProvideFeedback.Api.Repository;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore.Internal;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : Controller
    {
        private readonly IOptions<JwtOptions> jwtOptions;

        private readonly IOptions<AzureOptions> azureOptions;

        private readonly ITokenProvider tokenProvider;

        private readonly ILogger<TokenController> logger;

        public TokenController(IOptions<JwtOptions> jwtOptions, IOptions<AzureOptions> azureOptions, ILogger<TokenController> logger, ITokenProvider tokenProvider)
        {
            this.azureOptions = azureOptions;
            this.jwtOptions = jwtOptions;
            this.logger = logger;

            this.tokenProvider = tokenProvider;
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> CreateToken([FromHeader] string apiKey)
        {
            try
            {
                UserModel user = await this.Authenticate(apiKey);

                if (user != null)
                {
                    string tokenString = this.BuildToken(user);
                    return this.Ok(new { token = tokenString });
                }

                this.logger.LogWarning($"Unauthorized api key {apiKey} detected");
                return this.Unauthorized();
            }
            catch (Exception e)
            {
                this.logger.LogError($"Exception when attempting to get all Provider Feedback records. Message: {e.Message}");
                return this.StatusCode(500, e.Message);
            }
        }

        private async Task<UserModel> Authenticate(string apiKey)
        {
            UserModel user = null;

            var account = await this.tokenProvider.GetItemAsync(k => k.Key == apiKey);

            // TODO: verify API key with list of active keys
            if (account != null)
            {
                user = new UserModel { Name = "test", Email = "test@mkten.com" };
            }

            return user;
        }

        private string BuildToken(UserModel user)
        {
            var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Name),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.jwtOptions.Value.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                this.jwtOptions.Value.Issuer,
                this.jwtOptions.Value.Issuer,
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}