namespace NuKeeper.Commands
{
    public class ValidationResult
    {
        private ValidationResult(bool success, string errorMessage)
        {
            IsSuccess = success;
            ErrorMessage = errorMessage;
        }

        public bool IsSuccess { get; }

        public string ErrorMessage { get; }

        public static ValidationResult Success = new ValidationResult(true, string.Empty);

        public static ValidationResult Failure(string errorMessage)
        {
            return new ValidationResult(false, errorMessage);
        }
    }
}
