using Microsoft.Extensions.Localization;
using AbpOverallAuth.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace AbpOverallAuth;

[Dependency(ReplaceServices = true)]
public class AbpOverallAuthBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<AbpOverallAuthResource> _localizer;

    public AbpOverallAuthBrandingProvider(IStringLocalizer<AbpOverallAuthResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
