using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace AbpOverallAuth.Navigation
{
    /// <summary>
    /// 权限同步器 - 应用启动时同步数据库中的菜单权限
    /// </summary>
    public class PermissionSynchronizer : ITransientDependency
    {
        private readonly DynamicPermissionManager _dynamicPermissionManager;
        private readonly ILogger<PermissionSynchronizer> _logger;

        public PermissionSynchronizer(
            DynamicPermissionManager dynamicPermissionManager,
            ILogger<PermissionSynchronizer> logger)
        {
            _dynamicPermissionManager = dynamicPermissionManager;
            _logger = logger;
        }

        /// <summary>
        /// 同步所有菜单权限到权限系统
        /// </summary>
        public async Task SyncAllPermissionsAsync()
        {
            try
            {
                _logger.LogInformation("开始同步菜单权限到权限系统...");

                await _dynamicPermissionManager.RegisterPermissionsFromMenusAsync();

                _logger.LogInformation("菜单权限同步完成！");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "同步菜单权限失败");
                throw;
            }
        }
    }
}
