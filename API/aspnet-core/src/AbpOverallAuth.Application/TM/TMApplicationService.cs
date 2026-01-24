using AbpOverallAuth.Dtos.TM;
using AbpOverallAuth.Enums.InternalTask;
using AbpOverallAuth.Interfaces;
using AbpOverallAuth.TaskManage.InternalTask;
using AbpOverallAuth.XinSong.TM;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AbpOverallAuth.TM
{
    public class TMApplicationService : ApplicationService, ITMApplicationService
    {
		private readonly ILogger<TMApplicationService> _logger;

        private readonly ITMManager _tmManager;
        public TMApplicationService(IRepository<InternalTask, string> taskRepository, ILogger<TMApplicationService> logger, ITMManager tmManager)
        {
            _logger = logger;
            _tmManager = tmManager;
        }

        public async Task CreateTask(CreateTaskDto input)
        {
			try
			{
                _logger.LogInformation("通过前端提供必要参数创建任务");
                // 自动映射
                var args = ObjectMapper.Map<CreateTaskDto, TaskCreationArgs>(input);
                await _tmManager.CreateTask(args);
            }
			catch (Exception)
			{

				throw;
			}
        }
    }
}
