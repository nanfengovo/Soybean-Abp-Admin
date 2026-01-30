using AbpOverallAuth.Enums.InternalTask;
using AbpOverallAuth.Interfaces;
using AbpOverallAuth.TaskManage.InternalTask;
using AbpOverallAuth.ThirdParty.Base;
using AbpOverallAuth.TM;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using TaskStatus = AbpOverallAuth.Enums.InternalTask.TaskStatus;

namespace AbpOverallAuth.ThirdParty.TM
{
    public class TaskService : AbpOverallAuthAppService, ITaskService
    {
        private readonly IHttpExecutor _http;

        private readonly ILogger<TaskService> _logger;

        private readonly IRepository<InternalTask> _internalTask;

        private readonly FetchPutTaskFlowBuilder _taskBuilder;

        private readonly ITaskWorkflowPolicy _taskWorkflowPolicy;

        public TaskService(IHttpExecutor http, ILogger<TaskService> logger, IRepository<InternalTask> internalTask, FetchPutTaskFlowBuilder taskBuilder, ITaskWorkflowPolicy taskWorkflowPolicy)
        {
            _http = http;
            _logger = logger;
            _internalTask = internalTask;
            _taskBuilder = taskBuilder;
            _taskWorkflowPolicy = taskWorkflowPolicy;
        }

        /// <summary>
        /// 给TM派发任务的接口
        /// </summary>
        /// <param name="tMTask"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> PressTaskAsync()
        {
            var tasks = await _internalTask.GetListAsync(
                t => t.TaskStatus == TaskStatus.Init
            );

            foreach (var task in tasks)
            {
                var dto = _taskBuilder.Build(task);

                await _http.PostAsync<HttpResult<TMTaskResp>>(
                    "TM",
                    "/api/v1/xinsong/task_add",
                    dto
                );

                task.ExecuteAction(
                    TaskAction.Dispatch,
                    _taskWorkflowPolicy
                );
                await _internalTask.UpdateAsync(task);
            }

            return true;
        }


        private TMTaskDto ConvertTask(InternalTask tMTask,int taskcode1,int taskcode2,string type)
        {
            var taskDto = new TMTaskDto
            {
                Bulk_Task_Count = 1,
                Bulk_Task_Type = tMTask.TaskType.ToString(),
                Sub_Task = new List<sub_task>()
            };

            sub_task sub_Task = new sub_task()
            {
                AGV_Serial = 0,
                Robot_Type = "99",
                Succession = 0,
                Area_Property = new List<string>(),
                Cargo_Id = "#",
                Complete_Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Pre_Report = "0",
                Priority = 0,
                goal_action = 0,
                Mark = "3",
                Option_Code = $"{taskcode1},{taskcode2}",
                Storage = tMTask.FromAddress,
                Task_Serial = tMTask.Id+type
            };

            int fromAddr = 0;
            var success = int.TryParse(tMTask.FromAddress,out fromAddr);
            if (success)
            {
                _logger.LogInformation($"任务号：{tMTask.Id}的任务作业点为{fromAddr}");
            }
            else
            {
                throw new BusinessException("InvalidFromAddress")
                    .WithData("FromAddress", tMTask.FromAddress)
                    .WithData("TaskId", tMTask.Id);
            }

            sub_Task.Target = fromAddr;
            return taskDto;
        }
    }
}
