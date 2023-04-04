using Microsoft.CodeAnalysis;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal class SourceBuilder
{
    private readonly StringBuilder _builder = new();
    private int _indentLevel;

    public SourceBuilder(INamespaceSymbol ns)
    {
        _builder.AppendLine("#nullable enable");
        _builder.AppendLine();
        if (!ns.IsGlobalNamespace)
        {
            AppendLine($"namespace {ns.ToDisplayString()}");
            OpenBlock();
        }
    }

    public void AppendLine()
    {
        _builder.AppendLine();
    }

    public void AppendLine(string text)
    {
        WriteIndent();
        _builder.AppendLine(text);
    }

    public void OpenBlock()
    {
        AppendLine("{");
        ++_indentLevel;
    }

    public void CloseBlock()
    {
        --_indentLevel;
        AppendLine("}");
    }

    public string GetSource()
    {
        while (_indentLevel > 0)
        {
            CloseBlock();
        }

        return _builder.ToString();
    }

    private void WriteIndent()
    {
        _builder.Append(' ', _indentLevel * 4);
    }

}
