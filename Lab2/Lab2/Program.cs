using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;

class FiniteStateMachine
{
    private Dictionary<string, Dictionary<char, HashSet<string>>> transitions;
    private string startState;
    private HashSet<string> finalStates;

    public FiniteStateMachine(string filePath)
    {
        transitions = new Dictionary<string, Dictionary<char, HashSet<string>>>();
        finalStates = new HashSet<string>();
        startState = "q0";
        LoadTransitions(filePath);
    }

    private void LoadTransitions(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            var parts = line.Split(new[] { ',', '=', 'q' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                if (line.Contains("=="))
                {
                    var bufLst = parts.ToList();
                    bufLst.Insert(1, "=");
                    parts = bufLst.ToArray();
                }
                else if (line.Contains(",,"))
                {
                    var bufLst = parts.ToList();
                    bufLst.Insert(1, ",");
                    parts = bufLst.ToArray();
                }
            }

            var currentState = "q" + parts[0];
            var symbol = parts[1][0];
            var nextState = parts[2].StartsWith("f") ? "f" + parts[2].Substring(1) : "q" + parts[2];

            if (!transitions.ContainsKey(currentState))
                transitions[currentState] = new Dictionary<char, HashSet<string>>();

            if (!transitions[currentState].ContainsKey(symbol))
                transitions[currentState][symbol] = new HashSet<string>();

            transitions[currentState][symbol].Add(nextState);

            if (nextState.StartsWith("f"))
                finalStates.Add(nextState);
        }
    }

    public bool IsDeterministic()
    {
        foreach (var state in transitions)
            foreach (var transition in state.Value)
                if (transition.Value.Count > 1)
                    return false;
        return true;
    }

    public bool AnalyzeString(string input)
    {
        var currentStates = new HashSet<string> { startState };

        foreach (var symbol in input)
        {
            var nextStates = new HashSet<string>();

            foreach (var state in currentStates)
            {
                if (transitions.ContainsKey(state) && transitions[state].ContainsKey(symbol))
                    nextStates.UnionWith(transitions[state][symbol]);
            }

            if (nextStates.Count == 0)
                return false; // Нет доступных переходов по символу

            currentStates = nextStates;
        }

        return currentStates.Any(state => finalStates.Contains(state));
    }

    public void DisplayTransitionTable()
    {
        Console.WriteLine("Таблица переходов:");
        foreach (var state in transitions)
        {
            foreach (var transition in state.Value)
            {
                Console.WriteLine($"{state.Key},{transition.Key}={string.Join(",", transition.Value)}");
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        string filePath = "..\\..\\..\\..\\..\\Tasks\\Laba2\\var3_nd.txt";

        FiniteStateMachine fsm = new FiniteStateMachine(filePath);

        Console.WriteLine("Введите строку для анализа:");
        string input = Console.ReadLine();

        if (fsm.IsDeterministic())
        {
            Console.WriteLine("Автомат детерминирован.");
        }
        else
        {
            Console.WriteLine("Автомат недетерминирован");
            //fsm.DisplayTransitionTable();
        }

        if (fsm.AnalyzeString(input))
        {
            Console.WriteLine("Строка допустима автоматом.");
        }
        else
        {
            Console.WriteLine("Строка не допускается автоматом.");
        }
    }
}


//string filePath = "..\\..\\..\\..\\..\\Tasks\\Laba2\\var1.txt";