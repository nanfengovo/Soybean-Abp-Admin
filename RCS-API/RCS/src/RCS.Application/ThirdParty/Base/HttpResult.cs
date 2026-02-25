using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.ThirdParty.Base
{
    public class HttpResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Raw { get; set; }
        public string Error { get; set; }
        public T Data { get; set; }
    }

}
