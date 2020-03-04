using ApplicationRegistry.Collector.Model;
using Newtonsoft.Json;
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
            
            using (var response = await _client.PostAsync("/api/v1/Collector/Error", new StringContent(JsonConvert.SerializeObject(error))))
            {

            }
        }

        public async Task<bool> SendCollectedDataAsync(ApplicationInfo applicationInfo)
        {
            const string RequestUri = "/api/v1/collector";
            var url = Path.Combine(_client.BaseAddress?.ToString(), RequestUri);
            HttpResponseMessage postResult = null;

            try
            {
                postResult = await _client.PostAsync(RequestUri, new StringContent(JsonConvert.SerializeObject(applicationInfo)));
                postResult.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                
                if (postResult != null)
                {
                    var responseTest = await postResult.Content.ReadAsStringAsync();
                    "Exception during sending results over http to: {0}{1}Content returned:{2}{3}.".LogError(this, ex, url, Environment.NewLine, Environment.NewLine, responseTest);
                }
                else
                {
                    "Exception during sending results over http to {0}.".LogError(this, ex, url);
                }

                return false;
            }
            catch (Exception ex)
            {
                "Exception during sending results over http to {0}.".LogError(this, ex, url);
                return false;
            }

        }
    }
}
