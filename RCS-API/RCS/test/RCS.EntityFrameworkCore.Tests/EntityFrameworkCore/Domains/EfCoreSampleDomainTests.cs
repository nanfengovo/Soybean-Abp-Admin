using RCS.Samples;
using Xunit;

namespace RCS.EntityFrameworkCore.Domains;

[Collection(RCSTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<RCSEntityFrameworkCoreTestModule>
{

}
