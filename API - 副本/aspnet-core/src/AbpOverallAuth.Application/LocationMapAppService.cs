using AbpOverallAuth.DataManage;
using AbpOverallAuth.Dtos.DataManage;
using AbpOverallAuth.Interfaces;
using AbpOverallAuth.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AbpOverallAuth
{
    public class LocationMapAppService : CrudAppService<LocationMap, LocationMapDto, int, PagedAndSortedResultRequestDto, CreateLocationMapDto, UpdateLocationMapDto>
    {
        public LocationMapAppService(IRepository<LocationMap, int> repository) : base(repository)
        {
            GetPolicyName = AbpOverallAuthPermissions.LocationMap.Default;
            GetListPolicyName = AbpOverallAuthPermissions.LocationMap.Default;
            CreatePolicyName = AbpOverallAuthPermissions.LocationMap.Create;
            UpdatePolicyName = AbpOverallAuthPermissions.LocationMap.Edit;
            DeletePolicyName = AbpOverallAuthPermissions.LocationMap.Delete;
        }
    }
}
