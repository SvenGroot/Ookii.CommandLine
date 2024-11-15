using Ookii.CommandLine;

namespace Categories;

// This custom type has a value description, which will be shown in the usage help. There is no
// need to do this for every argument if it's specified on the type.
[ValueDescription("[Domain\\]User")]
class DomainUser
{
    public DomainUser(string? domain, string userName)
    {
        ArgumentNullException.ThrowIfNull(userName);
        Domain = domain;
        UserName = userName;
    }

    public DomainUser(string userName)
        : this(null, userName)
    {
    }

    public string? Domain { get; }

    public string UserName { get; }

    public override string ToString()
        => Domain == null ? UserName : $"{Domain}\\{UserName}";

    // The CommandLineParser will use this method to convert a string to a DomainUser instance.
    // Using a method with ReadOnlySpan<char> is only possible when using the
    // GeneratedParserAttribute.
    public static DomainUser Parse(ReadOnlySpan<char> value)
    {
        var index = value.IndexOf('\\');
        if (index == -1)
        {
            return new(value.ToString());
        }
        else
        {
            return new(value[0..index].ToString(), value[(index+1)..].ToString());
        }
    }
}
