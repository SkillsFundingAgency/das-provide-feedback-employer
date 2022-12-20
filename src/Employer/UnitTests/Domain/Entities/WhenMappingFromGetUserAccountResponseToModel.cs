using AutoFixture.NUnit3;
using ESFA.DAS.ProvideFeedback.Domain.Entities.ApiTypes;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTests.Domain.Entities
{
    public class WhenMappingFromGetUserAccountResponseToModel
    {
        [Test, AutoData]
        public void Then_The_Fields_Are_Mapped(GetUserAccountsResponse source)
        {
            var actual = (EmployerUserAccounts) source;
            
            actual.Should().BeEquivalentTo(source);
        }

        [Test, AutoData]
        public void Then_If_No_Accounts_Then_Empty_List_Returned(GetUserAccountsResponse source)
        {
            source.UserAccounts = null;
            
            var actual = (EmployerUserAccounts) source;
            
            actual.Should().BeEquivalentTo(source, options=> options.Excluding(c=>c.UserAccounts));
            actual.UserAccounts.Should().BeEmpty();
        }

        [Test]
        public void Then_If_Null_Response_Then_Empty_Returned()
        {
            var actual = (EmployerUserAccounts) ((GetUserAccountsResponse)null);

            actual.Should().BeNull();
        }
    }
}