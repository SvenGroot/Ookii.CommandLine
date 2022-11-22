using Ookii.CommandLine;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace SubCommand
{
    // A TypeConverter for the Encoding class, using the utility base class provided by
    // Ookii.CommandLine.
    internal class EncodingConverter : TypeConverterBase<Encoding>
    {
        protected override Encoding? Convert(ITypeDescriptorContext? context, CultureInfo? culture, string value)
        {
            try
            {
                return Encoding.GetEncoding(value);
            }
            catch (ArgumentException ex)
            {
                // This is the expected exception type for a type converter.
                throw new FormatException(ex.Message, ex);
            }
        }
    }
}
