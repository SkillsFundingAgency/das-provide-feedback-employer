namespace UnitTests.Api
{
    using System;
    using System.Collections.Generic;

    using ESFA.DAS.EmployerProvideFeedback.Api.Models;
    using ESFA.DAS.FeedbackDataAccess.Models;

    internal class EmployerFeedbackTestHelper
    {
        private static readonly Random Random = new Random();

        private static readonly Array Ratings = Enum.GetValues(typeof(Rating));

        private static readonly Array Scores = Enum.GetValues(typeof(Score));

        internal enum Rating
        {
            Excellent = 4,

            Good = 3,

            Bad = 2,

            Rubbish = 1
        }

        internal enum Score
        {
            Positive = 1,

            Neutral = 0,

            Negative = -1
        }

        internal ESFA.DAS.EmployerProvideFeedback.Api.Dto.EmployerFeedback GenerateRandomFeedback(string id) =>
            new ESFA.DAS.EmployerProvideFeedback.Api.Dto.EmployerFeedback
            {
                    Id = new Guid(id).ToString(),
                    Ukprn = 123456789,
                    AccountId = 987654321,
                    UserRef = Guid.NewGuid(),
                    DateTimeCompleted = DateTime.UtcNow.AddDays(-Random.Next(200)),
                    ProviderAttributes = new List<ESFA.DAS.EmployerProvideFeedback.Api.Dto.ProviderAttribute>()
                        {
                            new ESFA.DAS.EmployerProvideFeedback.Api.Dto.ProviderAttribute()
                                {
                                    Name = "Providing relevant training at the right time",
                                    Value = (int)Scores.GetValue(Random.Next(Scores.Length))
                                },
                            new ESFA.DAS.EmployerProvideFeedback.Api.Dto.ProviderAttribute()
                                {
                                    Name = "Communication between you and the provider",
                                    Value = (int)Scores.GetValue(Random.Next(Scores.Length))
                                },
                            new ESFA.DAS.EmployerProvideFeedback.Api.Dto.ProviderAttribute()
                                {
                                    Name = "On-boarding new apprentices",
                                    Value = (int)Scores.GetValue(Random.Next(Scores.Length))
                                },
                            new ESFA.DAS.EmployerProvideFeedback.Api.Dto.ProviderAttribute()
                                {
                                    Name = "Improving apprentice skills",
                                    Value = (int)Scores.GetValue(Random.Next(Scores.Length))
                                },
                            new ESFA.DAS.EmployerProvideFeedback.Api.Dto.ProviderAttribute()
                                {
                                    Name = "Improving business outcomes",
                                    Value = (int)Scores.GetValue(Random.Next(Scores.Length))
                                },
                            new ESFA.DAS.EmployerProvideFeedback.Api.Dto.ProviderAttribute()
                                {
                                    Name = "Taking on small cohorts (small number of apprentices)",
                                    Value = (int)Scores.GetValue(Random.Next(Scores.Length))
                                },
                            new ESFA.DAS.EmployerProvideFeedback.Api.Dto.ProviderAttribute()
                                {
                                    Name = "Recruiting apprentices",
                                    Value = (int)Scores.GetValue(Random.Next(Scores.Length))
                                },
                            new ESFA.DAS.EmployerProvideFeedback.Api.Dto.ProviderAttribute()
                                {
                                    Name = "Assessing and reporting on progress of apprentices",
                                    Value = (int)Scores.GetValue(Random.Next(Scores.Length))
                                },
                            new ESFA.DAS.EmployerProvideFeedback.Api.Dto.ProviderAttribute()
                                {
                                    Name = "Adapting to my needs",
                                    Value = (int)Scores.GetValue(Random.Next(Scores.Length))
                                }
                        },
                    ProviderRating = ((Rating)Ratings.GetValue(Random.Next(Ratings.Length))).ToString()
                };
    }
}