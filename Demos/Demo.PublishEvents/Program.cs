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
    class Program
    {
        static void Main(string[] args)
        {
            
        }
    }

    internal class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private const string TextAnalyticsApi = "{{api-key}}";

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Ocp-Apim-Subscription-Key", TextAnalyticsApi);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
