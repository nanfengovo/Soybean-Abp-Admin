using Volo.Abp.Modularity;

namespace AbpOverallAuth;

[DependsOn(
    typeof(AbpOverallAuthDomainModule),
    typeof(AbpOverallAuthTestBaseModule)
)]
public class AbpOverallAuthDomainTestModule : AbpModule
{

}
