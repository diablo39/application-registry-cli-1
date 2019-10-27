using ApplicationRegistry.Collector.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationRegistry.BackendHttpClient
{
    public class ServerClient
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;

        public ServerClient(HttpClient client, string baseUrl)
        {
            _client = client;
            _baseUrl = baseUrl;
        }

        public async Task ReportError (CollectorError error)
        {
            await _client.PostAsJsonAsync("/api/v1/Collector/Error", error);
        }
    }
}
