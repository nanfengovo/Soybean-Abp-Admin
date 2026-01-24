using AbpOverallAuth.Enums.InternalTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskStatus = AbpOverallAuth.Enums.InternalTask.TaskStatus;

namespace AbpOverallAuth.XinSong.TM
{
    public class TaskCreationArgs
    {
        public string Id { get; set; }
        public TaskType TaskType { get; set; } = TaskType.None;

        public string FromAddress { get; set; }

        public string? MiddleAddress { get; set; }

        public string ToAddress { get; set; }

        public TaskStatus TaskStatus { get; set; } = TaskStatus.Init;

        public int FetchCount { get; set; }

        public int PutCount { get; set; }

        public ContainerType FetchType { get; set; }

        public ContainerType PutType { get; set; }

        public MachineType FetchMachineType { get; set; }

        public MachineType PutMachineType { get; set; }
    }
}
