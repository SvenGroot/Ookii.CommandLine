// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
