using ESFA.DAS.EmployerProvideFeedback.Paging;

namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class PaginationLinksViewModel
    {
        public PaginatedList PaginatedList { get; set; }
        public string ChangePageAction { get; set; }
        public string ChangePageController { get; set; }
        public dynamic RouteValues { get; set; }
        public string Fragment { get; set; }
    }
}
