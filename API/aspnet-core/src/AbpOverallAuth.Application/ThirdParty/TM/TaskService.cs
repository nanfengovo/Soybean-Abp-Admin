using AbpOverallAuth.Configuration.ThirdParty;
using AbpOverallAuth.Enums.InternalTask;
using AbpOverallAuth.Interfaces;
using AbpOverallAuth.TaskManage.InternalTask;
using AbpOverallAuth.ThirdParty.Base;
using AbpOverallAuth.TM;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        /// 取消任务
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task CancelTaskAsync(string[] ids)
        {
            if (ids == null || ids.Length == 0)
                throw new UserFriendlyException("取消任务需要传入取消任务的ID");
            var idList = ids.ToList(); // ⭐ 关键
            var tasks = await _internalTask.GetListAsync(x => idList.Contains(x.Id));
            if (!tasks.Any())
                throw new UserFriendlyException("未找到要取消的任务");
            var dto = new
            {
                delete_task_count = 1,
                delete_flag = 0,
                sub_tasks = tasks.Select(x => new { task_serial = x.Id + "Fetch", AGV_serial = 0 }).ToArray(),
            };
            HttpResult<HttpResult<TMTaskResp>> res = new HttpResult<HttpResult<TMTaskResp>>();
            try
            {
                foreach (var task in tasks)
                {
                    res = await _http.PostAsync<HttpResult<TMTaskResp>>("TM",
                        "api/v1/xinsong/task_delete",
                        dto
                    );
                    var errorMsg = string.Empty;
                    if (!res.Raw.IsNullOrWhiteSpace())
                    {
                        errorMsg = JsonDocument.Parse(res.Raw).RootElement.GetProperty("ErrMsg").GetString();
                    }
                    if (res.Success && errorMsg.IsNullOrWhiteSpace())
                    {
                        task.ExecuteAction(
                        TaskAction.ForceCancel,
                        _taskWorkflowPolicy
                    );
                        await _internalTask.UpdateAsync(task);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(nameof(CancelTaskAsync)+"方法出现异常，异常信息为："+ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
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
                _logger.LogInformation("TM REQUEST DTO => {@Dto}", dto);
                _logger.LogDebug("TM REQUEST DTO (JSON) => {DtoJson}",
                System.Text.Json.JsonSerializer.Serialize(dto, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                }));
                var raw = await _http.PostAsync<HttpResult<TMTaskResp>>(
                    "TM",
                    "api/v1/xinsong/task_add",
                    dto
                );
                _logger.LogInformation("TM RAW RESPONSE => {@Response}", raw);
                _logger.LogInformation("API响应 => 成功: {Success}, 状态码: {StatusCode}, 错误: {Error}",
                    raw.Success, raw.StatusCode, raw.Error ?? "无");
                var errorMsg = string.Empty;
                if (!raw.Raw.IsNullOrWhiteSpace())
                {
                    errorMsg = JsonDocument.Parse(raw.Raw).RootElement.GetProperty("ErrMsg").GetString();
                }
                if (raw.Success && errorMsg.IsNullOrWhiteSpace())
                {
                    task.ExecuteAction(
                    TaskAction.Dispatch,
                    _taskWorkflowPolicy
                );
                    await _internalTask.UpdateAsync(task);
                }
                else
                {
                    _logger.LogWarning(
                        "PressTaskAsync异常: 状态码={StatusCode}, 错误信息为：{Error}, 任务ID={TaskId}",
                        raw.StatusCode,
                        errorMsg,
                        task.Id
                        );
                    throw new UserFriendlyException($"派发任务失败！失败原因为{errorMsg}");
                }
            }

            return true;
        }
    }
}
