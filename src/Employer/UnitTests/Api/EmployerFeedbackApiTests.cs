using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DAS.EmployerProvideFeedback.Api.Controllers;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.FeedbackDataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;
using Xunit.Sdk;

namespace UnitTests.Api
{
    public class FeedbackControllerTests
    {
        private readonly EmployerFeedbackTestHelper _testHelper;
        private readonly EmployerFeedbackTestContext _context;
        private readonly FeedbackController _controller;
        
        public FeedbackControllerTests()
        {
            _testHelper = new EmployerFeedbackTestHelper();
            _context = new InMemoryEmployerFeedbackTestContext().Context();

            if (EnumerableExtensions.Any(_context.EmployerFeedback)) return;
            for (var i = 0; i < 1000; i++)
            {
                _context.EmployerFeedback.Add(_testHelper.GenerateRandomFeedback());
            }
            _context.SaveChanges();

            _controller = new FeedbackController(_context);
        }

        ~FeedbackControllerTests()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller.Dispose();
        }

        public class GetAll : FeedbackControllerTests
        {
            [Fact]
            public void Should_Return_All_Items()
            {
                // arrange
                var items = _context.EmployerFeedback.ToList();
                var expected = items.Count;

                // act
                var result = _controller.GetAll();
                var actual = result.Value.Count;

                // assert
                Assert.Equal(expected, actual);
            }
        }
        public class GetById : FeedbackControllerTests
        {
            public GetById() : base()
            {
                _context.Database.EnsureDeleted();
                _context.Database.EnsureCreated();

                _context.EmployerFeedback.Add(_testHelper.AddedFeedback());
                _context.SaveChanges();
            }

            [Fact]
            public void Should_Return_NotFound_If_Not_Present()
            {
                // arrange
                var expected = typeof(NotFoundResult);

                // act
                var result = _controller.GetById(Guid.NewGuid()).Result;
                var actual = result.GetType();

                // assert
                Assert.Equal(expected, actual);
            }

            [Fact]
            public void Should_Return_The_Correct_Item()
            {
                // arrange
                var expected = _testHelper.AddedFeedback();

                // act
                var result = _controller.GetById(expected.Id);
                var actual = result.Value;

                // assert
                Assert.Equal(expected.Id, actual.Id);
                Assert.Equal(expected.Ukprn, actual.Ukprn);
                Assert.Equal(expected.AccountId, actual.AccountId);
                Assert.Equal(expected.UserRef, actual.UserRef);
                Assert.Equal(expected.DateTimeCompleted, actual.DateTimeCompleted);
                Assert.Equal(expected.ProviderAttributes.Count, actual.ProviderAttributes.Count);
                Assert.Equal(expected.ProviderRating, actual.ProviderRating);
            }
        }
    }

    //public class InMemoryDataContext<TContext> where TContext : DbContext
    //{
    //    private readonly DbContextOptions<TContext> _options;

    //    public InMemoryDataContext()
    //    {
    //        _options = CreateOptions();
    //    }

    //    public DbContextOptions<TContext> CreateOptions()
    //    {
    //        return new DbContextOptionsBuilder<TContext>()
    //            .UseInMemoryDatabase(databaseName: nameof(TContext)).Options;
    //    }

    //    public TContext CreateContext()
    //    {
    //        return new DbContext(_options) as TContext;
    //    }
    //}

    public class InMemoryEmployerFeedbackTestContext
    {
        private readonly DbContextOptions<EmployerFeedbackTestContext> options;

        public InMemoryEmployerFeedbackTestContext()
        {
            options = CreateOptions();
        }

        public DbContextOptions<EmployerFeedbackTestContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<EmployerFeedbackTestContext>()
                .UseInMemoryDatabase(databaseName: nameof(EmployerFeedbackTestContext)).Options;
        }

        public EmployerFeedbackTestContext Context()
        {
            var context = new EmployerFeedbackTestContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return context;;
        }
    }
}
