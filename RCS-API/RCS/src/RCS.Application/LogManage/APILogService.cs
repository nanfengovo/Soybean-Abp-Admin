using System.Linq;
using System.Threading.Tasks;
using RCS.Dtos.Logs;
using RCS.LogManage.APILogs;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace RCS.LogManage
{
    public class APILogService : CrudAppService<
        ThirdPartyCallLog,
        ExLogApiDto,
        long,
        ExLogApiPagedRequestDto>
    {
        public APILogService(IRepository<ThirdPartyCallLog, long> repository) : base(repository)
        {
            // 如果需要权限控制，可以在这里配置
            // GetPolicyName = RCSPermissions.APILog.Default;
            // GetListPolicyName = RCSPermissions.APILog.Default;
        }

        protected override async Task<IQueryable<ThirdPartyCallLog>> CreateFilteredQueryAsync(ExLogApiPagedRequestDto input)
        {
            var query = await base.CreateFilteredQueryAsync(input);

            // 如果有Filter参数，进行模糊查询
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                query = query.Where(x =>
                    x.SysName.Contains(input.Filter) ||
                    x.Url.Contains(input.Filter) ||
                    (x.ErrorMessage != null && x.ErrorMessage.Contains(input.Filter)) ||
                    (x.BusinessId != null && x.BusinessId.Contains(input.Filter))
                );
            }

            return query;
        }
    }
}
