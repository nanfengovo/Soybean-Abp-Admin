using System;
using System.Linq;
using System.Threading.Tasks;
using RCS.Dtos.Logs;
using Volo.Abp.Application.Services;
using Volo.Abp.AuditLogging;
using Volo.Abp.Domain.Repositories;

namespace RCS.LogManage
{
    public class InternalApiLogService : CrudAppService<
        AuditLog,
        InternalApiLogDto,
        Guid,
        InternalApiLogPagedRequestDto>
    {
        public InternalApiLogService(IRepository<AuditLog, Guid> repository) : base(repository)
        {
            // 配置权限（如果需要）
            // GetPolicyName = RCSPermissions.InternalApiLog.Default;
            // GetListPolicyName = RCSPermissions.InternalApiLog.Default;
        }

        protected override async Task<IQueryable<AuditLog>> CreateFilteredQueryAsync(InternalApiLogPagedRequestDto input)
        {
            var query = await base.CreateFilteredQueryAsync(input);

            // 模糊查询
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                query = query.Where(x =>
                    (x.Url != null && x.Url.Contains(input.Filter)) ||
                    (x.UserName != null && x.UserName.Contains(input.Filter)) ||
                    (x.ClientIpAddress != null && x.ClientIpAddress.Contains(input.Filter)) ||
                    (x.ClientName != null && x.ClientName.Contains(input.Filter)) ||
                    (x.ApplicationName != null && x.ApplicationName.Contains(input.Filter))
                );
            }

            // 时间范围过滤
            if (input.StartTime.HasValue)
            {
                query = query.Where(x => x.ExecutionTime >= input.StartTime.Value);
            }

            if (input.EndTime.HasValue)
            {
                query = query.Where(x => x.ExecutionTime <= input.EndTime.Value);
            }

            // HTTP方法过滤
            if (!string.IsNullOrWhiteSpace(input.HttpMethod))
            {
                query = query.Where(x => x.HttpMethod == input.HttpMethod);
            }

            // 用户ID过滤
            if (input.UserId.HasValue)
            {
                query = query.Where(x => x.UserId == input.UserId.Value);
            }

            // 执行时间过滤
            if (input.MinExecutionDuration.HasValue)
            {
                query = query.Where(x => x.ExecutionDuration >= input.MinExecutionDuration.Value);
            }

            if (input.MaxExecutionDuration.HasValue)
            {
                query = query.Where(x => x.ExecutionDuration <= input.MaxExecutionDuration.Value);
            }

            // 异常过滤
            if (input.HasException.HasValue)
            {
                if (input.HasException.Value)
                {
                    query = query.Where(x => x.Exceptions != null && x.Exceptions != "");
                }
                else
                {
                    query = query.Where(x => x.Exceptions == null || x.Exceptions == "");
                }
            }

            // HTTP状态码过滤
            if (input.HttpStatusCode.HasValue)
            {
                query = query.Where(x => x.HttpStatusCode == input.HttpStatusCode.Value);
            }

            return query;
        }
    }
}
