using Dapper;
using ESFA.DAS.Feedback.Employer.Emailer;
using ESFA.DAS.Feedback.Employer.Emailer.Configuration;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.ApiTypes;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Messages;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using ESFA.DAS.ProvideFeedback.Employer.Application;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer;
using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Types;

namespace IntegrationTests
{
    [NonParallelizable]
    public class IntegrationTest_NeedsToBeRanAsOne
    {
        private IConfigurationRoot _configuration;
        private Mock<ICommitmentService> _commitmentServiceMock;

        private Mock<IAccountService> _accountServiceMock;
        private Mock<INotificationsApi> _notificationsApiClientMock;

        private GetApprenticeshipsResponse _commitmentGetApprenticeshipsReturn;
        private ICollection<TeamMemberViewModel> _accountApiClientReturn;

        private Mock<ILogger<EmployerSurveyEmailer>> _surveyLoggerMock;

        private IEmployerFeedbackRepository _dbEmployerFeedbackRepository;
        private EmployerFeedbackDataRetrievalService _dataRetreivalService;
        private EmployerSurveyReminderEmailer _employerSurveyReminderEmailer;
        private InitiateDataRefreshFunction _initiateFunction;
        private ProviderRefreshFunction _providersRefreshFunction;
        private EmployerDataRetrieveFeedbackAccountsFunction _accountRetrieveFunction;
        private AccountRefreshFunction _accountDataRetrieveFunction;

        private ProcessActiveFeedbackFunction _processActiveFeedbackFunction;
        private EmployerSurveyInviteGeneratorFunction _surveyInviteGeneratorFunction;
        private UserRefreshService _helper;
        private SurveyInviteGenerator _surveyInviteGenerator;
        private IOptions<EmailSettings> _options;
        private DbConnection _dbConnection;

        private Guid _user1Guid;
        private Guid _user2Guid;
        private Guid _user3Guid;
        private Mock<IRoatpService> _roatpService;
        private ProviderRegistration[] _providerApiClientReturn;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            _options = new OptionsWrapper<EmailSettings>(new EmailSettings
            {
                BatchSize = 5,
                FeedbackSiteBaseUrl = "test.com",
                InviteCycleDays = 90,
                ReminderDays = 14
            });

            _user1Guid = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            _user2Guid = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            _user3Guid = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        }

        [SetUp]
        public void SetUp()
        {
            _commitmentServiceMock = new Mock<ICommitmentService>();
            _accountServiceMock = new Mock<IAccountService>();
            _notificationsApiClientMock = new Mock<INotificationsApi>();
            _surveyLoggerMock = new Mock<ILogger<EmployerSurveyEmailer>>();
            _roatpService = new Mock<IRoatpService>();

            _dbConnection = new SqlConnection(_configuration.GetConnectionString("EmployerEmailStoreConnection"));
            _dbEmployerFeedbackRepository = new EmployerFeedbackRepository(_dbConnection);

            _dataRetreivalService = new EmployerFeedbackDataRetrievalService(
                _commitmentServiceMock.Object,
                _accountServiceMock.Object,
                _dbEmployerFeedbackRepository);

            _helper = new UserRefreshService(new Mock<ILogger<UserRefreshService>>().Object, _dbEmployerFeedbackRepository);
            _surveyInviteGenerator = new SurveyInviteGenerator(_options, _dbEmployerFeedbackRepository, Mock.Of<ILogger<SurveyInviteGenerator>>());
            var providerRefreshSevice = new ProviderRefreshService(_dbEmployerFeedbackRepository, _roatpService.Object);

            SetupApiMocks(2);

            _initiateFunction = new InitiateDataRefreshFunction(_dbEmployerFeedbackRepository);
            _providersRefreshFunction = new ProviderRefreshFunction(providerRefreshSevice);
            _accountRetrieveFunction = new EmployerDataRetrieveFeedbackAccountsFunction(_commitmentServiceMock.Object);
            _accountDataRetrieveFunction = new AccountRefreshFunction(_dataRetreivalService);
            _processActiveFeedbackFunction = new ProcessActiveFeedbackFunction(_helper);
            _surveyInviteGeneratorFunction = new EmployerSurveyInviteGeneratorFunction(_surveyInviteGenerator);
        }

        [Test, Category("IntegrationTest")]
        public async Task InitiateEmployerFeedbackDataRefresh_ShouldSetAllFeedbackAsInactive()
        {
            // Arrange
            await CleanupData();
            await RunThroughRefreshFunctions();

            // Act
            var result = await _initiateFunction.Run(null, Mock.Of<ILogger>());

            // Assert
            result.Should().Be(string.Empty);
            var feedbackIsActiveFlags = await _dbConnection.QueryAsync<int>($@"SELECT IsActive FROM EmployerFeedback");
            feedbackIsActiveFlags.Should().AllBeEquivalentTo(0);
        }

