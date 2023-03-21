using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using SFA.DAS.GovUK.Auth.Services;

namespace ESFA.DAS.EmployerProvideFeedback.Authentication
{
    public class EmployerAccountPostAuthenticationClaimsHandler : ICustomClaims
    {
        private readonly ProvideFeedbackEmployerWebConfiguration _configuration;
        private readonly IEmployerAccountService _accountService;

        public EmployerAccountPostAuthenticationClaimsHandler(IOptions<ProvideFeedbackEmployerWebConfiguration> configuration, IEmployerAccountService accountService)
        {
            _configuration = configuration.Value;
            _accountService = accountService;
        }
        
        public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
        {
            string userId;
            var email = string.Empty;
            var claims = new List<Claim>();
            
            if (_configuration.UseGovSignIn)
            {
                userId = tokenValidatedContext.Principal.Claims
                    .First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                    .Value;
                email = tokenValidatedContext.Principal.Claims
                    .First(c => c.Type.Equals(ClaimTypes.Email))
                    .Value;
                claims.Add(new Claim(EmployerClaims.EmailAddress, email));
            }
            else
            {
                userId = tokenValidatedContext.Principal.Claims
                    .First(c => c.Type.Equals(EmployerClaims.UserId))
                    .Value;
            }
            
            var result = await _accountService.GetUserAccounts(userId, email);

            
            var accountsAsJson = JsonConvert.SerializeObject(result.UserAccounts.ToDictionary(k => k.AccountId));
            claims.Add(new Claim(EmployerClaims.Account, accountsAsJson, JsonClaimValueTypes.Json));

            if (!_configuration.UseGovSignIn)
            {
                return claims;
            }

            if (result.IsSuspended)
            {
                claims.Add(new Claim(ClaimTypes.AuthorizationDecision, "Suspended"));    
            }
            
            claims.Add(new Claim(EmployerClaims.UserId, result.EmployerUserId));
            claims.Add(new Claim(EmployerClaims.GivenName, result.FirstName));
            claims.Add(new Claim(EmployerClaims.FamilyName, result.LastName));

            return claims;
        }
    }
}