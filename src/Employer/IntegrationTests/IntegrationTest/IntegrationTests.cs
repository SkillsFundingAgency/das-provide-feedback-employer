using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data;
using ESFA.DAS.ProvideFeedback.Domain.Entities;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;
using SFA.DAS.Providers.Api.Client;
using NUnit.Framework;


namespace ESFA.DAS.Feedback.Employer.IntegrationTests
{
    public class IntegrationTest_NeedsToBeRanAsOne
    {
        private Mock<IProviderApiClient> _providerApiClientMock;
        private Mock<IEmployerCommitmentApi> _commitmentApiClientMock;
        private Mock<IAccountApiClient> _accountApiClientMock;
        private Mock<INotificationsApi> _notificationsApiClientMock;

        private ProviderSummary[] providerApiClientReturn;
        private List<Apprenticeship> commitmentApiClientReturn;
        private ICollection<TeamMemberViewModel> accountApiClientReturn;

        private Mock<ILogger<EmployerSurveyEmailer>> _surveyLoggerMock;

        private readonly IConfigurationRoot _configuration;

        private IStoreEmployerEmailDetails _dbEmployerFeedbackRepository;
        private EmployerFeedbackDataRefresh employerFeedbackDataRefresh;
        private EmployerSurveyInviteEmailer employerSurveyInviteEmailer;
        private EmployerSurveyReminderEmailer employerSurveyReminderEmailer;
        private DataRefreshMessageHelper helper;
        private IOptions<EmailSettings> options;
        private DbConnection _dbConnection;

        private Guid user1Guid;
        private Guid user2Guid;

        public IntegrationTest_NeedsToBeRanAsOne()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            _providerApiClientMock = new Mock<IProviderApiClient>();
            _commitmentApiClientMock = new Mock<IEmployerCommitmentApi>();
            _accountApiClientMock = new Mock<IAccountApiClient>();
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            _surveyLoggerMock = new Mock<ILogger<EmployerSurveyEmailer>>();

            options = new OptionsWrapper<EmailSettings>(new EmailSettings
            {
                BatchSize = 5,
                FeedbackSiteBaseUrl = "test.com",
                InviteCycleDays = 90,
                ReminderDays = 14
            });
            _dbConnection = new SqlConnection(_configuration.GetConnectionString("EmployerEmailStoreConnection"));
            _dbEmployerFeedbackRepository = new EmployerFeedbackRepository(_dbConnection);
            employerFeedbackDataRefresh = new EmployerFeedbackDataRefresh(_providerApiClientMock.Object,
                _commitmentApiClientMock.Object, _accountApiClientMock.Object);
            helper = new DataRefreshMessageHelper(new Mock<ILogger>().Object,_dbEmployerFeedbackRepository);
            
            SetUpApiReturn(2);

            user1Guid = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            user2Guid = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

            accountApiClientReturn = new List<TeamMemberViewModel>
            {
                new TeamMemberViewModel
                {
                    Email = "Test@test.com", Name = "Master Chef", UserRef = user1Guid.ToString(),
                    CanReceiveNotifications = true
                },
                new TeamMemberViewModel
                {
                    Email = "TheBestThereEverWas@90sReference.com", Name = "Flash Ketchup",
                    UserRef = user2Guid.ToString(), CanReceiveNotifications = true
                }
            };

            _accountApiClientMock.Setup(x => x.GetAccountUsers(It.IsAny<long>())).ReturnsAsync(accountApiClientReturn);
            _providerApiClientMock.Setup(x => x.FindAll()).Returns(providerApiClientReturn);
            _commitmentApiClientMock.Setup(x => x.GetEmployerApprenticeships(It.IsAny<long>()))
                .ReturnsAsync(commitmentApiClientReturn);
            _commitmentApiClientMock.Setup(x => x.GetAllEmployerAccountIds()).ReturnsAsync(new long[] {1});

            
        }

