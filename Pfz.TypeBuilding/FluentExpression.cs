using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Pfz.TypeBuilding;

/// <summary>
/// Represents an Expression that can be modified using a fluent API.
/// </summary>
public sealed class FluentExpression
{
    private FluentExpression() { }

    /// <summary>
    /// Creates a new fluent expression based on an already existing expression.
    /// </summary>
    public static FluentExpression Create(Expression expression)
    {
        var result = new FluentExpression();

        if (expression != null)
        {
            if (expression.NodeType != ExpressionType.Lambda)
            {
                result.Expression = expression;
            }
            else
            {
                LambdaExpression lamdaExpression = (LambdaExpression)expression;
                result.Expression = lamdaExpression.Body;
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a new fluent expression based on an already existing expression.
    /// </summary>
    public static FluentExpression Create<T>(Expression<Func<T>> expression)
    {
        var result = new FluentExpression();

        if (expression != null)
        {
            result.Expression = expression.Body;
        }

        return result;
    }

    /// <summary>
    /// Creates an Assign expression.
    /// </summary>
    public static FluentExpression Assign<T1, T2>(
        Expression<Func<T1>> left,
        Expression<Func<T2>> right
    ) where T2 : T1
    {
        if (left == null)
        {
            throw new ArgumentNullException("left");
        }

        if (right == null)
        {
            throw new ArgumentNullException("right");
        }

        return Expression.Assign(left.Body, right.Body);
    }

    /// <summary>
    /// Gets the LINQ expression held by this FluentExpression.
    /// </summary>
    public Expression? Expression { get; private set; }

    /// <summary>
    /// Does an implicit conversion from a fluent expression to a linq expression.
    /// </summary>
    public static implicit operator FluentExpression(Expression expression)
    {
        return Create(expression);
    }

    /// <summary>
    /// Does an implicit conversion from a linq expression to a fluent expression.
    /// </summary>
    public static implicit operator Expression(FluentExpression expression)
    {
        if (expression == null)
        {
            return null;
        }

        return expression.Expression;
    }

    /// <summary>
    /// Adds a non-static Call to this expression.
    /// </summary>
    public FluentExpression Call(MethodInfo method, params FluentExpression[] arguments)
    {
        if (method == null)
        {
            throw new ArgumentNullException("method");
        }

        if (method.IsStatic)
        {
            throw new ArgumentException("The given method is static.", "method");
        }

        Expression[] typedArguments;
        if (arguments == null || arguments.Length == 0)
        {
            typedArguments = null;
        }
        else
        {
            typedArguments = new Expression[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                typedArguments[i] = arguments[i];
            }
        }

        Expression = Expression.Call(Expression, method, typedArguments);
        return this;
    }

    /// <summary>
    /// Adds a member access to this expression.
    /// </summary>
    public FluentExpression MemberAccess(MemberInfo member)
    {
        if (member == null)
        {
            throw new ArgumentNullException("member");
        }

        Expression = Expression.MakeMemberAccess(Expression, member);
        return this;
    }

    /// <summary>
    /// Creates a new expression that inverts the bits of the result of the given expression.
    /// </summary>
    public static FluentExpression Not<T>(Expression<Func<T>> expression)
    {
        return FluentExpression.Create(expression).Not();
    }

    /// <summary>
    /// Adds an inversion of the bits as the last statement of this expression.
    /// </summary>
    public FluentExpression Not()
    {
        Expression = Expression.Not(Expression);
        return this;
    }

    /// <summary>
    /// Creates a new expression that "negates" the given one.
    /// </summary>
    public static FluentExpression Negate<T>(Expression<Func<T>> expression)
    {
        return FluentExpression.Create(expression).Negate();
    }

    /// <summary>
    /// Adds a final "negate" to this expression.
    /// </summary>
    /// <returns></returns>
    public FluentExpression Negate()
    {
        Expression = Expression.Negate(Expression);
        return this;
    }

    /// <summary>
    /// Adds a bitwise and to this expression.
    /// </summary>
    public FluentExpression BitwiseAnd(FluentExpression expression)
    {
        Expression = Expression.And(Expression, expression);
        return this;
    }

    /// <summary>
    /// Adds a bitwise or to this expression.
    /// </summary>
    public FluentExpression BitwiseOr(FluentExpression expression)
    {
        Expression = Expression.Or(Expression, expression);
        return this;
    }

    /// <summary>
    /// Adds a bitwise xor to this expression.
    /// </summary>
    public FluentExpression BitwiseXor(FluentExpression expression)
    {
        Expression = Expression.ExclusiveOr(Expression, expression);
        return this;
    }

    /// <summary>
    /// Adds a condition (boolean) and to this expression.
    /// </summary>
    public FluentExpression ConditionalAnd(FluentExpression expression)
    {
        Expression = Expression.AndAlso(Expression, expression);
        return this;
    }

    /// <summary>
    /// Adds a conditional (boolean) or to this expression.
    /// </summary>
    public FluentExpression ConditionalOr(FluentExpression expression)
    {
        Expression = Expression.OrElse(Expression, expression);
        return this;
    }

    /// <summary>
    /// Adds a value conversion to this expression.
    /// </summary>
    public FluentExpression Convert(Type toType)
    {
        Expression = Expression.Convert(Expression, toType);
        return this;
    }

    /// <summary>
    /// Adds an Unbox to this expression.
    /// </summary>
    public FluentExpression Unbox(Type toType)
    {
        Expression = Expression.Unbox(Expression, toType);
        return this;
    }

    /// <summary>
    /// Adds an arithmatic Add to this expression.
    /// </summary>
    public FluentExpression Add(FluentExpression expression)
    {
        Expression = Expression.Add(Expression, expression);
        return this;
    }

    /// <summary>
    /// Adds an arithmatic Subtract to this expression.
    /// </summary>
    public FluentExpression Subtract(FluentExpression expression)
    {
        Expression = Expression.Subtract(Expression, expression);
        return this;
    }

    /// <summary>
    /// Adds an arithmatic Multiply to this expression.
    /// </summary>
    public FluentExpression Multiply(FluentExpression expression)
    {
        Expression = Expression.Multiply(Expression, expression);
        return this;
    }

    /// <summary>
    /// Adds an arithmatic Divide to this expression.
    /// </summary>
    public FluentExpression Divide(FluentExpression expression)
    {
        Expression = Expression.Divide(Expression, expression);
        return this;
    }

    /// <summary>
    /// Adds an Equal comparison to this expression.
    /// </summary>
    public FluentExpression Equal<T>(Expression<Func<T>> expression)
    {
        return Equal(expression.Body);
    }

    /// <summary>
    /// Adds an Equal comparison to this expression.
    /// </summary>
    public FluentExpression Equal(FluentExpression expression)
    {
        Expression = Expression.Equal(Expression, expression);
        return this;
    }

    /// <summary>
    /// Adds a Different comparison to this expression.
    /// </summary>
    public FluentExpression NotEqual<T>(Expression<Func<T>> expression)
    {
        return NotEqual(expression.Body);
    }

    /// <summary>
    /// Adds a Different comparison to this expression.
    /// </summary>
    public FluentExpression NotEqual(FluentExpression expression)
    {
        Expression = Expression.NotEqual(Expression, expression);
        return this;
    }

    /// <summary>
    /// Adds a GreaterThan comparison to this expression.
    /// </summary>
    public FluentExpression GreaterThan(FluentExpression expression)
    {
        Expression = Expression.GreaterThan(Expression, expression);
        return this;
    }

    /// <summary>
    /// Adds a GreaterThanOrEqual comparison to this expression.
    /// </summary>
    public FluentExpression GreaterThanOrEqual(FluentExpression expression)
    {
        Expression = Expression.GreaterThanOrEqual(Expression, expression);
        return this;
    }

    /// <summary>
    /// Adds a LessThan comparison to this expression.
    /// </summary>
    public FluentExpression LessThan(FluentExpression expression)
    {
        Expression = Expression.LessThan(Expression, expression);
        return this;
    }

    /// <summary>
    /// Adds a LessThanOrEqual comparison to this expression.
    /// </summary>
    public FluentExpression LessThanOrEqual(FluentExpression expression)
    {
        Expression = Expression.LessThanOrEqual(Expression, expression);
        return this;
    }

    /// <summary>
    /// Creates an Increment expression over the given one.
    /// </summary>
    public static FluentExpression Increment<T>(Expression<Func<T>> expression)
    {
        return FluentExpression.Create(expression).Increment();
    }

    /// <summary>
    /// Adds an Increment to this expression.
    /// </summary>
    public FluentExpression Increment()
    {
        Expression = Expression.Increment(Expression);
        return this;
    }

    /// <summary>
    /// Creates a Decrement expression over the given one.
    /// </summary>
    public static FluentExpression Decrement<T>(Expression<Func<T>> expression)
    {
        return FluentExpression.Create(expression).Decrement();
    }

    /// <summary>
    /// Adds a Decrement to this expression.
    /// </summary>
    public FluentExpression Decrement()
    {
        Expression = Expression.Decrement(Expression);
        return this;
    }

    /// <summary>
    /// Creates an expression that is doing a static call to a method.
    /// </summary>
    public static FluentExpression StaticCall(
        MethodInfo method,
        params FluentExpression[] arguments
    )
    {
        if (method == null)
        {
            throw new ArgumentNullException("method");
        }

        if (!method.IsStatic)
        {
            throw new ArgumentException("The given method is an instance method.", "method");
        }

        Expression[] typedArguments;
        if (arguments == null || arguments.Length == 0)
        {
            typedArguments = null;
        }
        else
        {
            typedArguments = new Expression[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                typedArguments[i] = arguments[i];
            }
        }

        return Expression.Call(method, typedArguments);
    }
}
