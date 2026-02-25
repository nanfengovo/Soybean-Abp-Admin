using AbpOverallAuth.TaskManage.InternalTask;
using AbpOverallAuth.ThirdParty.TM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbpOverallAuth.Interfaces
{
    public interface ITaskFlowBuilder
    {
        bool CanHandle(InternalTask task);
        TMTaskDto Build(InternalTask task);
    }

}
