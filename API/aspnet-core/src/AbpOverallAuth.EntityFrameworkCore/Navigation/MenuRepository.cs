using AbpOverallAuth.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AbpOverallAuth.Navigation
{
    public class MenuRepository : EfCoreRepository<AbpOverallAuthDbContext, Menu, Guid>, IMenuRepository
    {
        public MenuRepository(IDbContextProvider<AbpOverallAuthDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<Menu>> GetAllMenus(CancellationToken cancellationToken = default)
        {
            var query = await GetQueryableAsync();
            return await query
                .Where(x => x.IsEnabled && !x.IsHidden)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.CreationTime)
                .ToListAsync(cancellationToken);
        }
    }
}
