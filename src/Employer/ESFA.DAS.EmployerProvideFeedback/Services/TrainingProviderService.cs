﻿using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Paging;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESFA.DAS.EmployerProvideFeedback.Services
{
    public interface ITrainingProviderService
    {
        Task<ProviderSearchViewModel> GetTrainingProviderSearchViewModel(string encodedAccountId, string selectedProviderName, string selectedFeedbackStatus, int pageSize, int pageIndex, string sortColumn, string sortDirection);

        Task<ProviderSearchConfirmationViewModel> GetTrainingProviderConfirmationViewModel(string encodedAccountId, long providerId);

        Task UpsertTrainingProvider(long providerId, string providerName);
    }



    public class TrainingProviderService : ITrainingProviderService
    {
        private readonly ICommitmentService _commitmentService;
        private readonly IEncodingService _encodingService;
        private readonly IEmployerFeedbackRepository _employerFeedbackRepository;
        private readonly ProvideFeedbackEmployerWebConfiguration _config;

        private const string NOT_YET_SUBMITTED = "Not yet submitted";

        public TrainingProviderService(
            ICommitmentService commitmentService
            , IEncodingService encodingService
            , IEmployerFeedbackRepository employerFeedbackRepository
            , ProvideFeedbackEmployerWebConfiguration config)
        {
            _commitmentService = commitmentService;
            _encodingService = encodingService;
            _employerFeedbackRepository = employerFeedbackRepository;
            _config = config;
        }

        public async Task<ProviderSearchViewModel> GetTrainingProviderSearchViewModel(
            string encodedAccountId, 
            string selectedProviderName,
            string selectedFeedbackStatus,
            int pageSize, 
            int pageIndex,
            string sortColumn,
            string sortDirection)
        {
            ProviderSearchViewModel model = new ProviderSearchViewModel();
            model.AccountId = _encodingService.Decode(encodedAccountId, EncodingType.AccountId);
            model.EncodedAccountId = encodedAccountId;
            model.SelectedProviderName = selectedProviderName;
            model.SelectedFeedbackStatus = selectedFeedbackStatus;
            model.SortColumn = sortColumn;
            model.SortDirection = sortDirection;

            // Select all the providers for this employer.

            var providers = await SelectAllProvidersForAccount(model.AccountId);

            // Augment the provider records with feedback data. Urgh.
            // We need to do this so that the date filtering will work.

            await AugmentProviderRecordsWithFeedbackStatus(model.AccountId, providers);

            // Initialise the filter options.

            model.UnfilteredTotalRecordCount = providers.Count;
            model.ProviderNameFilter = providers.Select(p => p.ProviderName).OrderBy(p => p).ToList();
            model.FeedbackStatusFilter = new string[] { NOT_YET_SUBMITTED };

            // Apply filters.

            var filteredProviders = providers.AsQueryable();
            filteredProviders = ApplyProviderNameFilter(filteredProviders, selectedProviderName);
            filteredProviders = ApplyFeedbackStatusFilter(filteredProviders, selectedFeedbackStatus);

            // Sort

            if(PagingState.SortDescending == model.SortDirection)
            {
                if(!string.IsNullOrWhiteSpace(model.SortColumn) && model.SortColumn.Equals("FeedbackStatus", StringComparison.InvariantCultureIgnoreCase))
                {
                    filteredProviders = filteredProviders.OrderByDescending(p => p.FeedbackStatus);
                }
                else if (!string.IsNullOrWhiteSpace(model.SortColumn) && model.SortColumn.Equals("DateSubmitted", StringComparison.InvariantCultureIgnoreCase))
                {
                    filteredProviders = filteredProviders.OrderByDescending(p => p.DateSubmitted);
                }
                else
                {
                    filteredProviders = filteredProviders.OrderByDescending(p => p.ProviderName);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(model.SortColumn) && model.SortColumn.Equals("FeedbackStatus", StringComparison.InvariantCultureIgnoreCase))
                {
                    filteredProviders = filteredProviders.OrderBy(p => p.FeedbackStatus);
                }
                else if (!string.IsNullOrWhiteSpace(model.SortColumn) && model.SortColumn.Equals("DateSubmitted", StringComparison.InvariantCultureIgnoreCase))
                {
                    filteredProviders = filteredProviders.OrderBy(p => p.DateSubmitted);
                }
                else
                {
                    filteredProviders = filteredProviders.OrderBy(p => p.ProviderName);
                }
            }

            // Page

            var pagedFilteredProviders = filteredProviders.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            model.TrainingProviders = new PaginatedList<ProviderSearchViewModel.EmployerTrainingProvider>(pagedFilteredProviders, filteredProviders.Count(), pageIndex, pageSize, 6);

            return model;
        }

        public async Task<ProviderSearchConfirmationViewModel> GetTrainingProviderConfirmationViewModel(string encodedAccountId, long providerId)
        {
            var response = await _commitmentService.GetProvider(providerId);

            if(null == response)
            {
                return null;
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

        private async Task<List<ProviderSearchViewModel.EmployerTrainingProvider>> SelectAllProvidersForAccount(long accountId)
        {
            // Select all 
            var apprenticeshipsResponse = await _commitmentService.GetApprenticeships(accountId);

            var providers = apprenticeshipsResponse.Apprenticeships.GroupBy(p => p.ProviderId)
                .Select(a => new ProviderSearchViewModel.EmployerTrainingProvider()
                {
                    ProviderId = a.First().ProviderId,
                    ProviderName = a.First().ProviderName
                })
                .ToList();

            return providers;
        }

        private async Task AugmentProviderRecordsWithFeedbackStatus(long accountId, List<ProviderSearchViewModel.EmployerTrainingProvider> providers)
        {
            var employerFeedback = await _employerFeedbackRepository.GetAllFeedbackAndResultFromEmployer(accountId);
            foreach (var provider in providers)
            {
                var feedBackForProvider = employerFeedback.FirstOrDefault(fp => fp.Ukprn == provider.ProviderId);
                if (null == feedBackForProvider)
                {
                    provider.FeedbackStatus = NOT_YET_SUBMITTED;
                    provider.DateSubmitted = null;
                }
                else
                {
                    provider.FeedbackStatus = "Submitted";
                    provider.DateSubmitted = feedBackForProvider.DateTimeCompleted;
                }

                provider.CanSubmitFeedback = true;
                if (provider.DateSubmitted.HasValue && (DateTime.UtcNow - provider.DateSubmitted.Value).TotalDays < _config.FeedbackWaitPeriodDays)
                {
                    provider.CanSubmitFeedback = false;
                }
            }
        }

        private IQueryable<ProviderSearchViewModel.EmployerTrainingProvider> ApplyProviderNameFilter(IQueryable<ProviderSearchViewModel.EmployerTrainingProvider> providers, string providerName)
        {
            if (!string.IsNullOrWhiteSpace(providerName) && providerName != "All")
            {
                providers = providers.Where(p => p.ProviderName == providerName);
            }
            return providers;
        }

        private IQueryable<ProviderSearchViewModel.EmployerTrainingProvider> ApplyFeedbackStatusFilter(IQueryable<ProviderSearchViewModel.EmployerTrainingProvider> providers, string feedbackStatus)
        {
            if (feedbackStatus == NOT_YET_SUBMITTED)
            {
                providers = providers.Where(p => null == p.DateSubmitted || !p.DateSubmitted.HasValue);
            }
            return providers;
        }
    }
}
