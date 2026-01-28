using AbpOverallAuth.TaskManage.InternalTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbpOverallAuth.XinSong.TM
{
    public interface ITMManager
    {
        /// <summary>
        /// 创建任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        Task<InternalTask> CreateTask(TaskCreationArgs task);

    }
}
