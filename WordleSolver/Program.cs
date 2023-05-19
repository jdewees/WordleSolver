// See https://aka.ms/new-console-template for more information
using System.Diagnostics;


var sw = new Stopwatch();
int len = 50;
sw.Start();

var f = new Finder();
for (int i = 0; i < len; i++)
{
    Finder.SolveRecursive();
}
sw.Stop();
Console.WriteLine("elapsed avg time of {0} ms.",(int) sw.ElapsedMilliseconds/len);
