using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Pfz.TypeBuilding;

/// <summary>
/// This class only helps using type-inference to create the right
/// NotifyPropertyChanged&lt;&gt; instance by the generic type of the
/// FluentTypeBuilder.
/// </summary>
public static class NotifyPropertyChangedGenerator
{
    /// <summary>
    /// Creates a new NotifyPropertyChangedGenerator that will work with the given typeBuilder.
    /// </summary>
    public static NotifyPropertyChangedGenerator<TBase> Create<TBase>(
        FluentTypeBuilder<TBase> typeBuilder
    )
    {
        return new NotifyPropertyChangedGenerator<TBase>(typeBuilder);
    }

    /// <summary>
    /// Creates a new NotifyPropertyChangedGenerator that will work with the given typeBuilder
    /// and also let you tell where is the field that stores the PropertyChanged delegates.
    /// </summary>
    public static NotifyPropertyChangedGenerator<TBase> Create<TBase>(
        FluentTypeBuilder<TBase> typeBuilder,
        Expression<Func<PropertyChangedEventHandler>> eventHandlerFieldExpression
    )
    {
        return new NotifyPropertyChangedGenerator<TBase>(typeBuilder, eventHandlerFieldExpression);
    }

    internal static MethodInfo _smartEquals = typeof(NotifyPropertyChangedGenerator).GetMethod(
        "_SmartEquals",
        BindingFlags.NonPublic | BindingFlags.Static
    );

    internal static bool _SmartEquals<T>(T value1, T value2)
    {
        if (typeof(T).IsValueType)
        {
            return EqualityComparer<T>.Default.Equals(value1, value2);
        }

        return object.ReferenceEquals(value1, value2);
    }
}

/// <summary>
/// This class helps in building types at run-time that implement the
/// INotifyPropertyChanged interface.
/// </summary>
public sealed class NotifyPropertyChangedGenerator<TBase>
{
    private static PropertyChangedEventHandler _defaultEventHandler = null;

    private static readonly Expression<
        Func<PropertyChangedEventHandler>
    > _defaultEventHandlerFieldExpression = () => _defaultEventHandler;

    private readonly FluentTypeBuilder<TBase> _typeBuilder;
    private readonly Expression<Func<PropertyChangedEventHandler>> _eventHandlerFieldExpression;

    /// <summary>
    /// Creates a new NotifyPropertyChangedGenerator that will work with the given typeBuilder.
    /// </summary>
    public NotifyPropertyChangedGenerator(FluentTypeBuilder<TBase> typeBuilder)
        : this(typeBuilder, null) { }

    /// <summary>
    /// Creates a new NotifyPropertyChangedGenerator that will work with the given typeBuilder
    /// and also let you tell where is the field that stores the PropertyChanged delegates.
    /// </summary>
    public NotifyPropertyChangedGenerator(
        FluentTypeBuilder<TBase> typeBuilder,
        Expression<Func<PropertyChangedEventHandler>> eventHandlerFieldExpression
    )
    {
        if (typeBuilder == null)
        {
            throw new ArgumentNullException("typeBuilder");
        }

        _typeBuilder = typeBuilder;
        if (eventHandlerFieldExpression != null)
        {
            _eventHandlerFieldExpression = eventHandlerFieldExpression;
        }
        else
        {
            _eventHandlerFieldExpression = _defaultEventHandlerFieldExpression;
            typeBuilder.AddField("_propertyChanged", _defaultEventHandlerFieldExpression);

            typeBuilder.AddEventWithDefaultImplementation(
                "PropertyChanged",
                _defaultEventHandlerFieldExpression,
                false
            );
        }
    }

