using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Services;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using Moq;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.Encoding;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Services
{
    public class TrainingProviderServiceTests
    {
        private static readonly Mock<ICommitmentService> _commitmentServiceMock = new Mock<ICommitmentService>();
        private static readonly Mock<IEncodingService> _encodingServiceMock = new Mock<IEncodingService>();
        private static readonly Mock<IEmployerFeedbackRepository> _employerFeedbackRepositoryMock = new Mock<IEmployerFeedbackRepository>();
        private static readonly ProvideFeedbackEmployerWebConfiguration _config = new ProvideFeedbackEmployerWebConfiguration()
        {
            FeedbackWaitPeriodDays = 21,
        };

        public class GetTrainingProviderSearchViewModel
        {
            public static IEnumerable<object[]> MultiplePagedProvidersTestData()
            {
                IEnumerable<GetApprenticeshipsResponse.ApprenticeshipDetailsResponse> providers = new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse[]
                {
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse()
                    {
                        ProviderId = 1,
                        ProviderName = "A",
                    },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse()
                    {
                        ProviderId = 2,
                        ProviderName = "B"
                    },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse()
                    {
                        ProviderId = 3,
                        ProviderName = "C"
                    },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse()
                    {
                        ProviderId = 4,
                        ProviderName = "D"
                    },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse()
                    {
                        ProviderId = 5,
                        ProviderName = "E"
                    },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse()
                    {
                        ProviderId = 6,
                        ProviderName = "F"
                    },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse()
                    {
                        ProviderId = 7,
                        ProviderName = "G"
                    },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse()
                    {
                        ProviderId = 8,
                        ProviderName = "H"
                    },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse()
                    {
                        ProviderId = 9,
                        ProviderName = "I"
                    },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse()
                    {
                        ProviderId = 10,
                        ProviderName = "J"
                    },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse()
                    {
                        ProviderId = 11,
                        ProviderName = "K"
                    },
                };

                yield return new object[] { providers, 10, 1, "All", "All", "ProviderName", "Asc", 11, 2 };
            }

            [Theory]
            [MemberData(nameof(MultiplePagedProvidersTestData))]
            public async Task When_Providers_Exist_Then_Return_PagedResult(
                IEnumerable<GetApprenticeshipsResponse.ApprenticeshipDetailsResponse> providers, 
                int pageSize, int pageIndex, string selectedProviderName, string selectedFeedbackStatus, string sortColumn, string sortDirection,
                int expectedTotalRecordCount, int expectedTotalPages)
            {
                var testAccountId = 1;
                var testAccountIdEncoded = "MANYTEST1";
                _encodingServiceMock.Setup(m => m.Decode(testAccountIdEncoded, EncodingType.AccountId)).Returns(testAccountId);

                _commitmentServiceMock.Setup(m => m.GetApprenticeships(testAccountId)).ReturnsAsync(
                    new GetApprenticeshipsResponse() { Apprenticeships = providers});

                ITrainingProviderService sut = new TrainingProviderService(
                    _commitmentServiceMock.Object,
                    _encodingServiceMock.Object,
                    _employerFeedbackRepositoryMock.Object,
                    _config);

                var model = await sut.GetTrainingProviderSearchViewModel(
                    testAccountIdEncoded, selectedProviderName, selectedFeedbackStatus, pageSize, pageIndex, sortColumn, sortDirection);

                Assert.Equal(expectedTotalRecordCount, model.TrainingProviders.TotalRecordCount);
                Assert.Equal(expectedTotalPages, model.TrainingProviders.TotalPages);
            }
        }

        public class GetTrainingProviderConfirmationViewModel
        {
            [Fact]
            public async Task When_Provider_Exists_Then_Return_ProviderViewModel()
            {
                var testAccountId = 2;
                var testAccountIdEncoded = "CONFIRMATIONMODELTEST1";
                _encodingServiceMock.Setup(m => m.Decode(testAccountIdEncoded, EncodingType.AccountId)).Returns(testAccountId);

                _commitmentServiceMock.Setup(m => m.GetProvider(1)).ReturnsAsync(
                    new GetProviderResponse() { ProviderId = 1, Name = "Test Provider" });

                ITrainingProviderService sut = new TrainingProviderService(
                    _commitmentServiceMock.Object,
                    _encodingServiceMock.Object,
                    _employerFeedbackRepositoryMock.Object,
                    _config);

                var model = await sut.GetTrainingProviderConfirmationViewModel(testAccountIdEncoded, 1);

                Assert.NotNull(model);
                Assert.Equal(1, model.ProviderId);
                Assert.Equal("Test Provider", model.ProviderName);
            }
        }
    }
}
