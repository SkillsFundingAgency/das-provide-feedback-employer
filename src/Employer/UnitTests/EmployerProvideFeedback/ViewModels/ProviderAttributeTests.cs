using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Xunit;

namespace UnitTests.EmployerProvideFeedback.ViewModels
{
    public class ProviderAttributeTests
    {
        [Theory]
        [InlineData(false, false, 0)]
        [InlineData(true, false, 1)]
        [InlineData(false, true, -1)]
        public void Should_Generate_Score_Based_On_Attribute_Selection(bool isDoingWell, bool toImprove, int expectedScore)
        {
            // Arrange
            var pa = new ProviderAttribute();

            // Act
            pa.IsDoingWell = isDoingWell;
            pa.IsToImprove = toImprove;

            // Assert
            Assert.Equal(expectedScore, pa.Score);
        }
    }
}
