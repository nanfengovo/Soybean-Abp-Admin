using RCS.DataManage;
using RCS.Dtos.DataManage;
using RCS.Interfaces;
using RCS.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace RCS
{
    public class LocationMapAppService : CrudAppService<LocationMap, LocationMapDto, int, PagedAndSortedResultRequestDto, CreateLocationMapDto, UpdateLocationMapDto>
    {
        public LocationMapAppService(IRepository<LocationMap, int> repository) : base(repository)
        {
            GetPolicyName = RCSPermissions.LocationMap.Default;
            GetListPolicyName = RCSPermissions.LocationMap.Default;
            CreatePolicyName = RCSPermissions.LocationMap.Create;
            UpdatePolicyName = RCSPermissions.LocationMap.Edit;
            DeletePolicyName = RCSPermissions.LocationMap.Delete;
        }
    }
}
