// Copyright (c) Sven Groot (Ookii.org) 2011
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at http://ookiicommandline.codeplex.com. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
using System;
using System.Collections.Generic;
using System.Text;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Specifies the kind of error that occurred while parsing arguments.
    /// </summary>
    public enum CommandLineArgumentErrorCategory
    {
        /// <summary>
        /// The category was not specified.
        /// </summary>
        Unspecified,
        /// <summary>
        /// The argument value supplied could not be converted to the type of the argument.
        /// </summary>
        ArgumentValueConversion,
        /// <summary>
        /// The argument name supplied does not name a known argument.
        /// </summary>
        UnknownArgument,
        /// <summary>
        /// An argument name was supplied, but without an accompanying value.
        /// </summary>
        MissingNamedArgumentValue,
        /// <summary>
        /// An argument was supplied more than once.
        /// </summary>
        DuplicateArgument,
        /// <summary>
        /// Too many positional arguments were supplied.
        /// </summary>
        TooManyArguments,
        /// <summary>
        /// Not all required arguments were supplied.
        /// </summary>
        MissingRequiredArgument
    }
}
