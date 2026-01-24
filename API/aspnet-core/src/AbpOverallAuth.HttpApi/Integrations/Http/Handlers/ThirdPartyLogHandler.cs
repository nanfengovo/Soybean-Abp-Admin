using System.Security.Claims;
using AbpOverallAuth.LogManage.APILogs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Claims;
using Volo.Abp.Tracing;
using Volo.Abp.Uow;

namespace AbpOverallAuth.Integrations.Http.Handlers
{
    /// <summary>
    /// 第三方API调用日志处理器
    /// 自动记录所有经过此Handler的HTTP请求/响应
    /// </summary>
    public class ThirdPartyLogHandler : DelegatingHandler
    {
        private readonly IRepository<ThirdPartyCallLog, long> _thirdPartyCallLogRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;
        private readonly ICorrelationIdProvider _correlationIdProvider;

        public ThirdPartyLogHandler(
            IRepository<ThirdPartyCallLog, long> thirdPartyCallLogRepository,
            IHttpContextAccessor httpContextAccessor,
            ICurrentPrincipalAccessor currentPrincipalAccessor,
            ICorrelationIdProvider correlationIdProvider)
        {
            _thirdPartyCallLogRepository = thirdPartyCallLogRepository;
            _httpContextAccessor = httpContextAccessor;
            _currentPrincipalAccessor = currentPrincipalAccessor;
            _correlationIdProvider = correlationIdProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var startTime = DateTime.UtcNow;
            var log = CreateLog(request, startTime);

            HttpResponseMessage? response = null;
            string? responseBody = null;
            string? errorMessage = null;
            string? errorStackTrace = null;
            bool isSuccess = true;

            try
            {
                response = await base.SendAsync(request, cancellationToken);
                responseBody = await GetResponseContentAsync(response);
                log.StatusCode = (int)response.StatusCode;
                log.ResponseBody = TruncateString(responseBody, 10000);
                log.IsSuccess = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                isSuccess = false;
                errorMessage = ex.Message;
                errorStackTrace = ex.StackTrace;
                log.StatusCode = 500;
                log.IsSuccess = false;
                log.ErrorMessage = TruncateString(ex.Message, 2000);
                log.ErrorStackTrace = TruncateString(ex.StackTrace, 4000);
            }
            finally
            {
                var duration = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                log.Duration = duration;
                log.ResponseBody = TruncateString(log.ResponseBody, 10000);

                await _thirdPartyCallLogRepository.InsertAsync(log, autoSave: true);
            }

            if (!isSuccess)
            {
                throw new HttpRequestException(errorMessage, null, System.Net.HttpStatusCode.InternalServerError);
            }

            return response!;
        }

        private ThirdPartyCallLog CreateLog(HttpRequestMessage request, DateTime startTime)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var currentPrincipal = _currentPrincipalAccessor.Principal;

            return new ThirdPartyCallLog
            {
                SysName = GetClientName(request),
                Url = request.RequestUri?.ToString() ?? string.Empty,
                Path = request.RequestUri?.PathAndQuery ?? string.Empty,
                HttpMethod = request.Method.Method,
                RequestHeaders = GetRequestHeaders(request),
                RequestBody = GetRequestBody(request),
                ClientIpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                TraceId = _correlationIdProvider.Get(),
                UserId = GetUserId(currentPrincipal),
                TenantId = GetTenantId(currentPrincipal)
            };
        }

        private string GetClientName(HttpRequestMessage request)
        {
            if (request.Headers.TryGetValues("X-Client-Name", out var clientNameHeaders))
            {
                return clientNameHeaders.FirstOrDefault() ?? "Unknown";
            }

            return request.Headers.UserAgent.ToString().Split(' ').FirstOrDefault() ?? "Unknown";
        }

        private string? GetRequestHeaders(HttpRequestMessage request)
        {
            try
            {
                var headers = new Dictionary<string, string>();
                foreach (var header in request.Headers)
                {
                    if (IsSensitiveHeader(header.Key))
                        continue;

                    headers[header.Key] = string.Join(", ", header.Value);
                }

                if (request.Content?.Headers != null)
                {
                    foreach (var header in request.Content.Headers)
                    {
                        if (IsSensitiveHeader(header.Key))
                            continue;

                        headers[$"[Content] {header.Key}"] = string.Join(", ", header.Value);
                    }
                }

                return JsonSerializer.Serialize(headers);
            }
            catch
            {
                return null;
            }
        }

        private string? GetRequestBody(HttpRequestMessage request)
        {
            try
            {
                if (request.Content == null)
                    return null;

                return request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch
            {
                return null;
            }
        }

        private async Task<string?> GetResponseContentAsync(HttpResponseMessage response)
        {
            try
            {
                if (response.Content == null)
                    return null;

                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }

        private static bool IsSensitiveHeader(string headerName)
        {
            var sensitiveHeaders = new[]
            {
                "Authorization",
                "Cookie",
                "Set-Cookie",
                "X-API-Key",
                "Api-Key",
                "Token",
                "Bearer"
            };

            return sensitiveHeaders.Any(h => headerName.Equals(h, StringComparison.OrdinalIgnoreCase));
        }

        private static string? TruncateString(string? str, int maxLength)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return str.Length > maxLength ? str[..maxLength] : str;
        }

        private static Guid? GetUserId(ClaimsPrincipal? principal)
        {
            if (principal == null)
                return null;

            var claim = principal.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == Volo.Abp.Security.Claims.AbpClaimTypes.UserId);
            if (claim != null && Guid.TryParse(claim.Value, out var userId))
                return userId;

            return null;
        }

        private static Guid? GetTenantId(ClaimsPrincipal? principal)
        {
            if (principal == null)
                return null;

            var claim = principal.Claims.FirstOrDefault(c => c.Type == Volo.Abp.Security.Claims.AbpClaimTypes.TenantId);
            if (claim != null && Guid.TryParse(claim.Value, out var tenantId))
                return tenantId;

            return null;
        }
    }
}
