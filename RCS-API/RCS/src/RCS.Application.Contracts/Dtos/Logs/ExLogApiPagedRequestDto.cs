using Volo.Abp.Application.Dtos;

namespace RCS.Dtos.Logs
{
    public class ExLogApiPagedRequestDto : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
    }
}
