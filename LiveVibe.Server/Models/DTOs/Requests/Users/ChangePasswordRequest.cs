using System.ComponentModel.DataAnnotations;

namespace LiveVibe.Server.Models.DTOs.Requests.Users
{
    public class ChangePasswordRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required, MinLength(8), MaxLength(100)]
        public required string CurrentPassword { get; set; }

        [Required, MinLength(8), MaxLength(100)]
        public required string NewPassword { get; set; }
    }
}
