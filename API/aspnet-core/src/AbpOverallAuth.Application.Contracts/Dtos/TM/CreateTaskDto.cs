using AbpOverallAuth.Enums.InternalTask;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using TaskStatus = AbpOverallAuth.Enums.InternalTask.TaskStatus;

namespace AbpOverallAuth.Dtos.TM
{
    public class CreateTaskDto
    {
        public TaskType TaskType { get; set; } = TaskType.None;

        public TaskFlowType FlowType { get; set; }

        public string FromAddress { get; set; }

        public string? MiddleAddress { get; set; }

        public string ToAddress { get; set; }

        public int FetchCount { get; set; }

        public int PutCount { get; set; }

        public ContainerType? FetchType { get; set; }

        public ContainerType? PutType { get; set; }

        public MachineType FetchMachineType { get; set; }

        public MachineType PutMachineType { get;set; }
    }
}
