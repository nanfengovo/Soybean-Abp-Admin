# 外部API调用自动记录日志功能使用指南

## 概述

本文档介绍如何在 ABP 项目中实现每次调用外部接口时自动记录日志的功能。

## 功能特性

- **自动记录请求/响应**: 自动捕获 HTTP 请求和响应的完整信息
- **敏感信息过滤**: 自动过滤 Authorization、Cookie 等敏感请求头
- **性能监控**: 记录每次调用的耗时（毫秒级）
- **错误追踪**: 记录失败请求的错误信息和堆栈
- **用户关联**: 自动关联发起调用的用户信息
- **多种使用方式**: 支持 HTTP 处理器、扩展方法、服务注入等方式

## 实现组件

### 1. ThirdPartyCallLog 实体
- **位置**: `src/AbpOverallAuth.Domain/LogManage/APILogs/ThirdPartyCallLog.cs`
- **功能**: 存储外部 API 调用日志
- **字段**:
  - `SysName`: 系统名称/客户端名称
  - `BusinessId`: 业务标识（可选）
  - `BusinessType`: 业务类型（可选）
  - `Url`: 完整请求 URL
  - `Path`: 相对路径
  - `HttpMethod`: HTTP 方法（GET/POST/PUT/DELETE）
  - `RequestHeaders`: 请求头（JSON格式，已过滤敏感信息）
  - `RequestBody`: 请求体内容
  - `StatusCode`: 响应状态码
  - `ResponseBody`: 响应体内容
  - `ResponseHeaders`: 响应头（JSON格式）
  - `Duration`: 调用耗时（毫秒）
  - `ClientIpAddress`: 客户端 IP
  - `TraceId`: 跟踪 ID
  - `IsSuccess`: 是否成功
  - `ErrorMessage`: 错误信息
  - `ErrorStackTrace`: 错误堆栈
  - `UserId`: 用户 ID
  - `TenantId`: 租户 ID

### 2. ThirdPartyLogHandler（推荐）
- **位置**: `src/AbpOverallAuth.HttpApi/Integrations/Http/Handlers/ThirdPartyLogHandler.cs`
- **功能**: HTTP 消息处理器，自动记录所有经过的请求
- **使用方式**: 在模块中注册为 HttpClient 的消息处理器

### 3. ExternalApiLogService
- **位置**: `src/AbpOverallAuth.Domain/LogManage/Services/ExternalApiLogService.cs`
- **功能**: 提供手动记录 API 调用日志的服务
- **主要方法**:
  - `LogAsync()`: 通用日志记录
  - `LogSuccessAsync()`: 记录成功调用
  - `LogErrorAsync()`: 记录失败调用
  - `LogHttpRequestAsync()`: 记录 HttpRequestMessage 调用

### 4. HttpClient 扩展方法
- **位置**: `src/AbpOverallAuth.HttpApi/Integrations/Http/Extensions/HttpClientLoggingExtensions.cs`
- **功能**: 为 HttpClient 提供带日志记录的方法
- **方法**:
  - `GetAsyncWithLog()`: GET 请求（自动记录日志）
  - `PostAsyncWithLog()`: POST 请求（自动记录日志）
  - `PutAsyncWithLog()`: PUT 请求（自动记录日志）
  - `DeleteAsyncWithLog()`: DELETE 请求（自动记录日志）
  - `SendAsyncWithLog()`: 通用请求（自动记录日志）

## 使用方式

### 方式1：HTTP 消息处理器（推荐 - 自动记录）

在模块中配置：

```csharp
// 在 AbpOverallAuthHttpApiHostModule.cs 中
context.Services.AddTransient<ThirdPartyLogHandler>();

// 为特定 HttpClient 添加日志处理器
context.Services.AddHttpClient<YourClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<ThirdPartyLogHandler>();

// 为所有 HttpClient 添加日志处理器（全局生效）
context.Services.AddHttpClient()
    .AddHttpMessageHandler<ThirdPartyLogHandler>();
```

### 方式2：注入客户端使用

创建客户端类：

```csharp
public class YourClient
{
    private readonly HttpClient _httpClient;
    
    public YourClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<string> GetDataAsync(string endpoint)
    {
        return await _httpClient.GetAsync(endpoint)
            .EnsureSuccessStatusCode()
            .Content.ReadAsStringAsync();
    }
}
```

在应用中使用：

```csharp
public class YourAppService : ApplicationService
{
    private readonly YourClient _yourClient;
    
    public YourAppService(YourClient yourClient)
    {
        _yourClient = yourClient;
    }
    
    public async Task<string> GetData()
    {
        // 自动记录日志
        return await _yourClient.GetDataAsync("/api/data");
    }
}
```

### 方式3：使用扩展方法

```csharp
public class YourAppService : ApplicationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ExternalApiLogService _logService;
    
    public YourAppService(
        IHttpClientFactory httpClientFactory,
        ExternalApiLogService logService)
    {
        _httpClientFactory = httpClientFactory;
        _logService = logService;
    }
    
    public async Task<string> GetData()
    {
        var httpClient = _httpClientFactory.CreateClient("YourClient");
        
        // 使用扩展方法
        var response = await httpClient.GetAsyncWithLog(
            "/api/data",
            "YourClient",
            _logService,
            "business-123",  // businessId
            "GetData"        // businessType
        );
        
        return await response.Content.ReadAsStringAsync();
    }
}
```

