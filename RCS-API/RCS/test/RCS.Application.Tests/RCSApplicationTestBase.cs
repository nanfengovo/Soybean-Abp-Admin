using Volo.Abp.Modularity;

namespace RCS;

public abstract class RCSApplicationTestBase<TStartupModule> : RCSTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
