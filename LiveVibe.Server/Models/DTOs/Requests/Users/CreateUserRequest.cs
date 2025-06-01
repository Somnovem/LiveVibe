using System.ComponentModel.DataAnnotations;

namespace LiveVibe.Server.Models.DTOs.Requests.Users
{
    public class CreateUserRequest
    {
        [Required, MaxLength(100)]
        public required string FirstName { get; set; }

        [Required, MaxLength(100)]
        public required string LastName { get; set; }

        [Required, MaxLength(20)]
        public required string Phone { get; set; }

        [Required, EmailAddress, MaxLength(255)]
        public required string Email { get; set; }

        [Required, MinLength(8), MaxLength(100)]
        public required string Password { get; set; }
    }

}
