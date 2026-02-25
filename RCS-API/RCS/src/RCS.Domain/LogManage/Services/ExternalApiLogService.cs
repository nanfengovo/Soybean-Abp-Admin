using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RCS.LogManage.APILogs;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Claims;
using Volo.Abp.Tracing;
using Volo.Abp.Uow;

namespace RCS.LogManage.Services
{
    /// <summary>
    /// 外部API日志服务
    /// 提供手动记录外部API调用日志的功能
    /// </summary>
    public class ExternalApiLogService
    {
        private readonly IRepository<ThirdPartyCallLog, long> _thirdPartyCallLogRepository;
        private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;
        private readonly ICorrelationIdProvider _correlationIdProvider;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ExternalApiLogService(
            IRepository<ThirdPartyCallLog, long> thirdPartyCallLogRepository,
            ICurrentPrincipalAccessor currentPrincipalAccessor,
            ICorrelationIdProvider correlationIdProvider,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _thirdPartyCallLogRepository = thirdPartyCallLogRepository;
            _currentPrincipalAccessor = currentPrincipalAccessor;
            _correlationIdProvider = correlationIdProvider;
            _unitOfWorkManager = unitOfWorkManager;
        }

        /// <summary>
        /// 记录外部API调用日志
        /// </summary>
        /// <param name="sysName">系统名称</param>
        /// <param name="url">请求URL</param>
        /// <param name="httpMethod">HTTP方法</param>
        /// <param name="requestBody">请求体</param>
        /// <param name="responseBody">响应体</param>
        /// <param name="statusCode">状态码</param>
        /// <param name="duration">耗时（毫秒）</param>
        /// <param name="businessId">业务ID（可选）</param>
        /// <param name="businessType">业务类型（可选）</param>
        /// <param name="extraData">额外数据（JSON格式，可选）</param>
        /// <returns>创建的日志实体</returns>
        public async Task<ThirdPartyCallLog> LogAsync(
            string sysName,
            string url,
            string httpMethod,
            string? requestBody = null,
            string? responseBody = null,
            int statusCode = 200,
            long duration = 0,
            string? businessId = null,
            string? businessType = null,
            string? extraData = null)
        {
            var currentPrincipal = _currentPrincipalAccessor.Principal;

            var log = new ThirdPartyCallLog
            {
                SysName = sysName,
                Url = url,
                Path = TryGetPathFromUrl(url),
                HttpMethod = httpMethod,
                RequestBody = TruncateString(requestBody, 10000),
                ResponseBody = TruncateString(responseBody, 10000),
                StatusCode = statusCode,
                Duration = duration,
                BusinessId = businessId,
                BusinessType = businessType,
                ExtraData = extraData,
                ClientIpAddress = null,
                TraceId = _correlationIdProvider.Get(),
                UserId = GetUserId(currentPrincipal),
                TenantId = GetTenantId(currentPrincipal),
                IsSuccess = statusCode >= 200 && statusCode < 300
            };

            // 使用独立的工作单元，确保日志不会因为外层事务回滚而丢失
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                await _thirdPartyCallLogRepository.InsertAsync(log, autoSave: true);
                await uow.CompleteAsync();
            }

            return log;
        }

        /// <summary>
        /// 记录成功的API调用
        /// </summary>
        public async Task<ThirdPartyCallLog> LogSuccessAsync(
            string sysName,
            string url,
            string httpMethod,
            string? requestBody = null,
            string? responseBody = null,
            long duration = 0,
            string? businessId = null,
            string? businessType = null)
        {
            return await LogAsync(
                sysName,
                url,
                httpMethod,
                requestBody,
                responseBody,
                200,
                duration,
                businessId,
                businessType);
        }

