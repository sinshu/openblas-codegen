using System;

static class Program
{
    static void Main(string[] args)
    {
        foreach (var p in CodeProcessor.Process("dgesdd.c"))
        {
            Console.WriteLine(p.Item1 + " : " + p.Item2);
        }
    }
}
