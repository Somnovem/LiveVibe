namespace LiveVibe.Server.HelperClasses
{
    public class PagedList<T> : List<T>
    {
        public PagedList(IEnumerable<T> items, int totalCount, int page, int pageSize)
        {
            TotalCount = totalCount;
            PageSize = pageSize;
            Page = page;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            AddRange(items);
        }

        public int Page { get; private set; }

        public int TotalPages { get; private set; }

        public int PageSize { get; private set; }

        public int TotalCount { get; private set; }

        public bool HasPrevious => Page > 1;

        public bool HasNext => Page < TotalPages;
    }
}
