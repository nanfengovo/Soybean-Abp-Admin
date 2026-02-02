using AbpOverallAuth.Configuration.ThirdParty;
using AbpOverallAuth.Interfaces;
using AbpOverallAuth.LogManage.Services;
using AbpOverallAuth.TaskManage.InternalTask.Implementation;
using AbpOverallAuth.ThirdParty.Base;
using AbpOverallAuth.TM;
using AbpOverallAuth.XinSong.TM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using System;
using System.Net.Http.Headers;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace AbpOverallAuth;

[DependsOn(
    typeof(AbpOverallAuthDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpOverallAuthApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class AbpOverallAuthApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 1️⃣ 注册依赖
        context.Services.AddTransient<IHttpExecutor, HttpExecutor>();
        context.Services.AddTransient<ITaskWorkflowPolicy, ProjectAWorkflowPolicy>();
        context.Services.AddTransient<FetchPutTaskFlowBuilder>();

        // 注册外部API日志服务
        context.Services.AddScoped<ExternalApiLogService>();
        // 注册日志记录Handler
        context.Services.AddTransient<LoggingDelegatingHandler>();

        //配置第三方api相关的
        // 注册命名 HttpClient
        var configuration = context.Services.GetConfiguration(); // 拿到 IConfiguration

        context.Services.Configure<AbpOverallAuth.Configuration.ThirdParty.TM>(configuration.GetSection("ThirdParty:TM"));
        context.Services.AddHttpClient("TM", (serviceProvider, client) =>
        {
            // 通过IOptions获取配置
            var tmConfig = serviceProvider.GetRequiredService<IOptions<AbpOverallAuth.Configuration.ThirdParty.TM>>().Value;
            // 获取针对当前配置类的日志记录器
            var logger = serviceProvider.GetRequiredService<ILogger<AbpOverallAuth.Configuration.ThirdParty.TM>>();
            //仿真地址
            if (tmConfig.IsSimulation)
            {
                client.BaseAddress = new Uri(tmConfig.SimulationUrl);
                logger.LogInformation("系统配置为仿真模式，BaseAddress: {SimulationUrl}", tmConfig.SimulationUrl);
            }
            else
            {
                client.BaseAddress = new Uri(tmConfig.URL); // TM 系统的根地址
                logger.LogInformation($"系统当前模式不是仿真，baseURL:{tmConfig.URL}");
            }
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // 添加系统名称标识，用于日志记录
            client.DefaultRequestHeaders.Add("X-System-Name", "TM");

            // 如果需要默认 Bearer Token
            // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "你的Token");
        })
        .AddHttpMessageHandler<LoggingDelegatingHandler>(); // 添加日志记录Handler
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AbpOverallAuthApplicationModule>();
        });

    }
}
