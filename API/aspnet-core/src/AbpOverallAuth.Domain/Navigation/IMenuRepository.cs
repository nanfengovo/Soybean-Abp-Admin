using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace AbpOverallAuth.Navigation
{
    public interface IMenuRepository: IRepository<Menu, Guid>
    {
        /// <summary>
        /// 获取所有已启用的菜单
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<Menu>> GetAllMenus(CancellationToken cancellationToken = default);
    }
}
