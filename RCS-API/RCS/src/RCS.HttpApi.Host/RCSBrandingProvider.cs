using Microsoft.Extensions.Localization;
using RCS.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace RCS;

[Dependency(ReplaceServices = true)]
public class RCSBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<RCSResource> _localizer;

    public RCSBrandingProvider(IStringLocalizer<RCSResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
