using RCS.Enums.InternalTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskStatus = RCS.Enums.InternalTask.TaskStatus;

namespace RCS.Interfaces
{
    public interface ITaskWorkflowPolicy
    {
        // 检查：在当前状态下，执行该动作是否合法？如果合法，下一个状态是什么？
        (bool IsValid, TaskStatus NextStatus, string Message) GetNextState(TaskStatus currentStatus, TaskAction action);
    }
}
