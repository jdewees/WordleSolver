// See https://aka.ms/new-console-template for more information
using System.Diagnostics;


var sw = new Stopwatch();
sw.Start();
var f = new Finder();
//Finder.SolveBruteForce();



Finder.SolveRecursive();
sw.Stop();
Console.WriteLine("elapsed total time of {0} ms.", sw.ElapsedMilliseconds);
