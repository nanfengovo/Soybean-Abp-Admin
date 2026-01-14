using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace AbpOverallAuth.Navigation
{
    /// <summary>
    /// 动态权限管理器 - 运行时动态注册和删除权限
    /// </summary>
    public class DynamicPermissionManager : ITransientDependency
    {
        private readonly DynamicPermissionDefinitionStore _definitionStore;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<DynamicPermissionManager> _logger;

        public DynamicPermissionManager(
            DynamicPermissionDefinitionStore definitionStore,
            IDistributedCache distributedCache,
            ILogger<DynamicPermissionManager> logger)
        {
            _definitionStore = definitionStore;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        /// <summary>
        /// 动态注册权限
        /// </summary>
        /// <param name="permissionName">权限名称</param>
        /// <param name="displayName">显示名称</param>
        /// <returns></returns>
        public async Task<bool> RegisterPermissionAsync(string permissionName, string displayName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
            {
                return false;
            }

            try
            {
                // 添加到动态存储
                var permission = new SimplePermissionDefinition(
                    permissionName,
                    Volo.Abp.Localization.LocalizableString.Create<Localization.AbpOverallAuthResource>($"Permission:{displayName}")
                );
                _definitionStore.AddPermission(permission);

                // 清除权限缓存，使新权限生效
                await ClearPermissionCacheAsync();

                _logger.LogInformation($"成功注册权限: {permissionName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"注册权限失败: {permissionName}");
                throw new BusinessException(
                    code: "DynamicPermission:RegisterFailed",
                    message: $"动态注册权限失败: {permissionName}",
                    innerException: ex
                );
            }
        }

        /// <summary>
        /// 动态删除权限
        /// </summary>
        /// <param name="permissionName">权限名称</param>
        /// <returns></returns>
        public async Task<bool> UnregisterPermissionAsync(string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
            {
                return false;
            }

            try
            {
                // 从动态存储中移除
                _definitionStore.RemovePermission(permissionName);

                // 清除权限缓存
                await ClearPermissionCacheAsync();

                _logger.LogInformation($"成功删除权限: {permissionName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除权限失败: {permissionName}");
                throw new BusinessException(
                    code: "DynamicPermission:UnregisterFailed",
                    message: $"动态删除权限失败: {permissionName}",
                    innerException: ex
                );
            }
        }

        /// <summary>
        /// 批量注册权限（从数据库加载所有菜单权限）
        /// </summary>
        public async Task RegisterPermissionsFromMenusAsync()
        {
            try
            {
                // 重新加载权限定义
                await _definitionStore.ReloadAsync();

                // 清除权限缓存
                await ClearPermissionCacheAsync();

                _logger.LogInformation("成功从菜单同步所有权限");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从菜单同步权限失败");
                throw;
            }
        }

        /// <summary>
        /// 清除权限缓存
        /// </summary>
        private async Task ClearPermissionCacheAsync()
        {
            // 清除 ABP 权限缓存
            var cacheKeys = new[]
            {
                "pn:prov",  // 权限定义提供者缓存键
                "AbpPermissions"
            };

            foreach (var key in cacheKeys)
            {
                try
                {
                    await _distributedCache.RemoveAsync(key);
                    _logger.LogDebug($"清除缓存: {key}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"清除缓存失败: {key}");
                }
            }
        }
    }
}
