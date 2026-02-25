using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbpOverallAuth.Enums.InternalTask
{
    public enum TaskType
    {
        /// <summary>
        /// 无类型（表示不关注这个字段）
        /// </summary>
        None = 0,

        /// <summary>
        /// 单纯的移动任务（兼容调度充电/回停车点任务需要RCS发起的情况）
        /// </summary>
        Move = 1,

        /// <summary>
        /// 上料任务
        /// </summary>
        Load = 2,

        /// <summary>
        /// 下料任务
        /// </summary>
        UnLoad = 3,

        /// <summary>
        /// 自动补料任务（不上机台）
        /// </summary>
        AutoFeed = 4,

        /// <summary>
        /// 自动上料任务
        /// </summary>
        AutoLoad = 5,

        /// <summary>
        /// 自动下料任务
        /// </summary>
        AutoUnLoad = 6
    }
}
