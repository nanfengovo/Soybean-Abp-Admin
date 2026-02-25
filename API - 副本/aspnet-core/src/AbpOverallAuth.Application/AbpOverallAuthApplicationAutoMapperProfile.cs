
using AbpOverallAuth.DataManage;
using AbpOverallAuth.Dtos.DataManage;
using AbpOverallAuth.Dtos.Logs;
using AbpOverallAuth.Dtos.TM;
using AbpOverallAuth.LogManage.APILogs;
using AbpOverallAuth.XinSong.TM;
using AutoMapper;
using Volo.Abp.AuditLogging;

namespace AbpOverallAuth;

public class AbpOverallAuthApplicationAutoMapperProfile : Profile
{
    public AbpOverallAuthApplicationAutoMapperProfile()
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
