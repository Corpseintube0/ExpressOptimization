﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressOptimization.Library;

namespace ExpressOptimization.ConsoleApp
{
    public class Program
    {
        private const string _argLabelDefault = "x"; // default argument name
        private static readonly Dictionary<string, Action<string>> _actionMap = new Dictionary<string, Action<string>>(); // сопоставление для входных параметров и действий

        private readonly IList<string> _operationOrder = new List<string> // operations order
        {
            "-v", "-d", "-o", "-c"
        };

        private static string _argLabel; // expression argument name
        private static string _inputString;
        
        public Program()
        {
            _actionMap.Add("-d", TakeDerivative);
            _actionMap.Add("-o", OptimizeExpression);
            _actionMap.Add("-v", SetArgumentLabel);
            _actionMap.Add("-с", CalculateExpression);
        }

        public static void Main(string[] args)
        {
            var index = Array.IndexOf(args, "-c");
            if (index > -1)
            {
                if (!TryMergeCalcParams(index, ref args))
                {
                    ShowHelp();
                    return;
                }
            }

            // arguments initialization
            for (int i = 0; i < args.Length; ++i)
            {
                var argumentValues = new Dictionary<string, string>();
                switch (args[i])
                {
                    case "-d":
                        break;
                    case "-o":
                        break;
                    case "-v":
                        argumentValues.Add("-v", args[i++]);
                        continue;
                    case "-c":
                        
                        break;
                    default:
                        if (String.IsNullOrEmpty(_inputString))
                        {
                            _inputString = args[i];
                        }
                        else
                        {
                            ShowHelp();
                            return;
                        }
                        break;
                }
            }

            // invocation of the actions 

            Console.ReadKey();
        }

        private static void ShowHelp()
        {
            Console.WriteLine("helping tips");
            // TODO: implement
        }

        private static void TakeDerivative(string arg)
        {
            var dt = new DerivativeTaker();
            dt.Derivation(arg, _argLabel);
        }

        private static void OptimizeExpression(string arg)
        {
            throw new NotImplementedException("[-o] Are not implemented yet!");
        }

        private static void CalculateExpression(string arg)
        {
            throw new NotImplementedException("[-c] Are not implemented yet!");
        }

        /// <summary>Задаёт обозначение аргумента в выражении.</summary>
        /// <param name="arg">Имя аргумента.</param>
        private static void SetArgumentLabel(string arg)
        {
            if (String.IsNullOrWhiteSpace(arg))
            {
                return;
            }

            _argLabel = arg;
        }

        // Объединение входных параметров метода Main(string[]), идущих после [-c] 
        private static bool TryMergeCalcParams(int startIndex, ref string[] args)
        {
            //TODO: test
            var result = new List<string>();

            // добавление к результату всего что было до параметров
            for (int i = 0; i < startIndex; ++i)
            {
                result.Add(args[i]); 
            }

            var isSetupCorrect = false;
            var union = new List<string>(); // объединение параметров в один
            if (!args[startIndex].StartsWith("'"))
            {
                return false;
            }

            union.Add(args[startIndex]);
            int endIndex = args.Length - 1;
            for (int i = startIndex + 1; i < args.Length; ++i)
            {
                union.Add(args[i]);
                if (args[i].EndsWith("'"))
                {
                    isSetupCorrect = true;
                    break;
                }
            }

            if (!isSetupCorrect || union.Count % 2 != 0) // проверка корректности
            {
                return false;
            }
            
            //TODO: удалить символ ' в первом и последнем аргументе
            result.Add(String.Join(" ", union));
            for (int i = endIndex; i < args.Length; ++i) // добавление оставшихся аргументов к результату
            {
                result.Add(args[i]);
            }

            args = result.ToArray();
            return true;
        }
    }
}
