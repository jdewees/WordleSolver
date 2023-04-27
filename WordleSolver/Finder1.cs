// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
internal class Finder
{
    //const bool Parallel = false; // figure this out
    const bool Order_by_Letter_Frequency = true; // figure out how to make this work

    //string NewLine = System.Environment.NewLine;

    static private List<string> Banned_Words = new();
    const int word_length = 5;
    const int Max_Candidates_level_Three = 8198;
    static private List<String> words = new List<string>();
    static private List<int[]> solutions = new List<int[]>(1024);
    static private Dictionary<char, List<string>> wordsExcluding;
    static private Dictionary<char, List<int>> indexsOfWordsExcluding;

    public Finder()
    {
        Banned_Words.Add("FLDXT");
        Banned_Words.Add("HDQRS");
        Banned_Words.Add("ZHMUD");
        Banned_Words.Add("SEQWL");
        Banned_Words.Add("CHIVW");
        Banned_Words.Add("GCONV");
        Banned_Words.Add("FCONV");
        Banned_Words.Add("EXPWY");
        Banned_Words.Add("PBXES");

        wordsExcluding = new Dictionary<char, List<string>>();
        indexsOfWordsExcluding = new Dictionary<char, List<int>>();

        string filename = "words_alpha.txt";
        var p = Path.Combine(Environment.CurrentDirectory, filename);
        words = System.IO.File.ReadLines(p).Where(w => w.Length == word_length).ToList();
        //System.Console.WriteLine("there are {0} words", words.Count);

        words = words.Select(w => w.Trim().ToUpper()).ToList();
        foreach (var bw in Banned_Words)
        {
            words.Remove(bw.ToUpper());
        }

        for (int i = words.Count - 1; i >= 0; i--)
        {
            if (!containsUniqueLetters(words[i]))
                words.RemoveAt(i);
        }
        //System.Console.WriteLine("there are {0} words", words.Count);
        populateWordsExcluding();

    }

