using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using ESFA.DAS.ProvideFeedback.Domain.Entities.ApiTypes;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.OuterApi;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices.OuterApi.EmployerAccounts;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.GovUK.Auth.Employer;
using SFA.DAS.Testing.AutoFixture;

namespace UnitTests.ApplicationServices
{
    public class WhenGettingUserAccounts
    {
        [Test, MoqAutoData]
        public async Task Then_The_Api_Is_Called_And_Data_Returned(
            string email,
            string userId,
            GetUserAccountsResponse response,
            [Frozen] Mock<IOuterApiClient> apiClient,
            EmployerAccountService service)
        {
            var expectedRequest = new GetUserAccountsRequest(userId, email);
            apiClient.Setup(x =>
                    x.Get<GetUserAccountsResponse>(
                        It.Is<GetUserAccountsRequest>(c => c.GetUrl.Equals(expectedRequest.GetUrl))))
                .ReturnsAsync(new ApiResponse<GetUserAccountsResponse>(response, HttpStatusCode.OK, ""));

            var actual = await service.GetUserAccounts(userId, email);

            actual.Should().BeEquivalentTo(new
            {
                EmployerAccounts = response.UserAccounts != null
                    ? response.UserAccounts.Select(c => new EmployerUserAccountItem
                    {
                        Role = c.Role,
                        AccountId = c.AccountId,
                        ApprenticeshipEmployerType = Enum.Parse<ApprenticeshipEmployerType>(c.ApprenticeshipEmployerType.ToString()),
                        EmployerName = c.EmployerName,
                    }).ToList()
                    : [],
                FirstName = response.FirstName,
                IsSuspended = response.IsSuspended,
                LastName = response.LastName,
                EmployerUserId = response.EmployerUserId,
            });
        }
    }
}