using System;
using System.Collections.Generic;
using System.Text;
using RCS.Localization;
using Volo.Abp.Application.Services;

namespace RCS;

/* Inherit your application services from this class.
 */
public abstract class RCSAppService : ApplicationService
{
    protected RCSAppService()
    {
        LocalizationResource = typeof(RCSResource);
    }
}
