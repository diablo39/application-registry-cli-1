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

        //public async Task ReportError(CollectorError error)
        //{
            
        //    using (var response = await _client.PostAsync("/api/v1/Collector/Error", new StringContent(JsonConvert.SerializeObject(error))))
        //    {

        //    }
        //}

        public async Task<bool> SendCollectedDataAsync(ApplicationInfo applicationInfo)
        {
            const string RequestUri = "/api/v1/collector";
            var url = Path.Combine(_client.BaseAddress?.ToString(), RequestUri);
            HttpResponseMessage postResult = null;
            var responseTest = "";
            try
            {
                var _requestContent = JsonConvert.SerializeObject(applicationInfo);
                var _httpRequestContent = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequestContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

                postResult = await _client.PostAsync(RequestUri, _httpRequestContent);

                responseTest = await postResult.Content.ReadAsStringAsync();

                ("Htpsend status code: " + postResult.StatusCode).LogInfo(this);
                responseTest.LogInfo(this);

                postResult.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                
                if (postResult != null)
                {
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
