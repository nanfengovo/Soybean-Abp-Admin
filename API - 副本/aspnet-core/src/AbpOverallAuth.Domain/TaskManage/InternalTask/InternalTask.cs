using AbpOverallAuth.Enums.InternalTask;
using AbpOverallAuth.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using TaskStatus = AbpOverallAuth.Enums.InternalTask.TaskStatus;

namespace AbpOverallAuth.TaskManage.InternalTask
{
    public class InternalTask:FullAuditedAggregateRoot<string>
    {
        /// <summary>
        /// 任务类型
        /// </summary>
        public TaskType TaskType { get;private set; } = TaskType.None;

        /// <summary>
        /// 任务流程类型
        /// </summary>
        public TaskFlowType FlowType { get; private set; } 

        /// <summary>
        /// 容器id
        /// </summary>
        public string? ContainerId { get; private set; } = string.Empty;

        /// <summary>
        /// 起点
        /// </summary>
        public string FromAddress { get; private set; } = string.Empty;

        /// <summary>
        /// 途经点（可以没有）
        /// </summary>
        public string? MiddleAddress { get; private set; } = string.Empty;

        /// <summary>
        /// 终点
        /// </summary>
        public string ToAddress { get; private set; } = string.Empty;

        /// <summary>
        /// 上一步的状态
        /// </summary>
        public TaskStatus? OldTaskStatus { get; private set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskStatus TaskStatus { get; private set; } = TaskStatus.Init;

        /// <summary>
        /// 失败原因
        /// </summary>
        public string? FailReason { get; private set; }  


        /// <summary>
        /// 取料数量
        /// </summary>
        public int FetchCount { get; private set; }

        /// <summary>
        /// 放料数量
        /// </summary>
        public int PutCount { get; private set; }

        /// <summary>
        /// 取料容器类型
        /// </summary>
        public ContainerType FetchType { get; private set; }

        /// <summary>
        /// 放料容器类型
        /// </summary>
        public ContainerType PutType { get; private set; }

        /// <summary>
        /// 取料机器类型
        /// </summary>
        public MachineType FetchMachineType { get; private set; }

        /// <summary>
        /// 放料机器类型
        /// </summary>
        public MachineType PutMachineType { get; private set; }

        /// <summary>
        /// 任务完成时间
        /// </summary>
        public DateTime? CompletionTime { get; private set; }

        #region === 构造函数 ===

        //efcore 用的无参构造函数
        public InternalTask()
        {

        }

        /// <summary>
        /// 最小构造函数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="taskType"></param>
        /// <param name="fromAddress"></param>
        /// <param name="toAddress"></param>
        /// <param name="taskStatus"></param>
        /// <param name="fetchCount"></param>
        /// <param name="putCount"></param>
        /// <param name="fetchType"></param>
        /// <param name="putType"></param>
        /// <param name="fetchMachineType"></param>
        /// <param name="putMachineType"></param>
        public InternalTask(string id, TaskType taskType,TaskFlowType flowType, string fromAddress,string? middleAddress,string toAddress,TaskStatus taskStatus,int fetchCount,int putCount,ContainerType fetchType,ContainerType putType,MachineType fetchMachineType, MachineType putMachineType) : base(id)
        {
            Id = id;
            TaskType = taskType;
            FlowType = flowType;
            FromAddress = fromAddress;
            MiddleAddress = middleAddress;
            ToAddress = toAddress;
            TaskStatus = taskStatus;
            FetchCount = fetchCount;
            PutCount = putCount;
            FetchType = fetchType;
            PutType = putType;
            FetchMachineType = fetchMachineType;
            PutMachineType = putMachineType;
        }
        #endregion



        #region === 领域行为 ===

        #region 更新状态

        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="action"></param>
        /// <param name="policy"></param>
        /// <param name="actualCount"></param>
        /// <exception cref="BusinessException"></exception>
        public void ExecuteAction(TaskAction action, ITaskWorkflowPolicy policy, int? actualCount = null)
        {
            // 调用策略判定
            var (isValid, nextStatus, message) = policy.GetNextState(this.TaskStatus, action);
            if (!isValid) throw new BusinessException(message);

            // 执行具体的业务逻辑修改
            if (action == TaskAction.FinishFetch && actualCount.HasValue)
            {
                if (actualCount <= 0) throw new BusinessException("实取数量非法");
                this.FetchCount = actualCount.Value;
            }

            // 最终状态变更
            this.OldTaskStatus = this.TaskStatus;
            this.TaskStatus = nextStatus;
        }



        #endregion

        /// <summary>
        /// 标记任务失败并记录失败原因
        /// </summary>
        /// <param name="reason"></param>
        /// <exception cref="BusinessException"></exception>
        public void MarkFailed(string reason)
        {
            if (TaskStatus is TaskStatus.Finished or TaskStatus.Canceled)
                throw new BusinessException("已结束任务不能失败");

            TaskStatus = TaskStatus.Failed;
            FailReason = reason;
        }
        #endregion


    }
}
