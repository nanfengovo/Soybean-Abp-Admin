using AbpOverallAuth.Enums.InternalTask;
using AbpOverallAuth.Interfaces;
using AbpOverallAuth.TaskManage.InternalTask;
using AbpOverallAuth.ThirdParty.TM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;

namespace AbpOverallAuth.TM
{
    public class FetchPutTaskFlowBuilder 
    {


        public TMTaskDto Build(InternalTask task)
        {
            var dto = CreateBaseDto(task);

            switch (task.FlowType)
            {
                case TaskFlowType.FetchPut:
                    // Fetch -> Put
                    dto.Sub_Task.Add(BuildSubTask(task, task.FromAddress, 0, 0, SubTaskAction.Fetch, 1));
                    dto.Sub_Task.Add(BuildSubTask(task, task.ToAddress, 0, 0, SubTaskAction.Put, 1));
                    break;

                case TaskFlowType.FetchPutPut:
                    // Fetch -> Put1(到中间点) -> Put2(到终点)
                    dto.Sub_Task.Add(BuildSubTask(task, task.FromAddress, 0, 0, SubTaskAction.Fetch, 1));
                    dto.Sub_Task.Add(BuildSubTask(task, task.MiddleAddress ?? task.ToAddress, 0, 0, SubTaskAction.Put, 1));
                    dto.Sub_Task.Add(BuildSubTask(task, task.ToAddress, 0, 0, SubTaskAction.Put, 2));
                    break;

                default:
                    throw new BusinessException("UnsupportedFlowType")
                        .WithData("FlowType", task.FlowType);
            }

            return dto;
        }

        private sub_task BuildSubTask(
            InternalTask task,
            string address,
            int taskcode1,
            int taskcode2,
            SubTaskAction action,
            int sequence)
        {
            var subTask = new sub_task
            {
                AGV_Serial = 0,
                Robot_Type = "99",
                Succession = 0,
                Area_Property = new List<string>(),
                Cargo_Id = "#",
                Complete_Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Pre_Report = "0",
                Priority = 0,
                Mark = "3",
                Option_Code = $"{taskcode1},{taskcode2}",
                Task_Serial = $"{task.Id}_{action}{sequence}"
            };

            subTask.goal_action = action switch
            {
                SubTaskAction.Fetch => 0,
                SubTaskAction.Put => 1,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (!int.TryParse(address, out int target))
            {
                throw new BusinessException("InvalidAddress")
                    .WithData("Address", address)
                    .WithData("TaskId", task.Id);
            }

            subTask.Storage = address;
            subTask.Target = target;

            return subTask;
        }

        private TMTaskDto CreateBaseDto(InternalTask task)
        {
            var Bulk_Task_Count = 1;
            switch (task.FlowType)
            {
                case TaskFlowType.FetchPut:
                    Bulk_Task_Count = 2;
                    break;
                case TaskFlowType.FetchPutPut:
                    Bulk_Task_Count = 3;
                    break;
                default:
                    // 保持默认值 1
                    break;
            }
            return new TMTaskDto
            {
                Bulk_Task_Count = Bulk_Task_Count,
                Bulk_Task_Type = "task",
                Sub_Task = new List<sub_task>()
            };
        }
    }

}
