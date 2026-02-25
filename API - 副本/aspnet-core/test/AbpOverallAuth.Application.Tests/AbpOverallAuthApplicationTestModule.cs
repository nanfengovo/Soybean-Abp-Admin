using Volo.Abp.Modularity;

namespace AbpOverallAuth;

[DependsOn(
    typeof(AbpOverallAuthApplicationModule),
    typeof(AbpOverallAuthDomainTestModule)
)]
public class AbpOverallAuthApplicationTestModule : AbpModule
{

}
