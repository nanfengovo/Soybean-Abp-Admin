using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization;

namespace AbpOverallAuth.Navigation
{
    public class SimplePermissionDefinition
    {
        public string Name { get; set; }
        public ILocalizableString DisplayName { get; set; }

        public SimplePermissionDefinition(string name, ILocalizableString displayName)
        {
            Name = name;
            DisplayName = displayName;
        }
    }

    /// <summary>
    /// 动态权限定义存储 - 提供运行时权限定义管理
    /// </summary>
    public class DynamicPermissionDefinitionStore : ISingletonDependency
    {
        private readonly ConcurrentDictionary<string, SimplePermissionDefinition> _permissions;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<DynamicPermissionDefinitionStore> _logger;
        private bool _initialized = false;
        private readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);

        public DynamicPermissionDefinitionStore(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<DynamicPermissionDefinitionStore> logger)
        {
            _permissions = new ConcurrentDictionary<string, SimplePermissionDefinition>();
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有动态权限定义
        /// </summary>
        public async Task<List<SimplePermissionDefinition>> GetPermissionsAsync()
        {
            await EnsureInitializedAsync();
            return _permissions.Values.ToList();
        }

        /// <summary>
        /// 添加权限定义
        /// </summary>
        public void AddPermission(SimplePermissionDefinition permission)
        {
            _permissions.TryAdd(permission.Name, permission);
        }

        /// <summary>
        /// 移除权限定义
        /// </summary>
        public void RemovePermission(string permissionName)
        {
            _permissions.TryRemove(permissionName, out _);
        }

        /// <summary>
        /// 清空所有权限
        /// </summary>
        public void Clear()
        {
            _permissions.Clear();
            _initialized = false;
        }

        /// <summary>
        /// 重新加载权限
        /// </summary>
        public async Task ReloadAsync()
        {
            Clear();
            await EnsureInitializedAsync();
        }

        /// <summary>
        /// 确保权限已初始化
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            if (_initialized)
            {
                return;
            }

            await _syncLock.WaitAsync();
            try
            {
                if (_initialized)
                {
                    return;
                }

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    try
                    {
                        var menuRepository = scope.ServiceProvider.GetRequiredService<IMenuRepository>();
                        var menus = await menuRepository.GetListAsync();
                        
                        foreach (var menu in menus.Where(m => !string.IsNullOrWhiteSpace(m.PermissionName)))
                        {
                            var permission = new SimplePermissionDefinition(
                                menu.PermissionName,
                                LocalizableString.Create<Localization.AbpOverallAuthResource>($"Permission:{menu.Name}")
                            );
                            _permissions.TryAdd(permission.Name, permission);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "从数据库加载动态权限失败");
                    }
                }

                _initialized = true;
            }
            finally
            {
                _syncLock.Release();
            }
        }
    }
}
