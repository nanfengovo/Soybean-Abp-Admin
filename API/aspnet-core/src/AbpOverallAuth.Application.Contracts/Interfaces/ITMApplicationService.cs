using AbpOverallAuth.Dtos.TM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AbpOverallAuth.Interfaces
{
    public interface ITMApplicationService:IApplicationService
    {
        /// <summary>
        /// 创建内部任务（通过前端下）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task CreateTask(CreateTaskDto input);

        /// <summary>
        /// 派发一个/多个任务
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public Task<bool> PressTaskAsync(string[] ids);
    }
}
