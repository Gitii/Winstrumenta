namespace Shared.Misc;

public static class AsyncEnumerableExtensions
{
    public static async Task<IList<T>> WhereAsync<T>(
        this IEnumerable<T> enumerable,
        Func<T, Task<bool>> predicate
    )
    {
        List<T> items = new List<T>();

        foreach (var item in enumerable)
        {
            if (await predicate(item).ConfigureAwait(false))
            {
                items.Add(item);
            }
        }

        return items;
    }
}
