using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly RequestResponseLoggingOptions _options;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger,
        IOptions<RequestResponseLoggingOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        // 检查是否跳过此请求
        if (ShouldSkipLogging(context))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestLog = await FormatRequestLog(context.Request);

        // 使用内存流捕获响应
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request processing error");
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // 记录响应
            var responseLog = await FormatResponseLog(context.Response, stopwatch.ElapsedMilliseconds);

            // 直接输出日志，避免Serilog对JSON的转义
            var logMessage = $"""
                Request: {context.Request.Method} {context.Request.Path}
                {requestLog}
                Response: {context.Response.StatusCode} in {stopwatch.ElapsedMilliseconds}ms
                {responseLog}
                """;

            _logger.LogInformation(logMessage);

            // 将响应体复制回原始流
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private bool ShouldSkipLogging(HttpContext context)
    {
        // 检查是否禁用日志
        if (!_options.Enabled) return true;

        // 检查排除路径
        if (_options.ExcludedPaths.Any(p =>
            context.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // 采样率控制
        if (_options.SamplingRate < 1.0 &&
            new Random().NextDouble() > _options.SamplingRate)
        {
            return true;
        }

        return false;
    }

    private async Task<string> FormatRequestLog(HttpRequest request)
    {
        var log = new StringBuilder();

        // 基本信息
        log.AppendLine($"Content-Type: {request.ContentType}");
        log.AppendLine($"Content-Length: {request.ContentLength}");

        // 请求头
        if (_options.LogHeaders)
        {
            log.AppendLine("Headers:");
            foreach (var header in request.Headers)
            {
                var value = ShouldSanitizeHeader(header.Key)
                    ? "******"
                    : header.Value.ToString();
                log.AppendLine($"  {header.Key}: {value}");
            }
        }

        // 请求体
        if (_options.LogRequestBody)
        {
            var body = await GetRequestBody(request);
            //log.AppendLine(body);
            log.AppendLine($"Body: {FormatJsonIfApplicable(body, request.ContentType)}");
        }

        return log.ToString();
    }

    private async Task<string> FormatResponseLog(HttpResponse response, long elapsedMs)
    {
        var log = new StringBuilder();

        // 基本信息
        log.AppendLine($"Content-Type: {response.ContentType}");
        log.AppendLine($"Reported Content-Length: {response.ContentLength}");
        log.AppendLine($"Actual Body Size: {response.Body.Length} bytes");

        // 响应头
        if (_options.LogHeaders)
        {
            log.AppendLine("Headers:");
            foreach (var header in response.Headers)
            {
                log.AppendLine($"  {header.Key}: {header.Value}");
            }
        }

        // 响应体
        if (_options.LogResponseBody && ShouldLogResponseBody(response))
        {
            var body = await GetResponseBody(response);
            //log.AppendLine(body);
            log.AppendLine($"Body: {FormatJsonIfApplicable(body, response.ContentType)}");
        }

        return log.ToString();
    }

    private string FormatJsonIfApplicable(string content, string contentType)
    {
        // 空内容直接返回
        if (string.IsNullOrWhiteSpace(content))
            return content;

        // 检查是否为 JSON 内容
        if (!IsJsonContentType(contentType))
            return content;

        try
        {
            // 尝试解析为 JSON 对象
            var jsonNode = JsonNode.Parse(content);

            // 使用格式化选项序列化，允许中文等Unicode字符不转义
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(
                    System.Text.Unicode.UnicodeRanges.All)
            };

            return jsonNode?.ToJsonString(options) ?? content;
        }
        catch (JsonException)
        {
            // 不是有效的 JSON，返回原始内容
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to format JSON content");
            return content;
        }
    }

    private bool IsJsonContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        return contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("application/problem+json", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("application/hal+json", StringComparison.OrdinalIgnoreCase);
    }

    private bool ShouldLogResponseBody(HttpResponse response)
    {
        // 特殊状态码不记录响应体
        if (response.StatusCode == StatusCodes.Status204NoContent ||
            response.StatusCode == StatusCodes.Status304NotModified)
        {
            return false;
        }

        // 二进制内容类型不记录
        var contentType = response.ContentType?.ToLowerInvariant();
        if (contentType != null &&
            (contentType.Contains("image") ||
             contentType.Contains("video") ||
             contentType.Contains("audio")))
        {
            return false;
        }

        return true;
    }

    private async Task<string> GetRequestBody(HttpRequest request)
    {
        // 空请求体
        if (request.ContentLength == null || request.ContentLength == 0)
            return "[Empty Body]";

        // 请求体过大
        if (request.ContentLength > _options.MaxRequestBodySize)
            return $"[Body too large: {request.ContentLength} bytes]";

        // 启用缓冲以便多次读取
        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0; // 重置位置

        return SanitizeBody(body);
    }

    private async Task<string> GetResponseBody(HttpResponse response)
    {
        try
        {
            // 检查响应体是否可读
            if (response.Body == null || !response.Body.CanRead || !response.Body.CanSeek)
                return "[Stream not readable]";

            // 获取实际内容大小
            var bodySize = response.Body.Length;

            // 空响应体
            if (bodySize == 0)
                return "[Empty Body]";

            // 响应体过大
            if (bodySize > _options.MaxResponseBodySize)
                return $"[Body too large: {bodySize} bytes]";

            // 保存当前位置
            var position = response.Body.Position;
            response.Body.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(
                response.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            response.Body.Seek(position, SeekOrigin.Begin); // 恢复位置

            return SanitizeBody(body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read response body");
            return $"[Error reading body: {ex.Message}]";
        }
    }

    private bool ShouldSanitizeHeader(string headerName)
    {
        var sensitiveHeaders = new[] { "Authorization", "Cookie", "Set-Cookie", "X-Api-Key" };
        return sensitiveHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }

    private string SanitizeBody(string body)
    {
        if (string.IsNullOrEmpty(body)) return body;

        try
        {
            // 过滤密码字段
            if (body.Contains("password", StringComparison.OrdinalIgnoreCase))
            {
                body = Regex.Replace(
                    body,
                    @"""password""\s*:\s*""[^""]*""",
                    @"""password"": ""******""",
                    RegexOptions.IgnoreCase);
            }

            // 过滤令牌字段
            if (body.Contains("token", StringComparison.OrdinalIgnoreCase))
            {
                body = Regex.Replace(
                    body,
                    @"""token""\s*:\s*""[^""]*""",
                    @"""token"": ""******""",
                    RegexOptions.IgnoreCase);
            }

            return body;
        }
        catch
        {
            return body;
        }
    }
}

public class RequestResponseLoggingOptions
{
    public bool Enabled { get; set; } = true;
    public bool LogHeaders { get; set; } = true;
    public bool LogRequestBody { get; set; } = true;
    public bool LogResponseBody { get; set; } = true;
    public bool FormatJson { get; set; } = true; // 新增：是否格式化JSON
    public int MaxRequestBodySize { get; set; } = 10240; // 10KB
    public int MaxResponseBodySize { get; set; } = 10240; // 10KB
    public double SamplingRate { get; set; } = 1.0; // 100%采样
    public List<string> ExcludedPaths { get; set; } = new List<string>
    {
        "/health",
        "/swagger",
        "/favicon.ico"
    };
}
