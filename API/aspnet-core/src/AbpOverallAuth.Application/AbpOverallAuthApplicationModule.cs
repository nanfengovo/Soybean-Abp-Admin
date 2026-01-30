using AbpOverallAuth.Interfaces;
using AbpOverallAuth.TaskManage.InternalTask.Implementation;
using AbpOverallAuth.ThirdParty.Base;
using AbpOverallAuth.TM;
using Microsoft.Extensions.DependencyInjection;
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
        context.Services.AddHttpClient("TM", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000"); // TM 系统的根地址
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
