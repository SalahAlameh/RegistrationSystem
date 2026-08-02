namespace RegistrationSystem.Models
{
    public class BorrowOperationResult
    {
        public bool Succeeded { get; private set; }
        public bool NotFound { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static BorrowOperationResult Success()
        {
            return new BorrowOperationResult
            {
                Succeeded = true
            };
        }

        public static BorrowOperationResult Failed(string errorMessage)
        {
            return new BorrowOperationResult
            {
                ErrorMessage = errorMessage
            };
        }

        public static BorrowOperationResult NotFoundResult(string errorMessage)
        {
            return new BorrowOperationResult
            {
                NotFound = true,
                ErrorMessage = errorMessage
            };
        }
    }
}
