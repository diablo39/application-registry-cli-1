using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches.Implementations.ResultSenders
{
    class ResultToHttpEndpointBatch : IBatch
    {
        public async Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            
            Uri url = context.Arguments.Url;

            if (url != null)
            {
                HttpResponseMessage postResult = null;

                try
                {
                    var client = new HttpClient
                    {
                        BaseAddress = url
                    };

                    postResult = await client.PostAsJsonAsync("/api/v1/collector", context.BatchResult);

                    postResult.EnsureSuccessStatusCode();


                }
                catch (HttpRequestException ex)
                {
                    if (postResult != null)
                    {
                        var responseTest = await postResult.Content.ReadAsStringAsync();
                        "Exception during sending results over http to {0}{1}Content returned{2}{3}.".LogError(this, ex, url, Environment.NewLine, Environment.NewLine, responseTest);
                    }
                    else
                    {
                        "Exception during sending results over http to {0}.".LogError(this, ex, url);
                    }

                    return BatchExecutionResult.CreateFailResult();

                }
                catch (Exception ex)
                {
                    "Exception during sending results over http to {0}.".LogError(this, ex, url);
                    return BatchExecutionResult.CreateFailResult();
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
