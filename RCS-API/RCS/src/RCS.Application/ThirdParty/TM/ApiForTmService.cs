using RCS.Dtos.TM;
using RCS.Enums.InternalTask;
using RCS.Interfaces;
using RCS.TaskManage.InternalTask;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using TaskStatus = RCS.Enums.InternalTask.TaskStatus;



namespace RCS.ThirdParty.TM
{

    [Route("api/v1/xinsong/")]
    public class ApiForTmService:RCSAppService
    {
        private readonly ILogger<ApiForTmService> _logger;
        private readonly IRepository<InternalTask> _internalTaskRepository;
        private readonly ITaskWorkflowPolicy _taskWorkflowPolicy;

        public ApiForTmService(
            ILogger<ApiForTmService> logger,
            IRepository<InternalTask> internalTaskRepository,
            ITaskWorkflowPolicy taskWorkflowPolicy)
        {
            _logger = logger;
            _internalTaskRepository = internalTaskRepository;
            _taskWorkflowPolicy = taskWorkflowPolicy;
        }

        [HttpPost("task_info")]
        public async Task<ResponseTMBaseDTO> BeginTask([FromBody] object requestObj)
        {
            _logger.LogInformation("receive from TM Action [{action}]:{requestStr}", nameof(BeginTask), requestObj.ToString());
            return ResponseTMBaseDTO.Success();
        }

