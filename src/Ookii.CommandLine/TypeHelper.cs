// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.Linq;

namespace Ookii.CommandLine
{
    static class TypeHelper
    {
        public static Type? FindGenericInterface(this Type type, Type interfaceType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));
            if (!(interfaceType.IsInterface && interfaceType.IsGenericTypeDefinition))
                throw new ArgumentException(Properties.Resources.TypeNotGenericDefinition, nameof(interfaceType));

            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType)
                return type;

            return type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }

        public static bool ImplementsInterface(this Type type, Type interfaceType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            return type.GetInterfaces().Any(i => i == interfaceType);
        }
    }
}
