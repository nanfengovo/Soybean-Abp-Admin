using Xunit;

namespace AbpOverallAuth.EntityFrameworkCore;

[CollectionDefinition(AbpOverallAuthTestConsts.CollectionDefinitionName)]
public class AbpOverallAuthEntityFrameworkCoreCollection : ICollectionFixture<AbpOverallAuthEntityFrameworkCoreFixture>
{

}
