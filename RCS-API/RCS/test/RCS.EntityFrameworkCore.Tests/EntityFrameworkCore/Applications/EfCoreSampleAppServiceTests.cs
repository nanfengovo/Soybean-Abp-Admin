using RCS.Samples;
using Xunit;

namespace RCS.EntityFrameworkCore.Applications;

[Collection(RCSTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<RCSEntityFrameworkCoreTestModule>
{

}
