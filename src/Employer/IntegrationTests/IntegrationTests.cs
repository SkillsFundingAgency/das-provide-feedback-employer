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
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;
using SFA.DAS.Providers.Api.Client;

namespace IntegrationTests
{
    public class IntegrationTest_NeedsToBeRanAsOne
    {
        private readonly Mock<IProviderApiClient> _providerApiClientMock;
        private readonly Mock<IEmployerCommitmentApi> _commitmentApiClientMock;
        private readonly Mock<IAccountApiClient> _accountApiClientMock;
        private Mock<INotificationsApi> _notificationsApiClientMock;

        private ProviderSummary[] _providerApiClientReturn;
        private List<Apprenticeship> _commitmentApiClientReturn;
        private ICollection<TeamMemberViewModel> _accountApiClientReturn;

        private readonly Mock<ILogger<EmployerSurveyEmailer>> _surveyLoggerMock;

        private readonly IStoreEmployerEmailDetails _dbEmployerFeedbackRepository;
        private readonly EmployerFeedbackDataRefresh _employerFeedbackDataRefresh;
        private EmployerSurveyInviteEmailer _employerSurveyInviteEmailer;
        private EmployerSurveyReminderEmailer _employerSurveyReminderEmailer;
        private readonly DataRefreshMessageHelper _helper;
        private readonly IOptions<EmailSettings> _options;
        private readonly DbConnection _dbConnection;

        private readonly Guid _user1Guid;
        private readonly Guid _user2Guid;
        private readonly Guid _user3Guid;

        public IntegrationTest_NeedsToBeRanAsOne()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            _providerApiClientMock = new Mock<IProviderApiClient>();
            _commitmentApiClientMock = new Mock<IEmployerCommitmentApi>();
            _accountApiClientMock = new Mock<IAccountApiClient>();
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            _surveyLoggerMock = new Mock<ILogger<EmployerSurveyEmailer>>();

            _options = new OptionsWrapper<EmailSettings>(new EmailSettings
            {
                BatchSize = 5,
                FeedbackSiteBaseUrl = "test.com",
                InviteCycleDays = 90,
                ReminderDays = 14
            });
            _dbConnection = new SqlConnection(configuration.GetConnectionString("EmployerEmailStoreConnection"));
            _dbEmployerFeedbackRepository = new EmployerFeedbackRepository(_dbConnection);
            _employerFeedbackDataRefresh = new EmployerFeedbackDataRefresh(_providerApiClientMock.Object,
                _commitmentApiClientMock.Object, _accountApiClientMock.Object);
            _helper = new DataRefreshMessageHelper(new Mock<ILogger>().Object,_dbEmployerFeedbackRepository);
            
            SetUpApiReturn(2);

            _user1Guid = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            _user2Guid = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            _user3Guid = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

            _accountApiClientReturn = new List<TeamMemberViewModel>
            {
                new TeamMemberViewModel
                {
                    Email = "Test@test.com", Name = "Master Chef", UserRef = _user1Guid.ToString(),
                    CanReceiveNotifications = true
                },
                new TeamMemberViewModel
                {
                    Email = "TheBestThereEverWas@90sReference.com", Name = "Flash Ketchup",
                    UserRef = _user2Guid.ToString(), CanReceiveNotifications = true
                }
            };

            _accountApiClientMock.Setup(x => x.GetAccountUsers(It.IsAny<long>())).ReturnsAsync(_accountApiClientReturn);
            _providerApiClientMock.Setup(x => x.FindAll()).Returns(_providerApiClientReturn);
            _commitmentApiClientMock.Setup(x => x.GetEmployerApprenticeships(It.IsAny<long>()))
                .ReturnsAsync(_commitmentApiClientReturn);
            _commitmentApiClientMock.Setup(x => x.GetAllEmployerAccountIds()).ReturnsAsync(new long[] {1});

            
        }

