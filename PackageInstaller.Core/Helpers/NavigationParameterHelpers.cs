using Sextant;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PackageInstaller.Core.Helpers;

public static class NavigationParameterHelpers
{
    public static INavigationParameter ToNavigationParameter<T>(this T structure) where T : struct
    {
        var navParms = new NavigationParameter();
        foreach (
            var p in typeof(T).GetProperties(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
            )
        )
        {
            navParms.Add(p.Name, p.GetValue(structure)!);
        }

        return navParms;
    }

    public static bool IsNullable(PropertyInfo property) =>
        IsNullableHelper(property.PropertyType, property.DeclaringType, property.CustomAttributes);

    public static bool IsNullable(FieldInfo field) =>
        IsNullableHelper(field.FieldType, field.DeclaringType, field.CustomAttributes);

    public static bool IsNullable(ParameterInfo parameter) =>
        IsNullableHelper(parameter.ParameterType, parameter.Member, parameter.CustomAttributes);

    private static bool IsNullableHelper(
        Type memberType,
        MemberInfo? declaringType,
        IEnumerable<CustomAttributeData> customAttributes
    )
    {
        if (memberType.IsValueType)
        {
            return Nullable.GetUnderlyingType(memberType) != null;
        }

        var nullable = customAttributes.FirstOrDefault(
            x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute"
        );
        if (nullable != null && nullable.ConstructorArguments.Count == 1)
        {
            var attributeArgument = nullable.ConstructorArguments[0];
            if (attributeArgument.ArgumentType == typeof(byte[]))
            {
                var args =
                    (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value!;
                if (args.Count > 0 && args[0].ArgumentType == typeof(byte))
                {
                    return (byte)args[0].Value! == 2;
                }
            }
            else if (attributeArgument.ArgumentType == typeof(byte))
            {
                return (byte)attributeArgument.Value! == 2;
            }
        }

        for (var type = declaringType; type != null; type = type.DeclaringType)
        {
            var context = type.CustomAttributes.FirstOrDefault(
                x =>
                    x.AttributeType.FullName
                    == "System.Runtime.CompilerServices.NullableContextAttribute"
            );
            if (
                context != null
                && context.ConstructorArguments.Count == 1
                && context.ConstructorArguments[0].ArgumentType == typeof(byte)
            )
            {
                return (byte)context.ConstructorArguments[0].Value! == 2;
            }
        }

        // Couldn't find a suitable attribute
        return false;
    }

    public static T FromNavigationParameter<T>(this INavigationParameter navParms) where T : struct
    {
        var structure = new T();
        var boxedStructure = RuntimeHelpers.GetObjectValue(structure);
        foreach (
            var p in typeof(T).GetProperties(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
            )
        )
        {
            object? value = null;
            bool isOptional = IsNullable(p);
            if (navParms.TryGetValue(p.Name, out value))
            {
                if (value == null && !isOptional)
                {
                    throw new Exception($"Parameter {p.Name} is required but is null.");
                }
            }
            else
            {
                if (!isOptional)
                {
                    throw new Exception($"Parameter {p.Name} is required but is missing.");
                }
            }

            p.SetValue(boxedStructure, value);
        }

        return (T)boxedStructure;
    }
}
