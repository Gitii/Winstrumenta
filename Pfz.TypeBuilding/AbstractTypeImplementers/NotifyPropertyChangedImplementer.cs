using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Pfz.TypeBuilding.AbstractTypeImplementers;

/// <summary>
/// This class is capable of implementing abstract properties of classes and
/// interfaces with the right INotifyPropertyChanged pattern.
/// </summary>
/// <typeparam name="TAbstract">The abstract type to implement.</typeparam>
public sealed class NotifyPropertyChangedImplementer<TAbstract> : AbstractTypeImplementer<TAbstract>
{
    private readonly Expression<Func<PropertyChangedEventHandler>>? _eventHandlerFieldExpression;

    /// <summary>
    /// Creates a new implementer for the type that will declare the PropertyChanged event.
    /// </summary>
    public NotifyPropertyChangedImplementer() : base(typeof(INotifyPropertyChanged)) { }

    /// <summary>
    /// Creates a new implementer for the type.
    /// If the eventHandlerFieldExpression is null, it will create the event and its backing field,
    /// if not it will consider that such expression is acessing the right field that contains
    /// the event to be invoked.
    /// </summary>
    public NotifyPropertyChangedImplementer(
        Expression<Func<PropertyChangedEventHandler>> eventHandlerFieldExpression
    ) : base(typeof(INotifyPropertyChanged))
    {
        _eventHandlerFieldExpression = eventHandlerFieldExpression;
    }

    private NotifyPropertyChangedGenerator<TAbstract>? _generator;

    /// <summary>
    /// Creates the typebuilder and the NotifyPropertyChangedGenerator.
    /// </summary>
    protected override FluentTypeBuilder<TAbstract> CreateTypeBuilder(Type[] additionalInterfaces)
    {
        var result = base.CreateTypeBuilder(additionalInterfaces);
        _generator = new NotifyPropertyChangedGenerator<TAbstract>(
            result,
            _eventHandlerFieldExpression
        );
        return result;
    }

    /// <summary>
    /// Asks the NotifyPropertyChangedGenerator to implement the property
    /// using its own backing field.
    /// </summary>
    protected override void ImplementProperty<T>(
        FluentTypeBuilder<TAbstract> typeBuilder,
        PropertyInfo property
    )
    {
        _generator!.AddProperty<T>(property.Name);
    }

    /// <summary>
    /// If the property is PropertyChanged it does nothing.
    /// For any other property this method will throw a NotSupportedException().
    /// </summary>
    protected override void ImplementEvent<T>(
        FluentTypeBuilder<TAbstract> typeBuilder,
        EventInfo eventInfo
    )
    {
        if (
            eventInfo.EventHandlerType != typeof(PropertyChangedEventHandler)
            || eventInfo.Name != "PropertyChanged"
        )
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Throws a NotSupportedException as this type only implements properties.
    /// </summary>
    protected override void ImplementMethod(
        FluentTypeBuilder<TAbstract> typeBuilder,
        MethodInfo method
    )
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Throws a NotSupportedException as this type only implements properties.
    /// </summary>
    protected override void ImplementIndexerUntyped(
        FluentTypeBuilder<TAbstract> typeBuilder,
        PropertyInfo indexer
    )
    {
        throw new NotImplementedException();
    }
}
