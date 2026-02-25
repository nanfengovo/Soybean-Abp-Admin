using AbpOverallAuth.Samples;
using Xunit;

namespace AbpOverallAuth.EntityFrameworkCore.Domains;

[Collection(AbpOverallAuthTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<AbpOverallAuthEntityFrameworkCoreTestModule>
{

}