        /// <summary>
        /// 到达站点上报
        /// </summary>
        /// <param name="requestObj"></param>
        /// <returns></returns>
        [HttpPost("task_arrive_target")]
        public async Task<ResponseTMBaseDTO> ArriveTarget([FromBody] object requestObj)
        {
            _logger.LogInformation("receive from TM Action [{action}]:{requestStr}", nameof(ArriveTarget), requestObj.ToString());

            try
            {
                var jObj = JObject.Parse(requestObj.ToString()!);
                var taskSerial = jObj.Value<string>("task_serial");

                if (string.IsNullOrWhiteSpace(taskSerial))
                {
                    _logger.LogWarning("ArriveTarget: task_serial 为空");
                    return ResponseTMBaseDTO.Fail("task_serial 不能为空");
                }

                if (!TryParseTaskSerial(taskSerial, out var taskId, out var subTaskAction, out _))
                {
                    _logger.LogWarning("ArriveTarget: 无法从 task_serial [{taskSerial}] 解析任务ID", taskSerial);
                    return ResponseTMBaseDTO.Fail($"无效的 task_serial: {taskSerial}");
                }

                var task = await _internalTaskRepository.FindAsync(x => x.Id == taskId);
                if (task == null)
                {
                    _logger.LogWarning("ArriveTarget: 任务不存在, taskId={taskId}", taskId);
                    return ResponseTMBaseDTO.Fail($"任务不存在: {taskId}");
                }

                var action = subTaskAction == SubTaskAction.Fetch
                    ? TaskAction.ArriveSource
                    : TaskAction.ArriveTarget;

                var oldStatus = task.TaskStatus;
                var advanced = AdvanceWhileValid(task, action);
                if (!advanced && IsArriveHandled(task, subTaskAction))
                    return ResponseTMBaseDTO.Success();

                if (!advanced)
                {
                    _logger.LogWarning("ArriveTarget: 状态转换失败, taskId={taskId}, 当前状态={currentStatus}",
                        taskId, task.TaskStatus);
                    return ResponseTMBaseDTO.Fail($"状态转换失败: 当前状态为 {task.TaskStatus}");
                }

                await _internalTaskRepository.UpdateAsync(task);

                _logger.LogInformation("ArriveTarget: 成功, taskId={taskId}, {oldStatus} -> {newStatus}",
                    taskId, oldStatus, task.TaskStatus);

                return ResponseTMBaseDTO.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ArriveTarget: 处理异常");
                return ResponseTMBaseDTO.Fail($"处理异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 完成任务上报
        /// </summary>
        /// <param name="requestObj"></param>
        /// <returns></returns>
        [HttpPost("task_finish")]
        public async Task<ResponseTMBaseDTO> TaskFinish([FromBody] object requestObj)
        {
            _logger.LogInformation("receive from TM Action [{action}]:{requestStr}", nameof(TaskFinish), requestObj.ToString());

            try
            {
                var jObj = JObject.Parse(requestObj.ToString()!);
                var taskSerial = jObj.Value<string>("task_serial");

                if (string.IsNullOrWhiteSpace(taskSerial))
                {
                    _logger.LogWarning("TaskFinish: task_serial 为空");
                    return ResponseTMBaseDTO.Fail("task_serial 不能为空");
                }

                if (!TryParseTaskSerial(taskSerial, out var taskId, out var subTaskAction, out var sequence))
                {
                    _logger.LogWarning("TaskFinish: 无法从 task_serial [{taskSerial}] 解析任务ID", taskSerial);
                    return ResponseTMBaseDTO.Fail($"无效的 task_serial: {taskSerial}");
                }

                var task = await _internalTaskRepository.FindAsync(x => x.Id == taskId);
                if (task == null)
                {
                    _logger.LogWarning("TaskFinish: 任务不存在, taskId={taskId}", taskId);
                    return ResponseTMBaseDTO.Fail($"任务不存在: {taskId}");
                }

                var oldStatus = task.TaskStatus;
                var advanced = false;

                if (subTaskAction == SubTaskAction.Fetch)
                {
                    advanced = TryExecuteAction(task, TaskAction.BeginFetch) || advanced;
                    advanced = TryExecuteAction(task, TaskAction.FinishFetch) || advanced;

                    if (!advanced && IsFetchCompleted(task.TaskStatus))
                        return ResponseTMBaseDTO.Success();
                }
                else
                {
                    advanced = TryExecuteAction(task, TaskAction.BeginPut) || advanced;
                    advanced = TryExecuteAction(task, TaskAction.FinishPut) || advanced;

                    if (IsFinalPut(task, sequence))
                        advanced = TryExecuteAction(task, TaskAction.Over) || advanced;

                    if (!advanced && IsPutCompleted(task.TaskStatus, IsFinalPut(task, sequence)))
                        return ResponseTMBaseDTO.Success();
                }

                if (!advanced)
                {
                    _logger.LogWarning("TaskFinish: 状态转换失败, taskId={taskId}, 当前状态={currentStatus}",
                        taskId, task.TaskStatus);
                    return ResponseTMBaseDTO.Fail($"状态转换失败: 当前状态为 {task.TaskStatus}");
                }

                await _internalTaskRepository.UpdateAsync(task);

                _logger.LogInformation("TaskFinish: 成功, taskId={taskId}, {oldStatus} -> {newStatus}",
                    taskId, oldStatus, task.TaskStatus);

                return ResponseTMBaseDTO.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TaskFinish: 处理异常");
                return ResponseTMBaseDTO.Fail($"处理异常: {ex.Message}");
            }
        }

        private static bool TryParseTaskSerial(string taskSerial, out string taskId, out SubTaskAction subTaskAction, out int sequence)
        {
            taskId = string.Empty;
            subTaskAction = SubTaskAction.Fetch;
            sequence = 1;

            if (string.IsNullOrWhiteSpace(taskSerial))
                return false;

            var lastUnderscoreIndex = taskSerial.LastIndexOf('_');
            var actionPart = lastUnderscoreIndex > 0 ? taskSerial.Substring(lastUnderscoreIndex + 1) : taskSerial;
            taskId = lastUnderscoreIndex > 0 ? taskSerial.Substring(0, lastUnderscoreIndex) : taskSerial;

            if (!TryParseActionPart(actionPart, out subTaskAction, out sequence))
            {
                if (!TryParseLegacyAction(taskSerial, out subTaskAction, out sequence))
                    return false;

                taskId = StripLegacySuffix(taskSerial);
            }

            return !string.IsNullOrWhiteSpace(taskId);
        }

        private static bool TryParseActionPart(string actionPart, out SubTaskAction subTaskAction, out int sequence)
        {
            subTaskAction = SubTaskAction.Fetch;
            sequence = 1;

            if (string.IsNullOrWhiteSpace(actionPart))
                return false;

            if (actionPart.Contains("fetch", StringComparison.OrdinalIgnoreCase))
                subTaskAction = SubTaskAction.Fetch;
            else if (actionPart.Contains("put", StringComparison.OrdinalIgnoreCase))
                subTaskAction = SubTaskAction.Put;
            else
                return false;

            sequence = ExtractSequence(actionPart, 1);
            return true;
        }

        private static bool TryParseLegacyAction(string taskSerial, out SubTaskAction subTaskAction, out int sequence)
        {
            subTaskAction = SubTaskAction.Fetch;
            sequence = 1;

            if (taskSerial.Contains("fetch", StringComparison.OrdinalIgnoreCase))
                subTaskAction = SubTaskAction.Fetch;
            else if (taskSerial.Contains("put", StringComparison.OrdinalIgnoreCase))
                subTaskAction = SubTaskAction.Put;
            else
                return false;

            sequence = ExtractSequence(taskSerial, 1);
            return true;
        }

        private static int ExtractSequence(string input, int defaultValue)
        {
            var digits = new string(input.Where(char.IsDigit).ToArray());
            if (string.IsNullOrWhiteSpace(digits))
                return defaultValue;

            return int.TryParse(digits, out var value) ? value : defaultValue;
        }

        private static string StripLegacySuffix(string taskSerial)
        {
            var cleaned = RemoveIgnoreCase(taskSerial, "0_fetch");
            cleaned = RemoveIgnoreCase(cleaned, "1_put");
            cleaned = RemoveIgnoreCase(cleaned, "2_put");
            cleaned = RemoveIgnoreCase(cleaned, "_fetch");
            cleaned = RemoveIgnoreCase(cleaned, "_put");
            return cleaned.TrimEnd('_');
        }

        private static string RemoveIgnoreCase(string source, string value)
        {
            var index = source.IndexOf(value, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return source;

            return source.Remove(index, value.Length);
        }

        private bool AdvanceWhileValid(InternalTask task, TaskAction action)
        {
            var advanced = false;
            while (TryExecuteAction(task, action))
                advanced = true;

            return advanced;
        }

        private bool TryExecuteAction(InternalTask task, TaskAction action)
        {
            var (isValid, _, _) = _taskWorkflowPolicy.GetNextState(task.TaskStatus, action);
            if (!isValid)
                return false;

            task.ExecuteAction(action, _taskWorkflowPolicy);
            return true;
        }

        private static bool IsArriveHandled(InternalTask task, SubTaskAction subTaskAction)
        {
            if (subTaskAction == SubTaskAction.Fetch)
            {
                return task.TaskStatus is TaskStatus.PreFetching or TaskStatus.Fetching or TaskStatus.Fetched
                    or TaskStatus.ToPutting or TaskStatus.PrePutting or TaskStatus.Putting or TaskStatus.Putted
                    or TaskStatus.Finished;
            }

            return task.TaskStatus is TaskStatus.PrePutting or TaskStatus.Putting or TaskStatus.Putted or TaskStatus.Finished;
        }

        private static bool IsFetchCompleted(TaskStatus status)
        {
            return status is TaskStatus.Fetched or TaskStatus.ToPutting or TaskStatus.PrePutting
                or TaskStatus.Putting or TaskStatus.Putted or TaskStatus.Finished;
        }

        private static bool IsPutCompleted(TaskStatus status, bool isFinalPut)
        {
            if (isFinalPut)
                return status is TaskStatus.Finished;

            return status is TaskStatus.Putted or TaskStatus.ToPutting or TaskStatus.PrePutting
                or TaskStatus.Putting or TaskStatus.Finished;
        }

        private static bool IsFinalPut(InternalTask task, int sequence)
        {
            return task.FlowType switch
            {
                TaskFlowType.FetchPut => sequence == 1,
                TaskFlowType.FetchPutPut => sequence == 2,
                _ => sequence == 1
            };
        }
    }
}
