namespace WebAPI.Exceptions
{
    public class ConflictException : Exception
    {
        // Eigen exception voor situaties waarbij de aanvraag conflicteert met de huidige situatie in de database.
        public ConflictException(string message)
            : base(message)
        {
        }
    }
}