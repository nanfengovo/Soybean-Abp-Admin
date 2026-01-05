using AbpOverallAuth.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace AbpOverallAuth.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpOverallAuthEntityFrameworkCoreModule),
    typeof(AbpOverallAuthApplicationContractsModule)
    )]
public class AbpOverallAuthDbMigratorModule : AbpModule
{
}
