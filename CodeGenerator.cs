using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class CodeGenerator
{
    private static readonly Regex regIsArray = new Regex(@"(.+)\[(\d+)\]");
    private static readonly Regex regRealNumber = new Regex(@"\d+\.\d*|\d*\.\d.");
    private static readonly Regex regCharPointer = new Regex(@"char\s*\*");
    private static readonly Regex regIfComma = new Regex(@"if\s*\(.*(\(.+,.+\)).*\)");
    private static readonly Regex regIs = new Regex(@"([^a-zA-Z0-9])is([^a-zA-Z0-9])");

    public static void Process(string srcPath, string dstPath)
    {
        using (var writer = new StreamWriter(dstPath))
        {
            writer.WriteLine("using System;");
            writer.WriteLine();
            writer.WriteLine("#pragma warning disable CS8981");
            writer.WriteLine("using integer = int;");
            writer.WriteLine("using doublereal = double;");
            writer.WriteLine("using logical = bool;");
            writer.WriteLine("using ftnlen = int;");
            writer.WriteLine("#pragma warning restore CS8981");
            writer.WriteLine();
            writer.WriteLine("namespace MatFlat");
            writer.WriteLine("{");
            writer.WriteLine("    internal static partial class OpenBlas");
            writer.WriteLine("    {");

            var functionDeclaration = new StringBuilder();
            var variableDeclaration = new StringBuilder();
            var constants = new List<string>();

            foreach (var tpl in CodeProcessor.Process(srcPath))
            {
                //Console.WriteLine(tpl);

                if (tpl.Item2 == LineType.None || tpl.Item2 == LineType.Extern)
                {
                    continue;
                }

                if (tpl.Item2 == LineType.FunctionDeclaration)
                {
                    functionDeclaration.Append(tpl.Item1.Replace("/* Subroutine */ ", ""));
                    continue;
                }

                if (tpl.Item2 == LineType.VariableDeclaration)
                {
                    variableDeclaration.Append(FixIs(tpl.Item1));
                    continue;
                }

                if (tpl.Item2 == LineType.Constant)
                {
                    constants.Add(FixRealNumber(tpl.Item1.Trim().Replace("static ", "")));
                    continue;
                }

                if (functionDeclaration.Length > 0 && tpl.Item2 != LineType.FunctionDeclaration)
                {
                    writer.WriteLine("internal static unsafe " + CharPointerToString(functionDeclaration.ToString()));
                    functionDeclaration.Clear();
                }

                if (tpl.Item2 == LineType.FunctionBegin)
                {
                    writer.WriteLine(tpl.Item1);
                    foreach (var constant in constants)
                    {
                        writer.WriteLine(constant);
                    }
                    writer.WriteLine();
                    continue;
                }

                if (variableDeclaration.Length > 0 && tpl.Item2 != LineType.VariableDeclaration)
                {
                    ProcessVariableDeclaration(variableDeclaration.ToString(), writer);
                    variableDeclaration.Clear();
                }

                var line = FixRealNumber(tpl.Item1);
                line = FixComma(line);
                line = FixIs(line);
                writer.WriteLine(line);
            }

            writer.WriteLine("    }");
            writer.WriteLine("}");
        }
    }

    private static void ProcessVariableDeclaration(string line, StreamWriter writer)
    {
        foreach (var singleType in line.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var split = singleType.Split(',').Select(value => value.Trim()).ToArray();
            var currentType = split[0].Split(' ')[0];
            split[0] = split[0].Split(' ')[1];
            foreach (var value in split)
            {
                var isArray = regIsArray.Match(value);
                if (isArray.Success)
                {
                    var name = isArray.Groups[1].Value;
                    var count = isArray.Groups[2].Value;
                    writer.WriteLine(currentType + "* " + name + " = stackalloc " + currentType + "[" + count + "];");
                }
                else
                {
                    if (currentType == "logical")
                    {
                        writer.WriteLine(currentType + " " + value + " = false;");
                    }
                    else
                    {
                        writer.WriteLine(currentType + " " + value + " = 0;");
                    }
                }
            }
        }
    }

    private static string FixRealNumber(string line)
    {
        return regRealNumber.Replace(line, match =>
        {
            if (match.Value.First() == '.')
            {
                return "0" + match.Value;
            }
            else if (match.Value.Last() == '.')
            {
                return match.Value + "0";
            }
            else
            {
                return match.Value;
            }
        });
    }

    private static string FixComma(string line)
    {
        for (var commaPos = 0; commaPos < line.Length; commaPos++)
        {
            if (line[commaPos] == ',')
            {
                var start = 0;
                var k = 0;
                for (var pos = commaPos - 1; pos >= 0; pos--)
                {
                    if (line[pos] == ')')
                    {
                        k++;
                    }
                    else if (line[pos] == '(')
                    {
                        if (k == 0)
                        {
                            start = pos;
                            break;
                        }
                        k--;
                    }
                }
                var end = line.Length;
                k = 0;
                for (var pos = commaPos + 1; pos < line.Length; pos++)
                {
                    if (line[pos] == '(')
                    {
                        k++;
                    }
                    else if (line[pos] == ')')
                    {
                        if (k == 0)
                        {
                            end = pos + 1;
                            break;
                        }
                        k--;
                    }
                }

                var target = line.Substring(start, end - start);
                var lineLeft = line.Substring(0, start);
                var lineRight = line.Substring(end, line.Length - end);
                if (target.First() == '(')
                {
                    var left = line.Substring(start, commaPos - start);
                    if (left.Contains('='))
                    {
                        return lineLeft + target + ".Item2" + lineRight;
                    }
                }
            }

        }

        var equalCount = line.Count(c => c == '=');
        if (equalCount >= 2)
        {
            if (line.Contains(','))
            {
                return line.Replace(',', ';');
            }
        }

        return line;
    }

    private static string CharPointerToString(string line)
    {
        return regCharPointer.Replace(line, "string ");
    }

    private static string FixIs(string line)
    {
        return regIs.Replace(line, match =>
        {
            return match.Groups[1].Value + "@is" + match.Groups[2].Value;
        });
    }
}
