using System;
using Volo.Abp.Application.Dtos;

namespace RCS.Dtos.Logs
{
    public class InternalApiLogDto : EntityDto<Guid>
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        public string? ApplicationName { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 租户名称
        /// </summary>
        public string? TenantName { get; set; }

        /// <summary>
        /// 模拟用户ID
        /// </summary>
        public Guid? ImpersonatorUserId { get; set; }

        /// <summary>
        /// 模拟租户ID
        /// </summary>
        public Guid? ImpersonatorTenantId { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime ExecutionTime { get; set; }

        /// <summary>
        /// 执行耗时（毫秒）
        /// </summary>
        public int ExecutionDuration { get; set; }

        /// <summary>
        /// 客户端IP地址
        /// </summary>
        public string? ClientIpAddress { get; set; }

        /// <summary>
        /// 客户端名称
        /// </summary>
        public string? ClientName { get; set; }

        /// <summary>
        /// 客户端ID
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// 关联ID
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// 浏览器信息
        /// </summary>
        public string? BrowserInfo { get; set; }

        /// <summary>
        /// HTTP方法
        /// </summary>
        public string? HttpMethod { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string? Exceptions { get; set; }

        /// <summary>
        /// HTTP状态码
        /// </summary>
        public int? HttpStatusCode { get; set; }

        /// <summary>
        /// 注释
        /// </summary>
        public string? Comments { get; set; }
    }
}
