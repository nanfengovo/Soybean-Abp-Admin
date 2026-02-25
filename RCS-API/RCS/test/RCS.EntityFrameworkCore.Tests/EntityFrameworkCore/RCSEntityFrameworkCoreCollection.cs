using Xunit;

namespace RCS.EntityFrameworkCore;

[CollectionDefinition(RCSTestConsts.CollectionDefinitionName)]
public class RCSEntityFrameworkCoreCollection : ICollectionFixture<RCSEntityFrameworkCoreFixture>
{

}
