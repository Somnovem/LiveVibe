namespace LiveVibe.Server.Models.DTOs.Responses
{
    public class UploadPhotoSuccessDTO
    {
        public string ImageUrl { get; set; } = string.Empty;

        public UploadPhotoSuccessDTO(string imageUrl)
        {
            ImageUrl = imageUrl;
        }
    }
}
