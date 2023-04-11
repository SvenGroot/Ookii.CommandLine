using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal static class Extensions
{
    public static bool DerivesFrom(this ITypeSymbol symbol, string baseClassName)
    {
        var current = symbol;
        while (current != null)
        {
            if (current.ToDisplayString() == baseClassName)
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    public static bool DerivesFrom(this ITypeSymbol type, ITypeSymbol baseClass)
    {
        var current = type;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseClass))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    public static bool IsNullableValueType(this INamedTypeSymbol symbol)
        => !symbol.IsReferenceType && symbol.IsGenericType && symbol.ConstructedFrom.ToDisplayString() == "System.Nullable<T>";

    public static bool AllowsNull(this INamedTypeSymbol type)
        => type.IsNullableValueType() || (type.IsReferenceType && type.NullableAnnotation != NullableAnnotation.NotAnnotated);

    public static INamedTypeSymbol GetUnderlyingType(this INamedTypeSymbol type)
        => type.IsNullableValueType() ? (INamedTypeSymbol)type.TypeArguments[0] : type;

    public static bool IsEnum(this ITypeSymbol type) => type.BaseType?.ToDisplayString() == "System.Enum";

    public static INamedTypeSymbol? FindGenericInterface(this ITypeSymbol symbol, string interfaceName)
    {
        foreach (var iface in symbol.AllInterfaces)
        {
            var realIface = iface;
            if (iface.IsGenericType)
            {
                realIface = iface.ConstructedFrom;
            }

            if (realIface.ToDisplayString() == interfaceName)
            {
                return iface;
            }
        }

        return null;
    }

    public static bool ImplementsInterface(this ITypeSymbol symbol, string interfaceName)
    {
        foreach (var iface in symbol.AllInterfaces)
        {
            if (iface.ToDisplayString() == interfaceName)
            {
                return true;
            }
        }

        return false;
    }

    public static bool ImplementsInterface(this ITypeSymbol type, ITypeSymbol interfaceType)
    {
        foreach (var iface in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface, interfaceType))
            {
                return true;
            }
        }

        return false;
    }

    public static bool CanAssignFrom(this ITypeSymbol targetType, ITypeSymbol sourceType)
        => SymbolEqualityComparer.Default.Equals(targetType, sourceType) || sourceType.DerivesFrom(targetType)
            || sourceType.ImplementsInterface(targetType);

    public static string CreateInstantiation(this AttributeData attribute)
    {
        var ctorArgs = attribute.ConstructorArguments.Select(c => c.ToCSharpString());
        var namedArgs = attribute.NamedArguments.Select(n => $"{n.Key} = {n.Value.ToCSharpString()}");
        return $"new {attribute.AttributeClass?.ToDisplayString()}({string.Join(", ", ctorArgs)}) {{ {string.Join(", ", namedArgs)} }}";
    }

    public static string ToCSharpString(this bool value)
    {
        return value ? "true" : "false";
    }
}
