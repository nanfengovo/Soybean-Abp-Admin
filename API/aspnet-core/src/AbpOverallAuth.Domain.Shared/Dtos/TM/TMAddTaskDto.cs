using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AbpOverallAuth.Dtos.TM
{
    public  class TMAddTaskDto
    { 
        public class TMTaskAddInput
        {
            /// <summary>
            /// 组合任务子任务数量
            /// </summary>
            [JsonPropertyName("bulk_task_count")]
            public int BulkTaskCount { get; set; }

            /// <summary>
            /// 组合任务类型名，即要求配置任务步 xml 的文件名
            /// </summary>

            [JsonPropertyName("bulk_task_type")]
            public string BulkTaskType { get; set; } = "task";

            /// <summary>
            /// 子任务数组
            /// </summary>
            [JsonPropertyName("sub_task")]
            public List<TMSubTaskDto> SubTasks { get; set; } = new();
        }
       
    }

    public class TMSubTaskDto
    {
        /// <summary>
        /// 指定任务执行车辆编号 ID，0 为不指定
        /// </summary>
        [JsonPropertyName("AGV_serial")]
        public int AgvSerial { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>

        [JsonPropertyName("task_serial")]
        public string TaskSerial { get; set; }

        /// <summary>
        /// 任务类型，即要求配置任务步 xml 的文件名，非空时优先使用，为空时任务类型采用 bulk_task_type 参数
        /// </summary>

        [JsonPropertyName("task_type")]
        public string TaskType { get; set; }

        /// <summary>
        /// 地图点
        /// </summary>

        [JsonPropertyName("target")]
        public int Target { get; set; }

        /// <summary>
        /// 目的地动作，1-从设备取出物料，2-放置物料到机台，0-普通任务
        /// </summary>

        [JsonPropertyName("goal_action")]
        public int GoalAction { get; set; } // 1-取，2-放

        /// <summary>
        /// 货物 RFID，机械臂请求放行时会用到
        /// </summary>
        [JsonPropertyName("cargo_id")]
        public string CargoId { get; set; }

        /// <summary>
        /// 任务执行优先级，取值 0-99，越大越优先
        /// </summary>

        [JsonPropertyName("priority")]
        public int Priority { get; set; } = 1;

        /// <summary>
        /// 任务最晚执行时间，YYYY-MM-DD HH:mm::SS，到了这个时间还没执行就会优先执行
        /// </summary>

        [JsonPropertyName("complete_time")]
        public string CompleteTime { get; set; } // YYYY-MM-DD HH:mm:ss

        /// <summary>
        /// 指定任务执行车辆类型
        /// </summary>
        [JsonPropertyName("robot_type")]
        public string RobotType { get; set; } = "3";

        /// <summary>
        /// 设备操作码，用于 plc 交互执行动作参数，目前有两个字
        /// </summary>
        [JsonPropertyName("option_code")]
        public string OptionCode { get; set; } = "0,0";

        /// <summary>
        /// 执行任务前集中上报标记，一般用于目的地不确定任务执行顺序有要求，“0”为无需集中上报
        /// </summary>
        [JsonPropertyName("pre_report")]
        public string PreReport { get; set; } = "0";

        /// <summary>
        /// 备用
        /// </summary>
        [JsonPropertyName("mark")]
        public string Mark { get; set; } = "3";

        /// <summary>
        /// 备用
        /// </summary>
        [JsonPropertyName("storage")]
        public string Storage { get; set; } = "";

        /// <summary>
        /// 任务接续号，两个任务需要连续执行接续号大于 0，相差1，如：第一个接续号 1，第二个接续号 2。与集中上报逻辑互斥不能同时使用
        /// </summary>
        [JsonPropertyName("succession")]
        public int Succession { get; set; } = 0;

        /// <summary>
        /// 指定任务执行车辆区域属性，空为不指定
        /// </summary>
        [JsonPropertyName("area_property")]
        public List<string> AreaProperty { get; set; } = new();
    }


}
