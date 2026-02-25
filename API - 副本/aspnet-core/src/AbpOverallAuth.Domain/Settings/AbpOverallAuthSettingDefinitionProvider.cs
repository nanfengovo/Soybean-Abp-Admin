using Volo.Abp.Settings;

namespace AbpOverallAuth.Settings;

public class AbpOverallAuthSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(AbpOverallAuthSettings.MySetting1));
    }
}