        /// <summary>
        /// 记录失败的API调用
        /// </summary>
        public async Task<ThirdPartyCallLog> LogErrorAsync(
            string sysName,
            string url,
            string httpMethod,
            string? requestBody = null,
            string errorMessage = "Unknown error",
            int statusCode = 500,
            long duration = 0,
            string? businessId = null,
            string? businessType = null,
            string? errorStackTrace = null)
        {
            var currentPrincipal = _currentPrincipalAccessor.Principal;

            var log = new ThirdPartyCallLog
            {
                SysName = sysName,
                Url = url,
                Path = TryGetPathFromUrl(url),
                HttpMethod = httpMethod,
                RequestBody = TruncateString(requestBody, 10000),
                ResponseBody = null,
                StatusCode = statusCode,
                Duration = duration,
                BusinessId = businessId,
                BusinessType = businessType,
                ClientIpAddress = null,
                TraceId = _correlationIdProvider.Get(),
                UserId = GetUserId(currentPrincipal),
                TenantId = GetTenantId(currentPrincipal),
                IsSuccess = false,
                ErrorMessage = TruncateString(errorMessage, 2000),
                ErrorStackTrace = TruncateString(errorStackTrace, 4000)
            };

            // 使用独立的工作单元，确保日志不会因为外层事务回滚而丢失
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                await _thirdPartyCallLogRepository.InsertAsync(log, autoSave: true);
                await uow.CompleteAsync();
            }

            return log;
        }

        private static string? TryGetPathFromUrl(string url)
        {
            try
            {
                return new Uri(url).PathAndQuery;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 记录HttpRequestMessage调用（可用于包装现有HttpClient调用）
        /// </summary>
        public async Task<ThirdPartyCallLog> LogHttpRequestAsync(
            HttpRequestMessage request,
            HttpResponseMessage? response,
            long duration,
            string? errorMessage = null)
        {
            var currentPrincipal = _currentPrincipalAccessor.Principal;

            string? requestBody = null;
            try
            {
                if (request.Content != null)
                {
                    requestBody = await request.Content.ReadAsStringAsync();
                }
            }
            catch
            {
                // 忽略读取错误
            }

            string? responseBody = null;
            try
            {
                if (response?.Content != null)
                {
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }
            catch
            {
                // 忽略读取错误
            }

            var log = new ThirdPartyCallLog
            {
                SysName = GetClientName(request),
                Url = request.RequestUri?.ToString() ?? string.Empty,
                Path = request.RequestUri?.PathAndQuery ?? string.Empty,
                HttpMethod = request.Method.Method,
                RequestBody = TruncateString(requestBody, 10000),
                ResponseBody = TruncateString(responseBody, 10000),
                StatusCode = response?.StatusCode != null ? (int)response.StatusCode : 500,
                Duration = duration,
                ClientIpAddress = null,
                TraceId = _correlationIdProvider.Get(),
                UserId = GetUserId(currentPrincipal),
                TenantId = GetTenantId(currentPrincipal),
                IsSuccess = response?.IsSuccessStatusCode ?? false,
                ErrorMessage = errorMessage != null ? TruncateString(errorMessage, 2000) : null
            };

            // 使用独立的工作单元，确保日志不会因为外层事务回滚而丢失
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                await _thirdPartyCallLogRepository.InsertAsync(log, autoSave: true);
                await uow.CompleteAsync();
            }

            return log;
        }

        private static string GetClientName(HttpRequestMessage request)
        {
            // 优先从 X-System-Name 头获取
            if (request.Headers.TryGetValues("X-System-Name", out var systemNameHeaders))
            {
                var name = systemNameHeaders.FirstOrDefault();
                if (!string.IsNullOrEmpty(name))
                    return name;
            }

            // 其次从 X-Client-Name 头获取
            if (request.Headers.TryGetValues("X-Client-Name", out var clientNameHeaders))
            {
                return clientNameHeaders.FirstOrDefault() ?? "Unknown";
            }

            // 从Host推断
            var host = request.RequestUri?.Host;
            if (!string.IsNullOrEmpty(host))
            {
                return host;
            }

            return request.Headers.UserAgent.ToString().Split(' ').FirstOrDefault() ?? "Unknown";
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
