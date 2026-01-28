using AbpOverallAuth.ThirdParty.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbpOverallAuth.ThirdParty.TM
{
    public class TMClient
    {
        private readonly IHttpExecutor _http;

        public TMClient(IHttpExecutor http)
        {
            _http = http;
        }

        public Task<HttpResult<TMResp>> CreateTask(TMTaskDto dto)
        => _http.PostAsync<TMResp>("xinsong", "/api/v1/xinsong/task_add", dto);
    }
}
