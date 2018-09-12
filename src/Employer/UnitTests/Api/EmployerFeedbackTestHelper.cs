using System;
using System.Collections.Generic;
using ESFA.DAS.FeedbackDataAccess.Models;

namespace UnitTests.Api
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

        internal EmployerFeedback AddedFeedback() => new EmployerFeedback
        {
            Id = new Guid("ddcf9d13-bf05-4e9c-bd5c-20c4133cc739"),
            Ukprn = 123456789,
            AccountId = 987654321,
            UserRef = new Guid("1e07393a-0d85-49e4-b18e-556de9bc3f67"),
            DateTimeCompleted = new DateTime(2018, 09, 12),
            ProviderAttributes = new List<ProviderAttribute>()
            {
                new ProviderAttribute() { Name = "Sword-fighting", Value = 1 },
                new ProviderAttribute() { Name = "Fire-juggling", Value = 1 },
                new ProviderAttribute() { Name = "Bar-tending", Value = -1 },
                new ProviderAttribute() { Name = "Tight-roping", Value = -1 }
            },
            ProviderRating = "awesome"
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