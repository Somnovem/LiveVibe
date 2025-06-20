﻿using System.ComponentModel.DataAnnotations;

namespace LiveVibe.Server.Models.DTOs.Requests.Events
{
    public class CreateEventRequest
    {
        [Required, MaxLength(255)]
        public required string Title { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public required Guid OrganizerId { get; set; }

        [Required]
        public required Guid CategoryId { get; set; }

        [Required, MaxLength(255)]
        public required string Location { get; set; }

        [Required]
        public required Guid CityId { get; set; }

        [Required, DataType(DataType.DateTime)]
        public required DateTime Time { get; set; }
    }
}
