using ESFA.DAS.EmployerProvideFeedback.Paging;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
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

        Task UpsertTrainingProvider(long providerId, string providerName);
    }



    public class TrainingProviderService : ITrainingProviderService
    {
        private readonly ICommitmentService _commitmentService;
        private readonly IEncodingService _encodingService;
        private readonly IEmployerFeedbackRepository _employerFeedbackRepository;

        public TrainingProviderService(ICommitmentService commitmentService, IEncodingService encodingService, IEmployerFeedbackRepository employerFeedbackRepository)
        {
            _commitmentService = commitmentService;
            _encodingService = encodingService;
            _employerFeedbackRepository = employerFeedbackRepository;
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
            model.TrainingProviders.PageSetSize = 6;

            // Urgh.

            var employerFeedback = await _employerFeedbackRepository.GetAllFeedbackAndResultFromEmployer(model.AccountId);
            foreach(var provider in pagedFilteredProviders)
            {
                var feedBackForProvider = employerFeedback.FirstOrDefault(fp => fp.Ukprn == provider.ProviderId);
                if(null == feedBackForProvider)
                {
                    provider.FeedbackStatus = "Not yet submitted";
                    provider.DateSubmitted = null;
                }
                else
                {
                    provider.FeedbackStatus = "Submitted";
                    provider.DateSubmitted = feedBackForProvider.DateTimeCompleted;
                }
            }

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
            model.ProviderId = response.ProviderId;
            model.ProviderName = response.Name;

            return model;
        }

        public async Task UpsertTrainingProvider(long providerId, string providerName)
        {
            await _employerFeedbackRepository.UpsertIntoProviders(new Provider[] { new Provider() { Ukprn = providerId, ProviderName = providerName } });
        }
    }
}
