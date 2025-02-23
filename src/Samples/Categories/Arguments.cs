using Ookii.CommandLine;
using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Validation;
using System.ComponentModel;
using System.Drawing;

namespace Categories;

// These arguments were taken from the GenerateAnswerFile project: https://www.github.com/SvenGroot/GenerateAnswerFile
// They have been simplified a little for this sample, shortening the descriptions, and removing
// some of the custom types and validation attributes.
//
// This sample demonstrates how to use categories to organize arguments in the usage help. This
// application has a lot of arguments, and using categories makes it easier for the user to find the
// arguments they're looking for.
[GeneratedParser]
[Description("A sample program that demonstrates how to use categories to organize arguments in the usage help. It does not perform any operation if arguments are provided.")]
[UsageFooter("Check out GenerateAnswerFile for a real version of this application: https://www.github.com/SvenGroot/GenerateAnswerFile")]
partial class Arguments
{
    // This argument does not have a category, so it will be shown at the top in the usage help,
    // along with the -Help and -Version arguments. If you want to set a category for the -Help
    // and -Version arguments, you can use `[ParseOptions(DefaultCategory = <value>)]` on the class.
    [CommandLineArgument(IsPositional = true)]
    [Description("The path and file name to write the answer file to.")]
    [ValueDescription("Path")]
    [Alias("o")]
    public FileInfo? OutputFile { get; set; }

