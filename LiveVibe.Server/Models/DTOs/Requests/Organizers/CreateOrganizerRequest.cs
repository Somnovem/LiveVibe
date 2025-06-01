using System.ComponentModel.DataAnnotations;

namespace LiveVibe.Server.Models.DTOs.Requests.Organizers
{
    public class CreateOrganizerRequest
    {
        [Required, MaxLength(100)]
        public required string Name { get; set; }

        [Required, MaxLength(20)]
        public required string Phone { get; set; }

        [Required, EmailAddress, MaxLength(255)]
        public required string Email { get; set; }
    }
}
