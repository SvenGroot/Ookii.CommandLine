// Copyright (c) Sven Groot (Ookii.org)
using Ookii.CommandLine.Conversion;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Ookii.CommandLine
{
    static class TypeHelper
    {
        private const string ParseMethodName = "Parse";

        public static Type? FindGenericInterface(this Type type, Type interfaceType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            if (!(interfaceType.IsInterface && interfaceType.IsGenericTypeDefinition))
            {
                throw new ArgumentException(Properties.Resources.TypeNotGenericDefinition, nameof(interfaceType));
            }

            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType)
            {
                return type;
            }

            return type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }

        public static bool ImplementsInterface(this Type type, Type interfaceType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            return type.GetInterfaces().Any(i => i == interfaceType);
        }

        public static object? CreateInstance(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return Activator.CreateInstance(type);
        }

        public static object? CreateInstance(this Type type, params object?[]? args)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return Activator.CreateInstance(type, args);
        }

        public static ArgumentConverter GetStringConverter(this Type type, Type? converterType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var converter = (ArgumentConverter?)converterType?.CreateInstance();
            if (converter != null)
            {
                return converter;
            }

            if (converterType == null)
            {
                var underlyingType = type.GetUnderlyingType();
                converter = GetDefaultConverter(underlyingType);
                if (converter != null)
                {
                    return type.IsNullableValueType()
                        ? new NullableConverter(converter)
                        : converter;
                }
            }

            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.NoArgumentConverterFormat, type));
        }

        public static bool IsNullableValueType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type GetUnderlyingType(this Type type)
            => type.IsNullableValueType() ? type.GetGenericArguments()[0] : type;

        private static ArgumentConverter? GetDefaultConverter(this Type type)
        {
            if (type == typeof(string))
            {
                return StringConverter.Instance;
            }

            if (type.IsEnum)
            {
                return new EnumConverter(type);
            }

#if NET7_0_OR_GREATER
            if (type.FindGenericInterface(typeof(ISpanParsable<>)) != null)
            {
                return (ArgumentConverter?)Activator.CreateInstance(typeof(SpanParsableConverter<>).MakeGenericType(type));
            }
#endif

            // If no explicit converter and the default one can't converter from string, see if
            // there's a Parse method we can use.
            var method = type.GetMethod(ParseMethodName, BindingFlags.Static | BindingFlags.Public,
                null, new[] { typeof(string), typeof(CultureInfo) }, null);

            if (method != null && method.ReturnType == type)
            {
                return new ParseConverter(method, true);
            }

            // Check for Parse without a culture arguments.
            method = type.GetMethod(ParseMethodName, BindingFlags.Static | BindingFlags.Public, null,
                new[] { typeof(string) }, null);

            if (method != null && method.ReturnType == type)
            {
                return new ParseConverter(method, false);
            }

            // Check for a constructor with a string argument.
            if (type.GetConstructor(new[] { typeof(string) }) != null)
            {
                return new ConstructorConverter(type);
            }

            return null;
        }
    }
}
