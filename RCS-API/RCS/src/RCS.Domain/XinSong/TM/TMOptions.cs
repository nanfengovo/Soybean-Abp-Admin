using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.XinSong.TM
{
    public class TMOptions
    {
        public bool IsSimulation { get; set; }

        public string SimulationUrl { get; set; }

        public string URL { get; set; }

        /// <summary>
        /// 辅助属性：根据配置自动获取最终有效的 URL
        /// </summary>
        public string GetActiveUrl()
        {
            var url = IsSimulation ? SimulationUrl : URL;
            // 简单处理：确保以 http 开头
            if (!string.IsNullOrEmpty(url) && !url.StartsWith("http"))
            {
                url = "http://" + url;
            }
            return url;
        }
    }
}
