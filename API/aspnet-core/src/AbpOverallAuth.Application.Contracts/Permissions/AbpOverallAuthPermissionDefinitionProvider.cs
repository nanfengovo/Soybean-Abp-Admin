using AbpOverallAuth.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace AbpOverallAuth.Permissions;

public class AbpOverallAuthPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(AbpOverallAuthPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(AbpOverallAuthPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AbpOverallAuthResource>(name);
    }
}
