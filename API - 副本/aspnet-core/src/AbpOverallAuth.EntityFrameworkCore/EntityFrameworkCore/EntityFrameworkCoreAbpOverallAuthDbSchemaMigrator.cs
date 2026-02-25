using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AbpOverallAuth.Data;
using Volo.Abp.DependencyInjection;

namespace AbpOverallAuth.EntityFrameworkCore;

public class EntityFrameworkCoreAbpOverallAuthDbSchemaMigrator
    : IAbpOverallAuthDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreAbpOverallAuthDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the AbpOverallAuthDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<AbpOverallAuthDbContext>()
            .Database
            .MigrateAsync();
    }
}
