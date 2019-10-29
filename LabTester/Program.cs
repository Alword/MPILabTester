using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LabTester
{
    internal class Program
    {
        private static readonly string[] ArgsTypes = { "-n", "-args" };

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
            Console.WriteLine($"Total time: {stopwatch.Elapsed}");

            Console.ReadLine();
        }

        private static bool TryReadArgs(string[] args, out List<int> processes, out List<string> tasksArgs)
        {
            processes = null;
            tasksArgs = null;

            // check args

            if (IsInvalidArgs(args))
            { 
                args = WriteInstruction().Split(" ");

                if (IsInvalidArgs(args))
                    return false;
            }


            // find index
            string argsString = GetArgsIndex(args, out int numberIndex, out int argsIndex);


            if (HasEachArgsType(argsIndex, numberIndex))
            {
                WriteInstruction();
                return false;
            }

            // process string
            List<string> resultStrings = ProcessString(numberIndex, argsIndex, argsString);
            // process args
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
                processString = argsString.Substring(numberIndex, argsIndex - numberIndex).Substring(ArgsTypes[0].Length);
                matrixString = argsString.Substring(argsIndex).Substring(ArgsTypes[1].Length);
            }
            else
            {
                processString = argsString.Substring(numberIndex).Substring(ArgsTypes[0].Length);
                matrixString = argsString.Substring(argsIndex, numberIndex - argsIndex).Substring(ArgsTypes[1].Length);
            }

            resultList.Add(processString);
            resultList.Add(matrixString);

            return resultList;
        }

        private static string GetArgsIndex(string[] args, out int numberIndex, out int argsIndex)
        {
            string argsString = string.Join(" ", args);
            numberIndex = argsString.IndexOf(ArgsTypes[0], StringComparison.Ordinal);
            argsIndex = argsString.IndexOf(ArgsTypes[1], StringComparison.Ordinal);
            return argsString;
        }

        private static bool HasEachArgsType(int indexOfm, int indexOfp)
        {
            return indexOfm < 0 || indexOfp < 0;
        }

        private static bool IsInvalidArgs(IReadOnlyCollection<string> args)
        {
            return args == null || args.Count == 0;
        }

        private static string WriteInstruction()
        {
            Console.WriteLine("Enter args -n and -args");
            Console.WriteLine("Rename your test program file to mpi.exe");
            Console.WriteLine("Directory must contain this mpi.exe!\n");
            Console.WriteLine("Example 1: (single arg per task)\n");
            Console.WriteLine("-n 1 2 3 -args 4; 5; 6;\n");
            Console.WriteLine("Example 2 (three args per task)\n");
            Console.WriteLine("-n 1 4 9 16 25 -args 1 2 3; 4 5 6;\n");
            Console.WriteLine("MPI accept args Example");
            Console.WriteLine();
            Console.WriteLine("#include <iostream>");
            Console.WriteLine("#include<stdlib.h>");
            Console.WriteLine("int main(int argc, char* argv[]) {");
            Console.WriteLine("   int arg1;");
            Console.WriteLine("   sscanf_s(argv[1],\"%d\",&arg1);");
            Console.WriteLine("   std::cout << myArg1;");
            Console.WriteLine("   return 0;");
            Console.WriteLine("}");
            Console.WriteLine();
            Console.WriteLine("This example should write value of myArg1 (N per -n times)");
            Console.WriteLine();
            // TO-DO string builder
            Console.Write("Enter params line:");
            return Console.ReadLine();
        }
    }
}