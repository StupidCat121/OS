using System;
using System.IO;
using CalculatingFunctions;
using System.Threading;
using System.Text.Json;
using System.Diagnostics;

class Program
{
    static decimal[] data = Array.Empty<decimal>();
    static decimal result = 0;
    static int threadCount = Environment.ProcessorCount;
    private static object lockObj = new object();

    private static void LoadData()
    {
        Console.WriteLine("Loading data...");
        using (FileStream fs = new FileStream("data.bin", FileMode.Open))
        using (BinaryReader br = new BinaryReader(fs))
        {
            int len = (int)(fs.Length / sizeof(float));
            data = new decimal[len];
            for (int i = 0; i < len; i++)
            {
                float f = br.ReadSingle();
                data[i] = (decimal)f * 36.0m;
            }
        }
        Console.WriteLine("Data loaded successfully.\n");
    }

    private static void ThreadWork(int startIndex, int endIndex)
    {
        CalClass CF = new CalClass();
        decimal localResult = 0;

        for (int i = 0; i < 30; i++)
        {
            int index = startIndex;
            while (index < endIndex)
            {
                localResult += CF.Calculate1(ref data, ref index);
            }
        }

        lock (lockObj)
        {
            result += localResult;
        }
    }

    private static void Main(string[] args)
    {
        LoadData();
        Console.WriteLine("Calculation start...");

        Stopwatch sw = Stopwatch.StartNew();

        Thread[] threads = new Thread[threadCount];
        int chunkSize = data.Length / threadCount;

        for (int t = 0; t < threadCount; t++)
        {
            int start = t * chunkSize;
            int end = (t == threadCount - 1) ? data.Length : start + chunkSize;

            threads[t] = new Thread(() => ThreadWork(start, end));
            threads[t].Start();
        }

        for (int t = 0; t < threadCount; t++)
        {
            threads[t].Join();
        }

        sw.Stop();
        Console.WriteLine($"Thread : {threadCount}");
        Console.WriteLine($"Calculation finished in {sw.ElapsedMilliseconds} ms. Result: {result:F25}");
    }
}
