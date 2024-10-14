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

    // Метод для проверки корректности арифметического выражения
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
            else if ("+-*/;".Contains(c)) // Поддержка разделителя ;
            {
                if (lastWasOperator && c != '-')  // Минус может быть перед числом
                {
                    throw new Exception("Некорректное использование оператора.");
                }
                lastWasOperator = true;
                hasDecimal = false;
            }
            else if (char.IsLetter(c)) // Проверка на начало функции
            {
                // Игнорируем часть для функции
                while (i < input.Length && char.IsLetter(input[i]))
                {
                    i++;
                }
                i--; // Шаг назад, чтобы вернуться на последний символ функции
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

    // Преобразование инфиксного выражения в постфиксное (ОПЗ)
    static List<string> InfixToPostfix(string expression)
    {
        Stack<string> operators = new Stack<string>();
        List<string> output = new List<string>();
        string numberBuffer = "";
        string functionBuffer = "";
        bool expectNegative = true; // Для отслеживания отрицательных чисел

        for (int i = 0; i < expression.Length; i++)
        {
            char c = expression[i];

            if (char.IsDigit(c) || c == '.')
            {
                numberBuffer += c; // Собираем число
                expectNegative = false; // После числа минус не может быть отрицательным числом
            }
            else
            {
                if (numberBuffer.Length > 0)
                {
                    output.Add(numberBuffer); // Добавляем число в выходную строку
                    numberBuffer = "";
                }

                if (char.IsLetter(c)) // Обработка функции
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
                    expectNegative = true; // После открывающей скобки минус может означать отрицательное число
                }
                else if (c == ')')
                {
                    while (operators.Peek() != "(")
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Pop(); // Убираем открывающую скобку
                    if (operators.Count > 0 && IsFunction(operators.Peek()))
                    {
                        output.Add(operators.Pop()); // Если есть функция, добавляем её в выходную строку
                    }
                    expectNegative = false; // После закрывающей скобки минус не может быть отрицательным числом
                }
                else if (c == ';')
                {
                    // Если встречается точка с запятой, продолжаем обработку аргументов функции
                    continue;
                }
                else if ("+-*/".Contains(c))
                {
                    if (c == '-' && expectNegative)
                    {
                        output.Add("0"); // Добавляем 0 перед минусом, чтобы обработать отрицательные числа в скобках, например, (-1)
                    }

                    while (operators.Count > 0 && Precedence(operators.Peek()) >= Precedence(c.ToString()))
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Push(c.ToString());
                    expectNegative = true; // Ожидаем, что после оператора может быть отрицательное число
                }
            }
        }

        if (numberBuffer.Length > 0)
        {
            output.Add(numberBuffer); // Добавляем последнее число
        }

        while (operators.Count > 0)
        {
            output.Add(operators.Pop()); // Добавляем оставшиеся операторы
        }

        return output;
    }

    // Метод для вычисления выражения в постфиксной записи (ОПЗ)
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

    // Применение арифметического оператора
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

    // Применение встроенной функции (например, log или pow)
    static double ApplyFunction(string functionName, double a, double b)
    {
        return functionName switch
        {
            "log" => Math.Log(b, a),
            _ => throw new ArgumentException("Неизвестная функция")
        };
    }

    // Определение приоритета операторов и функций
    static int Precedence(string op)
    {
        return op switch
        {
            "+" => 1,
            "-" => 1,
            "*" => 2,
            "/" => 2,
            _ when IsFunction(op) => 3, // Функции имеют наивысший приоритет
            _ => 0
        };
    }

    // Проверка, является ли строка функцией
    static bool IsFunction(string token)
    {
        return token == "log";
    }
}
