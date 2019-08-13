using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using Xunit;
using ESFA.DAS.Feedback.Employer.Emailer;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Providers.Api.Client;
using Moq;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.Types;
using FluentAssertions;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using ESFA.DAS.ProvideFeedback.Domain.Entities;

namespace ESFA.DAS.Feedback.Employer.UnitTests.Emailer
{
    public class EmailInviteDataRefreshTests
    {
        Mock<IProviderApiClient> _providerApiClientMock;
        Mock<IEmployerCommitmentApi> _employerApiClientMock;
        Mock<IAccountApiClient> _accountApiClientMock;
        private EmployerFeedbackDataRefreshService _dataRefresh;
        ProviderSummary[] providerApiReturn;
        List<Apprenticeship> employerApiReturn;
        ICollection<TeamMemberViewModel> accountApiReturn;
        Task<IEnumerable<long>> employerIdsReturn;

        public EmailInviteDataRefreshTests()
        {
            _providerApiClientMock = new Mock<IProviderApiClient>();
            _employerApiClientMock = new Mock<IEmployerCommitmentApi>();
            _accountApiClientMock = new Mock<IAccountApiClient>();
            _dataRefresh = new EmployerFeedbackDataRefreshService(_providerApiClientMock.Object, _employerApiClientMock.Object, _accountApiClientMock.Object);

            providerApiReturn = new ProviderSummary[]
            {
                new ProviderSummary { Ukprn = 1, ProviderName = "Fancy School of Fancyness" },
                new ProviderSummary { Ukprn = 2, ProviderName = "Hogwarts" },
                new ProviderSummary { Ukprn = 3, ProviderName = "Test Academy" }
            };

            employerApiReturn = new List<Apprenticeship>
                {
                    new Apprenticeship { HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Active, ULN = "1", EmployerAccountId = 1, ProviderId = 1 },
                    new Apprenticeship { HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Paused, ULN = "2", EmployerAccountId = 2, ProviderId = 1 },
                    new Apprenticeship { HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Completed, ULN = "1", EmployerAccountId = 1, ProviderId = 1 },
                    new Apprenticeship { HasHadDataLockSuccess = false, PaymentStatus = PaymentStatus.Active, ULN = "3", EmployerAccountId = 3, ProviderId = 2 },
                    new Apprenticeship { HasHadDataLockSuccess = false, PaymentStatus = PaymentStatus.Completed, ULN = "4", EmployerAccountId = 4, ProviderId = 4 },
                    new Apprenticeship { },
                    null
                };

            accountApiReturn = new TeamMemberViewModel[]
            {
                new TeamMemberViewModel{ Email = "Test@test.com", Name = "Harry Potter", UserRef = new Guid().ToString(), CanReceiveNotifications = false},
                new TeamMemberViewModel{ Email = "Test@account.com", Name = "Barry Trotter", UserRef = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa").ToString(), CanReceiveNotifications = true}
            };

            employerIdsReturn = Task.Run(() => new List<long>{1,2,3,4}.AsEnumerable());

            _providerApiClientMock.Setup(x => x.FindAll()).Returns(providerApiReturn);
            _employerApiClientMock.Setup(x => x.GetEmployerApprenticeships(It.IsAny<long>())).Returns(Task.Run(() => employerApiReturn));
            _employerApiClientMock.Setup(x => x.GetAllEmployerAccountIds()).Returns(employerIdsReturn);
            _accountApiClientMock.Setup(x => x.GetAccountUsers(It.IsAny<long>())).Returns(Task.Run(() => accountApiReturn));
        }

        [Fact]
        public void RoatpAPIShouldBeReturningData()
        {
            //Act	
            var result = _dataRefresh.GetRoatpProviders();

            //Assert	
            Assert.Equal(providerApiReturn, result);
        }


        [Fact]
        public void CommitmentsAPIShouldBeReturningEmployerIds()
        {
            //Act
            var result = _dataRefresh.GetCommitmentEmployerIdsData();

            //Assert
            Assert.Equal(employerIdsReturn.Result,result);
        }

        [Fact]
        public void ApprenticeshipsFromCommitmentsShouldBeCalledForEachEmployerId()
        {
            //Act
            _dataRefresh.GetApprenticeshipData();

            //Assert
            _employerApiClientMock.Verify(x => x.GetEmployerApprenticeships(It.IsAny<long>()),Times.Exactly(4));
        }

        [Fact]
        public void CheckWhetherValidApprenticeshipsAreReturnedFromTheApis()
        {
            //Assign
            var apprenticeships = _dataRefresh.GetApprenticeshipData();
            var expected = new List<Apprenticeship>
            {
                new Apprenticeship{ EmployerAccountId = 1, HasHadDataLockSuccess = true, ProviderId = 1, TrainingType = TrainingType.Standard, ULN = "1", PaymentStatus = PaymentStatus.Active, AgreementStatus = AgreementStatus.NotAgreed},
                new Apprenticeship{ EmployerAccountId = 2, HasHadDataLockSuccess = true, ProviderId = 1, TrainingType = TrainingType.Standard, ULN = "2", PaymentStatus = PaymentStatus.Paused, AgreementStatus = AgreementStatus.NotAgreed}
            };

            //Act
            var result = _dataRefresh.GetValidApprenticeshipCommitments(apprenticeships);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void AccountsApiShouldBeReturningValidUsers()
        {
            //Assign
            var expected = new List<User>
            {
                new User {EmailAddress = "Test@account.com", FirstName = "Barry", UserRef = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),AccountId = 1 }
            };

            //Act
            var result = _dataRefresh.GetUsersFromAccountId(1);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void WholeProcessShouldReturnData()
        {
            //Act
            var result = _dataRefresh.GetRefreshData();

            //Assert
            Assert.NotNull(result);
        }

        [Theory]
        [MemberData(nameof(ApprenticeshipData))]
        public void WillReturnValidCommitments(List<Apprenticeship> apprenticeships,int numOfValidApps)
        {
            //Act
            var result = _dataRefresh.GetValidApprenticeshipCommitments(apprenticeships);

            //Assert
            Assert.Equal(numOfValidApps, result.Count);
        }

        public static IEnumerable<object[]> ApprenticeshipData =>
            new List<object[]>
            {
                new object[] { new List<Apprenticeship> { new Apprenticeship { EmployerAccountId = 1, HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Active, ProviderId = 1 } },1 },
                new object[] { new List<Apprenticeship> { new Apprenticeship { EmployerAccountId = 1, HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Paused, ProviderId = 1 } },1 },
                new object[] { new List<Apprenticeship> { new Apprenticeship { EmployerAccountId = 1, HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Paused, ProviderId = 2 } },1 },
                new object[] { new List<Apprenticeship> { new Apprenticeship { EmployerAccountId = 1, HasHadDataLockSuccess = false, PaymentStatus = PaymentStatus.Active, ProviderId = 4 } },0 },
                new object[] { new List<Apprenticeship> { new Apprenticeship { EmployerAccountId = 1, HasHadDataLockSuccess = false, PaymentStatus = PaymentStatus.Active, ProviderId = 1 } },0 },
                new object[] { new List<Apprenticeship> { new Apprenticeship { EmployerAccountId = 1, HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Completed, ProviderId = 1 } },0 },
                new object[] { new List<Apprenticeship> { new Apprenticeship { EmployerAccountId = 1, HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Active, ProviderId = 1 }, new Apprenticeship { HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Active, ProviderId = 1 } },2 },
                new object[] { new List<Apprenticeship> { new Apprenticeship { EmployerAccountId = 1, HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Paused, ProviderId = 1 }, new Apprenticeship { HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Active, ProviderId = 1 } },2 },
                new object[] { new List<Apprenticeship> { new Apprenticeship { EmployerAccountId = 1, HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Paused, ProviderId = 1 }, new Apprenticeship { HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Active, ProviderId = 2 } },2 },
                new object[] { new List<Apprenticeship> { new Apprenticeship { EmployerAccountId = 1, HasHadDataLockSuccess = false, PaymentStatus = PaymentStatus.Active, ProviderId = 1 }, new Apprenticeship { HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Active, ProviderId = 4 } },0 },
                new object[] { new List<Apprenticeship> { new Apprenticeship { EmployerAccountId = 1, HasHadDataLockSuccess = false, PaymentStatus = PaymentStatus.Active, ProviderId = 1 }, new Apprenticeship { HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Active, ProviderId = 1 } },1 },
                new object[] { new List<Apprenticeship> { new Apprenticeship { EmployerAccountId = 1, HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Completed, ProviderId = 1 }, new Apprenticeship { HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Active, ProviderId = 1 } },1 },
                new object[] { new List<Apprenticeship> { new Apprenticeship { EmployerAccountId = 1, HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Paused, ProviderId = 4 }, new Apprenticeship { HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Active, ProviderId = 4 } },0 }
            };
    }
}
