using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableEntitySignalSample
{
    public static class Orchestration
    {
        [FunctionName("HttpTrigger")]
        public static async Task<IActionResult> Http(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [DurableClient] IDurableClient client)
        {
            string id = await client.StartNewAsync("Orchestration", null);
            return client.CreateCheckStatusResponse(req, id);
        }


        [FunctionName("Orchestration")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            
            EntityId id = new EntityId(nameof(Entity), "key");

            // Signal an entity
            context.SignalEntity(id, "MyOperation", "helloSignal");

            // Call an entity
            await context.CallEntityAsync(id, "MyOperation", "helloCall");
        }
    }

    public class Entity
    {
        private readonly ILogger _log;
        public Entity(ILogger log)
        {
            _log = log;
        }
        public int MyProperty { get; set; }
        public void MyOperation(string data)
        {
            _log.LogInformation(data);
        }

        [FunctionName(nameof(Entity))]
        public static async Task Run(
            [EntityTrigger] IDurableEntityContext context,
            ILogger log)
             => await context.DispatchAsync<Entity>(log);
    }
}