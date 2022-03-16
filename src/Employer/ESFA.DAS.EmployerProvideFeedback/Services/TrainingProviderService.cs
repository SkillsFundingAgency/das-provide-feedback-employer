using ESFA.DAS.EmployerProvideFeedback.Paging;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using SFA.DAS.Encoding;
using System.Linq;
using System.Threading.Tasks;

namespace ESFA.DAS.EmployerProvideFeedback.Services
{
    public interface ITrainingProviderService
    {
        Task<ProviderSearchViewModel> GetTrainingProviderSearchViewModel(string encodedAccountId, string selectedProviderName, string selectedFeedbackStatus, int pageSize = 10, int pageIndex = 0);

        Task<ProviderSearchConfirmationViewModel> GetTrainingProviderConfirmationViewModel(string encodedAccountId, long providerId);
    }



    public class TrainingProviderService : ITrainingProviderService
    {
        private readonly ICommitmentService _commitmentService;
        private readonly IEncodingService _encodingService;

        public TrainingProviderService(ICommitmentService commitmentService, IEncodingService encodingService)
        {
            _commitmentService = commitmentService;
            _encodingService = encodingService;
        }

        public async Task<ProviderSearchViewModel> GetTrainingProviderSearchViewModel(
            string encodedAccountId, 
            string selectedProviderName,
            string selectedFeedbackStatus,
            int pageSize = 10, 
            int page = 1)
        {
            ProviderSearchViewModel model = new ProviderSearchViewModel();
            model.AccountId = _encodingService.Decode(encodedAccountId, EncodingType.AccountId);
            model.EncodedAccountId = encodedAccountId;
            model.SelectedProviderName = selectedProviderName;
            model.SelectedFeedbackStatus = selectedFeedbackStatus;

            // Select all 
            var apprenticeshipsResponse = await _commitmentService.GetApprenticeships(model.AccountId);

            var providers = apprenticeshipsResponse.Apprenticeships.GroupBy(p => p.ProviderId)
                .Select(a => new ProviderSearchViewModel.EmployerTrainingProvider()
                {
                    ProviderId = a.First().ProviderId,
                    ProviderName = a.First().ProviderName
                })
                .ToList();

            // Filter

            model.ProviderNameFilter = providers.Select(p => p.ProviderName).OrderBy(p => p).ToList();
            model.FeedbackStatusFilter = new string[] { "Not yet submitted" };

            var filteredProviders = providers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(selectedProviderName) && selectedProviderName != "All")
            {
                filteredProviders = filteredProviders.Where(p => p.ProviderName == selectedProviderName);
            }
            if (selectedFeedbackStatus == "Not yet submitted")
            {
                filteredProviders = filteredProviders.Where(p => !p.DateSubmitted.HasValue);
            }

            // Page

            var pagedFilteredProviders = filteredProviders.OrderBy(p => p.ProviderName).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            model.TrainingProviders = new PaginatedList<ProviderSearchViewModel.EmployerTrainingProvider>(pagedFilteredProviders, filteredProviders.Count(), page, pageSize);

            return model;
        }

        public async Task<ProviderSearchConfirmationViewModel> GetTrainingProviderConfirmationViewModel(string encodedAccountId, long providerId)
        {
            var response = await _commitmentService.GetProvider(providerId);

            if(null == response)
            {
                // return error view
            }

            var model = new ProviderSearchConfirmationViewModel();
            //model.AccountId = _encodingService.Decode(encodedAccountId, EncodingType.AccountId);
            model.ProviderId = response.ProviderId;
            model.ProviderName = response.Name;

            return model;
        }
    }
}
