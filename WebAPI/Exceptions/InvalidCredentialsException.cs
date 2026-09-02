namespace WebAPI.Exceptions
{
    // Exception voor wanneer de ingevoerde login gegevens niet kloppen.
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string message)
            : base(message)
        {
        }
    }
}