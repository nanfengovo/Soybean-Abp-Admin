using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AbpOverallAuth.ThirdParty.Base
{
    public class HttpExecutor : IHttpExecutor
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;
        private readonly ILogger<HttpExecutor> _logger;

        public HttpExecutor(IHttpClientFactory factory,
                            IConfiguration config,
                            ILogger<HttpExecutor> logger)
        {
            _factory = factory;
            _config = config;
            _logger = logger;
        }

        private HttpClient Create(string system)
        {
            var client = _factory.CreateClient(system);
            //var token = _config[$"ThirdParty:{system}:Token"];

            //if (!string.IsNullOrEmpty(token))
            //    client.DefaultRequestHeaders.Authorization =
            //        new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        public async Task<HttpResult<T>> PostAsync<T>(string system, string url, object body)
        {
            var client = Create(system);
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PostAsync(url, content);
            return await BuildResult<T>(res);
        }

        public async Task<HttpResult<T>> GetAsync<T>(string system, string url)
        {
            var client = Create(system);
            var res = await client.GetAsync(url);
            return await BuildResult<T>(res);
        }

        public async Task<HttpResult<T>> PutAsync<T>(string system, string url, object body)
        {
            var client = Create(system);
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PutAsync(url, content);
            return await BuildResult<T>(res);
        }

        public async Task<HttpResult<T>> DeleteAsync<T>(string system, string url)
        {
            var client = Create(system);
            var res = await client.DeleteAsync(url);
            return await BuildResult<T>(res);
        }

        private async Task<HttpResult<T>> BuildResult<T>(HttpResponseMessage res)
        {
            var raw = await res.Content.ReadAsStringAsync();

            return new HttpResult<T>
            {
                Success = res.IsSuccessStatusCode,
                StatusCode = (int)res.StatusCode,
                Raw = raw,
                Data = res.IsSuccessStatusCode
                    ? JsonSerializer.Deserialize<T>(raw)
                    : default,
                Error = res.IsSuccessStatusCode ? null : raw
            };
        }
    }

}
