using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace LabTester
{
    public class StatsCalculator
    {
        private readonly List<int> processesList;
        private readonly List<string> testsArgs;

        public StatsCalculator(List<int> processesList, List<string> testsArgs)
        {
            this.processesList = processesList;
            this.testsArgs = testsArgs;
        }

        public Dictionary<TestKey, double> CalculateTimes()
        {
            Console.WriteLine("Время");

            var times = new Dictionary<TestKey, double>();

            WriteDictionary(t => CalculateTimesFunction(t, times));

            return times;
        }

        private double CalculateTimesFunction(TestKey testParamsKey, Dictionary<TestKey, double> timesDictionary)
        {
            double value = RunTest(testParamsKey.ProcessCount, testParamsKey.TaskArgs);
            timesDictionary.Add(testParamsKey, value);
            return value;
        }

        public Dictionary<TestKey, double> CalculateEffisiency(Dictionary<TestKey, double> acceleration)
        {
            Console.WriteLine("Эффективность");

            var efficiency = new Dictionary<TestKey, double>();

            WriteDictionary(t => CalculateEfficiencyFunction(t, acceleration, efficiency));

            return efficiency;
        }

        private double CalculateEfficiencyFunction(TestKey testParamsKey,
            Dictionary<TestKey, double> acceleration,
            Dictionary<TestKey, double> efficiency)
        {
            double value = acceleration[testParamsKey] / testParamsKey.ProcessCount;
            efficiency.Add(testParamsKey, value);
            return value;
        }

        public Dictionary<TestKey, double> CalculateAcceleration(Dictionary<TestKey, double> times)
        {
            Console.WriteLine("Ускорение");

            var acceleration = new Dictionary<TestKey, double>();

            WriteDictionary(t => CalculateAccelerationFunction(t, times, acceleration));

            return acceleration;
        }

        private double CalculateAccelerationFunction(TestKey testParamsKey,
            Dictionary<TestKey, double> times,
            Dictionary<TestKey, double> acceleration)
        {
            int processesKey = processesList[0]; // base process
            var singleProcessKey = new TestKey(processesKey, testParamsKey.TaskArgs);
            double value = times[singleProcessKey] / times[testParamsKey];
            acceleration.Add(testParamsKey, value);
            return value;
        }

        public Dictionary<TestKey, double> CalculateCost(Dictionary<TestKey, double> times)
        {
            Console.WriteLine("Стоимость");

            var cost = new Dictionary<TestKey, double>();

            WriteDictionary(t => CalculateCostFunction(t, times, cost));

            return cost;
        }

        private double CalculateCostFunction(TestKey testKey,
            Dictionary<TestKey, double> times,
            Dictionary<TestKey, double> cost)
        {
            double value = times[testKey] * testKey.ProcessCount;
            cost.Add(testKey, value);
            return value;
        }

        private void WriteDictionary(Func<TestKey, double> callBack)
        {
            WriteHeader(testsArgs);

            foreach (int process in processesList)
            {
                Console.Write($"{process,10:D}");
                foreach (string testArgs in testsArgs)
                {
                    var testKey = new TestKey(process, testArgs);
                    double result = callBack.Invoke(testKey);
                    Console.Write($"{result,10:F4}");
                }

                Console.WriteLine();
            }
        }

        private static void WriteHeader(List<string> testsArgs)
        {
            var header = @"proc\size";
            Console.Write($"{header,10}");
            foreach (string testArgs in testsArgs) Console.Write($"[{testArgs,10}]");
            Console.WriteLine();
        }

        private static double RunTest(int process, string testArgs)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C mpiexec -n {process} mpi.exe {testArgs}",
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
                catch (Exception)
                {
                    // ignored
                }

                return value;
            }

            return value;
        }
    }
}