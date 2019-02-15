using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace CosmosApi.Services
{
    public class DataQueryService : IDataQueryServices
    {

        //string endpointUrl = ConfigurationManager.AppSettings["CosmosEndpointUrl"];
        //string pk = ConfigurationManager.AppSettings["CosmosPk"];
        //string databaseName = ConfigurationManager.AppSettings["CosmosDatabase"];
        //string collectionName = ConfigurationManager.AppSettings["CosmosCollection"];

        //private string endpointUrl = "https://rx-test-cosmos.documents.azure.com:443/";
        //private string pk = "GO8qrXu6gL30UgpRluG0F7oF48Zba7I7lvQ2E1q8NFUHpsoD6VEmFoK99uaDiTCWRg9BdrEH3kSpW5LqEdRleg==";
        //private string databaseName = "DeviceInfo";
        //private string collectionName = "WebCrawler";

        private string _datetimeFormat = "yyyyMMdd";
        private readonly IConfiguration _configuration;
        private readonly string _dataToFetch = "c.DrawDate, c.N1, c.N2, c.N3, c.N4, c.N5, c.N6, c.NExtra, c.Year, c.Month, c.Day";

        public DataQueryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<dynamic> QueryCosmosData()
        {
            var client = new DocumentClient(new Uri(_configuration.GetSection("CosmosEndpointUrl").Value), _configuration.GetSection("CosmosPk").Value);
            var database = client?.CreateDatabaseQuery().Where(db => db.Id == _configuration.GetSection("CosmosDatabase").Value).ToArray().FirstOrDefault();
            if (database == null) return null;

            var collection = client.CreateDocumentCollectionQuery(database.SelfLink).Where(c => c.Id == _configuration.GetSection("CosmosCollection").Value)
                .ToArray().FirstOrDefault();
            if (collection == null) return null;

            string queryString = $"SELECT top 1 {_dataToFetch} FROM c order by c.DrawDateEpoch desc";
            SqlQuerySpec query = new SqlQuerySpec(queryString);

            var documentQuery = client.CreateDocumentQuery<dynamic>(
                    collection.SelfLink, query,
                    new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1 })
                .AsDocumentQuery();

            var results = new List<dynamic>();
            while (documentQuery.HasMoreResults)
            {
                var response = documentQuery.ExecuteNextAsync<dynamic>().Result;
                results.AddRange(response);
            }

            results = results.ToList();

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawDate">drawDate in format yyyy-MM-dd</param>
        /// <returns></returns>
        public dynamic QueryCosmosData(string drawDate)
        {
            //TODO: to validate drawDate

            var client = new DocumentClient(new Uri(_configuration.GetSection("CosmosEndpointUrl").Value),
                _configuration.GetSection("CosmosPk").Value);
            var database = client?.CreateDatabaseQuery()
                .Where(db => db.Id == _configuration.GetSection("CosmosDatabase").Value).ToArray().FirstOrDefault();
            if (database == null) return null;

            var collection = client.CreateDocumentCollectionQuery(database.SelfLink)
                .Where(c => c.Id == _configuration.GetSection("CosmosCollection").Value)
                .ToArray().FirstOrDefault();
            if (collection == null) return null;

            string queryString =
                $"SELECT top 1 {_dataToFetch} FROM c where c.DrawDate = '{drawDate}T00:00:00.0000000Z'";
            SqlQuerySpec query = new SqlQuerySpec(queryString);

            var documentQuery = client.CreateDocumentQuery<dynamic>(
                    collection.SelfLink, query,
                    new FeedOptions
                    {
                        MaxItemCount = -1,
                        EnableCrossPartitionQuery = true,
                        MaxDegreeOfParallelism = -1
                    })
                .AsDocumentQuery();

            var results = new List<dynamic>();
            while (documentQuery.HasMoreResults)
            {
                var response = documentQuery.ExecuteNextAsync<dynamic>().Result;
                results.AddRange(response);
            }

            if (results?.Count > 0)
            {
                results = results.ToList();
                return results;
            }
            else
            {
                return null;
            }
        }
    }
}
