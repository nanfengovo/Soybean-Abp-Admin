using System.Threading.Tasks;

namespace AbpOverallAuth.Data;

public interface IAbpOverallAuthDbSchemaMigrator
{
    Task MigrateAsync();
}
