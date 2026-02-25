using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Enums.InternalTask
{
    public enum TaskAction
    {
        /// <summary>
        /// 下发调度
        /// </summary>
        Dispatch,

        /// <summary>
        /// 到达起点
        /// </summary>
        ArriveSource,

        /// <summary>
        /// 开始取料
        /// </summary>
        BeginFetch,

        /// <summary>
        /// 完成取料
        /// </summary>
        FinishFetch,

        /// <summary>
        /// 到达终点
        /// </summary>
        ArriveTarget,

        /// <summary>
        /// 开始放料
        /// </summary>
        BeginPut,

        /// <summary>
        /// 完成放料
        /// </summary>
        FinishPut,

        /// <summary>
        /// 任务完成
        /// </summary>
        Over,

        /// <summary>
        /// 强制取消
        /// </summary>
        ForceCancel   
    }
}
