namespace RCS.Permissions;

public static class RCSPermissions
{
    public const string GroupName = "RCS";

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";

    /// <summary>
    /// 地图点位映射模块
    /// </summary>
    public static class LocationMap
    {
        /// <summary>
        /// 查看
        /// </summary>
        public const string Default = GroupName + ".LocationMap";

        /// <summary>
        /// 创建
        /// </summary>
        public const string Create = Default + ".Create";

        /// <summary>
        /// 编辑
        /// </summary>

        public const string Edit = Default + ".Edit";


        /// <summary>
        /// 删除
        /// </summary>

        public const string Delete = Default + ".Delete";
    }

    /// <summary>
    /// 内部任务管理模块
    /// </summary>
    public static class InTask
    {
        public const string Default = GroupName + ".InTask";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
}
