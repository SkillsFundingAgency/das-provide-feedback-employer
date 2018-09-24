namespace ESFA.DAS.EmployerProvideFeedback.Api.Configuration.Mappers
{
    using AutoMapper;

    using ESFA.DAS.EmployerProvideFeedback.Api.Models;

    using EmployerFeedbackDto = Dto.EmployerFeedback;
    using ProviderAttributeDto = Dto.ProviderAttribute;

    public class EmployerProvideFeedbackProfile : Profile
    {
        public EmployerProvideFeedbackProfile()
        {
            this.CreateMap<ProviderAttributeDto, ProviderAttribute>()
                .ForMember(destination => destination.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(destination => destination.Value, opt => opt.MapFrom(src => src.Value));

            this.CreateMap<EmployerFeedbackDto, PublicEmployerFeedback>()
                .ForMember(destination => destination.DateTimeCompleted, opt => opt.MapFrom(src => src.DateTimeCompleted))
                .ForMember(destination => destination.ProviderAttributes, opt => opt.MapFrom(src => src.ProviderAttributes))
                .ForMember(destination => destination.ProviderRating, opt => opt.MapFrom(src => src.ProviderRating))
                .ForMember(destination => destination.Ukprn, opt => opt.MapFrom(src => src.Ukprn));

            this.CreateMap<EmployerFeedbackDto, EmployerFeedback>()
                .ForMember(destination => destination.AccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(destination => destination.UserRef, opt => opt.MapFrom(src => src.UserRef))
                .ForMember(destination => destination.DateTimeCompleted, opt => opt.MapFrom(src => src.DateTimeCompleted))
                .ForMember(destination => destination.ProviderAttributes, opt => opt.MapFrom(src => src.ProviderAttributes))
                .ForMember(destination => destination.ProviderRating, opt => opt.MapFrom(src => src.ProviderRating))
                .ForMember(destination => destination.Ukprn, opt => opt.MapFrom(src => src.Ukprn));
        }
    }
}
