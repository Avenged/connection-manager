namespace ConnectionManager.Common
{
    public class GenericOperationResult
    {
        public string ErrorMessage { get; set; }
        public bool Succeeded { get; set; }

        public GenericOperationResult()
        {
        }

        public GenericOperationResult(bool succeeded, string errorMessage)
        {
            Succeeded = succeeded;
            ErrorMessage = errorMessage;
        }
    }
}
