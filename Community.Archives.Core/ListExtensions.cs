namespace Community.Archives.Core;

public static class ListExtensions
{
    public static T? FirstOrNullable<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        where T : struct
    {
        foreach (var item in list)
        {
            if (predicate(item))
            {
                return item;
            }
        }

        return default;
    }
}