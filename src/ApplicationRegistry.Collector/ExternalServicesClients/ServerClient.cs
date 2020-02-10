using ApplicationRegistry.Collector.Model;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApplicationRegistry.BackendHttpClient
{
    public class ServerClient
    {
        private readonly HttpClient _client;

        public ServerClient(HttpClient client)
        {
            _client = client;
        }

        public async Task ReportError(CollectorError error)
        {
            using (var response = await _client.PostAsJsonAsync("/api/v1/Collector/Error", error))
            {

            }
        }

        public async Task<bool> SendCollectedDataAsync(ApplicationInfo applicationInfo)
        {
            const string RequestUri = "/api/v1/collector";

            using (var postResult = await _client.PostAsJsonAsync(RequestUri, applicationInfo))
            {
                try
                {
                    postResult.EnsureSuccessStatusCode();
                    return true;
                }
                catch (HttpRequestException ex)
                {
                    var url = Path.Combine(_client.BaseAddress?.ToString(), RequestUri);
                    if (postResult != null)
                    {
                        var responseTest = await postResult.Content.ReadAsStringAsync();
                        "Exception during sending results over http to: {0}{1}Content returned:{2}{3}.".LogError(this, ex, RequestUri, Environment.NewLine, Environment.NewLine, responseTest);
                    }
                    else
                    {
                        "Exception during sending results over http to {0}.".LogError(this, ex, RequestUri);
                    }

                    return false;
                }
            }
        }
    }
}