        [Test, Order(1)]
        public async Task InitialDatabasePopulatedCorrectly()
        {
            //Assert
            await _dbEmployerFeedbackRepository.ResetFeedback();
            await _dbEmployerFeedbackRepository.ClearSurveyCodes(user1Guid);
            await _dbEmployerFeedbackRepository.ClearSurveyCodes(user2Guid);
            var expectedInvites = new List<EmployerSurveyInvite>
            {
                new EmployerSurveyInvite {UserRef = user1Guid, EmailAddress = "Test@test.com", FirstName = "Master", AccountId = 1, Ukprn = 1},
                new EmployerSurveyInvite {UserRef = user1Guid, EmailAddress = "Test@test.com", FirstName = "Master", AccountId = 2, Ukprn = 1},
                new EmployerSurveyInvite {UserRef = user1Guid, EmailAddress = "Test@test.com", FirstName = "Master", AccountId = 2, Ukprn = 2},
                new EmployerSurveyInvite {UserRef = user2Guid, EmailAddress = "TheBestThereEverWas@90sReference.com", FirstName = "Flash", AccountId = 1, Ukprn = 1},
                new EmployerSurveyInvite {UserRef = user2Guid, EmailAddress = "TheBestThereEverWas@90sReference.com", FirstName = "Flash", AccountId = 2, Ukprn = 1},
                new EmployerSurveyInvite {UserRef = user2Guid, EmailAddress = "TheBestThereEverWas@90sReference.com", FirstName = "Flash", AccountId = 2, Ukprn = 2},
            };
            _notificationsApiClientMock = new Mock<INotificationsApi>();

            //Act
            var result = employerFeedbackDataRefresh.GetRefreshData();
            foreach (var x in result)
            {
                await helper.SaveMessageToDatabase(x);
            }

            //Assert
            var invites = await _dbEmployerFeedbackRepository.GetEmployerUsersToBeSentInvite();
            invites.Count().Should().Be(6);
            var invitesList = invites.OrderBy(x => x.UserRef).ThenBy(x => x.AccountId).ThenBy(x => x.Ukprn).ToList();
            for (int i = 0; i < invitesList.Count(); i++)
            {
                invitesList[i].UserRef.Should().Be(expectedInvites[i].UserRef);
                invitesList[i].EmailAddress.Should().Be(expectedInvites[i].EmailAddress);
                invitesList[i].FirstName.Should().Be(expectedInvites[i].FirstName);
                invitesList[i].AccountId.Should().Be(expectedInvites[i].AccountId);
                invitesList[i].Ukprn.Should().Be(expectedInvites[i].Ukprn);
                invitesList[i].Should().NotBeNull();
            }
        }

        [Test, Order(2)]
        public async Task FirstRoundInviteEmailsSentCorrectly()
        {
            //Assign
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            employerSurveyInviteEmailer = new EmployerSurveyInviteEmailer(_dbEmployerFeedbackRepository,
                _notificationsApiClientMock.Object, options, _surveyLoggerMock.Object);

            //Act
            await employerSurveyInviteEmailer.SendEmailsAsync();

            //Assert
            _notificationsApiClientMock.Verify(x => x.SendEmail(It.IsAny<Email>()), Times.Exactly(2));
        }

        [Test, Order(3)]
        public async Task FirstRoundReminderEmailsSentCorrectly()
        {
            //Assign
            await _dbConnection.ExecuteAsync($@" 
                UPDATE EmployerSurveyHistory
                SET SentDate = @newSentDate
                WHERE UniqueSurveyCode IN (SELECT UniqueSurveyCode FROM EmployerSurveyCodes WHERE UserRef IN @userRefs)",
                new {newSentDate = DateTime.Now - TimeSpan.FromDays(15), userRefs = new Guid[] {user1Guid, user2Guid}});
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            employerSurveyReminderEmailer = new EmployerSurveyReminderEmailer(_dbEmployerFeedbackRepository,
                _notificationsApiClientMock.Object, options, _surveyLoggerMock.Object);

            //Act
            await employerSurveyReminderEmailer.SendEmailsAsync();

            //Assert
            _notificationsApiClientMock.Verify(x => x.SendEmail(It.IsAny<Email>()), Times.Exactly(2));
        }

        [Test, Order(4)]
        public async Task SomeApprenticeshipsChangeProvider()
        {
            //Assign
            SetUpApiReturn(3);
            _accountApiClientMock.Setup(x => x.GetAccountUsers(It.IsAny<long>())).ReturnsAsync(accountApiClientReturn);
            _providerApiClientMock.Setup(x => x.FindAll()).Returns(providerApiClientReturn);
            _commitmentApiClientMock.Setup(x => x.GetEmployerApprenticeships(It.IsAny<long>()))
                .ReturnsAsync(commitmentApiClientReturn);
            _commitmentApiClientMock.Setup(x => x.GetAllEmployerAccountIds()).ReturnsAsync(new long[] { 1 });
            await _dbEmployerFeedbackRepository.ResetFeedback();
            var expectedInvites = new List<EmployerSurveyInvite>
            {
                new EmployerSurveyInvite {UserRef = user1Guid, EmailAddress = "Test@test.com", FirstName = "Master", AccountId = 2, Ukprn = 3},
                new EmployerSurveyInvite {UserRef = user2Guid, EmailAddress = "TheBestThereEverWas@90sReference.com", FirstName = "Flash", AccountId = 2, Ukprn = 3},
            };
            _notificationsApiClientMock = new Mock<INotificationsApi>();

            //Act
            var result = employerFeedbackDataRefresh.GetRefreshData();
            foreach (var x in result)
            {
                await helper.SaveMessageToDatabase(x);
            }

            //Assert
            var invites = await _dbEmployerFeedbackRepository.GetEmployerUsersToBeSentInvite();
            invites.Count().Should().Be(2);
            var invitesList = invites.OrderBy(x => x.UserRef).ThenBy(x => x.AccountId).ThenBy(x => x.Ukprn).ToList();
            for (int i = 0; i < invitesList.Count(); i++)
            {
                invitesList[i].UserRef.Should().Be(expectedInvites[i].UserRef);
                invitesList[i].EmailAddress.Should().Be(expectedInvites[i].EmailAddress);
                invitesList[i].FirstName.Should().Be(expectedInvites[i].FirstName);
                invitesList[i].AccountId.Should().Be(expectedInvites[i].AccountId);
                invitesList[i].Ukprn.Should().Be(expectedInvites[i].Ukprn);
                invitesList[i].Should().NotBeNull();
            }
        }

