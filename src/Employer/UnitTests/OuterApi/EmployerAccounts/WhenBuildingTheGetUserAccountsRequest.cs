using System.Web;
using AutoFixture.NUnit3;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.OuterApi.EmployerAccounts;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTests.OuterApi.EmployerAccounts
{
    public class WhenBuildingTheGetUserAccountsRequest
    {
        [Test, AutoData]
        public void Then_The_Url_Is_Correctly_Constructed_And_Email_Encoded(string email, string userId)
        {
            //Arrange
            email = email + "!@£ $" + email;
            
            //Act
            var actual = new GetUserAccountsRequest(userId, email);

            //Assert
            actual.GetUrl.Should().Be($"accountusers/{userId}/accounts?email={HttpUtility.UrlEncode(email)}");
        }
    }
}