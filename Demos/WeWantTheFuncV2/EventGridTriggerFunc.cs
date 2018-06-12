// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGridExtensionConfig?functionName={functionname}

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WeWantTheFuncV2
{
    public static class EventGridTriggerFunc
    {
        [FunctionName("EventGridTriggerFunc")]
        public static void Run(
            [EventGridTrigger]EventGridEvent eventGridEvent,
            [CosmosDB(
                databaseName: "eventsDatabase",
                collectionName: "eventsCollection",
                ConnectionStringSetting = "CosmosConnectionString")]out dynamic document,
            TraceWriter log)
        {
            log.Info("EventGridTriggerFunc triggered");
            document = eventGridEvent.Data.ToString();
            log.Info(eventGridEvent.Data.ToString());
        }
    }
}
