using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbpOverallAuth.Navigation
{
    public class CreateMenuDto
    {
        /// <summary>
        /// 菜单名
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// 菜单图标
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 菜单路径
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 组件路径
        /// </summary>
        public string? Component { get; set; }


        /// <summary>
        /// 父菜单ID
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// 权限名称
        /// </summary>
        public string? PermissionName { get; set; }

        /// <summary>
        /// 菜单类型
        /// </summary>
        public MenuType MenuType { get; set; }

        /// <summary>
        /// 是否外链
        /// </summary>
        public bool IsExternal { get; set; }

        /// <summary>
        /// 外链地址
        /// </summary>
        public string? ExternalUrl { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
