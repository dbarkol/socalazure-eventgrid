using System;
using Newtonsoft.Json;

namespace Demo.Models
{

    public class GridEvent<T> where T : class
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("eventID")]
        public string Subject { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("eventTime")]
        public DateTime EventTime { get; set; }
    }
}
