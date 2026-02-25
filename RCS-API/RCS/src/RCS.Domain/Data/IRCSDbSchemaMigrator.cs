using System.Threading.Tasks;

namespace RCS.Data;

public interface IRCSDbSchemaMigrator
{
    Task MigrateAsync();
}
