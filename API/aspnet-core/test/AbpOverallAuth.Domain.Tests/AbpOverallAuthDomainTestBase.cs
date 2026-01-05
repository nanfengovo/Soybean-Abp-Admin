using Volo.Abp.Modularity;

namespace AbpOverallAuth;

/* Inherit from this class for your domain layer tests. */
public abstract class AbpOverallAuthDomainTestBase<TStartupModule> : AbpOverallAuthTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
