using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbpOverallAuth.ToolHelper
{
    public static class BitPacker
    {
        /* 用来处理给TM的报文中的option_code,支持任意类型的字段和任意数量的字段
        * 
        使用示例：
            // 计算 TaskCodea
            // 顺序：取任务料盒数量(8位), 料盒类型(8位), TT起始(8位), 批次(8位)
            int taskCode1 = BitPacker.Pack(
                (count, 8), 
                (carrierType, 8), 
                (ttStart, 8), 
                (lotId, 8)
            );

            // 计算 TaskCodeb
            // 哪怕某些字段是 String，Convert.ToInt64 也会帮你处理
            int taskCode2 = BitPacker.Pack(
                ("5", 8),             // 假设 count 是字符串
                (putOrFetchFlag, 8), 
                (0, 8),               // machineLocationId 一直为 0
                (machineType, 8)
            );

            // 最后组合
            subtask.option_code = $"{taskCode1},{taskCode2}";
        */

        /// <summary>
        /// 将多个字段打包成一个 int
        /// </summary>
        /// <param name="fields">字段定义：(字段值, 占据的位数)</param>
        /// <returns>打包后的整数</returns>
        public static int Pack(params (object Value, int BitWidth)[] fields)
        {
            uint result = 0;
            int currentOffset = 32; // 从最高位开始往下排

            foreach (var field in fields)
            {
                // 1. 自动转换类型为 long 以确保安全
                long val = Convert.ToInt64(field.Value);

                // 2. 计算该字段需要的偏移量
                currentOffset -= field.BitWidth;

                if (currentOffset < 0)
                    throw new ArgumentException("所有字段总位数超过了 32 位！");

                // 3. 掩码处理（防止值太大溢出到别的字段）并移位
                uint mask = (uint)((1L << field.BitWidth) - 1);
                result |= ((uint)(val & mask) << currentOffset);
            }

            return (int)result;
        }
    }
}
