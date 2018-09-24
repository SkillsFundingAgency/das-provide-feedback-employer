// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypedDocument.cs" company="Sohara Design Ltd">
//  This is based on the work of Adam Hockemeyer:
//   https://github.com/adamhockemeyer/Azure-Functions---CosmosDB-ResourceToken-Broker/blob/master/CosmosDBResourceTokenBroker.Shared/CosmosDBRepository.cs
// </copyright>
// <summary>
//   <see cref="TypedDocument{T}" /> inherits from CosmosDB <see cref="Document" /> and provides a <see cref="Type" />
//   property
//   to assist with queries of a specific document type <see cref="T" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.DAS.EmployerProvideFeedback.Api.Repository
{
    using Newtonsoft.Json;

    using Document = Microsoft.Azure.Documents.Document;

    /// <summary>
    ///     <see cref="TypedDocument{T}" /> inherits from CosmosDB <see cref="Document" /> and provides a <see cref="Type" />
    ///     property
    ///     to assist with queries of a specific document type <see cref="T" />.
    /// </summary>
    /// <typeparam name="T"> type of document </typeparam>
    public abstract class TypedDocument<T> : Document
        where T : class
    {
        /// <summary>
        ///     Gets or sets the partition Key
        /// </summary>
        [JsonProperty(PropertyName = "_pk")]
        public string PartitionKey { get; set; }

        /// <summary>
        ///     Type of the class or document that is saved to CosmosDB.
        ///     Useful for querying by document of type <see cref="T" />.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type => typeof(T).Name;
    }
}