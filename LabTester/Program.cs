using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace LabTester
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Enter args -p and -s");
                Console.WriteLine("Example:");
                Console.WriteLine("-p 1 4 9 16 25 -m 420 1260 2100");
                Console.ReadLine();
                return;
            }

            string argsString = string.Join(" ", args);
            int indexOfp = argsString.IndexOf("-p", StringComparison.Ordinal);
            int indexOfm = argsString.IndexOf("-s", StringComparison.Ordinal);

            if (indexOfm < 0 || indexOfp < 0)
            {
                Console.WriteLine("Enter args -p and -s");
                Console.WriteLine("Example:");
                Console.WriteLine("-p 1 4 9 16 25 -m 420 1260 2100");
                Console.ReadLine();
                return;
            }

            string processString = string.Empty;
            string matrixString = string.Empty;

            if (indexOfp < indexOfm)
            {
                processString = argsString.Substring(indexOfp, indexOfm - indexOfp).Substring(3);
                matrixString = argsString.Substring(indexOfm).Substring(3);
            }
            else
            {
                processString = argsString.Substring(indexOfp).Substring(3);
                matrixString = argsString.Substring(indexOfm, indexOfp - indexOfm).Substring(3);
            }

            var processesEnum = processString.Split(" ").Where(s => s.Length > 0);
            var matrixSizeEnum = matrixString.Split(" ").Where(s => s.Length > 0);

            var processes = processesEnum.Select(int.Parse).ToList(); //{ 1, 4, 9, 16, 25, 36, 49 };
            var matrixSize = matrixSizeEnum.Select(int.Parse).ToList();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var times = CalculateTimes(processes, matrixSize);

            var speedDictionary = CalculateSpeedUP(processes, matrixSize, times);

            var costDictionary = CalculateCost(processes, matrixSize, times);

            var efficiency = CalculateEfficiency(processes, matrixSize, speedDictionary);

            Console.WriteLine($"Total time: {stopwatch.Elapsed.Seconds} seconds");

            Console.ReadLine();
        }

        private static Dictionary<(int, int), double> CalculateEfficiency(List<int> processes, List<int> matrixSize, Dictionary<(int, int), double> speedDictionary)
        {
            var efficiency = new Dictionary<(int, int), double>();
            Console.WriteLine("Эффективность");
            //int processesKey = processes[0];
            WriteDictionary(processes, matrixSize,
                (matrix, process) =>
                {
                    var currentKey = (matrix, process);
                    var value = speedDictionary[currentKey] / process;
                    efficiency.Add(currentKey, value);
                    return value;
                });
            return efficiency;
        }

        private static Dictionary<(int, int), double> CalculateCost(List<int> processes, List<int> matrixSize, Dictionary<(int, int), double> times)
        {
            Console.WriteLine("Стоимость");
            var costDictionary = new Dictionary<(int, int), double>();
            //int processesKey = processes[0];
            WriteDictionary(processes, matrixSize,
                (matrix, process) =>
                {
                    var currentKey = (matrix, process);
                    var value = times[currentKey] * process;
                    costDictionary.Add(currentKey, value);
                    return value;
                });
            return costDictionary;
        }

        private static Dictionary<(int, int), double> CalculateSpeedUP(List<int> processes, List<int> matrixSize, Dictionary<(int, int), double> times)
        {
            Console.WriteLine("Ускорение");
            var speedDictionary = new Dictionary<(int, int), double>();
            int processesKey = processes[0];
            WriteDictionary(processes, matrixSize,
                (matrix, process) =>
                {
                    var delKey = (matrix, processesKey);
                    var currentKey = (matrix, process);
                    var value = times[delKey] / times[currentKey];
                    speedDictionary.Add(currentKey, value);
                    return value;
                });
            return speedDictionary;
        }

        private static Dictionary<(int, int), double> CalculateTimes(List<int> processes, List<int> matrixSize)
        {
            var times = new Dictionary<(int, int), double>();
            Console.WriteLine("Время");
            WriteDictionary(processes, matrixSize,
                (matrix, process) =>
                {
                    var key = (matrix, process);
                    var value = RunTest(process, matrix);
                    times.Add(key, value);
                    return value;
                });
            return times;
        }

        private static void WriteDictionary(List<int> processes, List<int> matrixSize, Func<int, int, double> callBack)
        {
            WriteHeader(matrixSize);

            foreach (var process in processes)
            {
                Console.Write($"{process,10:D}");
                foreach (var matrix in matrixSize)
                {
                    double result = callBack.Invoke(matrix, process);
                    Console.Write($"{result,10:F4}");
                }
                Console.WriteLine();
            }
        }

        private static void WriteHeader(List<int> matrixSize)
        {
            string header = @"proc\size";
            Console.Write($"{header,10}");
            foreach (var matrix in matrixSize)
            {
                Console.Write($"{matrix,10:D}");
            }
            Console.WriteLine();
        }

        private static double RunTest(int process, int matrixSize)
        {
            if (process > 256 && matrixSize > 1024) return -1;
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C mpiexec -n {process} mpi.exe {matrixSize}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            double value = -1.0;
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line == null) return -1;

                
                try
                {
                    value = double.Parse(line, CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    // ignored
                }

                return value;
            }

            return value;
        }
    }
}
