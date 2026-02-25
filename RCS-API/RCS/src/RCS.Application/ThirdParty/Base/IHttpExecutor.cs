using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.ThirdParty.Base
{
    public interface IHttpExecutor
    {
        Task<HttpResult<T>> GetAsync<T>(string system, string url);
        Task<HttpResult<T>> PostAsync<T>(string system, string url, object body);
        Task<HttpResult<T>> PutAsync<T>(string system, string url, object body);
        Task<HttpResult<T>> DeleteAsync<T>(string system, string url);
    }

}
