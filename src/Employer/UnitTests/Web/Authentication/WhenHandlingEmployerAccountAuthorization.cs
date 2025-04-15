using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using ESFA.DAS.EmployerProvideFeedback.Authentication;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.ProvideFeedback.Domain.Entities.ApiTypes;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.GovUK.Auth.Employer;
using SFA.DAS.Testing.AutoFixture;
using EmployerClaims = ESFA.DAS.EmployerProvideFeedback.Authentication.EmployerClaims;

namespace UnitTests.Web.Authentication
{
    public class WhenHandlingEmployerAccountAuthorization
    {
        [Test(Description = "Once decided on correct roles for actions, only Owner should work")]
        [MoqInlineAutoData("Owner")]
        [MoqInlineAutoData("Transactor")]
        [MoqInlineAutoData("Viewer")]
        public async Task Then_Returns_True_If_Employer_Is_Authorized(
            string role,
            EmployerIdentifier employerIdentifier,
            EmployerAccountRequirement requirement,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            EmployerAccountAuthorizationHandler authorizationHandler)
        {
            //Arrange
            employerIdentifier.Role = role;
            employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
            var employerAccounts = new Dictionary<string, EmployerIdentifier>{{employerIdentifier.AccountId, employerIdentifier}};
            var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
            var context = new AuthorizationHandlerContext(new [] {requirement}, claimsPrinciple, null);
            var httpContext = new DefaultHttpContext(new FeatureCollection());
            httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId,employerIdentifier.AccountId);
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            
            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeTrue();
        }
        
