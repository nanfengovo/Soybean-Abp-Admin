using AbpOverallAuth.Integrations.Http.Clients;
using AbpOverallAuth.Integrations.Http.Extensions;
using AbpOverallAuth.LogManage.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;

namespace AbpOverallAuth.HttpApi.Controllers
{
    /// <summary>
    /// 外部API调用示例控制器
    /// </summary>
    [Route("api/external-api-demo")]
    public class ExternalApiDemoController : AbpController
    {
        private readonly TMClient _tmClient;
        private readonly ExternalApiLogService _externalApiLogService;
        private readonly IClock _clock;

        public ExternalApiDemoController(
            TMClient tmClient,
            ExternalApiLogService externalApiLogService,
            IClock clock)
        {
            _tmClient = tmClient;
            _externalApiLogService = externalApiLogService;
            _clock = clock;
        }

        /// <summary>
        /// 方式1：使用注入的客户端（自动记录日志）
        /// </summary>
        [HttpGet("using-client")]
        public async Task<IActionResult> UsingClient()
        {
            var result = await _tmClient.GetDataAsync("/api/data");
            return Ok(new { method = "Using Client", data = result });
        }

        /// <summary>
        /// 方式2：使用扩展方法手动记录日志
        /// </summary>
        [HttpGet("using-extension")]
        public async Task<IActionResult> UsingExtension([FromServices] IHttpClientFactory httpClientFactory)
        {
            var httpClient = httpClientFactory.CreateClient("TMClient");
            var result = await httpClient.GetAsyncWithLog(
                "/api/data",
                "TMClient",
                _externalApiLogService,
                _clock,
                "business-123",
                "Demo");

            return Ok(new { method = "Using Extension", data = result });
        }

        /// <summary>
        /// 方式3：直接使用服务记录日志
        /// </summary>
        [HttpGet("using-service")]
        public async Task<IActionResult> UsingService([FromServices] IHttpClientFactory httpClientFactory)
        {
            var httpClient = httpClientFactory.CreateClient("TMClient");
            var startTime = System.DateTime.UtcNow;

            var response = await httpClient.GetAsync("/api/data");
            var duration = (long)(System.DateTime.UtcNow - startTime).TotalMilliseconds;

            await _externalApiLogService.LogAsync(
                "TMClient",
                "http://localhost:9999/api/data",
                "GET",
                null,
                await response.Content.ReadAsStringAsync(),
                (int)response.StatusCode,
                duration,
                "business-456",
                "Demo");

            return Ok(new { method = "Using Service", data = await response.Content.ReadAsStringAsync() });
        }

        /// <summary>
        /// 方式4：全局注册 - 为所有 HttpClient 自动添加日志处理器
        /// 在模块中配置 AddHttpClient().AddHttpMessageHandler<ThirdPartyLogHandler>()
        /// </summary>
        [HttpGet("global-handler")]
        public async Task<IActionResult> UsingGlobalHandler([FromServices] IHttpClientFactory httpClientFactory)
        {
            var httpClient = httpClientFactory.CreateClient("TMClient");
            var result = await httpClient.GetAsync("/api/data");
            return Ok(new { method = "Global Handler", data = await result.Content.ReadAsStringAsync() });
        }
    }
}
