using System;
using System.Diagnostics;

namespace Ookii.CommandLine.Support;

/// <summary>
/// Represents information about an argument determined by the source generator. Used for all
/// arguments except dictionary arguments.
/// </summary>
/// <remarks>
/// This class is used by the source generator when using the <see cref="GeneratedParserAttribute"/>
/// attribute. It should not normally be used by other code.
/// </remarks>
/// <threadsafety static="true" instance="false"/>
/// <typeparam name="TElementWithNullable">The element type of the argument, including <see cref="Nullable{T}"/> if it is one.</typeparam>
public class GeneratedArgument<TElementWithNullable> : GeneratedArgumentBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneratedArgument{TElementWithNullable}"/> class.
    /// </summary>
    /// <param name="info">The argument creation information.</param>
    public GeneratedArgument(in ArgumentCreationInfo info) : base(info)
    {
        Debug.Assert(info.Kind != ArgumentKind.Dictionary);
        Debug.Assert(typeof(TElementWithNullable) == info.ElementTypeWithNullable);
    }

    private protected override IValueHelper CreateDictionaryValueHelper() => throw new NotImplementedException();

    private protected override IValueHelper CreateMultiValueHelper() => new MultiValueHelper<TElementWithNullable>();
}
