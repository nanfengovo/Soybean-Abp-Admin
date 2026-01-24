using AbpOverallAuth.Integrations.Http.Clients;
using AbpOverallAuth.Localization;
using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp.Account;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace AbpOverallAuth;

[DependsOn(
    typeof(AbpOverallAuthApplicationContractsModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpTenantManagementHttpApiModule),
    typeof(AbpFeatureManagementHttpApiModule),
    typeof(AbpSettingManagementHttpApiModule)
    )]
public class AbpOverallAuthHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureLocalization();
        // 注册 TMClient 为 Transient
        context.Services.AddTransient<TMClient>();

        // 或者如果需要 HttpClient 配置
        context.Services.AddHttpClient<TMClient>(client =>
        {
            client.BaseAddress = new Uri("http://tm-service-url/");
            client.DefaultRequestHeaders.Add("User-Agent", "AbpOverallAuth");
        });
    }

    private void ConfigureLocalization()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<AbpOverallAuthResource>()
                .AddBaseTypes(
                    typeof(AbpUiResource)
                );
        });
    }
}
