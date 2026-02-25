using System;
using Volo.Abp.Application.Dtos;

namespace RCS.Dtos.Logs
{
    public class InternalApiLogPagedRequestDto : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 模糊查询字段（可搜索URL、用户名、客户端IP等）
        /// </summary>
        public string? Filter { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// HTTP方法
        /// </summary>
        public string? HttpMethod { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 最小执行时间（毫秒）
        /// </summary>
        public int? MinExecutionDuration { get; set; }

        /// <summary>
        /// 最大执行时间（毫秒）
        /// </summary>
        public int? MaxExecutionDuration { get; set; }

        /// <summary>
        /// 是否有异常
        /// </summary>
        public bool? HasException { get; set; }

        /// <summary>
        /// HTTP状态码
        /// </summary>
        public int? HttpStatusCode { get; set; }
    }
}
