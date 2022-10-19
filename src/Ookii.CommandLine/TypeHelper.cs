// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.Reflection;

namespace Ookii.CommandLine
{
    static class TypeHelper
    {
        public static Type? FindGenericInterface(Type type, Type interfaceType)
        {
            if( type == null )
                throw new ArgumentNullException(nameof(type));
            if( interfaceType == null )
                throw new ArgumentNullException(nameof(interfaceType));
            if( !(interfaceType.IsInterface && interfaceType.IsGenericTypeDefinition) )
                throw new ArgumentException(Properties.Resources.TypeNotGenericDefinition, nameof(interfaceType));

            if( type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType )
                return type;

            foreach( Type t in type.GetInterfaces() )
            {
                if( t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType )
                    return t;
            }

            return null;
        }

        public static T? GetAttribute<T>(ParameterInfo element)
            where T: Attribute
        {
            return (T?)Attribute.GetCustomAttribute(element, typeof(T));
        }

        public static T? GetAttribute<T>(PropertyInfo element)
            where T : Attribute
        {
            return (T?)Attribute.GetCustomAttribute(element, typeof(T));
        }

        public static T? GetAttribute<T>(Type element)
            where T : Attribute
        {
            return (T?)Attribute.GetCustomAttribute(element, typeof(T));
        }

    }
}
