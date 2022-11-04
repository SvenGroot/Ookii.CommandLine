// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.ComponentModel;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Provides data for the <see cref="CommandLineParser.ArgumentParsed"/> event.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   If the event handler sets the <see cref="CancelEventArgs.Cancel"/> property to <see langword="true"/>, command line processing will stop immediately,
    ///   and the <see cref="CommandLineParser.Parse(string[],int)"/> method will return <see langword="null"/>, even if all the required positional parameters have already
    ///   been parsed. You can use this for instance to implement a "/?" argument that will display usage and quit regardless of the other command line arguments.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public class ArgumentParsedEventArgs : CancelEventArgs
    {
        private readonly CommandLineArgument _argument;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentParsedEventArgs"/> class.
        /// </summary>
        /// <param name="argument">The information about the argument that has been parsed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        public ArgumentParsedEventArgs(CommandLineArgument argument)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(nameof(argument));
            }

            _argument = argument;
        }

        /// <summary>
        /// Gets the argument that was parsed.
        /// </summary>
        /// <value>
        /// The <see cref="CommandLineArgument"/> instance for the argument.
        /// </value>
        public CommandLineArgument Argument
        {
            get { return _argument; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether or not the <see cref="CommandLineArgumentAttribute.CancelParsing"/>
        /// property should be ignored.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if argument parsing should continue even if the argument has
        /// <see cref="CommandLineArgumentAttribute.CancelParsing"/> set to <see langword="true"/>;
        /// otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property does not affect the <see cref="CancelEventArgs.Cancel"/> property.
        ///   If <see cref="CancelEventArgs.Cancel"/> is set to <see langword="true"/>, parsing
        ///   is always canceled regardless of the value of <see cref="OverrideCancelParsing"/>.
        /// </para>
        /// </remarks>
        public bool OverrideCancelParsing { get; set; }
    }
}
