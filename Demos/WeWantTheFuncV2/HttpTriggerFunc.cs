
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace WeWantTheFuncV2
{
    /// <summary>
    /// HTTP triggered Function from Event Grid
    /// </summary>
    public static class HttpTriggerFunc
    {
        [FunctionName("HttpTriggerFunc")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "eventsDatabase",
                collectionName: "eventsCollection",
                ConnectionStringSetting = "CosmosConnectionString")]
                IAsyncCollector<string> documents,
            TraceWriter log)
        {
            log.Info("HttpTriggerFunc triggered");
            var requestBody = new StreamReader(req.Body).ReadToEnd();

            // Check the header for the event type.            
            if (!req.Headers.TryGetValue("Aeg-Event-Type", out var headerValues))
                return new BadRequestObjectResult("Not a valid request");

            var eventTypeHeaderValue = headerValues.FirstOrDefault();
            if (eventTypeHeaderValue == "SubscriptionValidation")
            {
                var events = JsonConvert.DeserializeObject<EventGridEvent[]>(requestBody);
                dynamic data = events[0].Data;
                var validationCode = data["validationCode"];
                return new JsonResult(new
                {
                    validationResponse = validationCode
                });
            }
            else if (eventTypeHeaderValue == "Notification")
            {
                documents.AddAsync(requestBody.ToString());
                log.Info(requestBody);
                return new OkObjectResult("");
            }

            return new BadRequestObjectResult("Not a valid request");
        }
    }

}
