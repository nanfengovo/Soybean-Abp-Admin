using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RCS.LogManage.Services
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
            string? errorStackTrace = null;

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
            var fullUrl = request.RequestUri?.ToString() ?? string.Empty;
            var httpMethod = request.Method.Method;

            try
            {
                response = await base.SendAsync(request, cancellationToken);

                // 将响应内容加载到缓冲区，这样可以多次读取
                if (response.Content != null)
                {
                    await response.Content.LoadIntoBufferAsync();
                }

                stopwatch.Stop();

                // 读取响应体
                string? responseBody = null;
                if (response.Content != null)
                {
                    try
                    {
                        responseBody = await response.Content.ReadAsStringAsync(CancellationToken.None);
                    }
                    catch
                    {
                        // 忽略读取错误
                    }
                }

                // 记录请求日志（根据状态码判断成功或失败）
                var statusCode = (int)response.StatusCode;
                var isSuccess = response.IsSuccessStatusCode; // 2xx 状态码

                try
                {
                    if (isSuccess)
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
                    else
                    {
                        // 非 2xx 状态码视为失败
                        await _logService.LogErrorAsync(
                            sysName,
                            fullUrl,
                            httpMethod,
                            requestBody,
                            responseBody ?? $"HTTP {statusCode} 错误",
                            statusCode,
                            stopwatch.ElapsedMilliseconds);
                    }
                }
                catch (Exception logEx)
                {
                    _logger.LogWarning(logEx, "记录HTTP请求日志失败: {Message}", logEx.Message);
                }

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                errorMessage = ex.Message;
                errorStackTrace = ex.StackTrace;
                _logger.LogError(ex, "HTTP请求失败: {Url}", request.RequestUri);

                // 记录失败的请求日志 - 使用 CancellationToken.None 确保日志能够写入
                try
                {
                    await _logService.LogErrorAsync(
                        sysName,
                        fullUrl,
                        httpMethod,
                        requestBody,
                        errorMessage,
                        0, // 没有响应时状态码为0
                        stopwatch.ElapsedMilliseconds,
                        errorStackTrace: errorStackTrace);
                }
                catch (Exception logEx)
                {
                    _logger.LogWarning(logEx, "记录HTTP请求错误日志失败: {Message}", logEx.Message);
                }

                throw;
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
