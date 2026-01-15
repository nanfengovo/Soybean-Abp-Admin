using AbpOverallAuth.DataManage;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace AbpOverallAuth.Dtos.DataManage
{
    /// <summary>
    /// 查询 / 返回 DTO
    /// </summary>
    public class LocationMapDto:AuditedEntityDto<int>
    {
        public string? Name { get; set; }
        public string MachinePoint { get; set; } = string.Empty;
        public string AgvPoint { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
