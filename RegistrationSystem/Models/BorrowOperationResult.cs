namespace RegistrationSystem.Models
{
    public class BorrowOperationResult
    {
        public bool Succeeded { get; private set; }
        public bool NotFound { get; private set; }
        // The service returns a resource key so the controller can
        // translate it using the language selected for this request.
        public string? ErrorKey { get; private set; }

        public static BorrowOperationResult Success()
        {
            return new BorrowOperationResult
            {
                Succeeded = true
            };
        }

        public static BorrowOperationResult Failed(string errorKey)
        {
            return new BorrowOperationResult
            {
                ErrorKey = errorKey
            };
        }

        public static BorrowOperationResult NotFoundResult(string errorKey)
        {
            return new BorrowOperationResult
            {
                NotFound = true,
                ErrorKey = errorKey
            };
        }
    }
}
