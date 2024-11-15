# Categories sample

This sample shows how an application with a large number of command line arguments can use
[argument categories](TODO) to group arguments together in the usage help. This makes the usage help
more organized and makes it easier for the user to find the argument they are looking for.

The [arguments](Arguments.cs) used by this sample were taken from the
[GenerateAnswerFile project](https://www.github.com/SvenGroot/GenerateAnswerFile). They have been
simplified a little for this sample, shortening the descriptions, and removing some of the custom
types and validation attributes. Check out the
[real arguments](https://github.com/SvenGroot/GenerateAnswerFile/blob/main/src/GenerateAnswerFile/Arguments.cs)
from GenerateAnswerFile to see all the features it uses.

The arguments are divided into four categories, in addition to `-OutputFile`, `-Help` and `-Version`
arguments, which do not have a category and will be shown at the top of the usage help.

This sample also shows how a custom type with a `Parse()` method can be used for arguments, with the
[`DomainUser`](DomainUser.cs) class. This class also defines a value description on the type, so
individual arguments using the type don't have to manually define one.

The usage help for this sample looks as follows:

```text
A sample program that demonstrates how to use categories to organize arguments in the usage help.
It does not perform any operation if arguments are provided.

Usage: Categories [[-OutputFile] <Path>] [arguments]

    -OutputFile <Path> (-o)
        The path and file name to write the answer file to.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Version [<Boolean>]
        Displays version information.

Installation options

    -Feature <String> (-c)
        The name of an optional feature to install. Must be used with: -WindowsVersion. Must not be
        blank.

    -ImageIndex <Number> (-wim)
        The index of the image in the WIM file to install.

    -Install <InstallMethod> (-i)
        The installation method to use. Possible values: PreInstalled, ExistingPartition, CleanEfi,
        CleanBios, Manual.
        Default value: PreInstalled.

    -InstallToDisk <Number> (-disk)
        The zero-based ID of the disk to install to. Must be at least 0. Default value: 0.

    -InstallToPartition <Number> (-part)
        The one-based ID of the partition to install to, on the disk specified by -InstallToDisk.
        Must be at least 1.

    -Partition <Label:Size> (-p)
        A partition to create on the disk specified by -InstallToDisk. Can have multiple values.

    -ProductKey <String> (-key)
        The product key used to select what edition to install, and to activate Windows. Must not be
        blank.

    -WindowsVersion <Version> (-v)
        he exact version and build number (e.g. 10.0.22621.1) of the OS being installed.

User account options

    -AutoLogonCount <Number> (-alc)
        The number of times the user specified by -AutoLogonUser will be automatically logged on.
        Must be used with: -AutoLogonUser. Must be at least 1. Default value: 1.

    -AutoLogonPassword <String> (-alp)
        The password of the user specified by -AutoLogonUser. Must be used with: -AutoLogonUser.
        Must not be blank.

    -AutoLogonUser <[Domain\]User> (-alu)
        The name of a user to automatically log on, in the format domain\user, or just user for
        local users. If not specified, automatic log-on will not be used. Must be used with:
        -AutoLogonPassword. Must not be blank.

    -LocalAccount <[Group:]Name,Password> (-a)
        A local account to create, using the format group:name,password or name,password.

Domain options

    -DomainAccount <[Group:][Domain\]User> (-da)
        The name of a domain account to add to a local group. Must not be blank.

    -JoinDomain <String> (-jd)
        The name of a domain to join. If not specified, the system will not be joined to a domain.
        Must be used with: -JoinDomainUser, -JoinDomainPassword. Must not be blank.

    -JoinDomainOffline [<Boolean>] (-jdo)
        Join the domain during the offlineServicing pass of Windows setup, rather than the
        specialize pass. Must be used with: -JoinDomainProvisioningFile.

    -JoinDomainPassword <String> (-jdp)
        The password of the user specified by -JoinDomainUser. This will be stored in plain text in
        the answer file.Must be used with: -JoinDomain. Must not be blank.

    -JoinDomainProvisioningFile <Path> (-jdpf)
        The path to a file containing provisioned account data to join the domain. Cannot be used
        with: -JoinDomain.

    -JoinDomainUser <[Domain\]User> (-jdu)
        The name of a user with permission to join the domain specified by -JoinDomain. Must be
        used with: -JoinDomain. Must not be blank.

    -OUPath <String> (-ou)
        The organizational unit to use when joining the domain specified by -JoinDomain. Must be
        used with:-JoinDomain. Must not be blank.

Other options

    -ComputerName <String> (-n)
        The network name for the computer. If not specified, Windows will generate a default name.
        Must not be blank.

    -DisableCloud [<Boolean>] (-dc)
        Disable Windows cloud consumer features. This prevents auto-installation of recommended
        store apps.

    -DisableDefender [<Boolean>] (-d)
        Disable Windows Defender virus and threat protection.

    -DisableServerManager [<Boolean>] (-dsm)
        Do not automatically start Server Manager when logging on (Windows Server only).

    -DisplayResolution <Size> (-res)
        The display resolution, in the format width,height. For example, 1920,1080. If not
        specified, the default resolution is determined by Windows.

    -EnableRemoteDesktop [<Boolean>] (-rdp)
        Turn on remote desktop, and create a Windows Defender Firewall rule to allow incoming
        connections.

    -FirstLogonCommand <String> (-cmd)
        A command to run during first logon. Must not be blank.

    -FirstLogonScript <String> (-SetupScript, -s)
        The full path of a Windows PowerShell script to run during first log-on, plus arguments.
        Must not be blank.

    -Language <String> (-lang)
        The language used for the UI language, and the input, system and user locales. Must not be
        blank. Default value: en-US.

    -ProcessorArchitecture <String> (-arch)
        The processor architecture of the Windows edition you're installing. Must not be blank.
        Default value: amd64.

    -TimeZone <String>
        The time zone that Windows will use. Must not be blank. Default value: Pacific Standard
        Time.

Check out GenerateAnswerFile for a real version of this application:
https://www.github.com/SvenGroot/GenerateAnswerFile
```
