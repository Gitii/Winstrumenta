using System;
using System.Reflection;

namespace Pfz.TypeBuilding;

/// <summary>
/// Base class that you can inherit if you want to generate types at run-time that
/// implement the abstract methods of abstract classes or interfaces.
/// </summary>
public abstract class AbstractTypeImplementer<TAbstract>
{
    /// <summary>
    /// Only verifies that the generic argument is really abstract.
    /// </summary>
    static AbstractTypeImplementer()
    {
        if (!typeof(TAbstract).IsAbstract)
        {
            throw new InvalidOperationException(
                typeof(TAbstract).FullName + " is not an abstract type."
            );
        }
    }

    private Type[]? _additionalInterfaces;

    /// <summary>
    /// Creates a new AbstractTypeImplementer instance, allowing to tell which
    /// interfaces (that aren't already part of the abstract type) should be implemented.
    /// </summary>
    protected AbstractTypeImplementer(params Type[] additionalInterfaces)
    {
        if (_additionalInterfaces != null && _additionalInterfaces.Length > 0)
        {
            _additionalInterfaces = (Type[])additionalInterfaces.Clone();
        }
    }

    private FluentTypeBuilder<TAbstract>? _typeBuilder;

    private FluentTypeBuilder<TAbstract> _GetTypeBuilder()
    {
        if (_typeBuilder != null)
        {
            return _typeBuilder;
        }

        var typeBuilder = CreateTypeBuilder(_additionalInterfaces);
        _Implement(typeBuilder, typeof(TAbstract));

        if (typeof(TAbstract).IsInterface)
        {
            var baseInterfaces = typeof(TAbstract).GetInterfaces();
            foreach (var baseInterface in baseInterfaces)
            {
                _Implement(typeBuilder, baseInterface);
            }
        }

        if (_additionalInterfaces != null)
        {
            foreach (var additionalInterface in _additionalInterfaces)
            {
                if (!additionalInterface.IsAssignableFrom(typeof(TAbstract)))
                {
                    _Implement(typeBuilder, additionalInterface);
                }
            }
        }

        _typeBuilder = typeBuilder;
        return typeBuilder;
    }

    /// <summary>
    /// Gets the type that was implemented at run-time.
    /// </summary>
    public Type? GetImplementedType()
    {
        return _GetTypeBuilder().Compile();
    }

    /// <summary>
    /// Gets a delegate capable of creating new instances of the implemented type.
    /// </summary>
    public Func<TAbstract>? GetConstructorDelegate()
    {
        return _GetTypeBuilder().GetConstructorDelegate();
    }

    /// <summary>
    /// Creates and initializes the TypeBuilder that will be used to implement
    /// properties, methods and events.
    /// </summary>
    protected virtual FluentTypeBuilder<TAbstract> CreateTypeBuilder(Type[] additionalInterfaces)
    {
        return new FluentTypeBuilder<TAbstract>(typeof(TAbstract).FullName, additionalInterfaces);
    }

    private void _Implement(FluentTypeBuilder<TAbstract> typeBuilder, Type interfaceType)
    {
        foreach (var method in interfaceType.GetMethods())
        {
            if (method.IsAbstract)
            {
                ImplementAnyMethod(typeBuilder, method);
            }
        }

        foreach (var property in interfaceType.GetProperties())
        {
            if (_IsAbstract(property))
            {
                ImplementAnyProperty(typeBuilder, property);
            }
        }

        foreach (var eventInfo in interfaceType.GetEvents())
        {
            if (eventInfo.GetAddMethod()?.IsAbstract ?? false)
            {
                ImplementEventUntyped(typeBuilder, eventInfo);
            }
        }
    }

    private static bool _IsAbstract(PropertyInfo property)
    {
        var getMethod = property.GetGetMethod(true);
        if (getMethod != null)
        {
            return getMethod.IsAbstract;
        }

        var setMethod = property.GetSetMethod(true);
        if (setMethod != null)
        {
            return setMethod.IsAbstract;
        }

        return false;
    }

    /// <summary>
    /// This method is invoked to implement any method, be it a normal method, a property getter, setter etc.
    /// If you want to implement only normal methods, inherit the ImplementMethod method.
    /// </summary>
    protected virtual void ImplementAnyMethod(
        FluentTypeBuilder<TAbstract> typeBuilder,
        MethodInfo method
    )
    {
        if (!method.IsSpecialName)
        {
            ImplementMethod(typeBuilder, method);
        }
    }

    /// <summary>
    /// This method is invoked to implement any property, be it a normal property or an indexer.
    /// If you want to generate only properties, override the ImplementPropertyUntyped or the ImplementProperty generic method.
    /// </summary>
    protected virtual void ImplementAnyProperty(
        FluentTypeBuilder<TAbstract> typeBuilder,
        PropertyInfo property
    )
    {
        if (property.GetIndexParameters().Length > 0)
        {
            ImplementIndexerUntyped(typeBuilder, property);
        }
        else
        {
            ImplementPropertyUntyped(typeBuilder, property);
        }
    }

    private static readonly MethodInfo _implementProperty =
        typeof(AbstractTypeImplementer<TAbstract>).GetMethod(
            "ImplementProperty",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

    /// <summary>
    /// This method is invoked to implement normal properties (that is, excluding indexers), but it does not have the
    /// parameters rightly typed, which can make it hard to build the expression. You will probably prefer to inherit
    /// the ImplementProperty generic method.
    /// </summary>
    protected virtual void ImplementPropertyUntyped(
        FluentTypeBuilder<TAbstract> typeBuilder,
        PropertyInfo property
    )
    {
        var implementProperty = _implementProperty.MakeGenericMethod(property.PropertyType);
        implementProperty.Invoke(this, new object[] { typeBuilder, property });
    }

    private static readonly MethodInfo _implementEvent =
        typeof(AbstractTypeImplementer<TAbstract>).GetMethod(
            "ImplementEvent",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

    /// <summary>
    /// This method is invoked to implement events, but it is not typed. It is usually better to only override the
    /// ImplementEvent generic method.
    /// </summary>
    protected virtual void ImplementEventUntyped(
        FluentTypeBuilder<TAbstract> typeBuilder,
        EventInfo eventInfo
    )
    {
        var implementEvent = _implementEvent.MakeGenericMethod(eventInfo.EventHandlerType);
        implementEvent.Invoke(this, new object[] { typeBuilder, eventInfo });
    }

    /// <summary>
    /// Method invoked to implement methods.
    /// </summary>
    protected abstract void ImplementMethod(
        FluentTypeBuilder<TAbstract> typeBuilder,
        MethodInfo method
    );

    /// <summary>
    /// Method invoked to implement properties.
    /// </summary>
    protected abstract void ImplementProperty<T>(
        FluentTypeBuilder<TAbstract> typeBuilder,
        PropertyInfo property
    );

    /// <summary>
    /// Method invoked to implement indexers.
    /// </summary>
    protected abstract void ImplementIndexerUntyped(
        FluentTypeBuilder<TAbstract> typeBuilder,
        PropertyInfo indexer
    );

    /// <summary>
    /// Method invoked to implement events.
    /// </summary>
    protected abstract void ImplementEvent<T>(
        FluentTypeBuilder<TAbstract> typeBuilder,
        EventInfo eventInfo
    );
}
