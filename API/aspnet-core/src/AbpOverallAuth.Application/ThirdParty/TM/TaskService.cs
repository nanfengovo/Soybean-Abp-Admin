using AbpOverallAuth.Interfaces;
using AbpOverallAuth.ThirdParty.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbpOverallAuth.ThirdParty.TM
{
    public class TaskService : AbpOverallAuthAppService, ITaskService
    {
        private readonly IHttpExecutor _http;

        public TaskService(IHttpExecutor http)
        {
            _http = http;
        }

        /// <summary>
        /// 给TM派发任务的接口
        /// </summary>
        /// <param name="tMTask"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> PressTaskAsync(TMTaskDto tMTask)
        {
            var task = new
            {
                bulk_task_count = tMTask.Bulk_Task_Count,
                bulk_task_type = tMTask.Bulk_Task_Type,
                sub_task = new sub_task[2],
            };
            await _http.PostAsync<HttpResult<TMTaskResp>>("TM", "/api/v1/xinsong/task_add",task);
            return true;
        }
    }
}
