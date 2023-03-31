using System.Collections.Generic;
using System.Globalization;
using System;
using System.Reflection;
using System.ComponentModel;
using Ookii.CommandLine.Conversion;
using System.Diagnostics;
using System.Text;
using Ookii.CommandLine.Validation;

namespace Ookii.CommandLine;

internal class ReflectionArgument : CommandLineArgument
{
    #region Nested types

    private struct MethodArgumentInfo
    {
        public MethodInfo Method { get; set; }
        public bool HasValueParameter { get; set; }
        public bool HasParserParameter { get; set; }
    }

    #endregion

    private readonly PropertyInfo? _property;
    private readonly MethodArgumentInfo? _method;

    private ReflectionArgument(ArgumentInfo info, PropertyInfo? property, MethodArgumentInfo? method)
        : base(info)
    {
        _property = property;
        _method = method;
    }

    internal static CommandLineArgument Create(CommandLineParser parser, PropertyInfo property)
    {
        if (parser == null)
        {
            throw new ArgumentNullException(nameof(parser));
        }

        if (property == null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        return Create(parser, property, null, property.PropertyType, DetermineAllowsNull(property));
    }

    internal static CommandLineArgument Create(CommandLineParser parser, MethodInfo method)
    {
        if (parser == null)
        {
            throw new ArgumentNullException(nameof(parser));
        }

        if (method == null)
        {
            throw new ArgumentNullException(nameof(method));
        }

        var infoTuple = DetermineMethodArgumentInfo(method);
        if (infoTuple == null)
        {
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.InvalidMethodSignatureFormat, method.Name));
        }

