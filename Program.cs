using System;
using System.IO;

static class Program
{
    static void Main(string[] args)
    {
        Directory.CreateDirectory("cs");

        foreach (var srcPath in Directory.EnumerateFiles("c"))
        {
            var dstPath = Path.Combine("cs", Path.GetFileNameWithoutExtension(srcPath) + ".cs");
            Console.WriteLine(srcPath + " => " + dstPath);
            CodeGenerator.Process(srcPath, dstPath);
        }
    }
}
