using RCS.DataManage;
using RCS.Dtos.DataManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace RCS.Interfaces
{
    public interface ILocationMapAppService:ICrudAppService<LocationMap,int,PagedAndSortedResultRequestDto,CreateLocationMapDto,UpdateLocationMapDto>
    {
    }
}
