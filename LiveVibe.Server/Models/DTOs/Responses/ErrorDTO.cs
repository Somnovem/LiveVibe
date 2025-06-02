namespace LiveVibe.Server.Models.DTOs.Responses
{
    public class ErrorDTO
    {
        public ErrorDTO(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
