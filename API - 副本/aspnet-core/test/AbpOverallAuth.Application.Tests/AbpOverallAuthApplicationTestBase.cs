using Volo.Abp.Modularity;

namespace AbpOverallAuth;

public abstract class AbpOverallAuthApplicationTestBase<TStartupModule> : AbpOverallAuthTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
