
using AbpOverallAuth.DataManage;
using AbpOverallAuth.Dtos.DataManage;
using AutoMapper;

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
    }
}