        [Test, Order(5)]
        public async Task SecondRoundInviteEmailsSentCorrectly()
        {
            //Assign
            await _dbConnection.ExecuteAsync($@" 
                UPDATE EmployerSurveyHistory
                SET SentDate = @newSentDate
                WHERE EmailType = 1 AND UniqueSurveyCode IN (SELECT UniqueSurveyCode FROM EmployerSurveyCodes WHERE UserRef IN @userRefs)",
                new {newSentDate = DateTime.Now - TimeSpan.FromDays(91), userRefs = new Guid[] {user1Guid, user2Guid}});
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            employerSurveyInviteEmailer = new EmployerSurveyInviteEmailer(_dbEmployerFeedbackRepository,
                _notificationsApiClientMock.Object, options, _surveyLoggerMock.Object);

            //Act
            var newCodesRequired =
                await _dbEmployerFeedbackRepository.GetEmployerInvitesForNextCycleAsync(options.Value.InviteCycleDays);
            await _dbEmployerFeedbackRepository.InsertNewSurveyInviteCodes(newCodesRequired);
            await employerSurveyInviteEmailer.SendEmailsAsync();

            //Assert
            _notificationsApiClientMock.Verify(x => x.SendEmail(It.IsAny<Email>()), Times.Exactly(2));
        }

        [Test, Order(6)]
        public async Task SecondRoundReminderEmailsSentCorrectly()
        {
            //Assign
            await _dbConnection.ExecuteAsync($@" 
                UPDATE EmployerSurveyHistory
                SET SentDate = DATEADD(DAY,-15,SentDate)
                WHERE UniqueSurveyCode IN (SELECT UniqueSurveyCode FROM EmployerSurveyCodes WHERE UserRef IN @userRefs)",
                new { userRefs = new Guid[] { user1Guid, user2Guid } });
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            employerSurveyReminderEmailer = new EmployerSurveyReminderEmailer(_dbEmployerFeedbackRepository,
                _notificationsApiClientMock.Object, options, _surveyLoggerMock.Object);

            //Act
            await employerSurveyReminderEmailer.SendEmailsAsync();

            //Assert
            _notificationsApiClientMock.Verify(x => x.SendEmail(It.IsAny<Email>()), Times.Exactly(2));
        }

        [Test,Order(7)]
        public async Task ThirdRoundInviteEmailsSentCorrectly()
        {
            await _dbConnection.ExecuteAsync($@" 
                UPDATE EmployerSurveyHistory
                SET SentDate = @newSentDate
                WHERE EmailType = 1 AND UniqueSurveyCode IN (SELECT UniqueSurveyCode FROM EmployerSurveyCodes WHERE UserRef IN @userRefs)",
                new { newSentDate = DateTime.Now - TimeSpan.FromDays(91), userRefs = new Guid[] { user1Guid, user2Guid } });
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            employerSurveyInviteEmailer = new EmployerSurveyInviteEmailer(_dbEmployerFeedbackRepository,
                _notificationsApiClientMock.Object, options, _surveyLoggerMock.Object);

            //Act
            var newCodesRequired =
                await _dbEmployerFeedbackRepository.GetEmployerInvitesForNextCycleAsync(options.Value.InviteCycleDays);
            await _dbEmployerFeedbackRepository.InsertNewSurveyInviteCodes(newCodesRequired);
            await employerSurveyInviteEmailer.SendEmailsAsync();

            //Assert
            _notificationsApiClientMock.Verify(x => x.SendEmail(It.IsAny<Email>()), Times.Exactly(2));
        }

