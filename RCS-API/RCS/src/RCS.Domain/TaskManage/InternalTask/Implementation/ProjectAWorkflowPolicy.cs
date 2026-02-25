using RCS.Enums.InternalTask;
using RCS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskStatus = RCS.Enums.InternalTask.TaskStatus;

namespace RCS.TaskManage.InternalTask.Implementation
{
    public class ProjectAWorkflowPolicy : ITaskWorkflowPolicy
    {
        public (bool IsValid, TaskStatus NextStatus, string Message) GetNextState(TaskStatus current, TaskAction action)
        {
            return (current, action) switch
            {
                // 派发任务
                (TaskStatus.Init, TaskAction.Dispatch) => (true, TaskStatus.Pressed, "将创建的任务派发给调度"),

                // 取料流程: Pressed -> ToFetching -> PreFetching -> Fetching -> Fetched
                (TaskStatus.Pressed, TaskAction.ArriveSource) => (true, TaskStatus.ToFetching, "AGV出发去取料点"),
                (TaskStatus.ToFetching, TaskAction.ArriveSource) => (true, TaskStatus.PreFetching, "到达取料点，申请取料"),
                (TaskStatus.PreFetching, TaskAction.BeginFetch) => (true, TaskStatus.Fetching, "开始取料"),
                (TaskStatus.Fetching, TaskAction.FinishFetch) => (true, TaskStatus.Fetched, "完成取料"),

                // 放料流程: Fetched -> ToPutting -> PrePutting -> Putting -> Putted
                (TaskStatus.Fetched, TaskAction.ArriveTarget) => (true, TaskStatus.ToPutting, "AGV出发去放料点"),
                (TaskStatus.ToPutting, TaskAction.ArriveTarget) => (true, TaskStatus.PrePutting, "到达放料点，申请放料"),
                (TaskStatus.PrePutting, TaskAction.BeginPut) => (true, TaskStatus.Putting, "开始放料"),
                (TaskStatus.Putting, TaskAction.FinishPut) => (true, TaskStatus.Putted, "完成放料"),

                // FetchPutPut: 第一次放料完成后，再次去下一个放料点
                (TaskStatus.Putted, TaskAction.ArriveTarget) => (true, TaskStatus.ToPutting, "AGV出发去下一个放料点"),

                // 任务完成
                (TaskStatus.Putted, TaskAction.Over) => (true, TaskStatus.Finished, "任务完成"),

                // 强制取消：终态不可取消，其它状态可直接取消
                (var status, TaskAction.ForceCancel) when status is TaskStatus.Finished or TaskStatus.Canceled or TaskStatus.Failed
                    => (false, status, "已结束任务不能取消"),
                (_, TaskAction.ForceCancel) => (true, TaskStatus.Canceled, "强制取消任务"),

                // 这里可以针对项目 A 特有的逻辑进行配置
                _ => (false, current, "当前状态不允许此操作")
            };
        }
    }
}
