using AbpOverallAuth.ThirdParty.TM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AbpOverallAuth.Interfaces
{
    public interface ITaskService:IApplicationService
    {
        Task<bool> PressTaskAsync();

        Task CancelTaskAsync(string[] ids);
    }
}
