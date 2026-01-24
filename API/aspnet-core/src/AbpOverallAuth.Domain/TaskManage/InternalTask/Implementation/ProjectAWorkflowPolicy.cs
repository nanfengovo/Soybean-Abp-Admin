using AbpOverallAuth.Enums.InternalTask;
using AbpOverallAuth.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskStatus = AbpOverallAuth.Enums.InternalTask.TaskStatus;

namespace AbpOverallAuth.TaskManage.InternalTask.Implementation
{
    public class ProjectAWorkflowPolicy : ITaskWorkflowPolicy
    {
        public (bool IsValid, TaskStatus NextStatus, string Message) GetNextState(TaskStatus current, TaskAction action)
        {
            return (current, action) switch
            {
                (TaskStatus.Init, TaskAction.Dispatch) => (true, TaskStatus.Pressed, "将创建的任务派发给调度"),
                (TaskStatus.Pressed, TaskAction.ArriveSource) => (true, TaskStatus.PreFetching, "去第一个任务的起点途中"),
                (TaskStatus.PreFetching,TaskAction.BeginFetch) => (true, TaskStatus.Fetching, "开始取料"),
                (TaskStatus.Fetching, TaskAction.FinishFetch) => (true, TaskStatus.Fetched, "完成取料"),
                (TaskStatus.Fetched,TaskAction.ArriveTarget) => (true, TaskStatus.PrePutting, "到达第一个任务的终点申请放料"),
                (TaskStatus.PrePutting, TaskAction.BeginPut) => (true, TaskStatus.Putting, "开始放料"),
                (TaskStatus.Putting, TaskAction.FinishPut) => (true, TaskStatus.Putted, "完成放料"),
                (TaskStatus.Putted,TaskAction.Over) => (true, TaskStatus.Finished, "任务完成"),
                // 这里可以针对项目 A 特有的逻辑进行配置
                _ => (false, current, "当前状态不允许此操作")
            };
        }
    }
}
