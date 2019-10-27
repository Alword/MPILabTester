using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LabTester
{
    internal class Program
    {
        private static readonly string[] ArgsTypes = {"-n", "-args"};

        private static void Main(string[] args)
        {
            if (!TryReadArgs(args, out List<int> processes, out List<string> tasksArgs)) return;


            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var statsCalculator = new StatsCalculator(processes, tasksArgs);

            Dictionary<TestKey, double> times = statsCalculator.CalculateTimes();

            Dictionary<TestKey, double> acceleration = statsCalculator.CalculateAcceleration(times);

            statsCalculator.CalculateCost(times);

            statsCalculator.CalculateEffisiency(acceleration);

            stopwatch.Stop();
            Console.WriteLine($"Total time: {stopwatch.Elapsed.Seconds} seconds");

            Console.ReadLine();
        }

        private static bool TryReadArgs(string[] args, out List<int> processes, out List<string> tasksArgs)
        {
            processes = null;
            tasksArgs = null;

            // check args
            if (IsArgsValid(args))
            {
                WriteInstruction();
                return false;
            }

            // find index
            string argsString = GetArgsIndex(out int numberIndex, out int argsIndex);

            if (HasEachArgsType(argsIndex, numberIndex))
            {
                WriteInstruction();
                return false;
            }

            // process string
            List<string> resultStrings = ProcessString(numberIndex, argsIndex, argsString);
            ConvertArgsToList(out processes, out tasksArgs, resultStrings);
            return true;
        }

        private static void ConvertArgsToList(out List<int> processes, out List<string> tasksArgs,
            IReadOnlyList<string> resultStrings)
        {
            IEnumerable<string> processesEnum = resultStrings[0].Split(" ").Where(s => s.Length > 0);
            IEnumerable<string> matrixSizeEnum = resultStrings[1].Split(";").Where(s => s.Length > 0);

            processes = processesEnum.Select(int.Parse).ToList();
            tasksArgs = matrixSizeEnum.Select(s => s.ToString()).ToList();
        }

        private static List<string> ProcessString(int numberIndex, int argsIndex, string argsString)
        {
            var resultList = new List<string>(2);
            string processString;
            string matrixString;

            if (numberIndex < argsIndex)
            {
                processString = argsString.Substring(numberIndex, argsIndex - numberIndex).Substring(3);
                matrixString = argsString.Substring(argsIndex).Substring(3);
            }
            else
            {
                processString = argsString.Substring(numberIndex).Substring(3);
                matrixString = argsString.Substring(argsIndex, numberIndex - argsIndex).Substring(3);
            }

            resultList.Add(processString);
            resultList.Add(matrixString);

            return resultList;
        }

        private static string GetArgsIndex(out int numberIndex, out int argsIndex)
        {
            string argsString = string.Join(" ", ArgsTypes);
            numberIndex = argsString.IndexOf(argsString[0], StringComparison.Ordinal);
            argsIndex = argsString.IndexOf(argsString[1], StringComparison.Ordinal);
            return argsString;
        }

        private static bool HasEachArgsType(int indexOfm, int indexOfp)
        {
            return indexOfm < 0 || indexOfp < 0;
        }

        private static bool IsArgsValid(IReadOnlyCollection<string> args)
        {
            return args == null || args.Count == 0;
        }

        private static void WriteInstruction()
        {
            Console.WriteLine("Enter args -n and -args");
            Console.WriteLine("Example:");
            Console.WriteLine("-p 1 4 9 16 25 -args 1 2 3; 4 5 6;");
            Console.WriteLine("-p 1 2 3 -args 4; 5; 6;");
            // TO-DO Explain whats mpi.exe will got
            Console.ReadLine();
        }
    }
}