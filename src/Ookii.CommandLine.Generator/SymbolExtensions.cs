using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal static class SymbolExtensions
{
    public static bool DerivesFrom(this INamedTypeSymbol symbol, string baseClassName)
    {
        INamedTypeSymbol? current = symbol;
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

    public static bool IsNullableValueType(this INamedTypeSymbol symbol)
        => !symbol.IsReferenceType && symbol.IsGenericType && symbol.ConstructedFrom.ToDisplayString() == "System.Nullable<T>";

    public static bool AllowsNull(this INamedTypeSymbol type)
        => type.IsNullableValueType() || (type.IsReferenceType && type.NullableAnnotation != NullableAnnotation.NotAnnotated);

    public static INamedTypeSymbol? FindGenericInterface(this INamedTypeSymbol symbol, string interfaceName)
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

    public static string CreateInstantiation(this AttributeData attribute)
    {
        var ctorArgs = attribute.ConstructorArguments.Select(c => c.ToCSharpString());
        var namedArgs = attribute.NamedArguments.Select(n => $"{n.Key} = {n.Value.ToCSharpString()}");
        return $"new {attribute.AttributeClass?.ToDisplayString()}({string.Join(", ", ctorArgs)}) {{ {string.Join(", ", namedArgs)} }}";
    }
}
