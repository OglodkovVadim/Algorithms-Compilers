using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("Введите арифметическое выражение (exit - выход):");
            string input = Console.ReadLine();
            input = input.Trim().Replace(" ", "").Replace(",", ".");

            if (input == "exit")
                return;

            try
            {
                if (IsValidExpression(input))
                {
                    List<string> postfix = InfixToPostfix(input);
                    double result = EvaluatePostfix(postfix);
                    Console.WriteLine($"Результат: {result}");
                }
            }
            catch (DivideByZeroException)
            {
                Console.WriteLine("Ошибка: Деление на ноль.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }

    static bool IsValidExpression(string input)
    {
        int openParentheses = 0;
        bool lastWasOperator = true;
        bool hasDecimal = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (char.IsDigit(c) || (c == '.' && !hasDecimal))
            {
                lastWasOperator = false;
                if (c == '.') hasDecimal = true;
            }
            else if (c == '(')
            {
                openParentheses++;
                lastWasOperator = true;
            }
            else if (c == ')')
            {
                openParentheses--;
                if (openParentheses < 0)
                {
                    throw new Exception("Несоответствие количества скобок.");
                }
                lastWasOperator = false;
            }
            else if ("+-*/;".Contains(c))
            {
                if (lastWasOperator && c != '-')
                {
                    throw new Exception("Некорректное использование оператора.");
                }
                lastWasOperator = true;
                hasDecimal = false;
            }
            else if (char.IsLetter(c))
            {
                while (i < input.Length && char.IsLetter(input[i]))
                {
                    i++;
                }
                i--;
            }
            else
            {
                throw new Exception($"Некорректный символ: {c}");
            }
        }

        if (openParentheses != 0)
        {
            throw new Exception("Несоответствие количества скобок.");
        }

        return true;
    }

    static List<string> InfixToPostfix(string expression)
    {
        Stack<string> operators = new Stack<string>();
        List<string> output = new List<string>();
        string numberBuffer = "";
        string functionBuffer = "";
        bool expectNegative = true;

        for (int i = 0; i < expression.Length; i++)
        {
            char c = expression[i];

            if (char.IsDigit(c) || c == '.')
            {
                numberBuffer += c;
                expectNegative = false;
            }
            else
            {
                if (numberBuffer.Length > 0)
                {
                    output.Add(numberBuffer);
                    numberBuffer = "";
                }

                if (char.IsLetter(c))
                {
                    functionBuffer += c;
                    while (i + 1 < expression.Length && char.IsLetter(expression[i + 1]))
                    {
                        i++;
                        functionBuffer += expression[i];
                    }
                    operators.Push(functionBuffer);
                    functionBuffer = "";
                }
                else if (c == '(')
                {
                    operators.Push(c.ToString());
                    expectNegative = true;
                }
                else if (c == ')')
                {
                    while (operators.Peek() != "(")
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Pop();
                    if (operators.Count > 0 && IsFunction(operators.Peek()))
                    {
                        output.Add(operators.Pop());
                    }
                    expectNegative = false;
                }
                else if (c == ';')
                {
                    continue;
                }
                else if ("+-*/".Contains(c))
                {
                    if (c == '-' && expectNegative)
                    {
                        output.Add("0");
                    }

                    while (operators.Count > 0 && Precedence(operators.Peek()) >= Precedence(c.ToString()))
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Push(c.ToString());
                    expectNegative = true;
                }
            }
        }

        if (numberBuffer.Length > 0)
        {
            output.Add(numberBuffer);
        }

        while (operators.Count > 0)
        {
            output.Add(operators.Pop());
        }

        return output;
    }

    static double EvaluatePostfix(List<string> postfix)
    {
        Stack<double> stack = new Stack<double>();

        foreach (var token in postfix)
        {
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
            {
                stack.Push(number);
            }
            else if (IsFunction(token))
            {
                double b = stack.Pop();
                double a = stack.Pop();
                stack.Push(ApplyFunction(token, a, b));
            }
            else
            {
                double b = stack.Pop();
                double a = stack.Pop();
                stack.Push(ApplyOperator(token, a, b));
            }
        }

        return stack.Pop();
    }

    static double ApplyOperator(string op, double a, double b)
    {
        return op switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" => b == 0 ? throw new DivideByZeroException() : a / b,
            _ => throw new ArgumentException("Неизвестный оператор")
        };
    }

    static double ApplyFunction(string functionName, double a, double b)
    {
        return functionName switch
        {
            "log" => Math.Log(b, a),
            _ => throw new ArgumentException("Неизвестная функция")
        };
    }

    static int Precedence(string op)
    {
        return op switch
        {
            "+" => 1,
            "-" => 1,
            "*" => 2,
            "/" => 2,
            _ when IsFunction(op) => 3,
            _ => 0
        };
    }

    static bool IsFunction(string token)
    {
        return token == "log";
    }
}
