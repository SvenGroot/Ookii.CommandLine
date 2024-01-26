﻿using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal class SourceBuilder
{
    private readonly StringBuilder _builder = new();
    private int _indentLevel;
    private bool _startOfLine = true;
    private bool _needArgumentSeparator;
    private string? _toolName;
    private string? _toolVersion;

    public SourceBuilder(INamespaceSymbol ns)
        : this(ns.IsGlobalNamespace ? null : ns.ToDisplayString())
    {
    }

    public SourceBuilder(string? ns)
    {
        _builder.AppendLine("// <auto-generated>");
        _builder.AppendLine("#nullable enable");
        _builder.AppendLine();
        if (ns != null)
        {
            AppendLine($"namespace {ns}");
            OpenBlock();
        }
    }

    public void Append(string text)
    {
        WriteIndent();
        _builder.Append(text);
        _startOfLine = false;
    }

    public void AppendLine()
    {
        _builder.AppendLine();
        _startOfLine = true;
    }

    public void AppendLine(string text)
    {
        WriteIndent();
        _builder.AppendLine(text);
        _startOfLine = true;
    }

    public void AppendArgument(string text)
    {
        if (_needArgumentSeparator)
        {
            AppendLine(",");
        }

        Append(text);
        _needArgumentSeparator = true;
    }

    public void CloseArgumentList(bool withSemicolon = true)
    {
        if (withSemicolon)
        {
            AppendLine(");");
        }
        else
        {
            AppendLine(")");
        }

        --_indentLevel;
        _needArgumentSeparator = false;
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

    public void AppendGeneratedCodeAttribute()
    {
        if (_toolName == null)
        {
            var assembly = Assembly.GetExecutingAssembly();
            _toolName = assembly.GetName().Name;
            _toolVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion ?? assembly.GetName().Version.ToString();
        }

        AppendLine($"[System.CodeDom.Compiler.GeneratedCode(\"{_toolName}\", \"{_toolVersion}\")]");
    }

    public string GetSource()
    {
        while (_indentLevel > 0)
        {
            CloseBlock();
        }

        return _builder.ToString();
    }

    public void IncreaseIndent() => ++_indentLevel;

    public void DecreaseIndent() => --_indentLevel;

    private void WriteIndent()
    {
        if (_startOfLine)
        {
            _builder.Append(' ', _indentLevel * 4);
        }
    }
}
