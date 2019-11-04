using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ExpressOptimization.Library
{
    //TODO: add logger
    //TODO: add console application

    // Выражения записываются в явном виде. Пример (x+2)^2+4*x
    // Приоритет операций (по убыванию):
    // 5: () - скобки имеют наивысший приоритет
    // 4: cos(x), sin(x), tg(x), ctg(x), ln(x), log(x), exp(x) - унарные операции 
    // 3: x^y - возведение x в степень y
    // 2: x*y, x/y - произведение, частное
    // 1: x+y, x-y - сумма, разность

    public abstract class ExpressOptimizerBase
    {
        #region - Private fields -
        
        //Показывает, следует ли считать следующий считанный оператор унарным.
        private static bool _unaryFlag; // TODO: похоже на костыль, лучше переделать

        #endregion

        #region - Properties -

        //Разделители.
        protected List<string> Separators => new List<string> { "(", ")", "+", "-", "*", "/", "^" };

        //Сложные функции.
        protected List<string> UnaryFunc => new List<string> { "sin", "cos", "tg", "ctg", "exp", "ln", "lg" };

        /// <summary>
        /// Задать или получить значение точности вычислений.
        /// </summary>
        public double Eps { get; set; }

        #endregion

        #region - Private methods -

        /// <summary>
        /// Преобразует введенную в виде строки функцию к постфиксной записи.
        /// </summary>
        /// <param name="func">Преобразуемая функция.</param>
        /// <returns>Постфиксная запись в виде массива значений string.</returns>
        protected List<string> ConvertToPostfix(string func)
        {
            func = func.Replace(" ", String.Empty).ToLower();

            var stack = new Stack<string>();
            var result = new List<string>();
            _unaryFlag = true;
            int i = 0;
            while (i < func.Length)
            {
                var lex = GetNextLexeme(func, ref i); // берем следующую лексему
                switch (lex)
                {
                    case ")":
                        while (stack.Peek() != "(")
                        {
                            result.Add(stack.Pop());
                        }

                        stack.Pop();
                        continue;
                    case "(":
                        stack.Push(lex);
                        continue;
                }

                if (this.Separators.Contains(lex) || this.UnaryFunc.Contains(lex)) // если лексема - знак операции
                {
                    if (stack.Count == 0)
                    {
                        stack.Push(lex);
                        continue;
                    }

                    if (stack.Count != 0)
                    {
                        while (PriorityOf(lex) <= PriorityOf(stack.Peek()))
                        {
                            result.Add(stack.Pop());
                            if (stack.Count == 0)
                            {
                                break;
                            }
                        }
                    }

                    stack.Push(lex);
                }
                else // если лексема - операнд
                {
                    result.Add(lex);
                }
            }

            while (stack.Count > 0)
            {
                result.Add(stack.Pop());
            }

            return result;
        }

    

        /// <summary>
        /// Преобразует постфиксное выражение в инфиксное (стандартная форма записи).
        /// </summary>
        /// <param name="arg">Постфиксная запись.</param>
        /// <returns>Инфиксная запись.</returns>
        protected string PostfixToInfix(IEnumerable<string> arg)
        {
            var argList = arg.ToList();
            var stack = new Stack<string>();
            var stackPr = new Stack<int>();

            for (int i = 0; i < argList.Count; ++i)
            {
                if (this.Separators.Contains(argList[i])) // если знак
                {
                    var prior = PriorityOf(argList[i]);
                    var i1 = stack.Pop();
                    var i2 = stack.Pop();
                    var i1P = stackPr.Pop();
                    var i2P = stackPr.Pop();
                    if (i1P < prior && i1P != 0)
                        i1 = $"({i1})";
                    if (i2P < prior && i2P != 0)
                        i2 = $"({i2})";
                    stack.Push($"{i2}{argList[i]}{i1}");
                    stackPr.Push(prior);
                }
                else if (this.UnaryFunc.Contains(argList[i])) //если унарная функция
                {
                    var i1 = stack.Pop();
                    stack.Push(String.Format("{1}({0})", i1, argList[i]));
                }
                else // если аргумент
                {
                    stack.Push(argList[i]);
                    stackPr.Push(0);
                }
            }
            return stack.Peek();
        }

        /// <summary>
        /// Оптимизирует запись функции в виде польской строки, упрощая её вид.
        /// </summary>
        /// <param name="arg">Польская строка.</param>
        /// <returns>Оптимизированная польская строка.</returns>
        protected List<string> PostfixOptimize(IEnumerable<string> arg)
        {
            var argList = arg.ToList();
            var example = argList;
            while (true)
            {
                var stack = new Stack<string>();
                var result = new List<string>();
                foreach (string item in argList)
                {
                    string u = "";
                    string v = "";
                    if (this.Separators.Contains(item))
                    {
                        v = stack.Pop();
                        u = stack.Pop();
                    }
                    else if (this.UnaryFunc.Contains(item))
                    {
                        u = stack.Pop();
                    }
                    double vt;
                    double ut;
                    switch (item)
                    {
                        case "+":
                            if (Double.TryParse(v, out vt) && Double.TryParse(u, out ut))
                                stack.Push((vt + ut).ToString(CultureInfo.InvariantCulture));
                            else if (v == "0")
                                stack.Push(u);
                            else if (u == "0")
                                stack.Push(v);
                            else
                                stack.Push($"{u} {v} +");
                            break;
                        case "-":
                            if (Double.TryParse(v, out vt) && Double.TryParse(u, out ut))
                                stack.Push((ut - vt).ToString(CultureInfo.InvariantCulture));
                            else if (v == "0")
                                stack.Push(u);
                            else if (u == "0")
                                stack.Push("-" + v);
                            else
                                stack.Push($"{u} {v} -");
                            break;
                        case "*":
                            if (Double.TryParse(v, out vt) && Double.TryParse(u, out ut))
                                stack.Push((vt * ut).ToString(CultureInfo.InvariantCulture));
                            else if (v == "0" || u == "0")
                                stack.Push("0");
                            else if (v == "1")
                                stack.Push(u);
                            else if (u == "1")
                                stack.Push(v);
                            else
                                stack.Push($"{u} {v} *");
                            break;
                        case "/":
                            if (Double.TryParse(v, out vt) && Double.TryParse(u, out ut))
                                stack.Push((ut / vt).ToString(CultureInfo.InvariantCulture));
                            else if (u == "0")
                                stack.Push("0");
                            else if (v == "1")
                                stack.Push(u);
                            else if (u == v)
                                stack.Push("1");
                            else
                                stack.Push($"{u} {v} /");
                            break;
                        case "^":
                            if (Double.TryParse(v, out vt) && Double.TryParse(u, out ut))
                                stack.Push((Math.Pow(ut, vt)).ToString(CultureInfo.InvariantCulture));
                            else if (u == "0" || u == "1" || v == "1")
                                stack.Push(u);
                            else if (v == "0")
                                stack.Push("1");
                            else
                                stack.Push($"{u} {v} ^");
                            break;
                        case "ln":
                            if (Double.TryParse(u, out ut))
                                stack.Push((Math.Log(ut).ToString(CultureInfo.InvariantCulture)));
                            else
                                stack.Push(u + " ln");
                            break;
                        case "cos":
                            if (Double.TryParse(u, out ut))
                                stack.Push((Math.Cos(ut).ToString(CultureInfo.InvariantCulture)));
                            else
                                stack.Push(u + " cos");
                            break;
                        case "sin":
                            if (Double.TryParse(u, out ut))
                                stack.Push((Math.Sin(ut).ToString(CultureInfo.InvariantCulture)));
                            else
                                stack.Push(u + " sin");
                            break;
                        case "tg":
                            if (Double.TryParse(u, out ut))
                                stack.Push((Math.Tan(ut).ToString(CultureInfo.InvariantCulture)));
                            else
                                stack.Push(u + " tg");
                            break;
                        case "exp":
                            if (Double.TryParse(u, out ut))
                                stack.Push(Math.Exp(ut).ToString(CultureInfo.InvariantCulture));
                            else
                                stack.Push(u + " exp");
                            break;
                        default:
                            stack.Push(item);
                            break;
                    }
                    if (stack.Count == 1)
                        result.Add(stack.Peek());
                }

                result = StrToList(stack.Peek());
                if (ListCompare(example, result))
                {
                    return result;
                }

                example = result;
            }
        }

        private bool ListCompare(IList<string> lhs, IList<string> rhs)
        {
            if (lhs.Count != rhs.Count)
            {
                return false;
            }

            return !lhs.Where((t, i) => t != rhs[i]).Any();
        }

        /// <summary>
        /// Конвертирует строку символов в лист.
        /// </summary>
        /// <param name="arg">Строка символов, разделенных пробелами.</param>
        protected List<string> StrToList(string arg)
        {
            var result = new List<string>();
            string item = "";
            foreach (char ch in arg)
            {
                if (ch == ' ')
                {
                    result.Add(item);
                    item = "";
                    continue;
                }

                item += ch;
            }
            result.Add(item);

            return result;
        }

        /// <summary>
        /// Определяет приоритет операции.
        /// </summary>
        /// <param name="symbol">Символ операнда, записанный в виде строки.</param>
        /// <returns>Приоритет операции от 1 до 5. Самый высокий приоритет - 5, самый низкий - 1.</returns>
        private int PriorityOf(string symbol)
        {
            switch (symbol)
            {
                //case "(": case ")":
                //    return 5;
                case "cos":
                case "sin":
                case "tg":
                case "ctg":
                case "ln":
                case "lg":
                case "exp":
                    return 4;
                case "^":
                    return 3;
                case "*":
                case "/":
                    return 2;
                case "+":
                case "-":
                    return 1;
            }
            return 0;
        }

        /// <summary>
        /// Выделяет лексему из входной строки и возвращает ее позицию.
        /// </summary>
        /// <param name="func">Функция записанная в виде строки.</param>
        /// <param name="pos">Индекс для начала выделения лексемы. После окончания работы метода параметр принимает значение индекса, следующего за лексемой.</param>
        /// <returns>Лексема.</returns>
        private string GetNextLexeme(string func, ref int pos)
        {
            string lexeme = "";
            if (this.Separators.Contains(func[pos].ToString(CultureInfo.InvariantCulture)))
            {
                if (func[pos] == '(')
                {
                    _unaryFlag = true;
                    return func[pos++].ToString(CultureInfo.InvariantCulture);
                }

                if (!_unaryFlag)
                {
                    return func[pos++].ToString(CultureInfo.InvariantCulture);
                }

                lexeme += func[pos++];
                _unaryFlag = false;
            }

            if (!Char.IsLetterOrDigit(func[pos]))
            {
                return lexeme;
            }

            for (var i = pos; i < func.Length; i++)
            {
                lexeme += func[pos];
                if (i == func.Length - 1)
                {
                    pos = func.Length;
                    return lexeme;
                }

                // если следующий символ - разделитель, возвращаем лексему
                if (!this.Separators.Contains(func[i + 1].ToString(CultureInfo.InvariantCulture)))
                {
                    ++pos;
                    continue;
                }
                pos = i + 1;
                _unaryFlag = false;
                return lexeme;
            }
            return lexeme;
        }

        #endregion

        #region - Public methods -

        /// <summary>
        /// Вычисляет значение постфиксной записи в заданной точке.
        /// </summary>
        /// <param name="postfix">Обратная польская запись.</param>
        /// <param name="values">Координаты n-мерной точки.</param>
        /// <param name="names">Идентификаторы переменных, заданные в строке функции.</param>
        /// <returns>Значение функции в заданной точке.</returns>
        public double CalculatePostfix(List<string> postfix, double[] values, params string[] names) //TODO: заменить на dictionary
        {
            var stack = new Stack<double>();
            foreach (var symb in postfix)
            {
                switch (symb)
                {
                    case "+":
                        double temp = stack.Pop();
                        stack.Push(stack.Pop() + temp);
                        break;
                    case "-":
                        temp = stack.Pop();
                        stack.Push(stack.Pop() - temp);
                        break;
                    case "*":
                        temp = stack.Pop();
                        stack.Push(stack.Pop() * temp);
                        break;
                    case "/":
                        temp = stack.Pop();
                        stack.Push(stack.Pop() / temp);
                        break;
                    case "^":
                        temp = stack.Pop();
                        stack.Push(Math.Pow(stack.Pop(), temp));
                        break;
                    case "sin":
                        stack.Push(Math.Sin(stack.Pop()));
                        break;
                    case "cos":
                        stack.Push(Math.Cos(stack.Pop()));
                        break;
                    case "tg":
                        stack.Push(Math.Tan(stack.Pop()));
                        break;
                    case "ctg":
                        stack.Push(Math.Cos(stack.Peek()) / Math.Sin(stack.Pop()));
                        break;
                    case "exp":
                        stack.Push(Math.Exp(stack.Pop()));
                        break;
                    case "ln":
                        stack.Push(Math.Log(stack.Pop()));
                        break;
                    case "lg":
                        stack.Push(Math.Log10(stack.Pop()));
                        break;
                    default:
                        double val;
                        if (Double.TryParse(symb, out val))
                        {
                            stack.Push(val);
                        }
                        else
                        {
                            int ind = Array.IndexOf(names, symb); // если лексема - аргумент, меняем на значение из списка
                            stack.Push(values[ind]);
                        }

                        break;
                }
            }

            if (stack.Count == 1)
            {
                return stack.Peek();
            }

            throw new Exception("Ошибка в вычислении функции.");
        }

        /// <summary>
        /// Вычисляет значение функции в заданной точке.
        /// </summary>
        /// <param name="func">Функция в строковом виде.</param>
        /// <param name="values">Координаты n-мерной точки.</param>
        /// <param name="names">Идентификаторы переменных, заданные в строке функции.</param>
        /// <returns>Значение функции в заданной точке.</returns>
        public double CalculateEquation(string func, double[] values, params string[] names) //TODO: заменить на dictionary
        {
            if (values.Length != names.Length)
                throw new Exception("Массив параметров не совпадает по размеру с массивом значений.");
            var postfixRec = ConvertToPostfix(func);

            return CalculatePostfix(postfixRec, values, names);
        }

        #endregion
    }
}
