using Volo.Abp.Modularity;

namespace RCS;

[DependsOn(
    typeof(RCSApplicationModule),
    typeof(RCSDomainTestModule)
)]
public class RCSApplicationTestModule : AbpModule
{

}
