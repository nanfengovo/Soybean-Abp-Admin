using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AbpOverallAuth.Navigation
{
    public interface IMenuAppService:IApplicationService
    {
        /// <summary>
        /// 创建新菜单
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<MenuDto> CreateAsync(CreateMenuDto input);

        /// <summary>
        /// 更新菜单
        /// </summary>
        Task<MenuDto> UpdateAsync(Guid id, CreateMenuDto input);

        /// <summary>
        /// 删除菜单
        /// </summary>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// 获取所有菜单（管理用）
        /// </summary>
        Task<List<MenuDto>> GetAllAsync();

        /// <summary>
        /// 获取当前登录用户的菜单
        /// </summary>
        /// <returns></returns>
        Task<List<MenuDto>> GetMyMenusAsync();

        /// <summary>
        /// 同步所有菜单权限到权限系统
        /// </summary>
        Task SyncPermissionsAsync();
    }
}
