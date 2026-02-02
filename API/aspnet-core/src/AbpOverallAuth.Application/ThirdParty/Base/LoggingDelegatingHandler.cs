using AbpOverallAuth.LogManage.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AbpOverallAuth.ThirdParty.Base
{
    /// <summary>
    /// HTTP请求日志记录处理器
    /// 自动拦截所有通过HttpClient发出的请求并记录到数据库
    /// </summary>
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ExternalApiLogService _logService;
        private readonly ILogger<LoggingDelegatingHandler> _logger;

        /// <summary>
        /// 用于标识系统名称的请求头
        /// </summary>
        public const string SystemNameHeader = "X-System-Name";

        public LoggingDelegatingHandler(
            ExternalApiLogService logService,
            ILogger<LoggingDelegatingHandler> logger)
        {
            _logService = logService;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            HttpResponseMessage? response = null;
            string? errorMessage = null;

            // 在发送前缓存请求体（因为 Content 只能读取一次）
            string? requestBody = null;
            if (request.Content != null)
            {
                try
                {
                    requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                }
                catch
                {
                    // 忽略读取错误
                }
            }

            // 获取系统名称
            string sysName = GetSystemName(request);

            try
            {
                response = await base.SendAsync(request, cancellationToken);

                // 将响应内容加载到缓冲区，这样可以多次读取
                if (response.Content != null)
                {
                    await response.Content.LoadIntoBufferAsync();
                }

                return response;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _logger.LogError(ex, "HTTP请求失败: {Url}", request.RequestUri);
                throw;
            }
            finally
            {
                stopwatch.Stop();

                // 读取响应体（由于已经 LoadIntoBufferAsync，可以多次读取）
                string? responseBody = null;
                if (response?.Content != null)
                {
                    try
                    {
                        responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    }
                    catch
                    {
                        // 忽略读取错误
                    }
                }

                try
                {
                    // 直接调用 LogAsync 而不是 LogHttpRequestAsync，避免重复读取 Content
                    var fullUrl = request.RequestUri?.ToString() ?? string.Empty;
                    var httpMethod = request.Method.Method;
                    var statusCode = response != null ? (int)response.StatusCode : 500;

                    if (errorMessage != null)
                    {
                        await _logService.LogErrorAsync(
                            sysName,
                            fullUrl,
                            httpMethod,
                            requestBody,
                            errorMessage,
                            statusCode,
                            stopwatch.ElapsedMilliseconds);
                    }
                    else
                    {
                        await _logService.LogAsync(
                            sysName,
                            fullUrl,
                            httpMethod,
                            requestBody,
                            responseBody,
                            statusCode,
                            stopwatch.ElapsedMilliseconds);
                    }
                }
                catch (Exception logEx)
                {
                    // 日志记录失败不应影响主流程
                    _logger.LogWarning(logEx, "记录HTTP请求日志失败: {Message}", logEx.Message);
                }
            }
        }

        /// <summary>
        /// 获取系统名称
        /// </summary>
        private static string GetSystemName(HttpRequestMessage request)
        {
            // 优先从请求头获取
            if (request.Headers.TryGetValues(SystemNameHeader, out var values))
            {
                foreach (var value in values)
                {
                    if (!string.IsNullOrEmpty(value))
                        return value;
                }
            }

            // 从Host推断
            var host = request.RequestUri?.Host;
            if (!string.IsNullOrEmpty(host))
            {
                return host;
            }

            return "Unknown";
        }
    }
}
