using Ookii.CommandLine;
using Ookii.CommandLine.Conversion;
using System.Globalization;
using System.Text;

namespace SubcommandSample;

// An ArgumentConverter for the Encoding class.
internal class EncodingConverter : ArgumentConverter
{
    public override object? Convert(ReadOnlyMemory<char> value, CultureInfo culture, CommandLineArgument argument)
    {
        try
        {
            return Encoding.GetEncoding(value.ToString());
        }
        catch (ArgumentException ex)
        {
            // This is the expected exception type for a converter.
            throw new FormatException(ex.Message, ex);
        }
    }
}