        [Test, Order(1)]
        public async Task FirstEmployerFeedbackDataRefresh()
        {
            // Assert
            await CleanupData();

            var expectedInvites = new List<EmployerSurveyInvite>
            {
                new EmployerSurveyInvite {UserRef = _user1Guid, EmailAddress = "Test@test.com", FirstName = "Master", AccountId = 1, Ukprn = 1, ProviderName = "Test Academy"},
                new EmployerSurveyInvite {UserRef = _user1Guid, EmailAddress = "Test@test.com", FirstName = "Master", AccountId = 1, Ukprn = 2, ProviderName = "Worst School"},
                new EmployerSurveyInvite {UserRef = _user2Guid, EmailAddress = "TheBestThereEverWas@90sReference.com", FirstName = "Flash", AccountId = 1, Ukprn = 1, ProviderName = "Test Academy"},
                new EmployerSurveyInvite {UserRef = _user2Guid, EmailAddress = "TheBestThereEverWas@90sReference.com", FirstName = "Flash", AccountId = 1, Ukprn = 2, ProviderName = "Worst School"},
            };

            // Act
            await RunThroughRefreshFunctions();

            // Assert
            var invites = await _dbEmployerFeedbackRepository.GetEmployerUsersToBeSentInvite();
            invites.Count().Should().Be(4);
            invites.Should().BeEquivalentTo(expectedInvites, options => options.Excluding(
                s => s.SelectedMemberPath.EndsWith(".UniqueSurveyCode")));
        }

        [Test, Order(3)]
        public async Task FirstRoundReminderEmailsSentCorrectly()
        {
            //Assign
            await _dbConnection.ExecuteAsync($@" 
                UPDATE EmployerSurveyHistory
                SET SentDate = @newSentDate
                WHERE UniqueSurveyCode IN (SELECT UniqueSurveyCode FROM EmployerSurveyCodes WHERE FeedbackId IN (SELECT FeedbackId FROM EmployerFeedback WHERE UserRef in @userRefs))",
                new { newSentDate = DateTime.UtcNow.AddDays(-15), userRefs = new[] { _user1Guid, _user2Guid } });
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
            SetupApiMocks(3);
            var expectedInvites = new List<EmployerSurveyInvite>
            {
                new EmployerSurveyInvite {UserRef = _user1Guid, EmailAddress = "Test@test.com", FirstName = "Master", AccountId = 1, Ukprn = 3, ProviderName = "Worst School"},
                new EmployerSurveyInvite {UserRef = _user2Guid, EmailAddress = "TheBestThereEverWas@90sReference.com", FirstName = "Flash", AccountId = 1, Ukprn = 3, ProviderName = "Worst School"},
            };

            //Act
            await RunThroughRefreshFunctions();

            //Assert
            var invites = await _dbEmployerFeedbackRepository.GetEmployerUsersToBeSentInvite();
            invites.Count().Should().Be(2);
            var invitesList = invites.OrderBy(x => x.UserRef).ThenBy(x => x.AccountId).ThenBy(x => x.Ukprn).ToList();
            invites.Should().BeEquivalentTo(expectedInvites, options => options.Excluding(
                s => s.SelectedMemberPath.EndsWith(".UniqueSurveyCode")));
        }

        [Test, Order(6)]
        public async Task SecondRoundReminder_OnlySendWhereNoPreviousReminder()
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

