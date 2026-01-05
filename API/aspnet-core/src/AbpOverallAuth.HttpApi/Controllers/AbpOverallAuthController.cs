using AbpOverallAuth.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace AbpOverallAuth.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class AbpOverallAuthController : AbpControllerBase
{
    protected AbpOverallAuthController()
    {
        LocalizationResource = typeof(AbpOverallAuthResource);
    }
}
