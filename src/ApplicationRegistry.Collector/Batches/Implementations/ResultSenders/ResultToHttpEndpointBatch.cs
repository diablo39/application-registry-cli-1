using System;
using System.Threading.Tasks;
using ApplicationRegistry.BackendHttpClient;

namespace ApplicationRegistry.Collector.Batches.Implementations.ResultSenders
{
    class ResultToHttpEndpointBatch : IBatch
    {
        private ServerClient _client;

        public ResultToHttpEndpointBatch(ServerClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            
            Uri url = context.Arguments.Url;

            if (url != null)
            {
                try
                {
                   var sendingResult = await _client.SendCollectedDataAsync(context.BatchResult);

                    if (sendingResult)
                        return BatchExecutionResult.CreateSuccessResult();

                    return BatchExecutionResult.CreateFailResult();
                }
                catch (Exception ex)
                {
                    "Exception during sending results over http to {0}.".LogError(this, ex, url);
                    return BatchExecutionResult.CreateFailResult();
                }
                finally
                {
                    _client = null; 
                }

            }
            else
            {
                "Url not provided. Skipping sending result oveer http".LogInfo(this);
            }

            return BatchExecutionResult.CreateSuccessResult();
        }
    }
}


//try
//{
//    await RunCollectorAsync(serviceProvider);
//}
//catch (Exception ex)
//{
//    logger.LogCritical(ex, "Execution failed");

//    if (Url != null)
//    {
//        var client = new HttpClient();
//        client.BaseAddress = Url;

//        await client.PostAsJsonAsync("/api/v1/Collector/Error", new CollectorError
//        {
//            ApplicationCode = Applicatnion,
//            ErrorMessage = ex.Message + System.Environment.NewLine + ex.StackTrace,
//            IdEnvironment = Environment,
//            Version = Version
//        });
//    }
//}
