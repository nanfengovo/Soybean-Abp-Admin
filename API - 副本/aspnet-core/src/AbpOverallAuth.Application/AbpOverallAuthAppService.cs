using System;
using System.Collections.Generic;
using System.Text;
using AbpOverallAuth.Localization;
using Volo.Abp.Application.Services;

namespace AbpOverallAuth;

/* Inherit your application services from this class.
 */
public abstract class AbpOverallAuthAppService : ApplicationService
{
    protected AbpOverallAuthAppService()
    {
        LocalizationResource = typeof(AbpOverallAuthResource);
    }
}
