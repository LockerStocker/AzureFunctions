using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Mvc;





namespace CounterFunctions
{
    public static class LockerFunc
    {
        /*
        [FunctionName("MessageReciver")]
        public static async Task Run([EventHubTrigger("sketeloneventhub", Connection = "receiverConnectionString")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {

                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    // Replace these two lines with your processing logic.
                    string response = await call_update_counter();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }*/


        [FunctionName("LockerFunc")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "LockerFunc/{action}/{id}/{user_key}")] 
            HttpRequestMessage request, ILogger log, string action, string id, string user_key) { 
            return await call_action(log, action, id, user_key);
        }


        public static async Task<HttpResponseMessage> call_action(ILogger log, string action, string id, string user_key)
        {
            var locker = new Locker();
            locker.Id = int.Parse(id);
            locker.available = true;
            locker.locked = true;
            if (action == "set-occupy")
            {
                locker.user_key = user_key;
            }
            if (action == "set-user-key")
            {
                action = action + "/" + user_key;
            }

            if ((action == "get-locker")||(action == "get-user-key"))
            {
                action = action + "/" + id;
            }
            //var BaseUri = "https://counterfunctions312546526.azurewebsites.net/api/update-counter";
            //var BaseUri = "http://localhost:7071/api/set-occupy";
            //BaseUri = "http://localhost:7071/api/set-available";

            var BaseUri = "http://localhost:7071/api/";
            var FuncUri = BaseUri + action;

            log.LogInformation("Triggering: " + FuncUri);
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, FuncUri))
            {
                var json = JsonConvert.SerializeObject(locker);
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    var response = await client.PostAsync(FuncUri, stringContent);
                    var contents = await response.Content.ReadAsStringAsync();
                    return response;
                }
            }
        }

    }
}






