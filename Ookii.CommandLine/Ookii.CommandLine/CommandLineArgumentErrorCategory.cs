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
