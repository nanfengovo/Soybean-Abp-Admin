using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbpOverallAuth
{
    public class TestAppService:AbpOverallAuthAppService
    {
        [Authorize]
        public string Create(string name)
        {
            return name;
        }
    }
}
