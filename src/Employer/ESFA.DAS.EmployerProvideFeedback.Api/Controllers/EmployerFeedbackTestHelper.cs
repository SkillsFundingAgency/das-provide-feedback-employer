using System;
using System.Collections.Generic;
using ESFA.DAS.FeedbackDataAccess.Models;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Controllers
{
    internal class EmployerFeedbackTestHelper
    {
        private static readonly Random Random = new Random();
        private static readonly Array Scores = Enum.GetValues(typeof(Score));
        private static readonly Array Ratings = Enum.GetValues(typeof(Rating));

        internal enum Score
        {
            Positive = 1,
            Neutral = 0,
            Negative = -1
        }

        internal enum Rating
        {
            Excellent = 4,
            Good = 3,
            Bad = 2,
            Rubbish = 1
        }

        internal EmployerFeedback OldFeedback() => new EmployerFeedback
        {
            Id = Guid.NewGuid(),
            Ukprn = 123456789,
            AccountId = 987654321,
            UserRef = Guid.NewGuid(),
            DateTimeCompleted = DateTime.UtcNow.AddMonths(-7),
            ProviderAttributes = new List<ProviderAttribute>()
            {
                new ProviderAttribute() { Name = "Sword-fighting", Value = 1 },
                new ProviderAttribute() { Name = "Fire-juggling", Value = 1 },
                new ProviderAttribute() { Name = "Bar-tending", Value = -1 },
                new ProviderAttribute() { Name = "Tight-roping", Value = -1 }
            }
        };

        internal EmployerFeedback GenerateRandomFeedback() => new EmployerFeedback
        {
            Id = Guid.NewGuid(),
            Ukprn = 123456789,
            AccountId = 987654321,
            UserRef = Guid.NewGuid(),
            DateTimeCompleted = DateTime.UtcNow.AddDays(-Random.Next(200)),
            ProviderAttributes = new List<ProviderAttribute>()
            {
                new ProviderAttribute() { Name = "Providing relevant training at the right time", Value = (int)Scores.GetValue(Random.Next(Scores.Length)) },
                new ProviderAttribute() { Name = "Communication between you and the provider", Value = (int)Scores.GetValue(Random.Next(Scores.Length)) },
                new ProviderAttribute() { Name = "On-boarding new apprentices", Value = (int)Scores.GetValue(Random.Next(Scores.Length)) },
                new ProviderAttribute() { Name = "Improving apprentice skills", Value = (int)Scores.GetValue(Random.Next(Scores.Length)) },
                new ProviderAttribute() { Name = "Improving business outcomes", Value = (int)Scores.GetValue(Random.Next(Scores.Length)) },
                new ProviderAttribute() { Name = "Taking on small cohorts (small number of apprentices)", Value = (int)Scores.GetValue(Random.Next(Scores.Length)) },
                new ProviderAttribute() { Name = "Recruiting apprentices", Value = (int)Scores.GetValue(Random.Next(Scores.Length)) },
                new ProviderAttribute() { Name = "Assessing and reporting on progress of apprentices", Value = (int)Scores.GetValue(Random.Next(Scores.Length)) },
                new ProviderAttribute() { Name = "Adapting to my needs", Value = (int)Scores.GetValue(Random.Next(Scores.Length)) }
            },
            ProviderRating = ((Rating) Ratings.GetValue(Random.Next(Ratings.Length))).ToString()
        };
    }
}