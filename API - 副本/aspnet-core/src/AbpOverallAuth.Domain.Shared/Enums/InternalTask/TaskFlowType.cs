using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbpOverallAuth.Enums.InternalTask
{
    public enum TaskFlowType
    {
        Fetch = 0,
        
        Put = 1,

        FetchPut  = 2,

        FetchPutPut = 3
    }
}
