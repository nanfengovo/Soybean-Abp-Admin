using RCS.TaskManage.InternalTask;
using RCS.ThirdParty.TM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Interfaces
{
    public interface ITaskFlowBuilder
    {
        bool CanHandle(InternalTask task);
        TMTaskDto Build(InternalTask task);
    }

}
