using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Services;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.Encoding;

namespace UnitTests.Services
{
    public class TrainingProviderServiceTests
    {
        private static readonly Mock<ICommitmentService> _commitmentServiceMock = new();
        private static readonly Mock<IEncodingService> _encodingServiceMock = new();
        private static readonly Mock<IEmployerFeedbackRepository> _employerFeedbackRepositoryMock = new();
        private static readonly ProvideFeedbackEmployerWebConfiguration _config = new()
        {
            FeedbackWaitPeriodDays = 21,
        };

        public class GetTrainingProviderSearchViewModel
        {
            public static IEnumerable<object[]> MultiplePagedProvidersTestData()
            {
                IEnumerable<GetApprenticeshipsResponse.ApprenticeshipDetailsResponse> providers = new[]
                {
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 1, ProviderName = "A" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 2, ProviderName = "B" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 3, ProviderName = "C" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 4, ProviderName = "D" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 5, ProviderName = "E" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 6, ProviderName = "F" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 7, ProviderName = "G" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 8, ProviderName = "H" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 9, ProviderName = "I" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 10, ProviderName = "J" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 11, ProviderName = "K" },
                };

                yield return new object[] { providers, 10, 1, "All", "All", "ProviderName", "Asc", 11, 2 };
            }

            [TestCaseSource(nameof(MultiplePagedProvidersTestData))]
            public async Task When_Providers_Exist_Then_Return_PagedResult(
                IEnumerable<GetApprenticeshipsResponse.ApprenticeshipDetailsResponse> providers,
                int pageSize, int pageIndex, string selectedProviderName, string selectedFeedbackStatus,
                string sortColumn, string sortDirection, int expectedTotalRecordCount, int expectedTotalPages)
            {
                // Arrange
                var testAccountId = 1;
                var testAccountIdEncoded = "MANYTEST1";
                _encodingServiceMock.Setup(m => m.Decode(testAccountIdEncoded, EncodingType.AccountId)).Returns(testAccountId);
                _commitmentServiceMock.Setup(m => m.GetApprenticeships(testAccountId))
                    .ReturnsAsync(new GetApprenticeshipsResponse { Apprenticeships = providers });

                ITrainingProviderService sut = new TrainingProviderService(
                    _commitmentServiceMock.Object,
                    _encodingServiceMock.Object,
                    _employerFeedbackRepositoryMock.Object,
                    _config);

                // Act
                var model = await sut.GetTrainingProviderSearchViewModel(
                    testAccountIdEncoded, selectedProviderName, selectedFeedbackStatus, pageSize, pageIndex, sortColumn, sortDirection);

                // Assert
                model.TrainingProviders.TotalRecordCount.Should().Be(expectedTotalRecordCount);
                model.TrainingProviders.TotalPages.Should().Be(expectedTotalPages);
            }
        }

        public class GetTrainingProviderConfirmationViewModel
        {
            [Test]
            public async Task When_Provider_Exists_Then_Return_ProviderViewModel()
            {
                // Arrange
                var testAccountId = 2;
                var testAccountIdEncoded = "CONFIRMATIONMODELTEST1";
                _encodingServiceMock.Setup(m => m.Decode(testAccountIdEncoded, EncodingType.AccountId)).Returns(testAccountId);
                _commitmentServiceMock.Setup(m => m.GetProvider(1)).ReturnsAsync(
                    new GetProviderResponse { ProviderId = 1, Name = "Test Provider" });

                ITrainingProviderService sut = new TrainingProviderService(
                    _commitmentServiceMock.Object,
                    _encodingServiceMock.Object,
                    _employerFeedbackRepositoryMock.Object,
                    _config);

                // Act
                var model = await sut.GetTrainingProviderConfirmationViewModel(testAccountIdEncoded, 1);

                // Assert
                model.Should().NotBeNull();
                model.ProviderId.Should().Be(1);
                model.ProviderName.Should().Be("Test Provider");
            }
        }
    }
}
