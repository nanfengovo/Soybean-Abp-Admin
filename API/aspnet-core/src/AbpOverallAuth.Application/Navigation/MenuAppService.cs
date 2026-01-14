using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace AbpOverallAuth.Navigation
{
    public class MenuAppService : ApplicationService, IMenuAppService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IPermissionManager _permissionManager;
        private readonly DynamicPermissionManager _dynamicPermissionManager;

        public MenuAppService(
            IMenuRepository menuRepository,
            IPermissionManager permissionManager,
            DynamicPermissionManager dynamicPermissionManager)
        {
            _menuRepository = menuRepository;
            _permissionManager = permissionManager;
            _dynamicPermissionManager = dynamicPermissionManager;
        }

        [UnitOfWork]
        public async Task<MenuDto> CreateAsync(CreateMenuDto input)
        {
            if (!input.PermissionName.IsNullOrWhiteSpace())
            {
                var exists = await _menuRepository.AnyAsync(
                    x => x.PermissionName == input.PermissionName);

                if (exists)
                {
                    throw new UserFriendlyException("该权限已经存在！");
                }
            }

            var menu = new Menu(GuidGenerator.Create())
            {
                Name = input.Name,
                Icon = input.Icon,
                Path = input.Path,
                Component = input.Component,
                ParentId = input.ParentId,
                SortOrder = input.SortOrder,
                IsHidden = input.IsHidden,
                PermissionName = input.PermissionName,
                MenuType = input.MenuType,
                IsExternal = input.IsExternal,
                ExternalUrl = input.ExternalUrl,
                Remark = input.Remark
            };

            await _menuRepository.InsertAsync(menu);

            // 只有存在权限名时才处理权限
            if (!input.PermissionName.IsNullOrWhiteSpace())
            {
                // 1. 动态注册权限到 ABP 权限系统
                await _dynamicPermissionManager.RegisterPermissionAsync(
                    input.PermissionName,
                    input.Name
                );

                // 2. 自动授予管理员角色该权限
                var adminRoleName = "admin";
                await _permissionManager.SetForRoleAsync(
                    adminRoleName,
                    input.PermissionName,
                    true
                );
            }

            return ObjectMapper.Map<Menu, MenuDto>(menu);
        }

        [UnitOfWork]
        public async Task<MenuDto> UpdateAsync(Guid id, CreateMenuDto input)
        {
            var menu = await _menuRepository.GetAsync(id);
            var oldPermissionName = menu.PermissionName;

            // 检查权限名是否被其他菜单使用
            if (!input.PermissionName.IsNullOrWhiteSpace() &&
                input.PermissionName != oldPermissionName)
            {
                var exists = await _menuRepository.AnyAsync(
                    x => x.Id != id && x.PermissionName == input.PermissionName);

                if (exists)
                {
                    throw new UserFriendlyException("该权限已经被其他菜单使用！");
                }
            }

            // 更新菜单信息
            menu.Name = input.Name;
            menu.Icon = input.Icon;
            menu.Path = input.Path;
            menu.Component = input.Component;
            menu.ParentId = input.ParentId;
            menu.SortOrder = input.SortOrder;
            menu.IsHidden = input.IsHidden;
            menu.PermissionName = input.PermissionName;
            menu.MenuType = input.MenuType;
            menu.IsExternal = input.IsExternal;
            menu.ExternalUrl = input.ExternalUrl;
            menu.Remark = input.Remark;

            await _menuRepository.UpdateAsync(menu);

            // 处理权限变更
            if (oldPermissionName != input.PermissionName)
            {
                // 如果旧权限存在，删除它（可选：也可以保留）
                if (!oldPermissionName.IsNullOrWhiteSpace())
                {
                    await _dynamicPermissionManager.UnregisterPermissionAsync(oldPermissionName);
                }

                // 注册新权限
                if (!input.PermissionName.IsNullOrWhiteSpace())
                {
                    await _dynamicPermissionManager.RegisterPermissionAsync(
                        input.PermissionName,
                        input.Name
                    );

                    // 授予管理员角色
                    await _permissionManager.SetForRoleAsync(
                        "admin",
                        input.PermissionName,
                        true
                    );
                }
            }

            return ObjectMapper.Map<Menu, MenuDto>(menu);
        }

        [UnitOfWork]
        public async Task DeleteAsync(Guid id)
        {
            var menu = await _menuRepository.GetAsync(id);

            // 删除权限
            if (!menu.PermissionName.IsNullOrWhiteSpace())
            {
                await _dynamicPermissionManager.UnregisterPermissionAsync(menu.PermissionName);
            }

            await _menuRepository.DeleteAsync(id);
        }

        public async Task<List<MenuDto>> GetAllAsync()
        {
            var menus = await _menuRepository.GetListAsync();
            return ObjectMapper.Map<List<Menu>, List<MenuDto>>(menus);
        }

        public async Task SyncPermissionsAsync()
        {
            await _dynamicPermissionManager.RegisterPermissionsFromMenusAsync();
        }

        public async Task<List<MenuDto>> GetMyMenusAsync()
        {
            var menus = await _menuRepository.GetListAsync(x => !x.IsHidden && x.IsEnabled);
            var result = new List<MenuDto>();

            foreach (var menu in menus)
            {
                if (menu.PermissionName.IsNullOrWhiteSpace() ||
                    await AuthorizationService.IsGrantedAsync(menu.PermissionName))
                {
                    result.Add(ObjectMapper.Map<Menu, MenuDto>(menu));
                }
            }

            return BuildTree(result); // 按 ParentId 组装树
        }

        private List<MenuDto> BuildTree(List<MenuDto> menus)
        {
            var menuDict = menus.ToDictionary(m => m.Id);
            var roots = new List<MenuDto>();

            foreach (var menu in menus)
            {
                if (menu.ParentId.HasValue &&
                    menuDict.TryGetValue(menu.ParentId.Value, out var parent))
                {
                    parent.Children.Add(menu);
                }
                else
                {
                    // ParentId 为空 或 父节点不存在 → 根节点
                    roots.Add(menu);
                }
            }

            // 递归排序
            SortMenus(roots);

            return roots;
        }
        private void SortMenus(List<MenuDto> menus)
        {
            menus.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));

            foreach (var menu in menus)
            {
                if (menu.Children.Any())
                {
                    SortMenus(menu.Children);
                }
            }
        }

    }
}
