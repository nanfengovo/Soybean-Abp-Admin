using Volo.Abp.Modularity;

namespace RCS;

[DependsOn(
    typeof(RCSDomainModule),
    typeof(RCSTestBaseModule)
)]
public class RCSDomainTestModule : AbpModule
{

}
