using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AbpOverallAuth.LogManage.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;

namespace AbpOverallAuth.Integrations.Http.Extensions
{
    /// <summary>
    /// HttpClient 扩展方法 - 自动记录API调用日志
    /// </summary>
    public static class HttpClientLoggingExtensions
    {
        /// <summary>
        /// 发送GET请求并自动记录日志
        /// </summary>
        public static async Task<HttpResponseMessage> GetAsyncWithLog(
            this HttpClient httpClient,
            string? requestUri,
            string sysName,
            ExternalApiLogService logService,
            IClock clock,
            string? businessId = null,
                        string? businessType = null,
            CancellationToken cancellationToken = default)
        {
            var startTime = clock.Now;
            var url = requestUri ?? string.Empty;

            try
            {
                var response = await httpClient.GetAsync(requestUri, cancellationToken);
                var duration = (long)(clock.Now - startTime).TotalMilliseconds;

                await logService.LogSuccessAsync(
                    sysName,
                    BuildFullUrl(httpClient, url),
                    "GET",
                    null,
                    await response.Content?.ReadAsStringAsync(),
                    duration,
                    businessId,
                    businessType);

                return response;
            }
            catch (Exception ex)
            {
                var duration = (long)(clock.Now - startTime).TotalMilliseconds;
                await logService.LogErrorAsync(
                    sysName,
                    BuildFullUrl(httpClient, url),
                    "GET",
                    null,
                    ex.Message,
                    500,
                    duration,
                    businessId,
                    businessType,
                    ex.StackTrace);

                throw;
            }
        }

        /// <summary>
        /// 发送POST请求并自动记录日志
        /// </summary>
        public static async Task<HttpResponseMessage> PostAsyncWithLog(
            this HttpClient httpClient,
            string? requestUri,
            HttpContent content,
            string sysName,
            ExternalApiLogService logService,
            IClock clock,
            string? businessId = null,
            string? businessType = null,
            CancellationToken cancellationToken = default)
        {
            var startTime = clock.Now;
            var url = requestUri ?? string.Empty;
            var requestBody = await content.ReadAsStringAsync();

            try
            {
                var response = await httpClient.PostAsync(requestUri, content, cancellationToken);
                var duration = (long)(clock.Now - startTime).TotalMilliseconds;

                await logService.LogSuccessAsync(
                    sysName,
                    BuildFullUrl(httpClient, url),
                    "POST",
                    requestBody,
                    await response.Content?.ReadAsStringAsync(),
                    duration,
                    businessId,
                    businessType);

                return response;
            }
            catch (Exception ex)
            {
                var duration = (long)(clock.Now - startTime).TotalMilliseconds;
                await logService.LogErrorAsync(
                    sysName,
                    BuildFullUrl(httpClient, url),
                    "POST",
                    requestBody,
                    ex.Message,
                    500,
                    duration,
                    businessId,
                    businessType,
                    ex.StackTrace);

                throw;
            }
        }

        /// <summary>
        /// 发送PUT请求并自动记录日志
        /// </summary>
        public static async Task<HttpResponseMessage> PutAsyncWithLog(
            this HttpClient httpClient,
            string? requestUri,
            HttpContent content,
            string sysName,
            ExternalApiLogService logService,
            IClock clock,
            string? businessId = null,
            string? businessType = null,
            CancellationToken cancellationToken = default)
        {
            var startTime = clock.Now;
            var url = requestUri ?? string.Empty;
            var requestBody = await content.ReadAsStringAsync();

            try
            {
                var response = await httpClient.PutAsync(requestUri, content, cancellationToken);
                var duration = (long)(clock.Now - startTime).TotalMilliseconds;

                await logService.LogSuccessAsync(
                    sysName,
                    BuildFullUrl(httpClient, url),
                    "PUT",
                    requestBody,
                    await response.Content?.ReadAsStringAsync(),
                    duration,
                    businessId,
                    businessType);

                return response;
            }
            catch (Exception ex)
            {
                var duration = (long)(clock.Now - startTime).TotalMilliseconds;
                await logService.LogErrorAsync(
                    sysName,
                    BuildFullUrl(httpClient, url),
                    "PUT",
                    requestBody,
                    ex.Message,
                    500,
                    duration,
                    businessId,
                    businessType,
                    ex.StackTrace);

                throw;
            }
        }

        /// <summary>
        /// 发送DELETE请求并自动记录日志
        /// </summary>
        public static async Task<HttpResponseMessage> DeleteAsyncWithLog(
            this HttpClient httpClient,
            string? requestUri,
            string sysName,
            ExternalApiLogService logService,
            IClock clock,
            string? businessId = null,
            string? businessType = null,
            CancellationToken cancellationToken = default)
        {
            var startTime = clock.Now;
            var url = requestUri ?? string.Empty;

            try
            {
                var response = await httpClient.DeleteAsync(requestUri, cancellationToken);
                var duration = (long)(clock.Now - startTime).TotalMilliseconds;

                await logService.LogSuccessAsync(
                    sysName,
                    BuildFullUrl(httpClient, url),
                    "DELETE",
                    null,
                    await response.Content?.ReadAsStringAsync(),
                    duration,
                    businessId,
                    businessType);

                return response;
            }
            catch (Exception ex)
            {
                var duration = (long)(clock.Now - startTime).TotalMilliseconds;
                await logService.LogErrorAsync(
                    sysName,
                    BuildFullUrl(httpClient, url),
                    "DELETE",
                    null,
                    ex.Message,
                    500,
                    duration,
                    businessId,
                    businessType,
                    ex.StackTrace);

                throw;
            }
        }

        /// <summary>
        /// 通用请求方法，自动记录日志
        /// </summary>
        public static async Task<HttpResponseMessage> SendAsyncWithLog(
            this HttpClient httpClient,
            HttpRequestMessage request,
            string sysName,
            ExternalApiLogService logService,
            IClock clock,
            string? businessId = null,
            string? businessType = null,
            CancellationToken cancellationToken = default)
        {
            var startTime = clock.Now;
            var url = request.RequestUri?.ToString() ?? string.Empty;

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
            }

            try
            {
                var response = await httpClient.SendAsync(request, cancellationToken);
                var duration = (long)(clock.Now - startTime).TotalMilliseconds;

                await logService.LogHttpRequestAsync(
                    request,
                    response,
                    duration);

                return response;
            }
            catch (Exception ex)
            {
                var duration = (long)(clock.Now - startTime).TotalMilliseconds;
                await logService.LogAsync(
                    sysName,
                    url,
                    request.Method.Method,
                    requestBody,
                    null,
                    500,
                    duration,
                    businessId,
                    businessType);

                throw;
            }
        }

        private static string BuildFullUrl(HttpClient client, string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl))
            {
                return client.BaseAddress?.ToString() ?? string.Empty;
            }

            if (relativeUrl.StartsWith("http://") || relativeUrl.StartsWith("https://"))
            {
                return relativeUrl;
            }

            if (client.BaseAddress != null)
            {
                return new Uri(client.BaseAddress, relativeUrl).ToString();
            }

            return relativeUrl;
        }
    }
}
