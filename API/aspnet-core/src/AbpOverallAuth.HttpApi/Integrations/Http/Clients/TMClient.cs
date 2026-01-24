using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AbpOverallAuth.Integrations.Http.Clients
{
    /// <summary>
    /// 示例外部API客户端
    /// </summary>
    public class TMClient
    {
        private readonly HttpClient _httpClient;

        public TMClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetDataAsync(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> PostDataAsync(string endpoint, HttpContent content)
        {
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
