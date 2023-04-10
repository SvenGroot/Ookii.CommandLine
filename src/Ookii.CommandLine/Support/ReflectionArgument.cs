using System.Collections.Generic;
using System.Globalization;
using System;
using System.Reflection;
using System.ComponentModel;
using Ookii.CommandLine.Conversion;
using System.Diagnostics;
using System.Text;
using Ookii.CommandLine.Validation;
using System.Threading;

namespace Ookii.CommandLine.Support;

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

    protected override bool CanSetProperty => _property?.GetSetMethod() != null;

    protected override void SetProperty(object target, object? value)
    {
        if (_property == null)
        {
            throw new InvalidOperationException();
        }

        _property.SetValue(target, value);
    }

    protected override object? GetProperty(object target)
    {
        if (_property == null)
        {
            throw new InvalidOperationException();
        }

        return _property.GetValue(target);
    }

    protected override bool CallMethod(object? value)
    {
        if (_method is not MethodArgumentInfo info)
        {
            throw new InvalidOperationException();
        }

        int parameterCount = (info.HasValueParameter ? 1 : 0) + (info.HasParserParameter ? 1 : 0);
        var parameters = new object?[parameterCount];
        int index = 0;
        if (info.HasValueParameter)
        {
            parameters[index] = Value;
            ++index;
        }

        if (info.HasParserParameter)
        {
            parameters[index] = Parser;
        }

        var returnValue = info.Method.Invoke(null, parameters);
        if (returnValue == null)
        {
            return true;
        }
        else
        {
            return (bool)returnValue;
        }
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

        var (methodInfo, argumentType, allowsNull) = DetermineMethodArgumentInfo(method)
            ?? throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.InvalidMethodSignatureFormat, method.Name));

        return Create(parser, null, methodInfo, argumentType, allowsNull);
    }

    private static CommandLineArgument Create(CommandLineParser parser, PropertyInfo? property, MethodArgumentInfo? method,
        Type argumentType, bool allowsNull)
    {
        var member = ((MemberInfo?)property ?? method?.Method)!;
        var attribute = member.GetCustomAttribute<CommandLineArgumentAttribute>()
            ?? throw new ArgumentException(Properties.Resources.MissingArgumentAttribute, nameof(method));

        var multiValueSeparatorAttribute = member.GetCustomAttribute<MultiValueSeparatorAttribute>();
        var descriptionAttribute = member.GetCustomAttribute<DescriptionAttribute>();
        var allowDuplicateDictionaryKeys = Attribute.IsDefined(member, typeof(AllowDuplicateDictionaryKeysAttribute));
        var keyValueSeparatorAttribute = member.GetCustomAttribute<KeyValueSeparatorAttribute>();
        var aliasAttributes = member.GetCustomAttributes<AliasAttribute>();
        var shortAliasAttributes = member.GetCustomAttributes<ShortAliasAttribute>();
        var validationAttributes = member.GetCustomAttributes<ArgumentValidationAttribute>();

        ArgumentInfo info = CreateArgumentInfo(parser, argumentType, allowsNull, member.Name, attribute, multiValueSeparatorAttribute, descriptionAttribute, allowDuplicateDictionaryKeys, keyValueSeparatorAttribute, aliasAttributes, shortAliasAttributes, validationAttributes);

        DetermineAdditionalInfo(ref info, member);
        return new ReflectionArgument(info, property, method);
    }

    private static void DetermineAdditionalInfo(ref ArgumentInfo info, MemberInfo member)
    {
        var converterAttribute = member.GetCustomAttribute<ArgumentConverterAttribute>();
        var keyArgumentConverterAttribute = member.GetCustomAttribute<KeyConverterAttribute>();
        var valueArgumentConverterAttribute = member.GetCustomAttribute<ValueConverterAttribute>();
        var converterType = converterAttribute?.GetConverterType();

        if (member is PropertyInfo property)
        {
            var (collectionType, dictionaryType, elementType) =
                DetermineMultiValueType(info.ArgumentName, info.ArgumentType, property);

            if (dictionaryType != null)
            {
                Debug.Assert(elementType != null);
                info.Kind = ArgumentKind.Dictionary;
                info.ElementTypeWithNullable = elementType!;
                info.AllowNull = DetermineDictionaryValueTypeAllowsNull(dictionaryType, property);
                info.KeyValueSeparator ??= KeyValuePairConverter.DefaultSeparator;
                var genericArguments = dictionaryType.GetGenericArguments();
                info.KeyType = genericArguments[0];
                info.ValueType = genericArguments[1];
                if (converterType == null)
                {
                    converterType = typeof(KeyValuePairConverter<,>).MakeGenericType(genericArguments);
                    var keyConverterType = keyArgumentConverterAttribute?.GetConverterType();
                    var valueConverterType = valueArgumentConverterAttribute?.GetConverterType();
                    info.Converter = (ArgumentConverter)Activator.CreateInstance(converterType, info.Parser.StringProvider,
                        info.ArgumentName, info.AllowNull, keyConverterType, valueConverterType, info.KeyValueSeparator)!;
                }
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

        var dictionaryType = argumentType.FindGenericInterface(typeof(IDictionary<,>));
        if (dictionaryType != null)
        {
            var elementType = typeof(KeyValuePair<,>).MakeGenericType(dictionaryType.GetGenericArguments());
            return (null, dictionaryType, elementType);
        }

        var collectionType = argumentType.FindGenericInterface(typeof(ICollection<>));
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
        if (actualType != null && (actualType.IsArray || actualType.IsGenericType &&
            actualType.GetGenericTypeDefinition() == typeof(ICollection<>)))
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
            method.ReturnType != typeof(bool) && method.ReturnType != typeof(void) ||
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
}
