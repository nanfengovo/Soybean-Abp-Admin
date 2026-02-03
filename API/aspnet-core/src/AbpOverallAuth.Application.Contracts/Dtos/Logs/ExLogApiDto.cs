using System;
using Volo.Abp.Application.Dtos;

namespace AbpOverallAuth.Dtos.Logs
{
    public class ExLogApiDto : EntityDto<long>
    {
        public string SysName { get; set; } = string.Empty;
        public string? BusinessId { get; set; }
        public string? BusinessType { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? Path { get; set; }
        public string HttpMethod { get; set; } = string.Empty;
        public string? RequestHeaders { get; set; }
        public string? RequestBody { get; set; }
        public int StatusCode { get; set; }
        public string? ResponseBody { get; set; }
        public string? ResponseHeaders { get; set; }
        public long Duration { get; set; }
        public string? ClientIpAddress { get; set; }
        public string? TraceId { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorStackTrace { get; set; }
        public Guid? UserId { get; set; }
        public Guid? TenantId { get; set; }
        public string? ExtraData { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid? CreatorId { get; set; }
    }
}