    // This argument is in the Install category. The header of the category, as well as the order
    // of the categories, is taken from the ArgumentCategory enumeration.
    [CommandLineArgument("Feature", Category = ArgumentCategory.Install)]
    [Description("The name of an optional feature to install.")]
    [Alias("c")]
    [Requires(nameof(WindowsVersion))]
    [ValidateNotWhiteSpace]
    [MultiValueSeparator]
    public string[]? Features { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Install)]
    [Description("The installation method to use.")]
    [Alias("i")]
    [ValidateEnumValue]
    public InstallMethod Install { get; set; } = InstallMethod.PreInstalled;

    [CommandLineArgument(Category = ArgumentCategory.Install)]
    [Description("The zero-based ID of the disk to install to.")]
    [Alias("disk")]
    [ValidateRange(0, null)]
    public int InstallToDisk { get; set; } = 0;

    [CommandLineArgument(Category = ArgumentCategory.Install)]
    [Description("The one-based ID of the partition to install to, on the disk specified by -InstallToDisk.")]
    [Alias("part")]
    [ValidateRange(1, null)]
    public int? InstallToPartition { get; set; }

    [CommandLineArgument("Partition", Category = ArgumentCategory.Install)]
    [Description("A partition to create on the disk specified by -InstallToDisk. Can have multiple values.")]
    [ValueDescription("Label:Size")]
    [Alias("p")]
    [MultiValueSeparator]
    public string[]? Partitions { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Install)]
    [Description("The index of the image in the WIM file to install.")]
    [Alias("wim")]
    public int ImageIndex { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Install)]
    [Description("The product key used to select what edition to install, and to activate Windows.")]
    [Alias("key")]
    [ValidateNotWhiteSpace]
    public string? ProductKey { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Install)]
    [Description("he exact version and build number (e.g. 10.0.22621.1) of the OS being installed.")]
    [Alias("v")]
    public Version? WindowsVersion { get; set; }

    [CommandLineArgument("LocalAccount", Category = ArgumentCategory.UserAccounts)]
    [Description("A local account to create, using the format group:name,password or name,password.")]
    [ValueDescription("[Group:]Name,Password")]
    [Alias("a")]
    [MultiValueSeparator]
    public string[]? LocalAccounts { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.UserAccounts)]
    [Description("The name of a user to automatically log on, in the format domain\\user, or just user for local users. If not specified, automatic log-on will not be used.")]
    [Alias("alu")]
    [Requires(nameof(AutoLogonPassword))]
    [ValidateNotWhiteSpace]
    public DomainUser? AutoLogonUser { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.UserAccounts)]
    [Description("The password of the user specified by -AutoLogonUser.")]
    [Alias("alp")]
    [Requires(nameof(AutoLogonUser))]
    [ValidateNotWhiteSpace]
    public string? AutoLogonPassword { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.UserAccounts)]
    [Description("The number of times the user specified by -AutoLogonUser will be automatically logged on.")]
    [Alias("alc")]
    [Requires(nameof(AutoLogonUser))]
    [ValidateRange(1, null)]
    public int AutoLogonCount { get; set; } = 1;

    [CommandLineArgument(Category = ArgumentCategory.Domain)]
    [Description("The name of a domain to join. If not specified, the system will not be joined to a domain.")]
    [Requires(nameof(JoinDomainUser), nameof(JoinDomainPassword))]
    [Alias("jd")]
    [ValidateNotWhiteSpace]
    public string? JoinDomain { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Domain)]
    [Description("The name of a user with permission to join the domain specified by -JoinDomain.")]
    [Alias("jdu")]
    [Requires(nameof(JoinDomain))]
    [ValidateNotWhiteSpace]
    public DomainUser? JoinDomainUser { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Domain)]
    [Description("The password of the user specified by -JoinDomainUser. This will be stored in plain text in the answer file.")]
    [Alias("jdp")]
    [Requires(nameof(JoinDomain))]
    [ValidateNotWhiteSpace]
    public string? JoinDomainPassword { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Domain)]
    [Description("The organizational unit to use when joining the domain specified by -JoinDomain.")]
    [Alias("ou")]
    [Requires(nameof(JoinDomain))]
    [ValidateNotWhiteSpace]
    public string? OUPath { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Domain)]
    [Description("The path to a file containing provisioned account data to join the domain.")]
    [ValueDescription("Path")]
    [Prohibits(nameof(JoinDomain))]
    [Alias("jdpf")]
    public FileInfo? JoinDomainProvisioningFile { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Domain)]
    [Description("Join the domain during the offlineServicing pass of Windows setup, rather than the specialize pass.")]
    [Requires(nameof(JoinDomainProvisioningFile))]
    [Alias("jdo")]
    public bool JoinDomainOffline { get; set; }

    [CommandLineArgument("DomainAccount", Category = ArgumentCategory.Domain)]
    [Description("The name of a domain account to add to a local group.")]
    [ValueDescription("[Group:][Domain\\]User")]
    [Alias("da")]
    [ValidateNotWhiteSpace]
    [MultiValueSeparator]
    public string[]? DomainAccounts { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Other)]
    [Description("The network name for the computer. If not specified, Windows will generate a default name.")]
    [Alias("n")]
    [ValidateNotWhiteSpace]
    public string? ComputerName { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Other)]
    [Description("Disable Windows Defender virus and threat protection.")]
    [Alias("d")]
    public bool DisableDefender { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Other)]
    [Description("Disable Windows cloud consumer features. This prevents auto-installation of recommended store apps.")]
    [Alias("dc")]
    public bool DisableCloud { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Other)]
    [Description("Do not automatically start Server Manager when logging on (Windows Server only).")]
    [Alias("dsm")]
    public bool DisableServerManager { get; set; }

    [CommandLineArgument("FirstLogonCommand", Category = ArgumentCategory.Other)]
    [Description("A command to run during first logon.")]
    [Alias("cmd")]
    [ValidateNotWhiteSpace]
    [MultiValueSeparator]
    public string[]? FirstLogonCommands { get; set; }

    [CommandLineArgument("FirstLogonScript", Category = ArgumentCategory.Other)]
    [Description("The full path of a Windows PowerShell script to run during first log-on, plus arguments.")]
    [Alias("SetupScript")]
    [Alias("s")]
    [ValidateNotWhiteSpace]
    [MultiValueSeparator]
    public string[]? FirstLogonScripts { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Other)]
    [Description("Turn on remote desktop, and create a Windows Defender Firewall rule to allow incoming connections.")]
    [Alias("rdp")]
    public bool EnableRemoteDesktop { get; set; }

    // This argument uses the default TypeConverter for the System.Drawing.Size type, rather than
    // defining a custom converter.
    [CommandLineArgument(Category = ArgumentCategory.Other)]
    [Description("The display resolution, in the format width,height. For example, 1920,1080. If not specified, the default resolution is determined by Windows.")]
    [Alias("res")]
    [ArgumentConverter(typeof(WrappedDefaultTypeConverter<Size>))]
    public Size? DisplayResolution { get; set; }

    [CommandLineArgument(Category = ArgumentCategory.Other)]
    [Description("The language used for the UI language, and the input, system and user locales.")]
    [Alias("lang")]
    [ValidateNotWhiteSpace]
    public string Language { get; set; } = "en-US";

    [CommandLineArgument(Category = ArgumentCategory.Other)]
    [Description("The processor architecture of the Windows edition you're installing.")]
    [Alias("arch")]
    [ValidateNotWhiteSpace]
    public string ProcessorArchitecture { get; set; } = "amd64";

    [CommandLineArgument(Category = ArgumentCategory.Other)]
    [Description("The time zone that Windows will use.")]
    [ValidateNotWhiteSpace]
    public string TimeZone { get; set; } = "Pacific Standard Time";
}
