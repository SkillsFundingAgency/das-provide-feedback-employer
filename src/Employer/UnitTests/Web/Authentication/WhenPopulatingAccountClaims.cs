using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using ESFA.DAS.EmployerProvideFeedback.Authentication;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace UnitTests.Web.Authentication
{
    public class WhenPopulatingAccountClaims
    {
        [Test, MoqAutoData]
        public async Task Then_The_Claims_Are_Populated_For_Gov_User(
            string nameIdentifier,
            string idamsIdentifier,
            string emailAddress,
            EmployerUserAccounts accountData,
            [Frozen] Mock<IEmployerAccountService> accountService,
            [Frozen] Mock<IOptions<ProvideFeedbackEmployerWebConfiguration>> configuration,
            EmployerAccountPostAuthenticationClaimsHandler handler)
        {
            accountData.IsSuspended = false;
            var tokenValidatedContext = ArrangeTokenValidatedContext(nameIdentifier, idamsIdentifier, emailAddress);
            accountService.Setup(x => x.GetUserAccounts(nameIdentifier, emailAddress)).ReturnsAsync(accountData);

            var actual = await handler.GetClaims(tokenValidatedContext);

            accountService.Verify(x => x.GetUserAccounts(nameIdentifier, emailAddress), Times.Once);
            accountService.Verify(x => x.GetUserAccounts(idamsIdentifier, emailAddress), Times.Never);
            actual.Should().ContainSingle(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
            var actualClaimValue = actual.First(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier)).Value;
            JsonConvert.SerializeObject(accountData.UserAccounts.ToDictionary(k => k.AccountId)).Should()
                .Be(actualClaimValue);
            actual.First(c=>c.Type.Equals(EmployerClaims.UserId)).Value.Should().Be(accountData.EmployerUserId);
            actual.First(c=>c.Type.Equals(EmployerClaims.GivenName)).Value.Should().Be(accountData.FirstName);
            actual.First(c=>c.Type.Equals(EmployerClaims.FamilyName)).Value.Should().Be(accountData.LastName);
            actual.First(c=>c.Type.Equals(EmployerClaims.EmailAddress)).Value.Should().Be(emailAddress);
            actual.FirstOrDefault(c=>c.Type.Equals(ClaimTypes.AuthorizationDecision)).Should().BeNull();
        }
        [Test, MoqAutoData]
        public async Task Then_The_Claims_Are_Populated_For_Gov_User_With_Suspended_Flag(
            string nameIdentifier,
            string idamsIdentifier,
            string emailAddress,
            EmployerUserAccounts accountData,
            [Frozen] Mock<IEmployerAccountService> accountService,
            [Frozen] Mock<IOptions<ProvideFeedbackEmployerWebConfiguration>> configuration,
            EmployerAccountPostAuthenticationClaimsHandler handler)
        {
            accountData.IsSuspended = true;
            var tokenValidatedContext = ArrangeTokenValidatedContext(nameIdentifier, idamsIdentifier, emailAddress);
            accountService.Setup(x => x.GetUserAccounts(nameIdentifier, emailAddress)).ReturnsAsync(accountData);

            var actual = await handler.GetClaims(tokenValidatedContext);

            accountService.Verify(x => x.GetUserAccounts(nameIdentifier, emailAddress), Times.Once);
            accountService.Verify(x => x.GetUserAccounts(idamsIdentifier, emailAddress), Times.Never);
            actual.Should().ContainSingle(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
            var actualClaimValue = actual.First(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier)).Value;
            JsonConvert.SerializeObject(accountData.UserAccounts.ToDictionary(k => k.AccountId)).Should()
                .Be(actualClaimValue);
            actual.First(c=>c.Type.Equals(EmployerClaims.UserId)).Value.Should().Be(accountData.EmployerUserId);
            actual.First(c=>c.Type.Equals(EmployerClaims.GivenName)).Value.Should().Be(accountData.FirstName);
            actual.First(c=>c.Type.Equals(EmployerClaims.FamilyName)).Value.Should().Be(accountData.LastName);
            actual.First(c=>c.Type.Equals(EmployerClaims.EmailAddress)).Value.Should().Be(emailAddress);
            actual.First(c=>c.Type.Equals(ClaimTypes.AuthorizationDecision)).Value.Should().Be("Suspended");
        }

        private TokenValidatedContext ArrangeTokenValidatedContext(string nameIdentifier, string idamsIdentifier,
            string emailAddress)
        {
            var identity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, nameIdentifier),
                new Claim(EmployerClaims.UserId, idamsIdentifier),
                new Claim(ClaimTypes.Email, emailAddress)
            });

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(identity));
            return new TokenValidatedContext(new DefaultHttpContext(),
                new AuthenticationScheme(",", "", typeof(TestAuthHandler)),
                new OpenIdConnectOptions(), Mock.Of<ClaimsPrincipal>(), new AuthenticationProperties())
            {
                Principal = claimsPrincipal
            };
        }


        private class TestAuthHandler : IAuthenticationHandler
        {
            public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
            {
                throw new NotImplementedException();
            }

            public Task<AuthenticateResult> AuthenticateAsync()
            {
                throw new NotImplementedException();
            }

            public Task ChallengeAsync(AuthenticationProperties? properties)
            {
                throw new NotImplementedException();
            }

            public Task ForbidAsync(AuthenticationProperties? properties)
            {
                throw new NotImplementedException();
            }
        }
    }
}