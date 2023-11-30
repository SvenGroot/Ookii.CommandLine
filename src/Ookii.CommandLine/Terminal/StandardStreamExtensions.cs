using System;
using System.IO;

namespace Ookii.CommandLine.Terminal;

/// <summary>
/// Provides extension methods for the <see cref="StandardStream"/> enumeration.
/// </summary>
/// <threadsafety static="true" instance="true"/>
public static class StandardStreamExtensions
{
    /// <summary>
    /// Gets the <see cref="TextWriter"/> for either <see cref="StandardStream.Output" qualifyHint="true"/>
    /// or <see cref="StandardStream.Error" qualifyHint="true"/>.
    /// </summary>
    /// <param name="stream">A <see cref="StandardStream"/> value.</param>
    /// <returns>
    /// The value of either <see cref="Console.Out" qualifyHint="true"/> or
    /// <see cref="Console.Error" qualifyHint="true"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="stream"/> was a value other than <see cref="StandardStream.Output" qualifyHint="true"/>
    /// or <see cref="StandardStream.Error" qualifyHint="true"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   The returned <see cref="TextWriter"/> should <em>not</em> be disposed by the caller.
    /// </para>
    /// </remarks>
    public static TextWriter GetWriter(this StandardStream stream)
    {
        return stream switch
        {
            StandardStream.Output => Console.Out,
            StandardStream.Error => Console.Error,
            _ => throw new ArgumentException(Properties.Resources.InvalidStandardStreamError, nameof(stream)),
        };
    }

    /// <summary>
    /// Gets the <see cref="TextWriter"/> for either <see cref="StandardStream.Output" qualifyHint="true"/>
    /// or <see cref="StandardStream.Error" qualifyHint="true"/>.
    /// </summary>
    /// <param name="stream">A <see cref="StandardStream"/> value.</param>
    /// <returns>
    /// The value of either <see cref="Console.Out" qualifyHint="true"/> or
    /// <see cref="Console.Error" qualifyHint="true"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="stream"/> was a value other than <see cref="StandardStream.Output" qualifyHint="true"/>
    /// or <see cref="StandardStream.Error" qualifyHint="true"/>.
    /// </exception>
    public static Stream OpenStream(this StandardStream stream)
    {
        return stream switch
        {
            StandardStream.Output => Console.OpenStandardOutput(),
            StandardStream.Error => Console.OpenStandardError(),
            StandardStream.Input => Console.OpenStandardInput(),
            _ => throw new ArgumentException(Properties.Resources.InvalidStandardStreamError, nameof(stream)),
        };
    }

    /// <summary>
    /// Gets the <see cref="StandardStream"/> associated with a <see cref="TextWriter"/> if that
    /// writer is for either the standard output or error stream.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/>.</param>
    /// <returns>
    /// The <see cref="StandardStream"/> that <paramref name="writer"/> is writing to, or
    /// <see langword="null"/> if it's not writing to either the standard output or standard error
    /// stream.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="writer"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   If <paramref name="writer"/> is an instance of the <see cref="LineWrappingTextWriter"/>
    ///   class, the <see cref="LineWrappingTextWriter.BaseWriter" qualifyHint="true"/> will be
    ///   checked.
    /// </para>
    /// </remarks>
    public static StandardStream? GetStandardStream(this TextWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        if (writer is LineWrappingTextWriter lwtw)
        {
            writer = lwtw.BaseWriter;
        }

        if (writer == Console.Out)
        {
            return StandardStream.Output;
        }
        else if (writer == Console.Error)
        {
            return StandardStream.Error;
        }

        return null;
    }

    /// <summary>
    /// Gets a value that indicates whether the specified standard stream is redirected.
    /// </summary>
    /// <param name="stream">The <see cref="StandardStream"/> value.</param>
    /// <returns>
    /// <see langword="true"/> if the standard stream indicated by <paramref name="stream"/> is
    /// redirected; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsRedirected(this StandardStream stream)
    {
        return stream switch
        {
            StandardStream.Output => Console.IsOutputRedirected,
            StandardStream.Error => Console.IsErrorRedirected,
            StandardStream.Input => Console.IsInputRedirected,
            _ => false,
        };
    }

    /// <summary>
    /// Gets the <see cref="StandardStream"/> associated with a <see cref="TextReader"/> if that
    /// reader is for the standard input stream.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/>.</param>
    /// <returns>
    /// The <see cref="StandardStream"/> that <paramref name="reader"/> is reader from, or
    /// <see langword="null"/> if it's not reader from the standard input stream.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="reader"/> is <see langword="null"/>.
    /// </exception>
    public static StandardStream? GetStandardStream(this TextReader reader)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        if (reader == Console.In)
        {
            return StandardStream.Input;
        }

        return null;
    }
}
