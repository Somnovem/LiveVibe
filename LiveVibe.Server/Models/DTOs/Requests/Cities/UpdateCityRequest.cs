﻿using System.ComponentModel.DataAnnotations;

namespace LiveVibe.Server.Models.DTOs.Requests.Countries
{
    public class UpdateCityRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }
    }
}