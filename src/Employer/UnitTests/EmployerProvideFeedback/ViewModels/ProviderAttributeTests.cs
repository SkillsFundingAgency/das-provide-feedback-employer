using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTests.EmployerProvideFeedback.ViewModels
{
    public class ProviderAttributeTests
    {
        [TestCase(false, false, 0)]
        [TestCase(true, false, 1)]
        [TestCase(false, true, -1)]
        public void Should_Generate_Score_Based_On_Attribute_Selection(bool isDoingWell, bool toImprove, int expectedScore)
        {
            // Arrange
            var pa = new ProviderAttributeModel
            {
                Good = isDoingWell,
                Bad = toImprove
            };

            // Assert
            pa.Score.Should().Be(expectedScore);
        }
    }
}
