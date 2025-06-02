using LiveVibe.Server.HelperClasses.Collections;
using LiveVibe.Server.Models.DTOs.Shared;
using Microsoft.EntityFrameworkCore;

namespace LiveVibe.Server.HelperClasses.Extensions
{
    public static class PaginationExtensions
    {
        public static PagedList<T> ToPagedList<T>(this ICollection<T> source, int pageNumber, int pageSize)
        {
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items, source.Count, pageNumber, pageSize);
        }

        public static PagedList<T> ToPagedList<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items, source.Count(), pageNumber, pageSize);
        }

        public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(items, source.Count(), pageNumber, pageSize);
        }

        public static PagedListDTO<T> ToDto<T>(this PagedList<T> pagedList)
        {
            return new PagedListDTO<T>
            {
                Items = pagedList,
                Page = pagedList.Page,
                TotalPages = pagedList.TotalPages,
                PageSize = pagedList.PageSize,
                TotalCount = pagedList.TotalCount,
                HasPrevious = pagedList.HasPrevious,
                HasNext = pagedList.HasNext
            };
        }
    }
}
