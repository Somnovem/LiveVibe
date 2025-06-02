namespace LiveVibe.Server.Models.DTOs.Responses
{
    public class SuccessDTO
    {
        public SuccessDTO(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
