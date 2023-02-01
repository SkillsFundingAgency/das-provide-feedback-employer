using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using ESFA.DAS.EmployerProvideFeedback.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace UnitTests.Web.Authentication
{
    public class WhenHandlingEmployerViewTransactorAuthorization
    {
        [Test, MoqAutoData]
        public async Task Then_Calls_Authorization_Service_And_Checks_AuthHandler(
            EmployerViewerTransactorRoleRequirement requirement,
            [Frozen] Mock<IEmployerAccountAuthorisationHandler> employerAccountAuthorisationHandler,
            EmployerViewerTransactorAuthorizationHandler handler)
        {
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity()});
            var context = new AuthorizationHandlerContext(new [] {requirement}, claimsPrinciple, null);
            employerAccountAuthorisationHandler.Setup(x=>x.IsEmployerAuthorised(context, true)).Returns(true);
            
            //Act
            await handler.HandleAsync(context);
            
            //Assert
            context.HasSucceeded.Should().BeTrue();
        }

        [Test, MoqAutoData]
        public async Task Then_If_Not_Valid_Then_Does_Not_Succeed_Requirement(
            EmployerViewerTransactorRoleRequirement requirement,
            [Frozen] Mock<IEmployerAccountAuthorisationHandler> employerAccountAuthorisationHandler,
            EmployerViewerTransactorAuthorizationHandler handler)
        {
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity()});
            var context = new AuthorizationHandlerContext(new [] {requirement}, claimsPrinciple, null);
            employerAccountAuthorisationHandler.Setup(x=>x.IsEmployerAuthorised(context, true)).Returns(false);
            
            //Act
            await handler.HandleAsync(context);
            
            //Assert
            context.HasSucceeded.Should().BeFalse();
        }
    }
}