        [Test, MoqAutoData]
        public async Task Then_Returns_False_If_Employer_Is_Not_Authorized(
            string accountId,
            EmployerIdentifier employerIdentifier,
            EmployerAccountRequirement requirement,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            EmployerAccountAuthorizationHandler authorizationHandler)
        {
            //Arrange
            employerIdentifier.Role = "Owner";
            employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
            var employerAccounts = new Dictionary<string, EmployerIdentifier>{{employerIdentifier.AccountId, employerIdentifier}};
            var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, null);
            var responseMock = new FeatureCollection();
            var httpContext = new DefaultHttpContext(responseMock);
            httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId,accountId.ToUpper());
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeFalse();
        }

        [Test, MoqAutoData]
        public async Task Then_If_Not_In_Context_Claims_EmployerAccountService_Checked_And_True_Returned_If_Exists(
            string accountId,
            string userId,
            string email,
            EmployerIdentifier employerIdentifier,
            EmployerAccountRequirement requirement,
            EmployerUserAccountItem serviceResponse,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            [Frozen] Mock<IGovAuthEmployerAccountService> employerAccountService,
            [Frozen] Mock<IOptions<ProvideFeedbackEmployerWebConfiguration>> configuration,
            EmployerAccountAuthorizationHandler authorizationHandler)
        {
            //Arrange
            configuration.Object.Value.UseGovSignIn = false;
            serviceResponse.AccountId = accountId.ToUpper();
            serviceResponse.Role = "Owner";
            employerAccountService.Setup(x => x.GetUserAccounts(userId, email))
                .ReturnsAsync(new EmployerUserAccounts
                {
                    EmployerAccounts = new List<EmployerUserAccountItem>{ serviceResponse }
                });
            
            var userClaim = new Claim(EmployerClaims.UserId, userId);
            var employerAccounts = new Dictionary<string, EmployerIdentifier>{{employerIdentifier.AccountId, employerIdentifier}};
            var employerAccountClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {employerAccountClaim, userClaim, new Claim(ClaimTypes.Email, email)})});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, null);
            var responseMock = new FeatureCollection();
            var httpContext = new DefaultHttpContext(responseMock);
            httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, accountId.ToUpper());
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            
            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeTrue();
            
        }

        [Test, MoqAutoData]
        public async Task Then_If_Not_In_Context_Claims_EmployerAccountService_Checked_And_True_Returned_If_Exists_For_GovSignIn(
            string accountId,
            string userId,
            string email,
            EmployerIdentifier employerIdentifier,
            EmployerAccountRequirement requirement,
            EmployerUserAccountItem serviceResponse,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            [Frozen] Mock<IGovAuthEmployerAccountService> employerAccountService,
            [Frozen] Mock<IOptions<ProvideFeedbackEmployerWebConfiguration>> configuration,
            EmployerAccountAuthorizationHandler authorizationHandler)
        {
            //Arrange
            configuration.Object.Value.UseGovSignIn = true;
            serviceResponse.AccountId = accountId.ToUpper();
            serviceResponse.Role = "Owner";
            employerAccountService.Setup(x => x.GetUserAccounts(userId, email))
                .ReturnsAsync(new EmployerUserAccounts
                {
                    EmployerAccounts = new List<EmployerUserAccountItem>{ serviceResponse }
                });
            
            var userClaim = new Claim(ClaimTypes.NameIdentifier, userId);
            var employerAccounts = new Dictionary<string, EmployerIdentifier>{{employerIdentifier.AccountId, employerIdentifier}};
            var employerAccountClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {employerAccountClaim, userClaim, new Claim(ClaimTypes.Email, email)})});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, null);
            var responseMock = new FeatureCollection();
            var httpContext = new DefaultHttpContext(responseMock);
            httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId,accountId.ToUpper());
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            
            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeTrue();
            
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Not_In_Context_Claims_EmployerAccountService_Checked_And_False_Returned_If_Not_Exists(
            string accountId,
            string userId,
            EmployerIdentifier employerIdentifier,
            EmployerAccountRequirement requirement,
            EmployerUserAccountItem serviceResponse,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            [Frozen] Mock<IGovAuthEmployerAccountService> employerAccountService,
            EmployerAccountAuthorizationHandler authorizationHandler)
        {
            //Arrange
            serviceResponse.AccountId = serviceResponse.AccountId.ToUpper();
            serviceResponse.Role = "Owner";
            employerAccountService.Setup(x => x.GetUserAccounts(userId,""))
                .ReturnsAsync(new EmployerUserAccounts
                {
                    EmployerAccounts = new List<EmployerUserAccountItem>{ serviceResponse }
                });
            
            var userClaim = new Claim(EmployerClaims.UserId, userId);
            var employerAccounts = new Dictionary<string, EmployerIdentifier>{{employerIdentifier.AccountId, employerIdentifier}};
            var employerAccountClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {employerAccountClaim, userClaim})});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, null);
            var responseMock = new FeatureCollection();
            var httpContext = new DefaultHttpContext(responseMock);
            httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId,accountId.ToUpper());
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            
            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeFalse();
        }
        
        [Test, MoqAutoData]
        public async Task Then_Returns_False_If_Employer_Is_Authorized_But_Has_Invalid_Role_But_Should_Allow_All_known_Roles(
            EmployerIdentifier employerIdentifier,
            EmployerAccountRequirement requirement,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            EmployerAccountAuthorizationHandler authorizationHandler)
        {
            //Arrange
            employerIdentifier.Role = "Viewer-Owner-Transactor";
            employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
            var employerAccounts = new Dictionary<string, EmployerIdentifier>{{employerIdentifier.AccountId, employerIdentifier}};
            var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, null);
            var responseMock = new FeatureCollection();
            var httpContext = new DefaultHttpContext(responseMock);
            httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId,employerIdentifier.AccountId);
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            
            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeFalse();
        }
        
        
        [Test, MoqAutoData]
        public async Task Then_Returns_False_If_AccountId_Not_In_Url(
            EmployerIdentifier employerIdentifier,
            EmployerAccountRequirement requirement,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            EmployerAccountAuthorizationHandler authorizationHandler)
        {
            //Arrange
            employerIdentifier.Role = "Owner";
            employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
            var employerAccounts = new Dictionary<string, EmployerIdentifier>{{employerIdentifier.AccountId, employerIdentifier}};
            var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, null);
            var responseMock = new FeatureCollection();
            var httpContext = new DefaultHttpContext(responseMock);
            httpContext.Request.RouteValues.Clear();
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            
            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeFalse();
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_False_If_No_Matching_AccountIdentifier_Claim_Found(
            EmployerIdentifier employerIdentifier,
            EmployerAccountRequirement requirement,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            EmployerAccountAuthorizationHandler authorizationHandler)
        {
            //Arrange
            employerIdentifier.Role = "Viewer-Owner-Transactor";
            employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
            var employerAccounts = new Dictionary<string, EmployerIdentifier>{{employerIdentifier.AccountId, employerIdentifier}};
            var claim = new Claim("SomeOtherClaim", JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, null);
            var responseMock = new FeatureCollection();
            var httpContext = new DefaultHttpContext(responseMock);
            httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId,employerIdentifier.AccountId);
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            
            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeFalse();
        }
        
        [Test, MoqAutoData]
        public async Task Then_Returns_False_If_No_Matching_NameIdentifier_Claim_Found_For_GovSignIn(
            EmployerIdentifier employerIdentifier,
            EmployerAccountRequirement requirement,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            [Frozen] Mock<IOptions<ProvideFeedbackEmployerWebConfiguration>> provideFeedbackEmployerConfiguration,
            EmployerAccountAuthorizationHandler authorizationHandler)
        {
            //Arrange
            provideFeedbackEmployerConfiguration.Object.Value.UseGovSignIn = true;
            employerIdentifier.Role = "Viewer-Owner-Transactor";
            employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
            var employerAccounts = new Dictionary<string, EmployerIdentifier>{{employerIdentifier.AccountId, employerIdentifier}};
            var claim = new Claim("SomeOtherClaim", JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, null);
            var responseMock = new FeatureCollection();
            var httpContext = new DefaultHttpContext(responseMock);
            httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId,employerIdentifier.AccountId);
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            
            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeFalse();
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_False_If_The_Claim_Cannot_Be_Deserialized(
            EmployerIdentifier employerIdentifier,
            EmployerAccountRequirement requirement,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            EmployerAccountAuthorizationHandler authorizationHandler)
        {
            //Arrange
            employerIdentifier.Role = "Owner";
            employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
            var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerIdentifier));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, null);
            var responseMock = new FeatureCollection();
            var httpContext = new DefaultHttpContext(responseMock);
            httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId,employerIdentifier.AccountId);
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            
            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeFalse();
        }
    }
}