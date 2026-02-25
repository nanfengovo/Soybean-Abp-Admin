using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbpOverallAuth.Dtos.TM
{
    public class ResponseTMBaseDTO
    {
        /// <summary>
        /// 返回消息提示, true表示成功，false失败
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// 返回错误信息，成功则为空，失败则显示具体出错信息
        /// </summary>
        public string ErrMsg { get; set; } = "";

        public static ResponseTMBaseDTO Fail(string msg = "失败")
        {
            return new ResponseTMBaseDTO()
            {
                Result = false,
                ErrMsg = msg
            };
        }

        public static ResponseTMBaseDTO Success(string msg = "成功")
        {
            return new ResponseTMBaseDTO()
            {
                Result = true,
                ErrMsg = msg
            };
        }
    }

    public class ResponseTMBaseDTO<T> : ResponseTMBaseDTO where T : class
    {
        /// <summary>
        /// 附加信息
        /// </summary>
        public T Data { get; set; }

        public static ResponseTMBaseDTO<T> Success(T data, string msg = "成功")
        {
            return new ResponseTMBaseDTO<T>()
            {
                Result = true,
                ErrMsg = msg,
                Data = data
            };
        }
    }
}
