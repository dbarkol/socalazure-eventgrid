using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Rest;
using Newtonsoft.Json;
using Demo.Models;

namespace Demo.PublishEvents
{
    internal class Program
    {
        #region Data Members

        private const string TopicHostName = "{topic-name}.westus2-1.eventgrid.azure.net";
        private const string TopicEndpoint = "{topic-endpoint}";
        private const string TopicKey = "{topic-key}";

        #endregion

        private static void Main(string[] args)
        {
            const string message = "Event Grid is super!";

            // Step 1: Get the text sentiment score
            var score = GetScore(message);
            Console.WriteLine("Score: {0}", score);
            Console.WriteLine("Sending message...");

            // Step 2: Instantiate the Feedback object
            var f = new Feedback
            {
                Id = Guid.NewGuid(),
                Score = score,
                Message = message
            };

            // Step 3: Send the custom event
            PublishWithSdk(f).Wait();
            //PublishWithHttpClient(f).Wait();
            //PublishCloudEvent(f).Wait();

            Console.WriteLine("Message sent");
            Console.ReadLine();
        }

        private static async Task PublishWithSdk(Feedback f)
        {
            // Step 1: Initialize credentials and client
            ServiceClientCredentials credentials = new TopicCredentials(TopicKey);
            var client = new EventGridClient(credentials);

            // Step 2: Populate list of events
            var events = new List<EventGridEvent>
            {
                new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    Data = f,
                    EventTime = DateTime.Now,
                    EventType = f.Score > 70 ? "Positive" : "Negative",
                    Subject = "eventgrid/demo/feedback",
                    DataVersion = "1.0"
                }
            };

            // Step 3: Publish
            await client.PublishEventsAsync(
                TopicHostName,
                events);
        }

        private static async Task PublishWithHttpClient(Feedback f)
        {
            // Step 1: Initialize HttpClient
            var client = new HttpClient { BaseAddress = new Uri(TopicEndpoint) };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Step 2: Add the topic key
            client.DefaultRequestHeaders.Add("aeg-sas-key", TopicKey);

            // Step 3: Create a list of events
            var events = new List<GridEvent<Feedback>>
            {
                new GridEvent<Feedback>()
                {
                    Data = f,
                    Subject = "eventgrid/demo/feedback",
                    EventType = f.Score > 70 ? "Positive" : "Negative",
                    EventTime = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString()
                }
            };

            // Serialize the data
            var json = JsonConvert.SerializeObject(events);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            // Publish grid event
            await client.PostAsync(string.Empty, stringContent);
        }

        private static async Task PublishCloudEvent(Feedback f)
        {
            // Event Grid must be configured to handle cloud events before publishing 
            // Reference: https://docs.microsoft.com/en-us/azure/event-grid/cloudevents-schema#configure-event-grid-for-cloudevents

            const string subscriptionId = "{subscription-id}";
            const string resourceGroupName = "{resource-group-name}";
            const string topicName = "{topic-name}";
            var topic = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}>/providers/Microsoft.EventGrid/topics/{topicName}";

            // Step 1: Initialize the client
            var client = new HttpClient { BaseAddress = new Uri(TopicEndpoint) };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("aeg-sas-key", TopicKey);

            // Step 2: Initialize the cloud event
            var cloudEvent = new CloudEvent<Feedback>
            {
                EventId = Guid.NewGuid().ToString(),
                EventType = "newFeedback",
                EventTypeVersion = "1.0",
                CloudEventVersion = "0.1",
                Data = f,
                Source = $"{topic}#subjectfeedback",
                EventTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
            };

            // Step 3: Send the event
            var json = JsonConvert.SerializeObject(cloudEvent);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await client.PostAsync(string.Empty, content);
        }

        private static int GetScore(string message)
        {
            ITextAnalyticsAPI client = new TextAnalyticsAPI(new ApiKeyServiceClientCredentials())
            {
                AzureRegion = AzureRegions.Westus2
            };

            var results = client.SentimentAsync(
                new MultiLanguageBatchInput(
                    new List<MultiLanguageInput>()
                    {
                        new MultiLanguageInput("en", "0", message)
                    })).Result;

            if (results.Documents.Count == 0) return 0;
            var score = results.Documents[0].Score.GetValueOrDefault();
            var fixedScore = (int)(score * 100);

            return fixedScore;
        }
    }

    internal class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private const string TextAnalyticsApi = "{api-key}";

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Ocp-Apim-Subscription-Key", TextAnalyticsApi);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
