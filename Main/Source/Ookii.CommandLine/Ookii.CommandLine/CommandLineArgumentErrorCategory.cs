// Copyright (c) Sven Groot (Ookii.org)
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at https://github.com/SvenGroot/ookii.commandline. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.

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
        MissingRequiredArgument,
        /// <summary>
        /// Invalid value for a dictionary argument; typically the result of a duplicate key.
        /// </summary>
        InvalidDictionaryValue,
        /// <summary>
        /// An error occurred creating an instance of the arguments type (e.g. the constructor threw an exception).
        /// </summary>
        CreateArgumentsTypeError,
        /// <summary>
        /// An error occurred applying the value of the argument (e.g. the property set accessor threw an exception).
        /// </summary>
        ApplyValueError,
    }
}
