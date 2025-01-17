using System.Collections;
using System.Collections.Generic;
using ESFA.DAS.EmployerProvideFeedback.Paging;

namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class PaginationLinksViewModel
    {
        public PaginatedList PaginatedList { get; set; }
        public string ChangePageAction { get; set; }
        public string ChangePageController { get; set; }
        public IDictionary<string,string> RouteValues { get; set; }
        public string Fragment { get; set; }
    }
}
