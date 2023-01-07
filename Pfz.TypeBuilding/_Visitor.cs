using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Pfz.TypeBuilding;

internal sealed class _Visitor : ExpressionVisitor
{
    internal readonly FluentMethodBuilder _method;

    internal _Visitor(FluentMethodBuilder method)
    {
        _method = method;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var method = node.Method;

        if (method.Name == "Call")
        {
            var declaringType = method.DeclaringType;
            if (
                declaringType == typeof(FluentVoidMethodBuilder)
                || declaringType!.IsGenericType
                    && declaringType.GetGenericTypeDefinition() == typeof(FluentMethodBuilder<>)
            )
            {
                var memberAccess = node.Object as MemberExpression;
                if (memberAccess != null)
                {
                    var constantExpresion = memberAccess.Expression as ConstantExpression;
                    var member = memberAccess.Member;
                    if (constantExpresion != null && member.MemberType == MemberTypes.Field)
                    {
                        object source = constantExpresion.Value;
                        var field = (FieldInfo)member;

                        var methodBuilder = field.GetValue(source) as FluentMethodBuilder;
                        if (methodBuilder == null)
                        {
                            throw new InvalidOperationException(
                                "The expression is trying to call a null reference to a method."
                            );
                        }

                        methodBuilder._CheckThread();

                        if (_method.TypeBuilder == null)
                        {
                            throw new NotSupportedException(
                                "At this moment only methods that are part of a type can call other methods."
                            );
                        }

                        if (methodBuilder.TypeBuilder != _method.TypeBuilder)
                        {
                            throw new NotSupportedException(
                                "At this moment only methods of the same FluentTypeBuilder can be invoked."
                            );
                        }

                        if (methodBuilder._respectVisibility)
                        {
                            throw new NotSupportedException(
                                "At this moment it is only possible to call another method that has the respectVisibility set to false."
                            );
                        }

                        foreach (var parameter in methodBuilder._parameters.Values)
                        {
                            var parameterType = parameter.Type;
                            if (
                                parameter.IsByRef
                                || (
                                    parameterType.IsGenericType
                                    && parameterType.GetGenericTypeDefinition()
                                        == typeof(FluentMethodBuilder.Box<>)
                                )
                            )
                            {
                                throw new InvalidOperationException(
                                    "InputAndOutput (ref) parameters are not supported when calling another method on the same type. Consider returning an struct with all the outputs you need instead."
                                );
                            }
                        }

                        methodBuilder.MakeParametersReadOnly();

                        var arguments = node.Arguments;
                        Expression callExpression = Expression.Call(
                            Expression.ArrayAccess(
                                _method.TypeBuilder.DelegatesExpression,
                                Expression.Constant(methodBuilder._delegateIndex)
                            ),
                            typeof(Delegate).GetMethod("DynamicInvoke"),
                            _RebuildArguments(methodBuilder, arguments[0])
                        );

                        if (
                            methodBuilder.ReturnType != typeof(void)
                            && methodBuilder.ReturnType != typeof(object)
                        )
                        {
                            callExpression = Expression.Convert(
                                callExpression,
                                methodBuilder.ReturnType
                            );
                        }

                        return base.Visit(callExpression);
                    }
                }
            }
        }

        return base.VisitMethodCall(node);
    }

    private static readonly MethodInfo _addInstanceMethod = typeof(_Visitor).GetMethod(
        "AddInstance"
    );

    public static object[] AddInstance(object instance, object[] array)
    {
        if (array == null)
        {
            return new object[] { instance };
        }

        var result = new object[array.Length + 1];
        result[0] = instance;
        array.CopyTo(result, 1);
        return result;
    }

    private Expression _RebuildArguments(FluentMethodBuilder method, Expression expression)
    {
        if (method.IsStatic)
        {
            return expression;
        }

        if (_method.IsStatic)
        {
            throw new InvalidOperationException(
                "A static method is trying to call a non-static method without giving the instance."
            );
        }

        var resultExpression = Expression.Call(
            _addInstanceMethod,
            _method.TypeBuilder!.ThisExpression,
            expression
        );

        return resultExpression;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        var member = node.Member;
        if (member.MemberType == MemberTypes.Field)
        {
            var innerExpression = node.Expression;
            if (innerExpression == null || innerExpression.NodeType == ExpressionType.Constant)
            {
                object value = null;
                if (innerExpression != null)
                {
                    var constantExpression = (ConstantExpression)innerExpression;
                    value = constantExpression.Value;
                }

                var field = (FieldInfo)member;
                var pair = new KeyValuePair<FieldInfo, object>(field, value);

                if (_method._sharedVariables.Contains(pair))
                {
                    return node;
                }

                ParameterExpression parameterExpression;
                if (_method._parameters.TryGetValue(pair, out parameterExpression))
                {
                    var type = parameterExpression.Type;
                    if (
                        type.IsGenericType
                        && type.GetGenericTypeDefinition() == typeof(FluentMethodBuilder.Box<>)
                    )
                    {
                        return Expression.Field(parameterExpression, type.GetField("Value"));
                    }

                    return parameterExpression;
                }

                if (_method._locals.TryGetValue(pair, out parameterExpression))
                {
                    return parameterExpression;
                }

                var typeBuilder = _method.TypeBuilder;
                if (typeBuilder != null)
                {
                    KeyValuePair<MemberExpression, string> memberPair;
                    if (typeBuilder.Fields.TryGetValue(pair, out memberPair))
                    {
                        var result = base.Visit(memberPair.Key);
                        return result;
                    }
                }

                if (!field.IsStatic)
                {
                    throw new InvalidOperationException(
                        "This expression is acessing a field (that is possible the result of an expression acessing a generator local variable) named \""
                            + field.Name
                            + "\" which is not registered either as a field, parameter, local or external variable."
                    );
                }
            }
        }

        return base.VisitMember(node);
    }
}
