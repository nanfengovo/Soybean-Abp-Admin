using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;

namespace AbpOverallAuth.LogManage.APILogs
{
    public class ThirdPartyCallLog : CreationAuditedEntity<long>
    {
        /// <summary>
        /// 系统名称/客户端名称
        /// </summary>
        public string SysName { get; set; } = string.Empty;

        /// <summary>
        /// 业务标识，用于关联业务数据
        /// </summary>
        public string? BusinessId { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string? BusinessType { get; set; }

        /// <summary>
        /// 完整请求URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 相对路径（不含域名）
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// HTTP方法（GET/POST/PUT/DELETE等）
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// 请求头（JSON格式）
        /// </summary>
        public string? RequestHeaders { get; set; }

        /// <summary>
        /// 请求体内容
        /// </summary>
        public string? RequestBody { get; set; }

        /// <summary>
        /// 响应状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 响应体内容
        /// </summary>
        public string? ResponseBody { get; set; }

        /// <summary>
        /// 响应头（JSON格式）
        /// </summary>
        public string? ResponseHeaders { get; set; }

        /// <summary>
        /// 调用耗时（毫秒）
        /// </summary>
        public long Duration { get; set; }

        /// <summary>
        /// 客户端IP地址
        /// </summary>
        public string? ClientIpAddress { get; set; }

        /// <summary>
        /// 跟踪ID（用于关联请求链）
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// 是否成功调用
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 错误信息（如果失败）
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 错误堆栈（如果失败）
        /// </summary>
        public string? ErrorStackTrace { get; set; }

        /// <summary>
        /// 用户ID（发起调用的用户）
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 额外数据（JSON格式）
        /// </summary>
        public string? ExtraData { get; set; }

        public ThirdPartyCallLog()
        {
        }
    }
}
