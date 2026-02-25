
using RCS.DataManage;
using RCS.Dtos.DataManage;
using RCS.Dtos.Logs;
using RCS.Dtos.TM;
using RCS.LogManage.APILogs;
using RCS.XinSong.TM;
using AutoMapper;
using Volo.Abp.AuditLogging;

namespace RCS;

public class RCSApplicationAutoMapperProfile : Profile
{
    public RCSApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        CreateMap<LocationMap, LocationMapDto>();
        CreateMap<CreateLocationMapDto, LocationMap>();
        CreateMap<UpdateLocationMapDto, LocationMap>();
        CreateMap<CreateTaskDto, TaskCreationArgs>();

        // 外部API日志映射
        CreateMap<ThirdPartyCallLog, ExLogApiDto>();

        // 内部API日志映射（审计日志）
        CreateMap<AuditLog, InternalApiLogDto>();
    }
}
