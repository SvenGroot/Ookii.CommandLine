namespace NestedCommands;

// We use this attribute to distinguish top-level commands from child commands.
//
// Top-level commands won't have this attribute. Child commands will have it with the type of
// their parent command set.
[AttributeUsage(AttributeTargets.Class)]
internal class ParentCommandAttribute : Attribute
{
    private readonly Type _parentCommand;

    public ParentCommandAttribute(Type parentCommand)
    {
        ArgumentNullException.ThrowIfNull(parentCommand);
        _parentCommand = parentCommand;
    }

    public Type ParentCommand => _parentCommand;
}
