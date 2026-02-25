using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace AbpOverallAuth.DataManage
{
    public class LocationMap:AuditedAggregateRoot<int>
    {
        /// <summary>
        /// 配置名
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 机器点位
        /// </summary>
        [Required]
        public string MachinePoint { get; set; } = string.Empty;

        /// <summary>
        /// Agv点位
        /// </summary>
        public string AgvPoint { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// EFCore用
        /// </summary>
        public LocationMap()
        {
        }

        public LocationMap(string? name, string machinePoint, string agvPoint, string? description)
        {
            Name = name;
            MachinePoint = machinePoint;
            AgvPoint = agvPoint;
            Description = description;
        }
    }
}
