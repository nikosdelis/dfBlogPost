using System.Diagnostics;
using System.Net;
using System.Threading;
using DurableTask.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace dfBlogPost
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("StartHere")]
        public async Task<HttpResponseData> StartHere(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [DurableClient] DurableClientContext durableContext,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(StartHere));

            string instanceId = await durableContext.Client.ScheduleNewOrchestrationInstanceAsync(nameof(RunOrchestrator));
            logger.LogInformation("Step 1. Created new orchestration with instance ID = {instanceId}", instanceId);

            return durableContext.CreateCheckStatusResponse(req, instanceId);
        }


        [Function(nameof(RunOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var log = context.CreateReplaySafeLogger(_logger);

            log.LogInformation("Step 2. Running a couple of activities");

            await context.CallActivityAsync(nameof(RunActivity), "John");
            await context.CallActivityAsync(nameof(RunActivity), "Jim");
            await context.CallActivityAsync(nameof(RunActivity), "Joe");
        }

        [Function(nameof(RunActivity))]
        public async Task RunActivity([ActivityTrigger] string input)
        {
            _logger.LogInformation("Hello " + input);

            await Task.Delay(100);
        }
    }
}
