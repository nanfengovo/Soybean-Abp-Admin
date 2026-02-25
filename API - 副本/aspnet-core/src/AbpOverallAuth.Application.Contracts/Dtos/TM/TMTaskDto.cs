using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AbpOverallAuth.ThirdParty.TM
{
    public class TMTaskDto
    {
        /// <summary>
        /// 组合任务子任务数量
        /// </summary>
        [JsonPropertyName("bulk_task_count")]
        public int Bulk_Task_Count { get; set; }

        /// <summary>
        /// 组合任务类型名，即要求配置任务步 xml 的文件名
        /// </summary>
        [JsonPropertyName("bulk_task_type")]
        public string Bulk_Task_Type { get; set; } = string.Empty;

        /// <summary>
        /// 子任务数组
        /// </summary>
        [JsonPropertyName("sub_task")]
        public List<sub_task> Sub_Task { get; set; } = new List<sub_task>();
    }

    public class sub_task
    {
        /// <summary>
        /// 指定任务执行车辆编号 ID，0 为不指定
        /// </summary>
        [JsonPropertyName("AGV_serial")]
        public int AGV_Serial { get; set; }

        /// <summary>
        /// 指定任务执行车辆类型
        /// </summary>
        [JsonPropertyName("robot_type")]
        public string Robot_Type { get; set; } = string.Empty;

        /// <summary>
        /// 指定任务执行车辆区域属性，空为不指定
        /// </summary>
        [JsonPropertyName("area_property")]
        public List<string> Area_Property { get; set; } = new List<string>();

        /// <summary>
        /// 货物 RFID，机械臂请求放行时会用到
        /// </summary>
        [JsonPropertyName("cargo_id")]
        public string Cargo_Id { get; set; } = string.Empty;

        /// <summary>
        /// 任务最晚执行时间，YYYY-MM-DD HH:mm::SS，到了这个时间还没执行就会优先执行
        /// </summary>
        [JsonPropertyName("complete_time")]
        public string Complete_Time { get; set; } = string.Empty;

        /// <summary>
        /// 任务接续号，两个任务需要连续执行接续号大于 0，相差1，如：第一个接续号 1，第二个接续号 2。与集中上报逻辑互斥不能同时使用。
        /// </summary>
        [JsonPropertyName("succession")]
        public int Succession { get; set; }

        /// <summary>
        /// 执行任务前集中上报标记，一般用于目的地不确定任务执行顺序有要求，“0”为无需集中上报
        /// </summary>
        [JsonPropertyName("pre_report")]
        public string Pre_Report { get; set; } = string.Empty;

        /// <summary>
        /// 任务执行优先级，取值 0-99，越大越优先
        /// </summary>
        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        /// <summary>
        /// 目的地动作，1-从设备取出物料，2-放置物料到机台，0-普通任务
        /// </summary>
        [JsonPropertyName("goal_action")]
        public int goal_action { get; set; }

        /// <summary>
        /// 备用
        /// </summary>
        [JsonPropertyName("mark")]
        public string Mark { get; set; } = string.Empty;

        /// <summary>
        /// 设备操作码，用于 plc 交互执行动作参数，目前有两个字段，用逗号分开
        /// </summary>
        [JsonPropertyName("option_code")]
        public string Option_Code { get; set; } = string.Empty;

        /// <summary>
        /// 地图点
        /// </summary>
        [JsonPropertyName("target")]
        public int Target { get; set; }

        /// <summary>
        /// 备用
        /// </summary>
        [JsonPropertyName("storage")]
        public string Storage { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        [JsonPropertyName("task_serial")]
        public string Task_Serial { get; set; } = string.Empty;

        /// <summary>
        /// 任务类型，即要求配置任务步 xml 的文件名，非空时优先使用，为空时任务类型采用 bulk_task_type 参数
        /// </summary>
        [JsonPropertyName("task_type")]
        public string Task_Type { get; set; } = string.Empty;
    }
}
