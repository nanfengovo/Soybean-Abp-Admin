using AbpOverallAuth.TaskManage.InternalTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using TaskStatus = AbpOverallAuth.Enums.InternalTask.TaskStatus;

namespace AbpOverallAuth.XinSong.TM
{
    public class TMManager : DomainService, ITMManager
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger<TMManager> _logger;

        private readonly TMOptions _options;

        private readonly IRepository<InternalTask, string> _internalTaskRepository;

        public TMManager(IHttpClientFactory httpClientFactory, ILogger<TMManager> logger, IOptions<TMOptions> options, IRepository<InternalTask, string> internalTaskRepository)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _options = options.Value;
            _internalTaskRepository = internalTaskRepository;
        }

        public async Task<InternalTask> CreateTask(TaskCreationArgs task)
        {
            InternalTask internalTask = new InternalTask(
                DateTime.Now.ToString("yyyyMMddHHmmssfffff"),
                task.TaskType,
                task.FromAddress,
                task.MiddleAddress,
                task.ToAddress,
                TaskStatus.Init,
                task.FetchCount,
                task.PutCount,
                task.FetchType,
                task.PutType,
                task.FetchMachineType,
                task.PutMachineType
            );

            await _internalTaskRepository.InsertAsync(internalTask);
            return internalTask;
        }

        
    }
}
