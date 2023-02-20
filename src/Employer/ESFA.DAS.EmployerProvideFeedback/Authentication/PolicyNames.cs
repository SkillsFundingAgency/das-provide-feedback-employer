namespace ESFA.DAS.EmployerProvideFeedback.Authentication
{
    public static class PolicyNames
    {
        public static string EmployerAuthenticated => nameof(EmployerAuthenticated);
        public static string HasEmployerAccount => nameof(HasEmployerAccount);
        public static string HasEmployerViewerTransactorAccount => nameof(HasEmployerViewerTransactorAccount);
    }
}