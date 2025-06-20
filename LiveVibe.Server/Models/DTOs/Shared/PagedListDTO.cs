﻿namespace LiveVibe.Server.Models.DTOs.Shared
{
    public class PagedListDTO<T>
    {
        public List<T> Items { get; set; } = [];
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
    }
}
