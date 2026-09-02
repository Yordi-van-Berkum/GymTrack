namespace WebAPI.Exceptions
{
    public class NotFoundException : Exception
    {
        // Eigen exception voor situaties waarbij de gevraagde gegevens niet bestaan.
        public NotFoundException(string message)
            : base(message)
        {
        }
    }
}