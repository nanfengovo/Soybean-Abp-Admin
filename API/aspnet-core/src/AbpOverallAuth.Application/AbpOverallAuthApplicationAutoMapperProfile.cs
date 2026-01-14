using AbpOverallAuth.Navigation;
using AutoMapper;

namespace AbpOverallAuth;

public class AbpOverallAuthApplicationAutoMapperProfile : Profile
{
    public AbpOverallAuthApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        CreateMap<Menu, MenuDto>();
        CreateMap<CreateMenuDto, Menu>();
    }
}
