// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.ComponentModel;
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

        public static TypeConverter GetStringConverter(this Type type, Type? converterType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var converter = (TypeConverter?)converterType?.CreateInstance() ?? TypeDescriptor.GetConverter(type);
            if (converter != null && converter.CanConvertFrom(typeof(string)))
            {
                return converter;
            }

            if (converterType == null)
            {
                // If no explicit converter and the default one can't converter from string, see if
                // there's a Parse method we can use.
                var method = type.GetMethod(ParseMethodName, BindingFlags.Static | BindingFlags.Public,
                    null, new[] { typeof(string), typeof(CultureInfo) }, null);

                if (method != null && method.ReturnType == type)
                {
                    return new ParseTypeConverter(method, true);
                }

                // Check for Parse without a culture arguments.
                method = type.GetMethod(ParseMethodName, BindingFlags.Static | BindingFlags.Public, null,
                    new[] { typeof(string) }, null);

                if (method != null && method.ReturnType == type)
                {
                    return new ParseTypeConverter(method, false);
                }

                // Check for a constructor with a string argument.
                if (type.GetConstructor(new[] { typeof(string) }) != null)
                {
                    return new ConstructorTypeConverter(type);
                }
            }

            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.NoTypeConverterFormat, type));
        }

        public static Type GetNullableCoreType(this Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                ? type.GetGenericArguments()[0]
                : type;
        }
    }
}
