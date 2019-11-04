using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressOptimization.Library
{
    /// <summary>
    /// Логика по взятию производных от функций, заданных в явном виде.
    /// </summary>
    public class DerivativeTaker : ExpressOptimizerBase
    {
        /// <summary>
        /// Берет производную от функции, заданной в виде строки.
        /// </summary>
        /// <param name="func">Функция.</param>
        /// <param name="dx">Переменная, по которой берется производная.</param>
        /// <returns>Производная от функции, представленная в виде строки.</returns>
        public string Derivation(string func, string dx)
        {
            var postFix = ConvertToPostfix(func);
            var postfixDer = GetDerive(postFix, dx);
            return PostfixToInfix(postfixDer);
        }

        /// <summary>
        /// Берет производную от функции, заданной в виде постфиксной записи.
        /// </summary>
        /// <param name="postfix">Функция.</param>
        /// <param name="dx">Диференцируемая переменная.</param>
        /// <returns>Производная в виде обратной польской строки.</returns>
        protected List<string> GetDerive(IEnumerable<string> postfix, string dx)
        {
            var postfixList = postfix.ToList();
            var stackVal = new Stack<string>(); //Стэк функций
            var stackDer = new Stack<string>(); //Стэк производных

            string v = "", vd = "", u = "", ud = "";
            for (int i = 0; i < postfixList.Count; i++)
            {
                if (this.Separators.Contains(postfixList[i]))
                {
                    v = stackVal.Pop();
                    vd = stackDer.Pop();
                    u = stackVal.Pop();
                    ud = stackDer.Pop();
                    var temp = $"{u} {v} {postfixList[i]}";
                    stackVal.Push(temp);
                }
                else if (this.UnaryFunc.Contains(postfixList[i]))
                {
                    u = stackVal.Pop();
                    ud = stackDer.Pop();
                    stackVal.Push($"{u} {postfixList[i]}");
                }
                switch (postfixList[i])
                {
                    case "+":
                    case "-":
                        var temp = $"{ud} {vd} {postfixList[i]}";
                        stackDer.Push(temp);
                        break;
                    case "*":
                        temp = String.Format("{0} {3} * {1} {2} * +", u, v, ud, vd);
                        stackDer.Push(temp);
                        break;
                    case "/":
                        temp = String.Format("{2} {1} * {3} {0} * - {1} 2 ^ /", u, v, ud, vd);
                        stackDer.Push(temp);
                        break;
                    case "^":
                        temp = $"{vd} {u} ln * {u} {v} ^ * {u} {v} 1 - ^ {ud} * {v} * +";
                        //temp = String.Format("{3} {0} ln * {0} {1} ^ * {0} {1} 1 - ^ {2} * {1} * +", u, v, ud, vd);
                        stackDer.Push(temp);
                        break;
                    case "sin":
                        temp = String.Format("{1} {0} cos *", u, ud);
                        stackDer.Push(temp);
                        break;
                    case "cos":
                        temp = String.Format("{1} {0} sin * -1 *", u, ud);
                        stackDer.Push(temp);
                        break;
                    case "tg":
                        temp = String.Format("{1} 1 {0} cos 2 ^ / *", u, ud);
                        stackDer.Push(temp);
                        break;
                    case "exp":
                        temp = $"{u} exp {ud} *";
                        stackDer.Push(temp);
                        break;
                    default:
                        stackVal.Push(postfixList[i]);
                        stackDer.Push(postfixList[i] == dx ? "1" : "0");
                        break;
                }
            }

            return PostfixOptimize(StrToList(stackDer.Peek()));
        }
    }
}
