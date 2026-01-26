using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace AbpOverallAuth;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
        // 基本日志级别配置
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .MinimumLevel.Override("System", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .MinimumLevel.Override("OpenIddict", LogEventLevel.Warning)
        .MinimumLevel.Override("OpenIddict.Validation", LogEventLevel.Warning)
        .MinimumLevel.Override("OpenIddict.Server", LogEventLevel.Warning)
        .MinimumLevel.Override("OpenIddict.AspNetCore", LogEventLevel.Warning)
        .Enrich.WithProperty("Application", "SiaSunRCS")

        // 文件输出配置
        .WriteTo.Async(c => c.File(
            path: "RCSLogs/.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}",
            rollOnFileSizeLimit: true, // 同时按大小滚动
            fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
            retainedFileCountLimit: 7, // 保留30天日志
            shared: true, // 允许多进程共享
            flushToDiskInterval: TimeSpan.FromSeconds(1)
        ))

        // 控制台输出配置
        .WriteTo.Async(c => c.Console(
            outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        ))
        .CreateLogger();

        try
        {
            Log.Information("Starting AbpOverallAuth.HttpApi.Host.");
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog();
            await builder.AddApplicationAsync<AbpOverallAuthHttpApiHostModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
