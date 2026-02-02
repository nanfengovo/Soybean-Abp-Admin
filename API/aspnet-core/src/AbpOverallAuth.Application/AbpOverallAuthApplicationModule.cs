using AbpOverallAuth.Configuration.ThirdParty;
using AbpOverallAuth.Interfaces;
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

        //配置第三方api相关的
        // 注册命名 HttpClient
        var configuration = context.Services.GetConfiguration(); // 拿到 IConfiguration

        // 1. 获取 LoggerFactory 并创建 Logger
        var loggerFactory = context.Services.GetSingletonInstance<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<AbpOverallAuthApplicationModule>();
        context.Services.Configure<AbpOverallAuth.Configuration.ThirdParty.TM>(configuration.GetSection("TM"));
        context.Services.AddHttpClient("TM", (serviceProvider, client) =>
        {
            // 通过IOptions获取配置
            var tmConfig = serviceProvider.GetRequiredService<IOptions<AbpOverallAuth.Configuration.ThirdParty.TM>>().Value;
            //仿真地址
            if (tmConfig.IsSimulation)
            {
                client.BaseAddress = new Uri(tmConfig.SimulationUrl);
                logger.LogInformation($"系统当前模式是仿真，baseURL{tmConfig.SimulationUrl}");
            }
            else
            {
                client.BaseAddress = new Uri(tmConfig.URL); // TM 系统的根地址
                logger.LogInformation($"系统当前模式不是仿真，baseURL{tmConfig.SimulationUrl}");
            }
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // 如果需要默认 Bearer Token
            // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "你的Token");
        });
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AbpOverallAuthApplicationModule>();
        });

    }
}
