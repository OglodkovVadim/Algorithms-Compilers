using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PushdownAutomaton
{
    class Program
    {
        // Структуры для хранения правил и состояний автомата
        static Dictionary<string, List<string>> GrammarRules = new Dictionary<string, List<string>>();
        static Stack<string> Stack = new Stack<string>();

        static void Main(string[] args)
        {
            // Чтение и разбор правил грамматики из файла
            string grammarFile = "..\\..\\..\\..\\..\\Tasks\\Laba3\\test1.txt";
            LoadGrammar(grammarFile);

            // Входная цепочка для анализа
            Console.WriteLine("Введите цепочку символов для анализа:");
            string inputString = Console.ReadLine();

            // Начальные параметры автомата
            string startSymbol = "E"; // Начальный символ
            Stack.Push("h0");  // Маркер дна магазина
            Stack.Push(startSymbol);  // Добавляем начальный символ грамматики в магазин

            // Запуск автомата и вывод результата
            bool isAccepted = ProcessString(inputString);
            Console.WriteLine(isAccepted ? "Цепочка допустима автоматом." : "Цепочка недопустима автоматом.");
        }

        static void LoadGrammar(string filename)
        {
            foreach (var line in File.ReadAllLines(filename))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Разделяем левую и правую части правила
                var parts = line.Split('>');
                if (parts.Length < 2) continue;

                string left = parts[0].Trim();
                string right = parts[1].Trim();

                // Разделяем альтернативные правила для правой части
                var rules = right.Split('|').Select(r => r.Trim()).ToList();

                if (!GrammarRules.ContainsKey(left))
                {
                    GrammarRules[left] = new List<string>();
                }
                GrammarRules[left].AddRange(rules);
            }
        }

        // Основной метод для обработки входной цепочки
        static bool ProcessString(string input)
        {
            int pointer = 0; // Указатель на текущую позицию во входной строке

            return ProcessConfiguration(input, ref pointer);
        }

        // Рекурсивный метод для обработки конфигурации с возвратом
        static bool ProcessConfiguration(string input, ref int pointer)
        {
            if (Stack.Count == 0)
            {
                return pointer == input.Length;
            }

            // Если стек пуст, но строка не разобрана до конца, цепочка недопустима
            if (pointer == input.Length && Stack.Count == 1 && Stack.Peek() == "h0")
            {
                return true;
            }

            string top = Stack.Pop();

            // Если верхушка магазина терминальный символ, сверяем с текущим символом входа
            if (top == "h0" || !GrammarRules.ContainsKey(top))
            {
                if (pointer < input.Length && top.ToString() == input[pointer].ToString())
                {
                    pointer++; // Переходим к следующему символу входной строки
                    return ProcessConfiguration(input, ref pointer);
                }
                else
                {
                    return false; // Несоответствие, цепочка недопустима
                }
            }
            else
            {
                // Если верхушка магазина - нетерминал, заменяем его на продукцию
                if (GrammarRules.ContainsKey(top))
                {
                    foreach (var production in GrammarRules[top])
                    {
                        // Сохраняем текущее состояние стека и указатель
                        var stackSnapshot = new Stack<string>(Stack.Reverse());
                        int pointerSnapshot = pointer;

                        // Записываем продукцию в магазин в обратном порядке
                        for (int i = production.Length - 1; i >= 0; i--)
                        {
                            Stack.Push(production[i].ToString());
                        }

                        // Рекурсивный вызов для проверки текущей конфигурации
                        if (ProcessConfiguration(input, ref pointer))
                        {
                            return true;
                        }

                        // Восстанавливаем состояние, если переход неуспешен
                        Stack = new Stack<string>(stackSnapshot.Reverse());
                        pointer = pointerSnapshot;
                    }
                }
            }

            return false;
        }
    }
}