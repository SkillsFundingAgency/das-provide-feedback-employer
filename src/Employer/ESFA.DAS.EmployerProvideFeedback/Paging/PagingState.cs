namespace ESFA.DAS.EmployerProvideFeedback.Paging
{
    public class PagingState
    {
        public const int DefaultPageIndex = 1;
        public const int DefaultPageSize = 2;

        public const string SortAscending = "Asc";
        public const string SortDescending = "Desc";

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SortDirection { get; set; }
        public string SortColumn { get; set; }
        public string SelectedProviderName { get; set; }
        public string SelectedFeedbackStatus { get; set; }

        public PagingState()
        {
            PageIndex = DefaultPageIndex;
            PageSize = DefaultPageSize;
            SortColumn = "TrainingProvider";
            SortDirection = SortAscending;
        }
    }
}
