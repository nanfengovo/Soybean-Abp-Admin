using AbpOverallAuth.Localization;
using AbpOverallAuth.Navigation;
using System.Linq;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace AbpOverallAuth.Permissions;

/// <summary>
/// 权限定义提供者 - 从动态存储加载权限
/// </summary>
public class AbpOverallAuthPermissionDefinitionProvider : PermissionDefinitionProvider
{
    private readonly DynamicPermissionDefinitionStore _dynamicStore;

    public AbpOverallAuthPermissionDefinitionProvider(DynamicPermissionDefinitionStore dynamicStore)
    {
        _dynamicStore = dynamicStore;
    }

    public override void Define(IPermissionDefinitionContext context)
    {
        // 定义权限组
        var menuGroup = context.AddGroup(
            "Menus",
            L("Permission:Menus")
        );

        // 从动态存储加载权限
        var permissions = _dynamicStore.GetPermissionsAsync().GetAwaiter().GetResult();
        foreach (var permission in permissions)
        {
            menuGroup.AddPermission(permission.Name, permission.DisplayName);
        }
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AbpOverallAuthResource>(name);
    }
}