        [Test, Order(8)]
        public async Task CurrentUsersLeftCompanyNewPersonJoins()
        {
            //Assign
            SetUpApiReturn(3);
            accountApiClientReturn = new List<TeamMemberViewModel>
            {
                new TeamMemberViewModel
                {
                    Email = "InWestPhiladelphiaBornAndRaised@PlaygroundDayz.com", Name = "Fresh Prince",
                    UserRef = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc").ToString(), CanReceiveNotifications = true
                }
            };
            _accountApiClientMock.Setup(x => x.GetAccountUsers(It.IsAny<long>())).ReturnsAsync(accountApiClientReturn);
            _providerApiClientMock.Setup(x => x.FindAll()).Returns(providerApiClientReturn);
            _commitmentApiClientMock.Setup(x => x.GetEmployerApprenticeships(It.IsAny<long>()))
                .ReturnsAsync(commitmentApiClientReturn);
            _commitmentApiClientMock.Setup(x => x.GetAllEmployerAccountIds()).ReturnsAsync(new long[] { 1 });
            await _dbEmployerFeedbackRepository.ResetFeedback();
            var expectedInvites = new List<EmployerSurveyInvite>
            {
                new EmployerSurveyInvite {UserRef = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), EmailAddress = "InWestPhiladelphiaBornAndRaised@PlaygroundDayz.com", FirstName = "Fresh", AccountId = 1, Ukprn = 1},
                new EmployerSurveyInvite {UserRef = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), EmailAddress = "InWestPhiladelphiaBornAndRaised@PlaygroundDayz.com", FirstName = "Fresh", AccountId = 2, Ukprn = 1},
                new EmployerSurveyInvite {UserRef = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), EmailAddress = "InWestPhiladelphiaBornAndRaised@PlaygroundDayz.com", FirstName = "Fresh", AccountId = 2, Ukprn = 3},
            };
            _notificationsApiClientMock = new Mock<INotificationsApi>();

            //Act
            var result = employerFeedbackDataRefresh.GetRefreshData();
            foreach (var x in result)
            {
                await helper.SaveMessageToDatabase(x);
            }

            //Assert
            var invites = await _dbEmployerFeedbackRepository.GetEmployerUsersToBeSentInvite();
            invites.Count().Should().Be(3);
            var invitesList = invites.OrderBy(x => x.UserRef).ThenBy(x => x.AccountId).ThenBy(x => x.Ukprn).ToList();
            for (int i = 0; i < invitesList.Count(); i++)
            {
                invitesList[i].UserRef.Should().Be(expectedInvites[i].UserRef);
                invitesList[i].EmailAddress.Should().Be(expectedInvites[i].EmailAddress);
                invitesList[i].FirstName.Should().Be(expectedInvites[i].FirstName);
                invitesList[i].AccountId.Should().Be(expectedInvites[i].AccountId);
                invitesList[i].Ukprn.Should().Be(expectedInvites[i].Ukprn);
                invitesList[i].Should().NotBeNull();
            }
        }

        [Test, Order(9)]
        public async Task ThirdRoundReminderEmailsSentCorrectly()
        {
            //Assign
            await _dbConnection.ExecuteAsync($@" 
                UPDATE EmployerSurveyHistory
                SET SentDate = DATEADD(DAY,-15,SentDate)
                WHERE UniqueSurveyCode IN (SELECT UniqueSurveyCode FROM EmployerSurveyCodes WHERE UserRef IN @userRefs)",
                new { userRefs = new Guid[] { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc") } });
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            employerSurveyReminderEmailer = new EmployerSurveyReminderEmailer(_dbEmployerFeedbackRepository,
                _notificationsApiClientMock.Object, options, _surveyLoggerMock.Object);

            //Act
            await employerSurveyReminderEmailer.SendEmailsAsync();

            //Assert
            _notificationsApiClientMock.Verify(x => x.SendEmail(It.IsAny<Email>()), Times.Exactly(0));
        }

        private void SetUpApiReturn(long changeableUkprn)
        {
            providerApiClientReturn = new ProviderSummary[]
            {
                new ProviderSummary {Ukprn = 1, ProviderName = "Test Academy"},
                new ProviderSummary {Ukprn = changeableUkprn, ProviderName = "Worst School"},
            };

            commitmentApiClientReturn = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Active, ULN = "1",
                    EmployerAccountId = 1, ProviderId = 1, ProviderName = "Test Academy"
                },
                new Apprenticeship
                {
                    HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Active, ULN = "2",
                    EmployerAccountId = 2, ProviderId = 1, ProviderName = "Test Academy"
                },
                new Apprenticeship
                {
                    HasHadDataLockSuccess = true, PaymentStatus = PaymentStatus.Active, ULN = "3",
                    EmployerAccountId = 2, ProviderId = changeableUkprn, ProviderName = "Worst School"
                }
            };
        }
    }
}
