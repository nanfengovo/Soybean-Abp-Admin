using AbpOverallAuth.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.PermissionManagement;

namespace AbpOverallAuth.Data
{
    public class OverAllAuthDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<Menu, Guid> _menuRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IPermissionManager _permissionManager;

        public OverAllAuthDataSeedContributor(IRepository<Menu, Guid> menuRepository, IGuidGenerator guidGenerator, IPermissionManager permissionManager)
        {
            _menuRepository = menuRepository;
            _guidGenerator = guidGenerator;
            _permissionManager = permissionManager;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if(await _menuRepository.GetCountAsync() > 0)
            {
                return;
            }

            //1、初始化基本的菜单包括仪表盘菜单、系统管理菜单、菜单管理，用户管理，角色管理
            await _menuRepository.InsertAsync(new Menu(_guidGenerator.Create())
            {
                Name = "仪表盘",
                Icon = "dashboard",
                Path = "/dashboard",
                Component = "Dashboard",
                SortOrder = 1,
                MenuType = MenuType.menu,
                IsHidden = false,
                PermissionName = "",
                Remark = "公开菜单不需要权限",
            });

            var systemMenu = await _menuRepository.InsertAsync(new Menu(_guidGenerator.Create())
            {
                Name = "系统管理",
                Icon = "setting",
                Path = "/system",
                Component = "System",
                SortOrder = 2,
                MenuType = MenuType.directory,
                IsHidden = false,
                PermissionName = "menu-overallauth",
                Remark = "需要menu-overallauth权限",
            });

            await _menuRepository.InsertAsync(new Menu(_guidGenerator.Create())
            {
                Name = "菜单管理",
                Icon = "menu",
                Path = "/system/menus",
                Component = "Menu",
                SortOrder = 3,
                ParentId = systemMenu.Id,
                MenuType = MenuType.menu,
                IsHidden = false,
                PermissionName = "menu-overallauth-menus",
                Remark = "需要menu-overallauth-menus权限",
            });

            await _menuRepository.InsertAsync(new Menu(_guidGenerator.Create())
            {
                Name = "用户管理",
                Icon = "user",
                Path = "/system/users",
                Component = "User",
                SortOrder = 4,
                ParentId = systemMenu.Id,
                MenuType = MenuType.menu,
                IsHidden = false,
                PermissionName = "menu-overallauth-users",
                Remark = "需要menu-overallauth-users权限",
            });

            await _menuRepository.InsertAsync(new Menu(_guidGenerator.Create())
            {
                Name = "角色管理",
                Icon = "team",
                Path = "/system/roles",
                Component = "Role",
                SortOrder = 5,
                ParentId = systemMenu.Id,
                MenuType = MenuType.menu,
                IsHidden = false,
                PermissionName = "menu-overallauth-roles",
                Remark = "需要menu-overallauth-roles权限",
            });

            //2、给上述的菜单默认分配给admin角色
            string adminRoleName = "admin";

            var permissions = new[]
            {
                "menu-overallauth",
                "menu-overallauth-menus",
                "menu-overallauth-users",
                "menu-overallauth-roles"
            };

            foreach (var permission in permissions)
            {
                // SetAsync 会在 AbpPermissionGrants 表中插入记录
                await _permissionManager.SetForRoleAsync(adminRoleName, permission, isGranted: true);
            }

        }
    }
}
