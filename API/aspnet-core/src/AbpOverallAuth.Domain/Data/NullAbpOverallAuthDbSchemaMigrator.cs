using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace AbpOverallAuth.Data;

/* This is used if database provider does't define
 * IAbpOverallAuthDbSchemaMigrator implementation.
 */
public class NullAbpOverallAuthDbSchemaMigrator : IAbpOverallAuthDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
