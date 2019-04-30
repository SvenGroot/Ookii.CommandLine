// Copyright (c) Sven Groot (Ookii.org) 2012
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at https://github.com/SvenGroot/ookii.commandline. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
using System;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Indicates an alternative argument name for an argument defined by a constructor parameter.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Apply the <see cref="ArgumentNameAttribute"/> attribute to a constructor parameter to indicate
    ///   that the name of the argument should be different than the parameter name.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class ArgumentNameAttribute : Attribute
    {
        private readonly string _argumentName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentNameAttribute"/> class.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        public ArgumentNameAttribute(string argumentName)
        {
            if( argumentName == null )
                throw new ArgumentNullException("argumentName");

            _argumentName = argumentName;
        }

        /// <summary>
        /// Gets the name of the argument.
        /// </summary>
        /// <value>
        /// The name of the argument.
        /// </value>
        public string ArgumentName
        {
            get { return _argumentName; }
        }
    }
}
