using System;
using System.Drawing;

namespace Ookii.CommandLine.Terminal;

/// <summary>
/// Represents a virtual terminal (VT) sequence for a change in text formatting.
/// </summary>
/// <remarks>
/// <para>
///   Write one of the predefined values in this class to a stream representing the console, such
///   as <see cref="Console.Out"/> or <see cref="Console.Error"/>, to set the specified text format
///   on that stream.
/// </para>
/// <para>
///   You should only write VT sequences to the console if they are supported. Use the
///   <see cref="VirtualTerminal.EnableColor"/> method to check whether VT sequences are supported,
///   and to enable them if required by the operating system.
/// </para>
/// <para>
///   You can combine <see cref="TextFormat"/> instances to apply multiple options by using
///   the <see cref="Combine"/> method or the <see cref="operator +"/> operator.
/// </para>
/// </remarks>
public readonly struct TextFormat : IEquatable<TextFormat>
{
    /// <summary>
    /// Resets the text format to the settings before modification.
    /// </summary>
    public static readonly TextFormat Default = new("\x1b[0m");
    /// <summary>
    /// Applies the brightness/intensity flag to the foreground color.
    /// </summary>
    public static readonly TextFormat BoldBright = new("\x1b[1m");
    /// <summary>
    /// Removes the brightness/intensity flag to the foreground color.
    /// </summary>
    public static readonly TextFormat NoBoldBright = new("\x1b[22m");
    /// <summary>
    /// Adds underline.
    /// </summary>
    public static readonly TextFormat Underline = new("\x1b[4m");
    /// <summary>
    /// Removes underline.
    /// </summary>
    public static readonly TextFormat NoUnderline = new("\x1b[24m");
    /// <summary>
    /// Swaps foreground and background colors.
    /// </summary>
    public static readonly TextFormat Negative = new("\x1b[7m");
    /// <summary>
    /// Returns foreground and background colors to normal.
    /// </summary>
    public static readonly TextFormat Positive = new("\x1b[27m");
    /// <summary>
    /// Sets the foreground color to Black.
    /// </summary>
    public static readonly TextFormat ForegroundBlack = new("\x1b[30m");
    /// <summary>
    /// Sets the foreground color to Red.
    /// </summary>
    public static readonly TextFormat ForegroundRed = new("\x1b[31m");
    /// <summary>
    /// Sets the foreground color to Green.
    /// </summary>
    public static readonly TextFormat ForegroundGreen = new("\x1b[32m");
    /// <summary>
    /// Sets the foreground color to Yellow.
    /// </summary>
    public static readonly TextFormat ForegroundYellow = new("\x1b[33m");
    /// <summary>
    /// Sets the foreground color to Blue.
    /// </summary>
    public static readonly TextFormat ForegroundBlue = new("\x1b[34m");
    /// <summary>
    /// Sets the foreground color to Magenta.
    /// </summary>
    public static readonly TextFormat ForegroundMagenta = new("\x1b[35m");
    /// <summary>
    /// Sets the foreground color to Cyan.
    /// </summary>
    public static readonly TextFormat ForegroundCyan = new("\x1b[36m");
    /// <summary>
    /// Sets the foreground color to White.
    /// </summary>
    public static readonly TextFormat ForegroundWhite = new("\x1b[37m");
    /// <summary>
    /// Sets the foreground color to Default.
    /// </summary>
    public static readonly TextFormat ForegroundDefault = new("\x1b[39m");
    /// <summary>
    /// Sets the background color to Black.
    /// </summary>
    public static readonly TextFormat BackgroundBlack = new("\x1b[40m");
    /// <summary>
    /// Sets the background color to Red.
    /// </summary>
    public static readonly TextFormat BackgroundRed = new("\x1b[41m");
    /// <summary>
    /// Sets the background color to Green.
    /// </summary>
    public static readonly TextFormat BackgroundGreen = new("\x1b[42m");
    /// <summary>
    /// Sets the background color to Yellow.
    /// </summary>
    public static readonly TextFormat BackgroundYellow = new("\x1b[43m");
    /// <summary>
    /// Sets the background color to Blue.
    /// </summary>
    public static readonly TextFormat BackgroundBlue = new("\x1b[44m");
    /// <summary>
    /// Sets the background color to Magenta.
    /// </summary>
    public static readonly TextFormat BackgroundMagenta = new("\x1b[45m");
    /// <summary>
    /// Sets the background color to Cyan.
    /// </summary>
    public static readonly TextFormat BackgroundCyan = new("\x1b[46m");
    /// <summary>
    /// Sets the background color to White.
    /// </summary>
    public static readonly TextFormat BackgroundWhite = new("\x1b[47m");
    /// <summary>
    /// Sets the background color to Default.
    /// </summary>
    public static readonly TextFormat BackgroundDefault = new("\x1b[49m");
    /// <summary>
    /// Sets the foreground color to bright Black.
    /// </summary>
    public static readonly TextFormat BrightForegroundBlack = new("\x1b[90m");
    /// <summary>
    /// Sets the foreground color to bright Red.
    /// </summary>
    public static readonly TextFormat BrightForegroundRed = new("\x1b[91m");
    /// <summary>
    /// Sets the foreground color to bright Green.
    /// </summary>
    public static readonly TextFormat BrightForegroundGreen = new("\x1b[92m");
    /// <summary>
    /// Sets the foreground color to bright Yellow.
    /// </summary>
    public static readonly TextFormat BrightForegroundYellow = new("\x1b[93m");
    /// <summary>
    /// Sets the foreground color to bright Blue.
    /// </summary>
    public static readonly TextFormat BrightForegroundBlue = new("\x1b[94m");
    /// <summary>
    /// Sets the foreground color to bright Magenta.
    /// </summary>
    public static readonly TextFormat BrightForegroundMagenta = new("\x1b[95m");
    /// <summary>
    /// Sets the foreground color to bright Cyan.
    /// </summary>
    public static readonly TextFormat BrightForegroundCyan = new("\x1b[96m");
    /// <summary>
    /// Sets the foreground color to bright White.
    /// </summary>
    public static readonly TextFormat BrightForegroundWhite = new("\x1b[97m");
    /// <summary>
    /// Sets the background color to bright Black.
    /// </summary>
    public static readonly TextFormat BrightBackgroundBlack = new("\x1b[100m");
    /// <summary>
    /// Sets the background color to bright Red.
    /// </summary>
    public static readonly TextFormat BrightBackgroundRed = new("\x1b[101m");
    /// <summary>
    /// Sets the background color to bright Green.
    /// </summary>
    public static readonly TextFormat BrightBackgroundGreen = new("\x1b[102m");
    /// <summary>
    /// Sets the background color to bright Yellow.
    /// </summary>
    public static readonly TextFormat BrightBackgroundYellow = new("\x1b[103m");
    /// <summary>
    /// Sets the background color to bright Blue.
    /// </summary>
    public static readonly TextFormat BrightBackgroundBlue = new("\x1b[104m");
    /// <summary>
    /// Sets the background color to bright Magenta.
    /// </summary>
    public static readonly TextFormat BrightBackgroundMagenta = new("\x1b[105m");
    /// <summary>
    /// Sets the background color to bright Cyan.
    /// </summary>
    public static readonly TextFormat BrightBackgroundCyan = new("\x1b[106m");
    /// <summary>
    /// Sets the background color to bright White.
    /// </summary>
    public static readonly TextFormat BrightBackgroundWhite = new("\x1b[107m");

    private readonly string? _value;

    /// <summary>
    /// Returns the virtual terminal sequence to the foreground or background color to an RGB
    /// color.
    /// </summary>
    /// <param name="color">The color to use.</param>
    /// <param name="foreground">
    ///   <see langword="true"/> to apply the color to the background; otherwise, it's applied
    ///   to the background.
    /// </param>
    /// <returns>A <see cref="TextFormat"/> instance with the virtual terminal sequence.</returns>
    public static TextFormat GetExtendedColor(Color color, bool foreground = true)
    {
        return new(FormattableString.Invariant($"{VirtualTerminal.Escape}[{(foreground ? 38 : 48)};2;{color.R};{color.G};{color.B}m"));
    }

    private TextFormat(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Returns the text formatting string contained in this instance.
    /// </summary>
    /// <returns>The value of the <see cref="Value"/> property.</returns>
    public override string ToString() => Value ?? string.Empty;

    /// <summary>
    /// Combines two text formatting values.
    /// </summary>
    /// <param name="other">The <see cref="TextFormat"/> value to combine with this one.</param>
    /// <returns>A <see cref="TextFormat"/> instance that applies both the input format options.</returns>
    /// <seealso cref="operator +(TextFormat, TextFormat)"/>
    public TextFormat Combine(TextFormat other) => new(Value + other.Value);

    /// <summary>
    /// Determine whether this instance and another <see cref="TextFormat"/> instance have the
    /// same value.
    /// </summary>
    /// <param name="other">The <see cref="TextFormat"/> instance to compare to.</param>
    /// <returns>
    /// <see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(TextFormat other) => Value.Equals(other.Value, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is TextFormat format)
        {
            return Equals(format);
        }

        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Gets the text formatting string/
    /// </summary>
    /// <value>
    /// A string containing virtual terminal sequences, or an empty string if this structure was
    /// default-initialized.
    /// </value>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Combines two text formatting values.
    /// </summary>
    /// <param name="left">The first <see cref="TextFormat"/> value.</param>
    /// <param name="right">The second <see cref="TextFormat"/> value.</param>
    /// <returns>A <see cref="TextFormat"/> instance that applies both the input format options.</returns>
    public static TextFormat operator +(TextFormat left, TextFormat right) => left.Combine(right);

    /// <summary>
    /// Determine whether this instance and another <see cref="TextFormat"/> instance have the
    /// same value.
    /// </summary>
    /// <param name="left">The first <see cref="TextFormat"/> value.</param>
    /// <param name="right">The second <see cref="TextFormat"/> value.</param>
    /// <returns>
    /// <see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(TextFormat left, TextFormat right) => left.Equals(right);

    /// <summary>
    /// Determine whether this instance and another <see cref="TextFormat"/> instance have a
    /// different value.
    /// </summary>
    /// <param name="left">The first <see cref="TextFormat"/> value.</param>
    /// <param name="right">The second <see cref="TextFormat"/> value.</param>
    /// <returns>
    /// <see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(TextFormat left, TextFormat right) => !left.Equals(right);
}