    /// <summary>
    /// Adds a new property to the generated type.
    /// The property will have its own field. You can optionally
    /// tell which other properties should be notified as having
    /// changed by the change on the actual property (this happens
    /// when you use calculated properties).
    /// </summary>
    public void AddProperty(string name, Type type, params string[] otherPropertiesToNotify)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }

        if (type == null)
        {
            throw new ArgumentNullException("type");
        }

        if (type == typeof(void))
        {
            throw new ArgumentException("A property can't be declared having the type void.");
        }

        var addProperty = _addProperty.MakeGenericMethod(type);
        addProperty.Invoke(this, new object[] { name, otherPropertiesToNotify });
    }

    private static readonly MethodInfo _addProperty =
        typeof(NotifyPropertyChangedGenerator<TBase>).GetMethod(
            "AddProperty",
            new Type[] { typeof(string), typeof(string[]) }
        );

    /// <summary>
    /// Adds a new property to the generated type.
    /// The property will have its own field. You can optionally
    /// tell which other properties should be notified as having
    /// changed by the change on the actual property (this happens
    /// when you use calculated properties).
    /// </summary>
    public void AddProperty<T>(string name, params string[] otherPropertiesToNotify)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }

        if (otherPropertiesToNotify != null && otherPropertiesToNotify.Length > 0)
        {
            otherPropertiesToNotify = (string[])otherPropertiesToNotify.Clone();

            foreach (string otherName in otherPropertiesToNotify)
            {
                if (otherName == null)
                {
                    throw new ArgumentException(
                        "otherPropertiesToNotify has null values.",
                        "otherPropertiesToNotify"
                    );
                }
            }
        }

        T field = default(T);
        _typeBuilder.AddField("_" + name, () => field);

        T value = default(T);
        var property = _typeBuilder.AddProperty(name, () => value, false);
        property.GetMethod.Body.Return(() => field).EndBody();

        PropertyChangedEventArgs mainArgs = new PropertyChangedEventArgs(name);
        PropertyChangedEventHandler eventHandler = null;

        var methodPart = property.SetMethod
            .AddExternal(() => mainArgs)
            .AddLocal(() => eventHandler)
            .Body.Assign(() => eventHandler, _eventHandlerFieldExpression)
            .If(() => eventHandler == null)
            .Assign(() => field, () => value)
            .Else()
            .If(() => NotifyPropertyChangedGenerator._SmartEquals(field, value))
            .Return()
            .EndIf()
            .Assign(() => field, () => value)
            .Do(() => eventHandler!(FluentTypeBuilder<TBase>.This!, mainArgs));

        if (otherPropertiesToNotify != null)
        {
            foreach (string otherName in otherPropertiesToNotify)
            {
                var otherArgs = new PropertyChangedEventArgs(otherName);
                property.SetMethod.AddExternal(() => otherArgs);
                methodPart.Do(() => eventHandler!(FluentTypeBuilder<TBase>.This!, otherArgs));
            }
        }

        methodPart.EndIf().EndBody();
    }

    /// <summary>
    /// Adds a property to the type being built. In this overload you should
    /// give the expressions that represent the get and the set, without invoking
    /// the PropertyChanged event. The effective get will be unchanged, but the
    /// set will verify if the value was really changed before doing the actual
    /// call to your set and notifying observers of the change.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <param name="getValueExpression">The expression that represents the get.</param>
    /// <param name="setValueExpression">
    /// The expression that represents a direct set (that is, your expression should
    /// not invoke the PropertyChanged event).
    /// </param>
    /// <param name="valueExpression">
    /// An expression that access a local variable. Such variable will be considered to
    /// be the "value" variable for the set method.
    /// </param>
    /// <param name="otherPropertiesToNotify">
    /// Optional names of other properties that should be notified as changed when this
    /// property is changed. This will happen on calculated properties based on this
    /// property.
    /// </param>
    public void AddProperty<T>(
        string name,
        Expression<Func<T>> getValueExpression,
        Expression<Action> setValueExpression,
        Expression<Func<T>> valueExpression,
        params string[] otherPropertiesToNotify
    )
    {
        if (setValueExpression == null)
        {
            throw new ArgumentNullException("setValueExpression");
        }

        var setAction = FluentMethodBuilder.CreateAction(setValueExpression);
        AddProperty<T>(
            name,
            getValueExpression,
            setAction,
            valueExpression,
            otherPropertiesToNotify
        );
    }

    /// <summary>
    /// Adds a property to the type being built. In this overload you should
    /// give the expressions that represent the get and the set, without invoking
    /// the PropertyChanged event. The effective get will be unchanged, but the
    /// set will verify if the value was really changed before doing the actual
    /// call to your set and notifying observers of the change.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <param name="getValueExpression">The expression that represents the get.</param>
    /// <param name="setValueAction">An action that does the set.</param>
    /// <param name="valueExpression">
    /// An expression that access a local variable. Such variable will be considered to
    /// be the "value" variable for the set method.
    /// </param>
    /// <param name="otherPropertiesToNotify">
    /// Optional names of other properties that should be notified as changed when this
    /// property is changed. This will happen on calculated properties based on this
    /// property.
    /// </param>
    public void AddProperty<T>(
        string name,
        Expression<Func<T>> getValueExpression,
        FluentMethodBuilder setValueAction,
        Expression<Func<T>> valueExpression,
        params string[] otherPropertiesToNotify
    )
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }

        if (getValueExpression == null)
        {
            throw new ArgumentNullException("getValueExpression");
        }

        if (setValueAction == null)
        {
            throw new ArgumentNullException("setValueAction");
        }

        setValueAction._CheckThread();

        if (setValueAction.ReturnType != typeof(void))
        {
            throw new ArgumentException("setAction must return void.", "setAction");
        }

        if (setValueAction._parameters.Count > 0)
        {
            throw new ArgumentException("setAction must not have any explicitly set parameters.");
        }

        setValueAction.MakeParametersReadOnly();

        if (otherPropertiesToNotify != null && otherPropertiesToNotify.Length > 0)
        {
            otherPropertiesToNotify = (string[])otherPropertiesToNotify.Clone();

            foreach (string otherName in otherPropertiesToNotify)
            {
                if (otherName == null)
                {
                    throw new ArgumentException(
                        "otherPropertiesToNotify has null values.",
                        "otherPropertiesToNotify"
                    );
                }
            }
        }

        var property = _typeBuilder.AddProperty(name, valueExpression, false);
        property.GetMethod.Body.Return(getValueExpression).EndBody();

        PropertyChangedEventArgs mainArgs = new PropertyChangedEventArgs(name);
        PropertyChangedEventHandler eventHandler = null;

        var methodPart = property.SetMethod
            .AddExternal(() => mainArgs)
            .AddLocal(() => eventHandler)
            .Body.Assign(() => eventHandler, _eventHandlerFieldExpression)
            .If(() => eventHandler == null)
            .Inline(setValueAction)
            .Else()
            .If(
                FluentExpression.StaticCall(
                    NotifyPropertyChangedGenerator._smartEquals.MakeGenericMethod(typeof(T)),
                    valueExpression,
                    getValueExpression
                )
            )
            .Return()
            .EndIf()
            .Inline(setValueAction)
            .Do(() => eventHandler!(FluentTypeBuilder<TBase>.This, mainArgs));

        if (otherPropertiesToNotify != null)
        {
            foreach (string otherName in otherPropertiesToNotify)
            {
                var otherArgs = new PropertyChangedEventArgs(otherName);
                property.SetMethod.AddExternal(() => otherArgs);
                methodPart.Do(() => eventHandler!(FluentTypeBuilder<TBase>.This, otherArgs));
            }
        }

        methodPart.EndIf().EndBody();
    }
}
