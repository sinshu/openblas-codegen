using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public static class CodeProcessor
{
    private static readonly string[] primitiveTypes = new string[]
    {
        "integer",
        "doublereal",
        "logical",
        "address",
        "char",
    };

    public static IEnumerable<(string, LineType)> Process(string path)
    {
        var currentState = State.None;
        var constantFound = false;
        var inFunctionDeclaration = false;
        var inExtern = false;
        var inVariableDeclaration = false;

        foreach (var line in File.ReadLines(path))
        {
            switch (currentState)
            {
                case State.None:
                    if (IsSingleLineFunctionDeclaration(line))
                    {
                        yield return (line, LineType.FunctionDeclaration);
                        currentState = State.Function;
                    }
                    else if (IsMultiLineFunctionDeclarationBegin(line))
                    {
                        yield return (line, LineType.FunctionDeclaration);
                        currentState = State.Function;
                        inFunctionDeclaration = true;
                    }
                    else if (constantFound && IsConstant(line))
                    {
                        yield return (line, LineType.Constant);
                    }
                    else
                    {
                        if (line.Contains("/* Table of constant values */"))
                        {
                            constantFound = true;
                        }
                        yield return (line, LineType.None);
                    }
                    break;

                case State.Function:
                    if (inFunctionDeclaration)
                    {
                        if (IsMultiLineFunctionDeclarationEnd(line))
                        {
                            yield return (line, LineType.FunctionDeclaration);
                            inFunctionDeclaration = false;
                        }
                        else
                        {
                            yield return (line, LineType.FunctionDeclaration);
                        }
                    }
                    else if (inExtern)
                    {
                        if (IsMultiLineExternEnd(line))
                        {
                            yield return (line, LineType.Extern);
                            inExtern = false;
                        }
                        else
                        {
                            yield return (line, LineType.Extern);
                        }
                    }
                    else if (inVariableDeclaration)
                    {
                        if (IsMultiLineVariableDeclarationEnd(line))
                        {
                            yield return (line, LineType.VariableDeclaration);
                            inVariableDeclaration = false;
                        }
                        else
                        {
                            yield return (line, LineType.VariableDeclaration);
                        }
                    }
                    else
                    {
                        if (IsSingleLineExtern(line))
                        {
                            yield return (line, LineType.Extern);
                        }
                        else if (IsMultiLineExternBegin(line))
                        {
                            yield return (line, LineType.Extern);
                            inExtern = true;
                        }
                        else if (IsSingleLineVariableDeclaration(line))
                        {
                            yield return (line, LineType.VariableDeclaration);
                        }
                        else if (IsMultiLineVariableDeclarationBegin(line))
                        {
                            yield return (line, LineType.VariableDeclaration);
                            inVariableDeclaration = true;
                        }
                        else if (IsSingleLineComment(line))
                        {
                            yield return (line, LineType.Comment);
                        }
                        else if (line.StartsWith("{"))
                        {
                            yield return (line, LineType.FunctionBegin);
                        }
                        else if (line.StartsWith("}"))
                        {
                            yield return (line, LineType.FunctionEnd);
                            currentState = State.None;
                        }
                        else
                        {
                            yield return (line, LineType.FunctionBody);
                        }
                    }
                    break;
            }
        }
    }

    private static bool IsSingleLineFunctionDeclaration(string line)
    {
        return line.Trim().StartsWith("/* Subroutine */") && line.Trim().EndsWith(")");
    }

    private static bool IsMultiLineFunctionDeclarationBegin(string line)
    {
        return line.Trim().StartsWith("/* Subroutine */") && !line.Trim().EndsWith(")");
    }

    private static bool IsMultiLineFunctionDeclarationEnd(string line)
    {
        return line.Trim().EndsWith(")");
    }

    private static bool IsSingleLineExtern(string line)
    {
        return line.Trim().StartsWith("extern") && line.Trim().EndsWith(";");
    }

    private static bool IsMultiLineExternBegin(string line)
    {
        return line.Trim().StartsWith("extern") && !line.Trim().EndsWith(";");
    }

    private static bool IsMultiLineExternEnd(string line)
    {
        return line.Trim().EndsWith(";");
    }

    private static bool IsSingleLineVariableDeclaration(string line)
    {
        return primitiveTypes.Any(pt => line.Trim().StartsWith(pt)) && line.Trim().EndsWith(";");
    }

    private static bool IsMultiLineVariableDeclarationBegin(string line)
    {
        return primitiveTypes.Any(pt => line.Trim().StartsWith(pt)) && !line.Trim().EndsWith(";");
    }

    private static bool IsMultiLineVariableDeclarationEnd(string line)
    {
        return line.Trim().EndsWith(";");
    }

    private static bool IsSingleLineComment(string line)
    {
        return line.Trim().StartsWith("/*") && line.Trim().EndsWith("*/");
    }

    private static bool IsConstant(string line)
    {
        return line.Trim().StartsWith("static ");
    }



    private enum State
    {
        None,
        Function,
    }
}
