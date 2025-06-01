using System.ComponentModel.DataAnnotations;

namespace LiveVibe.Server.Models.DTOs.Requests.Countries
{
    public class CreateCountryRequest
    {
        [Required, MaxLength(100)]
        public required string Name { get; set; }
    }
}
