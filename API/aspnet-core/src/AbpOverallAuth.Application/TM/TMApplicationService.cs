using AbpOverallAuth.Dtos.TM;
using AbpOverallAuth.Enums.InternalTask;
using AbpOverallAuth.Interfaces;
using AbpOverallAuth.Permissions;
using AbpOverallAuth.TaskManage.InternalTask;
using AbpOverallAuth.XinSong.TM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AbpOverallAuth.TM
{
    public class TMApplicationService : ApplicationService, ITMApplicationService
    {
		private readonly ILogger<TMApplicationService> _logger;

        private readonly ITMManager _tmManager;
        public TMApplicationService(ILogger<TMApplicationService> logger, ITMManager tmManager)
        {
            _logger = logger;
            _tmManager = tmManager;
        }

        /// <summary>
        /// 通过前端提供必要参数创建任务
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        [Authorize(AbpOverallAuthPermissions.InTask.Create)]
        public async Task CreateTask(CreateTaskDto input)
        {
            try
            {
                _logger.LogInformation($"通过前端提供必要参数创建任务，进入{nameof(CreateTask)}");
                _logger.LogInformation($"{nameof(CreateTask)}方法传入的参数为：任务类型：{input.TaskType},起点：{input.FromAddress},途经点：{input.MiddleAddress},终点：{input.ToAddress},取的数量：{input.FetchCount},放的数量{input.PutCount},取的类型{input.FetchType},放的类型{input.PutType},取的机器类型{input.FetchMachineType},放的机器类型{input.PutMachineType}");
                // 自动映射:将DTO转为领域参数对象（Args）
                var args = ObjectMapper.Map<CreateTaskDto, TaskCreationArgs>(input);
                await _tmManager.CreateTask(args);
            }
			catch (Exception ex)
			{
				throw new UserFriendlyException($"{nameof(CreateTask)}方法发生异常,异常信息为：{ex.Message}");
			}
        }


    }
}
