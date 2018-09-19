// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypedCosmosDbRepository.cs" company="Sohara Design Ltd">
//  This is based on the work of Adam Hockemeyer:
//   https://github.com/adamhockemeyer/Azure-Functions---CosmosDB-ResourceToken-Broker/blob/master/CosmosDBResourceTokenBroker.Shared/CosmosDBRepository.cs
// </copyright>
// <summary>
//   The cosmos db repository.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.DAS.EmployerProvideFeedback.Api.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using ESFA.DAS.EmployerProvideFeedback.Api.Dto;

    using Microsoft.Azure.Documents.Client;

    public class TypedCosmosDbRepository<TInstance, TEntity> : CosmosDbRepository, IDataRepository<TEntity>
        where TInstance : class, IDataRepository where TEntity : TypedDocument<TEntity>
    {
        private static readonly Lazy<TInstance> LazyInstance =
            new Lazy<TInstance>(Activator.CreateInstance<TInstance>);

        public Task<IEnumerable<TEntity>> GetAllItemsAsync(FeedOptions feedOptions = null)
        {
            return base.GetAllItemsAsync<TEntity>(feedOptions);
        }

        public Task<TEntity> GetItemAsync(Expression<Func<TEntity, bool>> predicate, FeedOptions feedOptions = null)
        {
            return base.GetItemAsync(predicate, feedOptions);
        }

        public Task<TEntity> GetItemAsync(string documentId, RequestOptions requestOptions = null)
        {
            return base.GetItemAsync<TEntity>(documentId, requestOptions);
        }

        public Task<IEnumerable<TEntity>> GetItemsAsync(Expression<Func<TEntity, bool>> predicate, FeedOptions feedOptions = null)
        {
            return base.GetItemsAsync(predicate, feedOptions);
        }

        public Task<bool> RemoveItemAsync(string documentId, RequestOptions requestOptions = null)
        {
            return base.RemoveItemAsync<TEntity>(documentId, requestOptions);
        }

        public Task<bool> RemoveItemAsync(TEntity document, RequestOptions requestOptions = null)
        {
            return base.RemoveItemAsync(document, requestOptions);
        }

        public Task<bool> RemoveItemsAsync(IEnumerable<TEntity> documents, RequestOptions requestOptions = null)
        {
            return base.RemoveItemsAsync(documents, requestOptions);
        }

        public Task<TEntity> UpsertItemAsync(TEntity document, RequestOptions requestOptions = null)
        {
            return base.UpsertItemAsync(document, requestOptions);
        }

        public new TInstance WithAuthKeyOrResourceToken(string keyOrToken)
        {
            return base.WithAuthKeyOrResourceToken(keyOrToken) as TInstance;
        }

        public new TInstance UsingCollection(string collectionName)
        {
            return base.UsingCollection(collectionName) as TInstance;
        }

        public new TInstance WithConnectionString(string connectionString)
        {
            return base.WithConnectionString(connectionString) as TInstance;
        }

        public new TInstance UsingDatabase(string databaseName)
        {
            return base.UsingDatabase(databaseName) as TInstance;
        }

        public new TInstance ConnectTo(string endpoint)
        {
            return base.ConnectTo(endpoint) as TInstance;
        }

        public new TInstance WithPartitionKey(string pk)
        {
            return base.WithPartitionKey(pk) as TInstance;
        }

        public static new TInstance Instance => LazyInstance.Value;
    }

    public interface IEmployerFeedbackRepository : IDataRepository<EmployerFeedback>
    {
    }

    public class CosmosEmployerFeedbackRepository : TypedCosmosDbRepository<CosmosEmployerFeedbackRepository, EmployerFeedback>, IEmployerFeedbackRepository
    {
    }

    public interface ITokenProvider : IDataRepository<ApiAccount>
    {
    }

    public class CosmosTokenStore : TypedCosmosDbRepository<CosmosTokenStore, ApiAccount>, ITokenProvider
    {
    }
}