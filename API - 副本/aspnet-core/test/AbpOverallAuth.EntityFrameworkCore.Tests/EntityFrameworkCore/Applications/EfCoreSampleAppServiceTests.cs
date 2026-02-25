using AbpOverallAuth.Samples;
using Xunit;

namespace AbpOverallAuth.EntityFrameworkCore.Applications;

[Collection(AbpOverallAuthTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<AbpOverallAuthEntityFrameworkCoreTestModule>
{

}
