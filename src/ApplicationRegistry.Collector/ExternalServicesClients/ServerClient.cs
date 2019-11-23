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

        public ServerClient(HttpClient client)
        {
            _client = client;
        }

        public async Task ReportError (CollectorError error)
        {
            using (var response = await _client.PostAsJsonAsync("/api/v1/Collector/Error", error)) { }
        }
    }
}
