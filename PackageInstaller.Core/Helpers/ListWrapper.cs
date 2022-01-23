using System.Collections;

namespace PackageInstaller.Core.Helpers;

public static class ListWrapper
{
    /// <summary>
    /// Wraps a <see cref="List{T}"/> in another <see cref="List{T}"/> and converts the types between them.
    /// The original list is only wrapped, not copied!
    /// </summary>
    /// <typeparam name="TFrom">The original type</typeparam>
    /// <typeparam name="TTo">The destination type</typeparam>
    /// <param name="list">The source list.</param>
    /// <param name="forwardConverter">A function that converts an instance of <typeparamref name="TFrom"/> to <typeparamref name="TTo"/>.</param>
    /// <param name="backwardConverter">A function that converts an instance of <typeparamref name="TTo"/> to <typeparamref name="TFrom"/>. If <code>null</code>, backward conversion is not supported and at runtime a <see cref="NotSupportedException"/> will be thrown.</param>
    /// <returns>A list of type <see cref="List{T}"/> </returns>
    public static IList<TTo> WrapAs<TFrom, TTo>(
        this IList<TFrom> list,
        Func<TFrom, TTo> forwardConverter,
        Func<TTo, TFrom>? backwardConverter = null
    )
    {
        return new WrappedList<TFrom, TTo>(
            list,
            forwardConverter,
            backwardConverter
                ?? (
                    _ =>
                        throw new NotSupportedException(
                            $"Conversion from {typeof(TTo).FullName} to {typeof(TFrom).FullName} is not supported."
                        )
                )
        );
    }

    class WrappedList<TFrom, TTo> : IList<TTo>
    {
        private readonly IList<TFrom> _listImplementation;
        private readonly Func<TFrom, TTo> _forwardConverter;
        private readonly Func<TTo, TFrom> _backwardConverter;

        public WrappedList(
            IList<TFrom> origList,
            Func<TFrom, TTo> forwardConverter,
            Func<TTo, TFrom> backwardConverter
        )
        {
            _listImplementation = origList;
            _forwardConverter = forwardConverter;
            _backwardConverter = backwardConverter;
        }

        public IEnumerator<TTo> GetEnumerator()
        {
            foreach (var item in _listImplementation)
            {
                yield return _forwardConverter(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_listImplementation).GetEnumerator();
        }

        public void Add(TTo item)
        {
            _listImplementation.Add(_backwardConverter(item));
        }

        public void Clear()
        {
            _listImplementation.Clear();
        }

        public bool Contains(TTo item)
        {
            return _listImplementation.Contains(_backwardConverter(item));
        }

        public void CopyTo(TTo[] array, int arrayIndex)
        {
            for (var i = arrayIndex; i < array.Length; i++)
            {
                array[i] = _forwardConverter(_listImplementation[i]);
            }
        }

        public bool Remove(TTo item)
        {
            return _listImplementation.Remove(_backwardConverter(item));
        }

        public int Count => _listImplementation.Count;

        public bool IsReadOnly => _listImplementation.IsReadOnly;

        public int IndexOf(TTo item)
        {
            return _listImplementation.IndexOf(_backwardConverter(item));
        }

        public void Insert(int index, TTo item)
        {
            _listImplementation.Insert(index, _backwardConverter(item));
        }

        public void RemoveAt(int index)
        {
            _listImplementation.RemoveAt(index);
        }

        public TTo this[int index]
        {
            get => _forwardConverter(_listImplementation[index]);
            set => _listImplementation[index] = _backwardConverter(value);
        }
    }
}
