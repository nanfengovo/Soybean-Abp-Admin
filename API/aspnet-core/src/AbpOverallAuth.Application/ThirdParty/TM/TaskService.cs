using AbpOverallAuth.Configuration.ThirdParty;
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
using Microsoft.Extensions.Options;

namespace AbpOverallAuth.ThirdParty.TM
{
    public class TaskService : AbpOverallAuthAppService, ITaskService
    {
        private readonly IHttpExecutor _http;

        private readonly ILogger<TaskService> _logger;

        private readonly IRepository<InternalTask> _internalTask;

        private readonly FetchPutTaskFlowBuilder _taskBuilder;

        private readonly ITaskWorkflowPolicy _taskWorkflowPolicy;

        private readonly AbpOverallAuth.Configuration.ThirdParty.TM _tm;
        public TaskService(IHttpExecutor http, ILogger<TaskService> logger, IRepository<InternalTask> internalTask, FetchPutTaskFlowBuilder taskBuilder, ITaskWorkflowPolicy taskWorkflowPolicy, IOptions<Configuration.ThirdParty.TM> tm)
        {
            _http = http;
            _logger = logger;
            _internalTask = internalTask;
            _taskBuilder = taskBuilder;
            _taskWorkflowPolicy = taskWorkflowPolicy;
            _tm = tm.Value;
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

            if (!tasks.Any())
            {
                _logger.LogInformation("当前没有待派发的任务,请先创建任务！！");
                throw new UserFriendlyException("当前没有待派发的任务");
            }

            foreach (var task in tasks)
            {
                var dto = _taskBuilder.Build(task);
                _logger.LogInformation("TM REQUEST DTO => " + dto);
                var raw = await _http.PostAsync<HttpResult<TMTaskResp>>(
                    "TM",
                    "/api/v1/xinsong/task_add",
                    dto
                );
                _logger.LogInformation("TM RAW RESPONSE => " + raw);
                task.ExecuteAction(
                    TaskAction.Dispatch,
                    _taskWorkflowPolicy
                );
                await _internalTask.UpdateAsync(task);
            }

            return true;
        }
    }
}
