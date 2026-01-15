using AbpOverallAuth.Localization;
using System.Linq;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace AbpOverallAuth.Permissions;

/// <summary>
/// 权限定义提供者 - 从动态存储加载权限
/// </summary>
public class AbpOverallAuthPermissionDefinitionProvider : PermissionDefinitionProvider
{

    public override void Define(IPermissionDefinitionContext context)
    {
        // 定义权限组
        var myGroup = context.AddGroup(AbpOverallAuthPermissions.GroupName, L("Permission:AbpOverallAuth"));

        //LocationMap权限树
        var mapPermission = myGroup.AddPermission(AbpOverallAuthPermissions.LocationMap.Default, L("Permission:点位映射管理"));
        mapPermission.AddChild(AbpOverallAuthPermissions.LocationMap.Create, L("Permission:点位映射创建"));
        mapPermission.AddChild(AbpOverallAuthPermissions.LocationMap.Edit, L("Permission:点位映射编辑"));
        mapPermission.AddChild(AbpOverallAuthPermissions.LocationMap.Delete,L("Permission:点位映射删除"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AbpOverallAuthResource>(name);
    }
}
