// Copyright (c) Sven Groot (Ookii.org)
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at https://github.com/SvenGroot/ookii.commandline. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
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
            if( argument == null )
                throw new ArgumentNullException("argument");

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
    }
}
