using Volo.Abp.Application.Dtos;

namespace AbpOverallAuth.Dtos.Logs
{
    public class ExLogApiPagedRequestDto : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
    }
}
