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
        public Task CreateTask(CreateTaskDto input);
    }
}