        [Test, Order(1)]
        public async Task InitialDatabasePopulatedCorrectly()
        {
            //Assert
            await _dbEmployerFeedbackRepository.ResetFeedback();
            await _dbEmployerFeedbackRepository.ClearSurveyCodes(_user1Guid);
            await _dbEmployerFeedbackRepository.ClearSurveyCodes(_user2Guid);
            await _dbEmployerFeedbackRepository.ClearSurveyCodes(_user3Guid);
            var expectedInvites = new List<EmployerSurveyInvite>
            {
                new EmployerSurveyInvite {UserRef = _user1Guid, EmailAddress = "Test@test.com", FirstName = "Master", AccountId = 1, Ukprn = 1, ProviderName = "Test Academy"},
                new EmployerSurveyInvite {UserRef = _user1Guid, EmailAddress = "Test@test.com", FirstName = "Master", AccountId = 2, Ukprn = 1, ProviderName = "Test Academy"},
                new EmployerSurveyInvite {UserRef = _user1Guid, EmailAddress = "Test@test.com", FirstName = "Master", AccountId = 2, Ukprn = 2, ProviderName = "Worst School"},
                new EmployerSurveyInvite {UserRef = _user2Guid, EmailAddress = "TheBestThereEverWas@90sReference.com", FirstName = "Flash", AccountId = 1, Ukprn = 1, ProviderName = "Test Academy"},
                new EmployerSurveyInvite {UserRef = _user2Guid, EmailAddress = "TheBestThereEverWas@90sReference.com", FirstName = "Flash", AccountId = 2, Ukprn = 1, ProviderName = "Test Academy"},
                new EmployerSurveyInvite {UserRef = _user2Guid, EmailAddress = "TheBestThereEverWas@90sReference.com", FirstName = "Flash", AccountId = 2, Ukprn = 2, ProviderName = "Worst School"},
            };
            _notificationsApiClientMock = new Mock<INotificationsApi>();

            //Act
            var result = _employerFeedbackDataRefresh.GetRefreshData();
            foreach (var x in result)
            {
                await _helper.SaveMessageToDatabase(x);
            }

            //Assert
            var invites = await _dbEmployerFeedbackRepository.GetEmployerUsersToBeSentInvite();
            invites.Count().Should().Be(6);
            invites.ShouldBeEquivalentTo(expectedInvites, options => options.Excluding(
                s => s.SelectedMemberPath.EndsWith(".UniqueSurveyCode")));
        }

        [Test, Order(2)]
        public async Task FirstRoundInviteEmailsSentCorrectly()
        {
            //Assign
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            _employerSurveyInviteEmailer = new EmployerSurveyInviteEmailer(_dbEmployerFeedbackRepository,
                _notificationsApiClientMock.Object, _options, _surveyLoggerMock.Object);

            //Act
            await _employerSurveyInviteEmailer.SendEmailsAsync();

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
                WHERE UniqueSurveyCode IN (SELECT UniqueSurveyCode FROM EmployerSurveyCodes WHERE FeedbackId IN (SELECT FeedbackId FROM EmployerFeedback WHERE UserRef in @userRefs))",
                new {newSentDate = DateTime.Now - TimeSpan.FromDays(15), userRefs = new[] {_user1Guid, _user2Guid}});
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            _employerSurveyReminderEmailer = new EmployerSurveyReminderEmailer(_dbEmployerFeedbackRepository,
                _notificationsApiClientMock.Object, _options, _surveyLoggerMock.Object);

            //Act
            await _employerSurveyReminderEmailer.SendEmailsAsync();

