using RCS.Localization;
using System.Linq;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace RCS.Permissions;

/// <summary>
/// 权限定义提供者 - 从动态存储加载权限
/// </summary>
public class RCSPermissionDefinitionProvider : PermissionDefinitionProvider
{

    public override void Define(IPermissionDefinitionContext context)
    {
        // 定义权限组
        var myGroup = context.AddGroup(RCSPermissions.GroupName, L("Permission:RCS"));

        //LocationMap权限树
        var mapPermission = myGroup.AddPermission(RCSPermissions.LocationMap.Default, L("Permission:点位映射管理"));
        mapPermission.AddChild(RCSPermissions.LocationMap.Create, L("Permission:点位映射创建"));
        mapPermission.AddChild(RCSPermissions.LocationMap.Edit, L("Permission:点位映射编辑"));
        mapPermission.AddChild(RCSPermissions.LocationMap.Delete,L("Permission:点位映射删除"));

        var InTask = myGroup.AddPermission(RCSPermissions.InTask.Default, L("Permission:内部任务管理"));
        InTask.AddChild(RCSPermissions.InTask.Create, L("Permission:内部任务创建"));
        InTask.AddChild(RCSPermissions.InTask.Edit, L("Permission:内部任务编辑"));
        InTask.AddChild(RCSPermissions.InTask.Delete, L("Permission:内部任务删除"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<RCSResource>(name);
    }
}
