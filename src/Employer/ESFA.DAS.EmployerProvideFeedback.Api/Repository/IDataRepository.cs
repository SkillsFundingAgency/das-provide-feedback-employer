// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDataRepository.cs" company="Sohara Design Ltd">
//  This is based on the work of Adam Hockemeyer:
//   https://github.com/adamhockemeyer/Azure-Functions---CosmosDB-ResourceToken-Broker/blob/master/CosmosDBResourceTokenBroker.Shared/CosmosDBRepository.cs
// </copyright>
// <summary>
//   The DataRepository interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.DAS.EmployerProvideFeedback.Api.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Microsoft.Azure.Documents.Client;

    public interface IDataRepository
    {
        /// <summary>
        ///     Get all documents of type <see cref="T"/> from the data store
        /// </summary>
        /// <typeparam name="T">The type of documents to retrieve</typeparam>
        /// <param name="feedOptions">the <see cref="FeedOptions"/></param>
        /// <returns>An <see cref="IEnumerable{T}"/> collection of objects</returns>
        Task<IEnumerable<T>> GetAllItemsAsync<T>(FeedOptions feedOptions = null)
            where T : TypedDocument<T>;

        /// <summary>
        ///     Get a single document of type <see cref="T"/> from the data store, using a predicate
        /// </summary>
        /// <typeparam name="T">The type of document to retrieve</typeparam>
        /// <param name="predicate"> The expression to use for obtaining the item</param>
        /// <param name="feedOptions"> the <see cref="FeedOptions"/> </param>
        /// <returns>An object of type <see cref="T"/></returns>
        Task<T> GetItemAsync<T>(Expression<Func<T, bool>> predicate, FeedOptions feedOptions = null)
            where T : TypedDocument<T>;

        /// <summary>
        ///     Get a single item of type <see cref="T"/> from the data store, using the documentId
        /// </summary>
        /// <typeparam name="T">The type of document to retrieve</typeparam>
        /// <param name="documentId"> The ID of the document to return</param>
        /// <param name="requestOptions"> the <see cref="RequestOptions"/> </param>
        /// <returns>An object of type <see cref="T"/></returns>
        Task<T> GetItemAsync<T>(string documentId, RequestOptions requestOptions = null)
            where T : TypedDocument<T>;

        /// <summary>
        ///     Get all documents of type <see cref="T"/> from the data store that match a given predicate
        /// </summary>
        /// <typeparam name="T">The type of documents to retrieve</typeparam>
        /// <param name="predicate"> The expression to use for obtaining the items</param>
        /// <param name="feedOptions"> the <see cref="FeedOptions"/> </param>
        /// <returns>An <see cref="IEnumerable{T}"/> collection of objects</returns>
        Task<IEnumerable<T>> GetItemsAsync<T>(Expression<Func<T, bool>> predicate, FeedOptions feedOptions = null)
            where T : TypedDocument<T>;

        /// <summary>
        ///     Remove a single document of type <see cref="T"/> from the data store, using the documentId
        /// </summary>
        /// <typeparam name="T">The type of document to remove</typeparam>
        /// <param name="documentId"> The ID of the document to return</param>
        /// <param name="requestOptions"> the <see cref="RequestOptions"/> </param>
        /// <returns> A boolean value indicating success or failure </returns>
        Task<bool> RemoveItemAsync<T>(string documentId, RequestOptions requestOptions = null)
            where T : TypedDocument<T>;

        /// <summary>
        ///     Remove a single document of type <see cref="T"/> from the data store, using the document <see cref="T"/>
        /// </summary>
        /// <typeparam name="T">The type of document to remove</typeparam>
        /// <param name="document"> The document of type <see cref="T"/> to remove </param>
        /// <param name="requestOptions"> the <see cref="RequestOptions"/> </param>
        /// <returns> A boolean value indicating success or failure </returns>
        Task<bool> RemoveItemAsync<T>(T document, RequestOptions requestOptions = null)
            where T : TypedDocument<T>;

        /// <summary>
        ///     Remove a collection of documents of type <see cref="T"/> from the data store that are present in a given document list of type <see cref="T"/>
        /// </summary>
        /// <typeparam name="T">The type of documents to remove</typeparam>
        /// <param name="documents"> The collection of documents of type <see cref="T"/> to remove </param>
        /// <param name="requestOptions"> the <see cref="RequestOptions"/> </param>
        /// <returns> A boolean value indicating success or failure </returns>
        Task<bool> RemoveItemsAsync<T>(IEnumerable<T> documents, RequestOptions requestOptions = null)
            where T : TypedDocument<T>;

        /// <summary>
        ///     Creates or updates a single document of type <see cref="T"/> from the data store, using the document <see cref="T"/>
        /// </summary>
        /// <typeparam name="T">The type of document to remove</typeparam>
        /// <param name="document"> The document of type <see cref="T"/> to create/update </param>
        /// <param name="requestOptions"> the <see cref="RequestOptions"/> </param>
        /// <returns>An updated object of type <see cref="T"/></returns>
        Task<T> UpsertItemAsync<T>(T document, RequestOptions requestOptions = null)
            where T : TypedDocument<T>;
    }

    /// <summary>
    /// The DataRepository interface.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The default entity type to work with
    /// </typeparam>
    public interface IDataRepository<TEntity> : IDataRepository
        where TEntity : TypedDocument<TEntity>
    {
        /// <summary>
        ///     Get all documents of type <see cref="TEntity"/> from the data store
        /// </summary>
        /// <param name="feedOptions">the <see cref="FeedOptions"/></param>
        /// <returns>An <see cref="IEnumerable{TEntity}"/> collection of objects</returns>
        Task<IEnumerable<TEntity>> GetAllItemsAsync(FeedOptions feedOptions = null);

        /// <summary>
        ///     Get a single document of type <see cref="TEntity"/> from the data store, using a predicate
        /// </summary>
        /// <param name="predicate"> The expression to use for obtaining the item</param>
        /// <param name="feedOptions"> the <see cref="FeedOptions"/> </param>
        /// <returns>An object of type <see cref="TEntity"/></returns>
        Task<TEntity> GetItemAsync(Expression<Func<TEntity, bool>> predicate, FeedOptions feedOptions = null);

        /// <summary>
        ///     Get a single item of type <see cref="TEntity"/> from the data store, using the documentId
        /// </summary>
        /// <param name="documentId"> The ID of the document to return</param>
        /// <param name="requestOptions"> the <see cref="RequestOptions"/> </param>
        /// <returns>An object of type <see cref="TEntity"/></returns>
        Task<TEntity> GetItemAsync(string documentId, RequestOptions requestOptions = null);

        /// <summary>
        ///     Get all documents of type <see cref="TEntity"/> from the data store that match a given predicate
        /// </summary>
        /// <param name="predicate"> The expression to use for obtaining the items</param>
        /// <param name="feedOptions"> the <see cref="FeedOptions"/> </param>
        /// <returns>An <see cref="IEnumerable{T}"/> collection of objects</returns>
        Task<IEnumerable<TEntity>> GetItemsAsync(Expression<Func<TEntity, bool>> predicate, FeedOptions feedOptions = null);

        /// <summary>
        ///     Remove a single document of type <see cref="TEntity"/> from the data store, using the documentId
        /// </summary>
        /// <param name="documentId"> The ID of the document to return</param>
        /// <param name="requestOptions"> the <see cref="RequestOptions"/> </param>
        /// <returns> A boolean value indicating success or failure </returns>
        Task<bool> RemoveItemAsync(string documentId, RequestOptions requestOptions = null);

        /// <summary>
        ///     Remove a single document of type <see cref="TEntity"/> from the data store, using the document <see cref="TEntity"/>
        /// </summary>
        /// <param name="document"> The document of type <see cref="TEntity"/> to remove </param>
        /// <param name="requestOptions"> the <see cref="RequestOptions"/> </param>
        /// <returns> A boolean value indicating success or failure </returns>
        Task<bool> RemoveItemAsync(TEntity document, RequestOptions requestOptions = null);

        /// <summary>
        ///     Remove a collection of documents of type <see cref="TEntity"/> from the data store that are present in a given document list of type <see cref="TEntity"/>
        /// </summary>
        /// <param name="documents"> The collection of documents of type <see cref="TEntity"/> to remove </param>
        /// <param name="requestOptions"> the <see cref="RequestOptions"/> </param>
        /// <returns> A boolean value indicating success or failure </returns>
        Task<bool> RemoveItemsAsync(IEnumerable<TEntity> documents, RequestOptions requestOptions = null);

        /// <summary>
        ///     Creates or updates a single document of type <see cref="TEntity"/> from the data store, using the document <see cref="TEntity"/>
        /// </summary>
        /// <param name="document"> The document of type <see cref="TEntity"/> to create/update </param>
        /// <param name="requestOptions"> the <see cref="RequestOptions"/> </param>
        /// <returns>An updated object of type <see cref="TEntity"/></returns>
        Task<TEntity> UpsertItemAsync(TEntity document, RequestOptions requestOptions = null);
    }
}