### 方式4：直接使用日志服务

```csharp
public class YourAppService : ApplicationService
{
    private readonly ExternalApiLogService _logService;
    private readonly IHttpClientFactory _httpClientFactory;
    
    public YourAppService(
        ExternalApiLogService logService,
        IHttpClientFactory httpClientFactory)
    {
        _logService = logService;
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<string> GetData()
    {
        var httpClient = _httpClientFactory.CreateClient("YourClient");
        var startTime = DateTime.UtcNow;
        
        try
        {
            var response = await httpClient.GetAsync("/api/data");
            var duration = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            
            await _logService.LogAsync(
                sysName: "YourClient",
                url: "https://api.example.com/api/data",
                httpMethod: "GET",
                requestBody: null,
                responseBody: await response.Content.ReadAsStringAsync(),
                statusCode: (int)response.StatusCode,
                duration: duration,
                businessId: "business-123",
                businessType: "GetData"
            );
            
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(
                sysName: "YourClient",
                url: "https://api.example.com/api/data",
                httpMethod: "GET",
                requestBody: null,
                errorMessage: ex.Message,
                statusCode: 500,
                duration: (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                errorStackTrace: ex.StackTrace
            );
            
            throw;
        }
    }
}
```

## 配置说明

### 数据库配置

实体已在 `AbpOverallAuthDbContext` 中注册，并创建了数据表 `tb_ThirdPartyCallLog`。

### 服务注册

在 `AbpOverallAuthHttpApiHostModule` 中已自动注册：
- `ThirdPartyLogHandler`: 瞬态服务
- `ExternalApiLogService`: 作用域服务

## 监控和查询

### 查询最近的调用记录

```csharp
public class YourAppService : ApplicationService
{
    private readonly IRepository<ThirdPartyCallLog, long> _logRepository;
    
    public YourAppService(IRepository<ThirdPartyCallLog, long> logRepository)
    {
        _logRepository = logRepository;
    }
    
    public async Task<List<ThirdPartyCallLog>> GetRecentLogs(int count = 100)
    {
        return await _logRepository.GetDbSet()
            .OrderByDescending(x => x.CreationTime)
            .Take(count)
            .ToListAsync();
    }
    
    public async Task<List<ThirdPartyCallLog>> GetFailedLogs()
    {
        return await _logRepository.GetDbSet()
            .Where(x => !x.IsSuccess)
            .OrderByDescending(x => x.CreationTime)
            .Take(100)
            .ToListAsync();
    }
    
    public async Task<ThirdPartyCallLog?> GetLogById(long id)
    {
        return await _logRepository.GetAsync(id);
    }
}
```

### 按条件查询

```csharp
// 按系统名称查询
var logs = await _logRepository.GetDbSet()
    .Where(x => x.SysName == "YourClient")
    .OrderByDescending(x => x.CreationTime)
    .Take(100)
    .ToListAsync();

// 按时间范围查询
var startDate = DateTime.UtcNow.AddHours(-1);
var endDate = DateTime.UtcNow;
var logs = await _logRepository.GetDbSet()
    .Where(x => x.CreationTime >= startDate && x.CreationTime <= endDate)
    .ToListAsync();

// 按状态码查询
var errorLogs = await _logRepository.GetDbSet()
    .Where(x => x.StatusCode >= 400)
    .ToListAsync();
```

## 性能考虑

1. **异步操作**: 所有日志记录操作都是异步的，不会阻塞主线程
2. **批量保存**: 使用 `autoSave: true` 自动保存，也可以手动批量保存
3. **数据截断**: 自动截断过长的请求/响应体（最大 10000 字符）
4. **索引优化**: 在 `OnModelCreating` 中已创建常用查询字段的索引

## 最佳实践

1. **选择合适的记录方式**:
   - 大量外部调用 → 使用 HTTP 处理器（无侵入）
   - 关键业务调用 → 使用日志服务（可添加额外信息）
   - 临时调试 → 使用扩展方法

2. **设置合理的日志级别**:
   - 生产环境：记录所有调用
   - 调试环境：可以记录更详细的调试信息

3. **定期清理旧日志**:
   - 建议配置定时任务清理超过 30-90 天的日志
   - 可以按业务类型分表存储

4. **监控异常模式**:
   - 定期检查失败率
   - 设置告警阈值

## 常见问题

### Q: 如何为不同的 HttpClient 配置不同的系统名称？
A: 在请求头中添加 `X-Client-Name`：
```csharp
request.Headers.Add("X-Client-Name", "YourSystemName");
```

### Q: 如何记录文件上传/下载？
A: 对于大文件，建议只记录元数据，不记录文件内容：
```csharp
await _logService.LogAsync(
    sysName: "FileService",
    url: "https://api.example.com/upload",
    httpMethod: "POST",
    requestBody: $"FileName: {fileName}, Size: {fileSize}",
    // 不记录实际文件内容
    statusCode: 200,
    duration: duration
);
```

### Q: 如何处理循环引用或大型 JSON？
A: 日志服务会自动截断过长的内容，如需特殊处理可在记录前处理：
```csharp
var summary = TruncateString(largeJson, 5000);
await _logService.LogAsync(sysName, url, method, summary, ...);
```
