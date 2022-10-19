// Copyright (c) Sven Groot (Ookii.org)
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
                throw new ArgumentNullException(nameof(argumentName));

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
