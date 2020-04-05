using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace SearchFunction
{
    public static class Search
    {
        private static SearchIndexClient indexClient;

        [FunctionName("Search")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] string req,
            ILogger log)
        {
            var requestData = JsonConvert.DeserializeObject<request>(req);

            if (requestData.text == "*")
            {
                return new BadRequestResult();
            }
            else
            {
                requestData.text.Replace("*", " ");
            }

            log.LogInformation("C# HTTP trigger function processed a request.");

            string searchServiceName = Environment.GetEnvironmentVariable("SearchUrl");
            string adminApiKey = Environment.GetEnvironmentVariable("SearchKey");

            string indexName = "cosmosdb-index";

            SearchParameters parameters = new SearchParameters() 
            {
                SearchFields = new[] { "Content" },
                Filter = $"AmountOfVotes ge {Environment.GetEnvironmentVariable("MinimumVotes")}",
                Select = new[] { "ApprovedByModerator", "Votes", "AmountOfVotes", "Content"}
            };

            indexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(adminApiKey));

            var result = await indexClient.Documents.SearchAsync(requestData.text, parameters);
    
            return new OkObjectResult(result.Results);
        }

        [FunctionName("SearchTwitter")]
        public static async Task<IActionResult> RunTwitter(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] string req,
            ILogger log)
        {
            var requestData = JsonConvert.DeserializeObject<request>(req);
            log.LogInformation("C# HTTP trigger function processed a request.");

            string searchServiceName = Environment.GetEnvironmentVariable("SearchUrl");
            string adminApiKey = Environment.GetEnvironmentVariable("SearchKey");

            string indexName = "twitter-feed";

            indexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(adminApiKey));

            SearchParameters parameters = new SearchParameters()
            {
                SearchFields = new[] { "text" }
            };

            var result = await indexClient.Documents.SearchAsync(requestData.text, parameters);

            return new OkObjectResult(result);
        }
    }
}
