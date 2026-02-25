using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AbpOverallAuth.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class AbpOverallAuthDbContextFactory : IDesignTimeDbContextFactory<AbpOverallAuthDbContext>
{
    public AbpOverallAuthDbContext CreateDbContext(string[] args)
    {
        AbpOverallAuthEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<AbpOverallAuthDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));

        return new AbpOverallAuthDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../AbpOverallAuth.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
