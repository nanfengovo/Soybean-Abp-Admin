namespace AbpOverallAuth.Permissions;

public static class AbpOverallAuthPermissions
{
    public const string GroupName = "AbpOverallAuth";

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
}
