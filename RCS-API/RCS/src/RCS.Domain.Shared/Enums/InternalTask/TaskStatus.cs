using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Enums.InternalTask
{
    public enum TaskStatus
    {
        /// <summary>
        /// 初始化，表示任务刚创建但是还没发给调度系统
        /// </summary>
        Init = 0,

        /// <summary>
        /// 任务已下发调度系统
        /// </summary>
        Pressed = 1,

        /// <summary>
        /// 去取料途中
        /// </summary>
        ToFetching = 2,

        /// <summary>
        /// 申请取料
        /// </summary>
        PreFetching = 3,

        /// <summary>
        /// 取料中
        /// </summary>
        Fetching = 4,

        /// <summary>
        /// 取料完成
        /// </summary>
        Fetched = 5,

        /// <summary>
        /// 去放料途中
        /// </summary>

        ToPutting = 6,

        /// <summary>
        /// 申请放料
        /// </summary>
        PrePutting = 7,

        /// <summary>
        /// 放料中
        /// </summary>
        Putting = 8,

        /// <summary>
        /// 放料完成
        /// </summary>
        Putted = 9,

        /// <summary>
        /// 任务完成
        /// </summary>
        Finished = 10,

        /// <summary>
        /// 申请取消任务
        /// </summary>
        CancelRequested = 11,

        /// <summary>
        /// 取消任务
        /// </summary>
        Canceled = 12,

        /// <summary>
        /// 异常终止
        /// </summary>
        Failed = 99   


    }
}