        [Test, Order(8)]
        public async Task CurrentUsersLeftCompanyNewPersonJoins()
        {
            //Assign
            SetupApiMocks(3);
            _accountApiClientReturn = new List<TeamMemberViewModel>
            {
                new TeamMemberViewModel
                {
                    Email = "InWestPhiladelphiaBornAndRaised@PlaygroundDayz.com", Name = "Fresh Prince",
                    UserRef = _user3Guid.ToString(), CanReceiveNotifications = true
                }
            };
            _accountServiceMock.Setup(x => x.GetAccountUsers(It.IsAny<long>())).ReturnsAsync(_accountApiClientReturn);

            var expectedInvites = new List<EmployerSurveyInvite>
            {
                new EmployerSurveyInvite {UserRef = _user3Guid, EmailAddress = "InWestPhiladelphiaBornAndRaised@PlaygroundDayz.com", FirstName = "Fresh", AccountId = 1, Ukprn = 1, ProviderName = "Test Academy"},
                new EmployerSurveyInvite {UserRef = _user3Guid, EmailAddress = "InWestPhiladelphiaBornAndRaised@PlaygroundDayz.com", FirstName = "Fresh", AccountId = 1, Ukprn = 3 , ProviderName = "Worst School"},
            };

            //Act
            await RunThroughRefreshFunctions();

            //Assert
            var invites = await _dbEmployerFeedbackRepository.GetEmployerUsersToBeSentInvite();
            invites.Count().Should().Be(2);
            invites.Should().BeEquivalentTo(expectedInvites, options => options.Excluding(
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
                new ProviderRegistration {Ukprn = 1, LegalName = "Test Academy"},
                new ProviderRegistration {Ukprn = changeableUkprn, LegalName = "Worst School"},
            };

            _commitmentGetApprenticeshipsReturn = new GetApprenticeshipsResponse
            {
                TotalApprenticeships = 3,
                Apprenticeships = new List<GetApprenticeshipsResponse.ApprenticeshipDetailsResponse>
                {
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse
                    {
                        PaymentStatus = PaymentStatus.Active,
                        Uln = "1",
                        AccountLegalEntityId = 1,
                        ProviderId = 1,
                        ProviderName = "Test Academy"
                    },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse
                    {
                        PaymentStatus = PaymentStatus.Active,
                        Uln = "2",
                        AccountLegalEntityId = 1,
                        ProviderId = 1,
                        ProviderName = "Test Academy"
                    },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse
                    {
                        PaymentStatus = PaymentStatus.Active,
                        Uln = "3",
                        AccountLegalEntityId = 1,
                        ProviderId = changeableUkprn,
                        ProviderName = "Worst School"
                    }
                }
            };


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
        }

        private async Task CleanupData()
        {
            await _dbConnection.ExecuteAsync($@" 
                DELETE FROM EmployerSurveyHistory;
                DELETE FROM EmployerSurveyCodes
                DELETE FROM EmployerFeedback;
                DELETE FROM Users
                DELETE FROM Providers");
        }

        private void SetupApiMocks(int changeableUkprn)
        {
            SetUpApiReturn(changeableUkprn);

            _accountServiceMock.Setup(x => x.GetAccountUsers(It.IsAny<long>())).ReturnsAsync(_accountApiClientReturn);
            _roatpService.Setup(x => x.GetAll()).ReturnsAsync(_providerApiClientReturn);
           
            _commitmentServiceMock
                .Setup(x => x.GetApprenticeships(It.IsAny<long>()))
                .ReturnsAsync(_commitmentGetApprenticeshipsReturn);

            _commitmentServiceMock
                .Setup(x => x.GetAllCohortAccountIds())
                .ReturnsAsync(new GetAllCohortAccountIdsResponse(new List<long> {1}));

        }

        private async Task RunThroughRefreshFunctions()
        {
            // Callback which populates these can be called on multiple threads. 
            var accountsMessages = new ConcurrentBag<string>();
            var generateCodeMessages = new ConcurrentBag<string>();
            var processActiveMessages = new ConcurrentBag<string>();

            var accountsCollectorMock = new Mock<ICollector<string>>();
            accountsCollectorMock
                .Setup(mock => mock.Add(It.IsAny<string>()))
                .Callback((string accountId) => accountsMessages.Add(accountId));

            var processActiveFeedbackCollectorMock = new Mock<ICollector<GroupedFeedbackRefreshMessage>>();
            processActiveFeedbackCollectorMock
                .Setup(mock => mock.Add(It.IsAny<GroupedFeedbackRefreshMessage>()))
                .Callback((GroupedFeedbackRefreshMessage message) => processActiveMessages.Add(JsonConvert.SerializeObject(message)));

            var generateSurveyCodeMessageCollectorMock = new Mock<ICollector<GenerateSurveyCodeMessage>>();
            generateSurveyCodeMessageCollectorMock
                .Setup(mock => mock.Add(It.IsAny<GenerateSurveyCodeMessage>()))
                .Callback((GenerateSurveyCodeMessage message) => generateCodeMessages.Add(JsonConvert.SerializeObject(message)));

            var initiateMessage = await _initiateFunction.Run(null, Mock.Of<ILogger>());
            var providersRefreshedMessage = await _providersRefreshFunction.Run(initiateMessage, Mock.Of<ILogger>());
            await _accountRetrieveFunction.Run(providersRefreshedMessage, accountsCollectorMock.Object, Mock.Of<ILogger>());

            foreach (var accountId in accountsMessages)
            {
                await _accountDataRetrieveFunction.Run(accountId, processActiveFeedbackCollectorMock.Object, Mock.Of<ILogger>());

                foreach (var refreshMessage in processActiveMessages)
                {
                    generateCodeMessages.Clear();
                    await _processActiveFeedbackFunction.Run(refreshMessage, Mock.Of<ILogger>(), generateSurveyCodeMessageCollectorMock.Object);
                    foreach (var generateCodeMessage in generateCodeMessages)
                    {
                        await _surveyInviteGeneratorFunction.Run(generateCodeMessage, Mock.Of<ILogger>());
                    }
                }
            }
        }
    }
}
