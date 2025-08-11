using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

class Program
{
    static double[] data = Array.Empty<double>();

    private static void LoadData()
    {
        Console.WriteLine("Loading data...");
        using (FileStream fs = new FileStream("data.bin", FileMode.Open))
        using (BinaryReader br = new BinaryReader(fs))
        {
            int len = (int)(fs.Length / sizeof(float));
            data = new double[len];
            for (int i = 0; i < len; i++)
            {
                float f = br.ReadSingle();
                data[i] = f * 36.0;
            }
        }
        Console.WriteLine("Data loaded successfully.\n");
    }

    private static double Calculate(double value)
    {
        double sum;
        int iv = (int)value;

        if ((iv & 1) == 0) sum = value * 0.002;
        else if (iv % 3 == 0) sum = value * 0.003;
        else if (iv % 5 == 0) sum = value * 0.005;
        else if (iv % 7 == 0) sum = value * 0.007;
        else sum = value * 0.001;

        return (((long)sum & 1) == 0 ? sum : -sum) * 0.00001;
    }

    private static void Main(string[] args)
    {
        LoadData();
        Console.WriteLine("Calculation start...");

        Stopwatch sw = Stopwatch.StartNew();

        double finalResult = 0;

        Parallel.For(0, Environment.ProcessorCount, core =>
        {
            double localResult = 0;
            int chunkSize = data.Length / Environment.ProcessorCount;
            int start = core * chunkSize;
            int end = (core == Environment.ProcessorCount - 1) ? data.Length : start + chunkSize;

            Console.WriteLine(Environment.ProcessorCount > 1
                ? $"Core {core} processing data from index {start} to {end - 1}."
                : "Single core processing.");
                
            for (int loop = 0; loop < 30; loop++)
            {
                for (int i = start; i < end; i++)
                {
                    localResult += Calculate(data[i]);
                    data[i] *= 0.1;
                }
            }

            lock (typeof(Program))
            {
                finalResult += localResult;
            }
        });

        sw.Stop();
        Console.WriteLine($"Calculation finished in {sw.ElapsedMilliseconds} ms. Result: {finalResult:F25}");
    }
}
