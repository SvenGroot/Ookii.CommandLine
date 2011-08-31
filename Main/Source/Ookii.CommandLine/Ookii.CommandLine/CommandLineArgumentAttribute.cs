// Copyright (c) Sven Groot (Ookii.org) 2011
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at http://ookiicommandline.codeplex.com. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
using System;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Indicates a property of a class defines a command line argument.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CommandLineArgumentAttribute : Attribute
    {
        private readonly string _argumentName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineArgumentAttribute"/> class using the property name as the argument name.
        /// </summary>
        public CommandLineArgumentAttribute()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineArgumentAttribute"/> class using the specified argument name.
        /// </summary>
        /// <param name="argumentName">The name of the argument, or <see langword="null"/> to indicate the property name should be used.</param>
        public CommandLineArgumentAttribute(string argumentName)
        {
            Position = -1;
            _argumentName = argumentName;
        }

        /// <summary>
        /// Gets the name of the argument's command switch.
        /// </summary>
        /// <value>
        /// The name of the command switch used to set the argument, or <see langword="null"/> if the property name should be used.
        /// </value>
        public string ArgumentName
        {
            get { return _argumentName; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the named argument is required.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the named argument must be supplied on the command line; otherwise, <see langword="false"/>.
        ///   The default value is <see langword="false"/>.
        /// </value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the position of a positional argument.
        /// </summary>
        /// <value>
        /// The position of the argument, or a negative value if the argument can only be specified by name. The default value is -1.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The <see cref="Position"/> property specifies the relative position of the positional arguments created by properties. If
        ///   you skip any numbers, they will be ignored; if you have only two positional arguments with positions set to
        ///   4 and 7, they will be the first and second positional arguments, not the 4th and 7th. It is an error
        ///   to use the same number more than once.
        /// </para>
        /// <para>
        ///   If you have arguments defined by the type's constructor parameters, positional arguments defined by properties will
        ///   always come after them; for example, if you have two contructor parameter arguments and one property positional argument with
        ///   position 0, then that argument will actually be the third positional argument.
        /// </para>
        /// <para>
        ///   The <see cref="CommandLineArgument.Position"/> property will be set to reflect the actual position of the argument,
        ///   which may not match the value of the <see cref="Position"/> property.
        /// </para>
        /// </remarks>
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the default value to be assigned to the property if the argument is not supplied on the command line.
        /// </summary>
        /// <value>
        /// The default value for the argument. The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The <see cref="DefaultValue"/> property will not be used if the <see cref="IsRequired"/> property is <see langword="true"/>.
        /// </para>
        /// <para>
        ///   The <see cref="DefaultValue"/> is ignored for multi-value and dictionary arguments.
        /// </para>
        /// </remarks>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the description of the property's value to use when printing usage information.
        /// </summary>
        /// <value>
        /// The description of the value, or <see langword="null"/> to indicate that the property's type name should be used.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The value description is a short (typically one word) description that indicates the type of value that
        ///   the user should supply. By default the type of the property is used. If the type is an array type, the
        ///   array's element type is used. If the type is a nullable type, the nullable type's underlying type is used.
        /// </para>
        /// <para>
        ///   The value description is used when printing usage. For example, the usage for an argument named Sample with
        ///   a value description of String would look like "-Sample &lt;String&gt;".
        /// </para>
        /// <note>
        ///   This is not the long description used to describe the purpose of the argument. That should be specified
        ///   using the <see cref="System.ComponentModel.DescriptionAttribute"/> attribute.
        /// </note>
        /// </remarks>
        public string ValueDescription { get; set; }
    }
}