            //Assert
            _notificationsApiClientMock.Verify(x => x.SendEmail(It.IsAny<Email>()), Times.Exactly(2));
        }

        [Test, Order(4)]
        public async Task SomeApprenticeshipsChangeProvider()
        {
            //Assign
            SetUpApiReturn(3);
            _accountApiClientMock.Setup(x => x.GetAccountUsers(It.IsAny<long>())).ReturnsAsync(_accountApiClientReturn);
            _providerApiClientMock.Setup(x => x.FindAll()).Returns(_providerApiClientReturn);
            _commitmentApiClientMock.Setup(x => x.GetEmployerApprenticeships(It.IsAny<long>()))
                .ReturnsAsync(_commitmentApiClientReturn);
            _commitmentApiClientMock.Setup(x => x.GetAllEmployerAccountIds()).ReturnsAsync(new long[] { 1 });
            await _dbEmployerFeedbackRepository.ResetFeedback();
            var expectedInvites = new List<EmployerSurveyInvite>
            {
                new EmployerSurveyInvite {UserRef = _user1Guid, EmailAddress = "Test@test.com", FirstName = "Master", AccountId = 2, Ukprn = 3, ProviderName = "Worst School"},
                new EmployerSurveyInvite {UserRef = _user2Guid, EmailAddress = "TheBestThereEverWas@90sReference.com", FirstName = "Flash", AccountId = 2, Ukprn = 3, ProviderName = "Worst School"},
            };
            _notificationsApiClientMock = new Mock<INotificationsApi>();

            //Act
            var result = _employerFeedbackDataRefresh.GetRefreshData();
            foreach (var x in result)
            {
                await _helper.SaveMessageToDatabase(x);
            }

            //Assert
            var invites = await _dbEmployerFeedbackRepository.GetEmployerUsersToBeSentInvite();
            invites.Count().Should().Be(2);
            var invitesList = invites.OrderBy(x => x.UserRef).ThenBy(x => x.AccountId).ThenBy(x => x.Ukprn).ToList();
            invites.ShouldBeEquivalentTo(expectedInvites, options => options.Excluding(
                s => s.SelectedMemberPath.EndsWith(".UniqueSurveyCode")));
        }

        [Test, Order(5)]
        public async Task SecondRoundInviteEmailsSentCorrectly()
        {
            //Assign
            await _dbConnection.ExecuteAsync($@" 
                UPDATE EmployerSurveyHistory
                SET SentDate = @newSentDate
                WHERE EmailType = 1 AND UniqueSurveyCode IN (SELECT UniqueSurveyCode FROM EmployerSurveyCodes WHERE FeedbackId IN (SELECT FeedbackId FROM EmployerFeedback WHERE UserRef in @userRefs))",
                new {newSentDate = DateTime.Now - TimeSpan.FromDays(91), userRefs = new[] {_user1Guid, _user2Guid}});
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            _employerSurveyInviteEmailer = new EmployerSurveyInviteEmailer(_dbEmployerFeedbackRepository,
                _notificationsApiClientMock.Object, _options, _surveyLoggerMock.Object);

            //Act
            var newCodesRequired =
                await _dbEmployerFeedbackRepository.GetEmployerInvitesForNextCycleAsync(_options.Value.InviteCycleDays);
            await _dbEmployerFeedbackRepository.InsertNewSurveyInviteCodes(newCodesRequired);
            await _employerSurveyInviteEmailer.SendEmailsAsync();

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
                WHERE UniqueSurveyCode IN (SELECT UniqueSurveyCode FROM EmployerSurveyCodes WHERE FeedbackId IN (SELECT FeedbackId FROM EmployerFeedback WHERE UserRef in @userRefs))",
                new { userRefs = new[] { _user1Guid, _user2Guid } });
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            _employerSurveyReminderEmailer = new EmployerSurveyReminderEmailer(_dbEmployerFeedbackRepository,
                _notificationsApiClientMock.Object, _options, _surveyLoggerMock.Object);

            //Act
            await _employerSurveyReminderEmailer.SendEmailsAsync();

            //Assert
            _notificationsApiClientMock.Verify(x => x.SendEmail(It.IsAny<Email>()), Times.Exactly(2));
        }

        [Test,Order(7)]
        public async Task ThirdRoundInviteEmailsSentCorrectly()
        {
            await _dbConnection.ExecuteAsync($@" 
                UPDATE EmployerSurveyHistory
                SET SentDate = @newSentDate
                WHERE EmailType = 1 AND UniqueSurveyCode IN (SELECT UniqueSurveyCode FROM EmployerSurveyCodes WHERE FeedbackId IN (SELECT FeedbackId FROM EmployerFeedback WHERE UserRef in @userRefs))",
                new { newSentDate = DateTime.Now - TimeSpan.FromDays(91), userRefs = new[] { _user1Guid, _user2Guid } });
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            _employerSurveyInviteEmailer = new EmployerSurveyInviteEmailer(_dbEmployerFeedbackRepository,
                _notificationsApiClientMock.Object, _options, _surveyLoggerMock.Object);

            //Act
            var newCodesRequired =
                await _dbEmployerFeedbackRepository.GetEmployerInvitesForNextCycleAsync(_options.Value.InviteCycleDays);
            await _dbEmployerFeedbackRepository.InsertNewSurveyInviteCodes(newCodesRequired);
            await _employerSurveyInviteEmailer.SendEmailsAsync();

            //Assert
            _notificationsApiClientMock.Verify(x => x.SendEmail(It.IsAny<Email>()), Times.Exactly(2));
        }

        [Test, Order(8)]
        public async Task CurrentUsersLeftCompanyNewPersonJoins()
        {
            //Assign
            SetUpApiReturn(3);
            _accountApiClientReturn = new List<TeamMemberViewModel>
            {
                new TeamMemberViewModel
                {
                    Email = "InWestPhiladelphiaBornAndRaised@PlaygroundDayz.com", Name = "Fresh Prince",
                    UserRef = _user3Guid.ToString(), CanReceiveNotifications = true
                }
            };
            _accountApiClientMock.Setup(x => x.GetAccountUsers(It.IsAny<long>())).ReturnsAsync(_accountApiClientReturn);
            _providerApiClientMock.Setup(x => x.FindAll()).Returns(_providerApiClientReturn);
            _commitmentApiClientMock.Setup(x => x.GetEmployerApprenticeships(It.IsAny<long>()))
                .ReturnsAsync(_commitmentApiClientReturn);
            _commitmentApiClientMock.Setup(x => x.GetAllEmployerAccountIds()).ReturnsAsync(new long[] { 1 });
            await _dbEmployerFeedbackRepository.ResetFeedback();
            var expectedInvites = new List<EmployerSurveyInvite>
            {
                new EmployerSurveyInvite {UserRef = _user3Guid, EmailAddress = "InWestPhiladelphiaBornAndRaised@PlaygroundDayz.com", FirstName = "Fresh", AccountId = 1, Ukprn = 1, ProviderName = "Test Academy"},
                new EmployerSurveyInvite {UserRef = _user3Guid, EmailAddress = "InWestPhiladelphiaBornAndRaised@PlaygroundDayz.com", FirstName = "Fresh", AccountId = 2, Ukprn = 1, ProviderName = "Test Academy"},
                new EmployerSurveyInvite {UserRef = _user3Guid, EmailAddress = "InWestPhiladelphiaBornAndRaised@PlaygroundDayz.com", FirstName = "Fresh", AccountId = 2, Ukprn = 3 , ProviderName = "Worst School"},
            };
            _notificationsApiClientMock = new Mock<INotificationsApi>();

            //Act
            var result = _employerFeedbackDataRefresh.GetRefreshData();
            foreach (var x in result)
            {
                await _helper.SaveMessageToDatabase(x);
            }

            //Assert
            var invites = await _dbEmployerFeedbackRepository.GetEmployerUsersToBeSentInvite();
            invites.Count().Should().Be(3);
            invites.ShouldBeEquivalentTo(expectedInvites, options => options.Excluding(
                s => s.SelectedMemberPath.EndsWith(".UniqueSurveyCode")));
        }

        [Test, Order(9)]
        public async Task ThirdRoundReminderEmailsSentCorrectly()
        {
            //Assign
            await _dbConnection.ExecuteAsync($@" 
                UPDATE EmployerSurveyHistory
                SET SentDate = DATEADD(DAY,-15,SentDate)
                WHERE UniqueSurveyCode IN (SELECT UniqueSurveyCode FROM EmployerSurveyCodes WHERE FeedbackId IN (SELECT FeedbackId FROM EmployerFeedback WHERE UserRef in @userRefs))",
                new { userRefs = new[] { _user3Guid } });
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            _employerSurveyReminderEmailer = new EmployerSurveyReminderEmailer(_dbEmployerFeedbackRepository,
                _notificationsApiClientMock.Object, _options, _surveyLoggerMock.Object);

            //Act
            await _employerSurveyReminderEmailer.SendEmailsAsync();

            //Assert
            _notificationsApiClientMock.Verify(x => x.SendEmail(It.IsAny<Email>()), Times.Exactly(0));
        }

        private void SetUpApiReturn(long changeableUkprn)
        {
            _providerApiClientReturn = new[]
            {
                new ProviderSummary {Ukprn = 1, ProviderName = "Test Academy"},
                new ProviderSummary {Ukprn = changeableUkprn, ProviderName = "Worst School"},
            };

            _commitmentApiClientReturn = new List<Apprenticeship>
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
