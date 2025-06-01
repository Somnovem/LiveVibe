using System.ComponentModel.DataAnnotations;

namespace LiveVibe.Server.Models.DTOs.Requests.EventCategories
{
    public class CreateEventCategoryRequest
    {
        [Required, MaxLength(100)]
        public required string Name { get; set; }
    }
}
