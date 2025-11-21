namespace Transit.Application;

public static class PaginationHelper
{
    public static async Task<PaginatedResult<T>> GetPaginatedResultAsync<T>(
        List<T> items,
        int pageNumber,
        int pageSize)
    {
        var totalItems = items.Count;
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var pagedItems = items
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<T>(pagedItems, totalItems, pageNumber, pageSize);
    }
}