        var (methodInfo, argumentType, allowsNull) = infoTuple.Value;
        return Create(parser, null, methodInfo, argumentType, allowsNull);
    }

    private static CommandLineArgument Create(CommandLineParser parser, PropertyInfo? property, MethodArgumentInfo? method,
        Type argumentType, bool allowsNull)
    {
        var member = ((MemberInfo?)property ?? method?.Method)!;
        var attribute = member.GetCustomAttribute<CommandLineArgumentAttribute>()
            ?? throw new ArgumentException(Properties.Resources.MissingArgumentAttribute, nameof(method));

        var argumentName = DetermineArgumentName(attribute.ArgumentName, member.Name, parser.Options.ArgumentNameTransform);
        var multiValueSeparatorAttribute = member.GetCustomAttribute<MultiValueSeparatorAttribute>();

        var info = new ArgumentInfo()
        {
            Parser = parser,
            ArgumentName = argumentName,
            Long = attribute.IsLong,
            Short = attribute.IsShort,
            ShortName = attribute.ShortName,
            ArgumentType = argumentType,
            Description = member.GetCustomAttribute<DescriptionAttribute>()?.Description,
            ValueDescription = attribute.ValueDescription,  // If null, the constructor will sort it out.
            Position = attribute.Position < 0 ? null : attribute.Position,
            AllowDuplicateDictionaryKeys = Attribute.IsDefined(member, typeof(AllowDuplicateDictionaryKeysAttribute)),
            MultiValueSeparator = GetMultiValueSeparator(multiValueSeparatorAttribute),
            AllowMultiValueWhiteSpaceSeparator = multiValueSeparatorAttribute != null && multiValueSeparatorAttribute.Separator == null,
            KeyValueSeparator = member.GetCustomAttribute<KeyValueSeparatorAttribute>()?.Separator,
            Aliases = GetAliases(member.GetCustomAttributes<AliasAttribute>(), argumentName),
            ShortAliases = GetShortAliases(member.GetCustomAttributes<ShortAliasAttribute>(), argumentName),
            DefaultValue = attribute.DefaultValue,
            IsRequired = attribute.IsRequired,
            MemberName = member.Name,
            AllowNull = allowsNull,
            CancelParsing = attribute.CancelParsing,
            IsHidden = attribute.IsHidden,
            Validators = member.GetCustomAttributes<ArgumentValidationAttribute>(),
        };

        DetermineAdditionalInfo(ref info, member, argumentType, argumentName);
        return new ReflectionArgument(info, property, method);
    }

    private static void DetermineAdditionalInfo(ref ArgumentInfo info, MemberInfo member, Type argumentType, string argumentName)
    {
        var converterAttribute = member.GetCustomAttribute<ArgumentConverterAttribute>();
        var keyArgumentConverterAttribute = member.GetCustomAttribute<KeyConverterAttribute>();
        var valueArgumentConverterAttribute = member.GetCustomAttribute<ValueConverterAttribute>();
        var converterType = converterAttribute?.GetConverterType();

        if (member is PropertyInfo property)
        {
            var (collectionType, dictionaryType, elementType) = DetermineMultiValueType(argumentName, argumentType, property);

            if (dictionaryType != null)
            {
                Debug.Assert(elementType != null);
                info.Kind = ArgumentKind.Dictionary;
                info.ElementTypeWithNullable = elementType!;
                info.AllowNull = DetermineDictionaryValueTypeAllowsNull(dictionaryType, property);
                info.KeyValueSeparator ??= KeyValuePairConverter.DefaultSeparator;
                var genericArguments = dictionaryType.GetGenericArguments();
                if (converterType == null)
                {
                    converterType = typeof(KeyValuePairConverter<,>).MakeGenericType(genericArguments);
                    var keyConverterType = keyArgumentConverterAttribute?.GetConverterType();
                    var valueConverterType = valueArgumentConverterAttribute?.GetConverterType();
                    info.Converter = (ArgumentConverter)Activator.CreateInstance(converterType, info.Parser.StringProvider,
                        info.ArgumentName, info.AllowNull, keyConverterType, valueConverterType, info.KeyValueSeparator)!;
                }

                var valueDescription = info.ValueDescription ?? GetDefaultValueDescription(info.ElementTypeWithNullable,
                    info.Parser.Options.DefaultValueDescriptions);

                if (valueDescription == null)
                {
                    var key = DetermineValueDescription(genericArguments[0].GetUnderlyingType(), info.Parser.Options);
                    var value = DetermineValueDescription(genericArguments[1].GetUnderlyingType(), info.Parser.Options);
                    valueDescription = $"{key}{info.KeyValueSeparator}{value}";
                }

                info.ValueDescription = valueDescription;
            }
            else if (collectionType != null)
            {
                Debug.Assert(elementType != null);
                info.Kind = ArgumentKind.MultiValue;
                info.ElementTypeWithNullable = elementType!;
                info.AllowNull = DetermineCollectionElementTypeAllowsNull(collectionType, property);
            }
        }
        else
        {
            info.Kind = ArgumentKind.Method;
        }

        // If it's a Nullable<T>, now get the underlying type.
        info.ElementType = info.ElementTypeWithNullable.GetUnderlyingType();

        // Use the original Nullable<T> for this if it is one.
        info.Converter ??= info.ElementTypeWithNullable.GetStringConverter(converterType);
        info.ValueDescription ??= info.ValueDescription ?? DetermineValueDescription(info.ElementType, info.Parser.Options);
    }

    private static string? GetDefaultValueDescription(Type type, IDictionary<Type, string>? defaultValueDescriptions)
    {
        if (defaultValueDescriptions == null)
        {
            return null;
        }

        if (defaultValueDescriptions.TryGetValue(type, out string? value))
        {
            return value;
        }

        return null;
    }

    private static string DetermineValueDescription(Type type, ParseOptions options)
    {
        var result = GetDefaultValueDescription(type, options.DefaultValueDescriptions);
        if (result == null)
        {
            var typeName = GetFriendlyTypeName(type);
            result = options.ValueDescriptionTransform?.Apply(typeName) ?? typeName;
        }

        return result;
    }

    private static string GetFriendlyTypeName(Type type)
    {
        // This is used to generate a value description from a type name if no custom value description was supplied.
        if (type.IsGenericType)
        {
            var name = new StringBuilder(type.FullName?.Length ?? 0);
            name.Append(type.Name, 0, type.Name.IndexOf("`", StringComparison.Ordinal));
            name.Append('<');
            // AppendJoin is not supported in .Net Standard 2.0
            bool first = true;
            foreach (Type typeArgument in type.GetGenericArguments())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    name.Append(", ");
                }

                name.Append(GetFriendlyTypeName(typeArgument));
            }

            name.Append('>');
            return name.ToString();
        }
        else
        {
            return type.Name;
        }
    }

    // Returns a tuple of (collectionType, dictionaryType, elementType)
    private static (Type?, Type?, Type?) DetermineMultiValueType(string argumentName, Type argumentType, PropertyInfo property)
    {
        // If the type is Dictionary<TKey, TValue> it doesn't matter if the property is
        // read-only or not.
        if (argumentType.IsGenericType && argumentType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var elementType = typeof(KeyValuePair<,>).MakeGenericType(argumentType.GetGenericArguments());
            return (null, argumentType, elementType);
        }

        if (argumentType.IsArray)
        {
            if (argumentType.GetArrayRank() != 1)
            {
                throw new NotSupportedException(Properties.Resources.InvalidArrayRank);
            }

            if (property.GetSetMethod() == null)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.PropertyIsReadOnlyFormat, argumentName));
            }

            var elementType = argumentType.GetElementType()!;
            return (argumentType, null, elementType);
        }

        // The interface approach requires a read-only property. If it's read-write, treat it
        // like a non-multi-value argument.
        // Don't use CanWrite because that returns true for properties with a private set
        // accessor.
        if (property.GetSetMethod() != null)
        {
            return (null, null, null);
        }

        var dictionaryType = TypeHelper.FindGenericInterface(argumentType, typeof(IDictionary<,>));
        if (dictionaryType != null)
        {
            var elementType = typeof(KeyValuePair<,>).MakeGenericType(dictionaryType.GetGenericArguments());
            return (null, dictionaryType, elementType);
        }

        var collectionType = TypeHelper.FindGenericInterface(argumentType, typeof(ICollection<>));
        if (collectionType != null)
        {
            var elementType = collectionType.GetGenericArguments()[0];
            return (collectionType, null, elementType);
        }

        // This is a read-only property with an unsupported type.
        throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.PropertyIsReadOnlyFormat, argumentName));
    }

    private static bool DetermineDictionaryValueTypeAllowsNull(Type type, PropertyInfo property)
    {
        var valueTypeNull = DetermineValueTypeNullable(type.GetGenericArguments()[1]);
        if (valueTypeNull != null)
        {
            return valueTypeNull.Value;
        }

#if NET6_0_OR_GREATER
        // Type is the IDictionary<,> implemented interface, not the actual type of the property
        // or parameter, which is what we need here.
        var actualType = property.PropertyType;

        // We can only determine the nullability state if the property or parameter's actual
        // type is Dictionary<,> or IDictionary<,>. Otherwise, we just assume nulls are
        // allowed.
        if (actualType != null && actualType.IsGenericType &&
            (actualType.GetGenericTypeDefinition() == typeof(Dictionary<,>) || actualType.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
        {
            var context = new NullabilityInfoContext();
            var info = context.Create(property);
            return info.GenericTypeArguments[1].ReadState != NullabilityState.NotNull;
        }
#endif

        return true;
    }

    private static bool DetermineCollectionElementTypeAllowsNull(Type type, PropertyInfo property)
    {
        Type elementType = type.IsArray ? type.GetElementType()! : type.GetGenericArguments()[0];
        var valueTypeNull = DetermineValueTypeNullable(elementType);
        if (valueTypeNull != null)
        {
            return valueTypeNull.Value;
        }

#if NET6_0_OR_GREATER
        // Type is the ICollection<> implemented interface, not the actual type of the property
        // or parameter, which is what we need here.
        var actualType = property.PropertyType;

        // We can only determine the nullability state if the property or parameter's actual
        // type is an array or ICollection<>. Otherwise, we just assume nulls are allowed.
        if (actualType != null && (actualType.IsArray || (actualType.IsGenericType &&
            actualType.GetGenericTypeDefinition() == typeof(ICollection<>))))
        {
            var context = new NullabilityInfoContext();
            var info = context.Create(property);
            if (actualType.IsArray)
            {
                return info.ElementType?.ReadState != NullabilityState.NotNull;
            }
            else
            {
                return info.GenericTypeArguments[0].ReadState != NullabilityState.NotNull;
            }
        }
#endif

        return true;
    }

    private static bool DetermineAllowsNull(PropertyInfo property)
    {
        var valueTypeNull = DetermineValueTypeNullable(property.PropertyType);
        if (valueTypeNull != null)
        {
            return valueTypeNull.Value;
        }

#if NET6_0_OR_GREATER
        var context = new NullabilityInfoContext();
        var info = context.Create(property);
        return info.WriteState != NullabilityState.NotNull;
#else
            return true;
#endif
    }

    private static bool DetermineAllowsNull(ParameterInfo parameter)
    {
        var valueTypeNull = DetermineValueTypeNullable(parameter.ParameterType);
        if (valueTypeNull != null)
        {
            return valueTypeNull.Value;
        }

#if NET6_0_OR_GREATER
        var context = new NullabilityInfoContext();
        var info = context.Create(parameter);
        return info.WriteState != NullabilityState.NotNull;
#else
            return true;
#endif
    }

    private static bool? DetermineValueTypeNullable(Type type)
    {
        if (type.IsValueType)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        return null;
    }

    private static (MethodArgumentInfo, Type, bool)? DetermineMethodArgumentInfo(MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (!method.IsStatic ||
            (method.ReturnType != typeof(bool) && method.ReturnType != typeof(void)) ||
            parameters.Length > 2)
        {
            return null;
        }

        bool allowsNull = false;
        var argumentType = typeof(bool);
        var info = new MethodArgumentInfo() { Method = method };
        if (parameters.Length == 2)
        {
            argumentType = parameters[0].ParameterType;
            if (parameters[1].ParameterType != typeof(CommandLineParser))
            {
                return null;
            }

            info.HasValueParameter = true;
            info.HasParserParameter = true;
        }
        else if (parameters.Length == 1)
        {
            if (parameters[0].ParameterType == typeof(CommandLineParser))
            {
                info.HasParserParameter = true;
            }
            else
            {
                argumentType = parameters[0].ParameterType;
                info.HasValueParameter = true;
            }
        }

        if (info.HasValueParameter)
        {
            allowsNull = DetermineAllowsNull(parameters[0]);
        }

        return (info, argumentType, allowsNull);
    }

    private static string? GetMultiValueSeparator(MultiValueSeparatorAttribute? attribute)
    {
        var separator = attribute?.Separator;
        if (string.IsNullOrEmpty(separator))
        {
            return null;
        }
        else
        {
            return separator;
        }
    }
}