    private static void populateWordsExcluding()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToList();
        foreach (char c in chars)
        {
            wordsExcluding.Add(c, words.Where(words => !words.Contains(c)).ToList());
        }


    }

    private static int letterbits(string str)
    {
        int n = 0;
        for (int i = str.Length - 1; i >= 0; i--)
        {
            n |= 1 << (str[i] - 'A');
        }
        return n;
    }


    public static void SolveBruteForce()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        if (Order_by_Letter_Frequency)
        {
            sortWordsbyLetterEntropy();
        }

        int[] bitmaps = new int[words.Count];
        int minwordVal = int.MaxValue;
        string minWord = "";
        for (int i = 0; i < words.Count; i++)
        {
            bitmaps[i] = letterbits(words[i]);

        }



        int[] thirdwordbuffer = new int[Max_Candidates_level_Three / 2];
        int[] thirdindexbuffer = new int[Max_Candidates_level_Three / 2];
        int noWords = words.Count;
        Console.WriteLine("starting to solve");
        for (int i = 0; i < noWords; i++)
        {
            solveWithFirstWord(i, bitmaps, thirdwordbuffer, thirdindexbuffer);
        }

        sw.Stop();
        Console.WriteLine("Evaluation Took {0} ms", sw.ElapsedMilliseconds);

        dumpSolutions();

    }
    public static void sortWordsbyLetterEntropy()
    {
        int[] letterCounts = new int[26];
        foreach (string w in words)
        {
            for (int n = w.Length - 1; n >= 0; n--)
            {
                int letterNo = w[n] - 'A';
                if (letterNo < 0 || letterNo > 26)
                {
                    throw new ApplicationException("unexpected letter " + letterNo + " in '" + w + "'");
                }

                letterCounts[letterNo]++;
            }
        }

        Dictionary<string, long> totalFreqs = new Dictionary<string, long>(words.Count);
        foreach (string s in words)
        {
            totalFreqs.Add(s, totalFreq(s, letterCounts));
        }

        // sort the words list by the frequency (desc) of the values stored in dictionary thus first two selections will likely overlap less
        List<KeyValuePair<string, long>> tfreqs = totalFreqs.ToList();
        tfreqs.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

        for (int i = 0; i < words.Count; i++)
        {
            words[i] = tfreqs[i].Key;
        }

        //do sometthing to the "wordsExcluding" list here so that they are also sorted by 
    }

    private static long totalFreq(string w, int[] freqs)
    {
        long t = 0;
        for (int n = w.Length - 1; n >= 0; n--)
        {
            int letterNo = w[n] - 'A';
            t += freqs[letterNo];
        }
        return t;
    }



    private static void solveWithFirstWord(int i, int[] bitmaps, int[] thirdwordsbuffer, int[] thirdindexbuffer)
    {
        int bitmap1 = bitmaps[i];
        for (int j = 0; j < i; j++)

        {
            //totalSecondTests++;
            int bitmap2 = bitmaps[j];
            int bitmap3;
            int bitOneOrTwo = bitmap1 | bitmap2;
            bool cond1 = (bitmap1 & bitmap2) == 0;

            if (cond1)
            {
                int noThirdWords = 0;
                for (int k = 0; k < j; k++)

                {
                    bitmap3 = bitmaps[k];
                    bool cond3 = ((bitOneOrTwo) & bitmap3) == 0;
                    if (cond3)
                    {
                        thirdindexbuffer[noThirdWords] = k;
                        thirdwordsbuffer[noThirdWords] = bitmap3;
                        noThirdWords++;
                    }
                }

                if (noThirdWords > 0)
                {
                    findSolution(bitOneOrTwo, thirdwordsbuffer, thirdindexbuffer, noThirdWords, i, j);

                }
            }
        }


    }

    private static void findSolution(int firstTwoWordsBitmap, int[] availableWords, int[] availableIndex, int noAvailableWords, int wordNo1, int wordNo2)
    {
        for (int k = 0; k < noAvailableWords; k++)

        {
            int bitmap3 = availableWords[k];
            int bitsUpTo3 = firstTwoWordsBitmap | bitmap3;
            int bitmap4;
            int bitmap5;
            for (int l = 0; l < k; l++)

            {
                bitmap4 = availableWords[l];
                bool cond4 = ((bitsUpTo3) & bitmap4) == 0;
                if (cond4)
                {
                    for (int m = 0; m < l; m++)

                    {
                        bitmap5 = availableWords[m];
                        if (((bitsUpTo3 | bitmap4) & bitmap5) == 0)
                        {
                            int[] solution = new int[5];
                            solution[0] = wordNo1;
                            solution[1] = wordNo2;
                            solution[2] = availableIndex[k];
                            solution[3] = availableIndex[l];
                            solution[4] = availableIndex[m];
                            solutions.Add(solution);
                        }
                    }
                }

            }

        }
    }

    private static int[] bitmaps_for_recursive_algo;
    private static Dictionary<char, List<int>> letterindex;
    private static int[] letterOrder;

    public static void SolveRecursive()
    {

        solutions.Clear();

        char[] freqs = new char[26];
        foreach (string w in words)
        {
            foreach (char ch in w)
            {
                freqs[ch - 'A']++;
            }
        }
        char[] chars = new char[26];
        chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();


        List<KeyValuePair<char, int>> tfreqs = new List<KeyValuePair<char, int>>();

        for (int i = 0; i < freqs.Length; i++)
        {
            tfreqs.Add(new KeyValuePair<char, int>(Convert.ToChar('A' + i), freqs[i]));
        }
        //sort in reverse order so least used letter is at lowest index
        tfreqs.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

        bitmaps_for_recursive_algo = new int[words.Count];
        for (int i = 0; i < words.Count; i++)
        {
            bitmaps_for_recursive_algo[i] = letterbits(words[i]);
        }

        letterOrder = new int[26];
        var reverseletterOrder = new int[26];

        for (int i = 0; i < 26; i++)
        {
            letterOrder[i] = Convert.ToInt32(tfreqs[i].Key - 'A');
            reverseletterOrder[Convert.ToInt32(tfreqs[i].Key) - 'A'] = i;
        }

        letterindex = new Dictionary<char, List<int>>();
        foreach (char c in chars)
        {
            letterindex.Add(c, new List<int>());
        }

        //build index on the least used letter
        for (int i = 0; i < bitmaps_for_recursive_algo.Count(); i++)
        {
            int bitmap = bitmaps_for_recursive_algo[i];
            int letter = CountZeros(bitmap);
            int min = reverseletterOrder[letter];

            bitmap &= bitmap - 1;
            while (bitmap > 0)
            {
                letter = CountZeros(bitmap);
                min = Math.Min(min, reverseletterOrder[letter]);
                bitmap &= bitmap - 1;
            }

            letterindex[Convert.ToChar(min + 'A')].Add(bitmaps_for_recursive_algo[i]);
        }
        List<List<int>> solns = new List<List<int>>();
        var accWords = new List<int>(5) { 0, 0, 0, 0, 0 };
        Stopwatch sw = new Stopwatch();
        sw.Start();

        SolveRecurse(solns, 0, 0, accWords, 0, false);
        sw.Stop();
        Console.WriteLine("Completed in {0} ms", sw.ElapsedMilliseconds);

        Console.WriteLine("solution sets found: {0}", solns.Count);

    }

    private static void SolveRecurse(List<List<int>> solns, int totalbits, int numwords, List<int> accumulatedWords, int maxLetter, bool skipped, bool force = false)
    {

        if (numwords == 5)
        {
            solns.Add(accumulatedWords);
            return;
        }

        if (!force && numwords == 1)
        {
            // multithreading here
        }

        int max = bitmaps_for_recursive_algo.Length;

        //iterate over all letters until we find one not used...
        for (int i = maxLetter; i < 26; i++)
        {
            int letter = letterOrder[i];
            int m = 1 << letter;
            if ((totalbits & m) != 0) // does the accumulated set of bits already have this letter?
            {
                continue;
            }

            // iterate over each word startign with that letter to see if we can find a solution
            foreach (int currentbitmap in letterindex[Convert.ToChar(i + 'A')])
            {

                if ((totalbits & currentbitmap) != 0) // if the solutions don't already contain the 
                {
                    continue;
                }

                accumulatedWords[numwords] = currentbitmap;
                SolveRecurse(solns, totalbits | currentbitmap, numwords + 1, accumulatedWords, i + 1, skipped);
            }

            if (skipped)
            {
                break;
            }
            skipped = true;
        }


    }


    private static void dumpSolutions()
    {

        for (int i = 0; i < solutions.Count; i++)
        {
            System.Console.WriteLine("-------- Solution " + (i + 1) + " -------");
            printSolution(solutions[i]);
        }

    }
    private static void printSolution(int[] wordNos)
    {
        StringBuilder sb = new StringBuilder(256);
        for (int i = 0; i < wordNos.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(System.Environment.NewLine);
            }
            sb.Append(words[wordNos[i]]);
        }
        Console.WriteLine(sb.ToString());
    }
    private static bool containsUniqueLetters(string str)
    {
        return CountBits(letterbits(str)) == str.Length;
    }
    private static int CountBits(int value)
    {
        int count = 0;
        while (value != 0)
        {
            count++;
            value &= value - 1;
        }
        return count;
    }
    public static int CountZeros(int value)
    {
        int count = Mod37BitPosition[(-value & value) % 37];
        return count;
    }

    static int[] Mod37BitPosition = new int[] { 32, 0, 1, 26, 2, 23, 27, 0, 3, 16, 24, 30, 28, 11, 0, 13, 4, 7, 17, 0, 25, 22, 31, 15, 29, 10, 12, 6, 0, 21, 14, 9, 5, 20, 8, 19, 18 };
}