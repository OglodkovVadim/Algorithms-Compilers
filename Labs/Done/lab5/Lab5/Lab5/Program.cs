using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleInterpreter
{
    public class Program
    {
        static void Main(string[] args)
        {
            var code = File.ReadAllText("../../Resources/Code.txt");
            var lstCode = code.Split('\n');
            
            Console.WriteLine("Code.txt");
            for (int i = 0; i < lstCode.Length; i++)
                Console.WriteLine($"{i + 1}.  {lstCode[i]}");
            Console.WriteLine();

            var interpreter = new Interpreter();
            interpreter.Execute(code);
            Console.ReadLine();
        }
    }

    public class Interpreter
    {
        public void Execute(string code)
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize(code).Where(t => t.Type != TokenType.Space);
            var context = new ExecutionContext(tokens);

            try
            {
                while (context.Tokens.Count > 0)
                {
                    ExecuteStatement(context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Execution Error: {ex.Message}");
            }
        }

        private void ExecuteStatement(ExecutionContext context)
        {
            if (context.Tokens.Count == 0) return;

            var currentToken = context.Tokens.Pop();

            switch (currentToken.Value)
            {
                case "if":
                    ExecuteIf(context);
                    break;
                case "for":
                    ExecuteFor(context);
                    break;
                case "print":
                    ExecutePrint(context);
                    break;
                case "scan":
                    ExecuteScan(context);
                    break;
                default:
                    if (currentToken.Type == TokenType.Identifier)
                    {
                        context.Tokens.Push(currentToken);
                        ExecuteAssignment(context);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unexpected token: {currentToken.Value}");
                    }
                    break;
            }
        }

        private void ExecuteAssignment(ExecutionContext context)
        {
            EnsureTokensExist(context);
            var identifier = context.Tokens.Pop();
            var equalsToken = context.Tokens.Pop();

            if (equalsToken.Type != TokenType.Equal)
                throw new InvalidOperationException($"Expected '=', found {equalsToken.Value}");

            var value = ExpressionEvaluator.EvaluateCondition(context);
            context.Variables[identifier.Value] = (int)value;
        }

        private void ExecuteScan(ExecutionContext context)
        {
            EnsureTokensExist(context);
            var identifier = context.Tokens.Pop();

            if (identifier.Type != TokenType.Identifier)
                throw new InvalidOperationException($"Expected identifier, found {identifier.Value}");

            Console.Write($"Enter {identifier.Value}: ");
            if (!int.TryParse(Console.ReadLine(), out var inputValue))
                throw new InvalidOperationException("Invalid input. Expected an integer.");

            context.Variables[identifier.Value] = inputValue;

            var semicolon = context.Tokens.Pop();
            if (semicolon.Value != ";")
                throw new InvalidOperationException($"Expected ';', found {semicolon.Value}");
        }

        private void ExecutePrint(ExecutionContext context)
        {
            var output = PrintHelper.Parse(context);
            Console.WriteLine(output);
        }

        private void ExecuteFor(ExecutionContext context)
        {
            EnsureTokensExist(context);
            var variable = context.Tokens.Pop();
            var equalsToken = context.Tokens.Pop();

            if (equalsToken.Type != TokenType.Equal)
                throw new InvalidOperationException($"Expected '=', found {equalsToken.Value}");

            var startValue = ExpressionEvaluator.EvaluateCondition(context);
            var toToken = context.Tokens.Pop();

            if (toToken.Value != "to")
                throw new InvalidOperationException($"Expected 'to', found {toToken.Value}");

            var endValue = ExpressionEvaluator.EvaluateCondition(context);

            if (context.Tokens.Pop().Value != "{")
                throw new InvalidOperationException("Expected '{' to start loop body.");

            var loopBody = ExtractBlock(context);

            for (context.Variables[variable.Value] = (int)startValue; context.Variables[variable.Value] <= endValue; context.Variables[variable.Value]++)
            {
                var loopContext = new ExecutionContext(loopBody, context.Variables);
                while (loopContext.Tokens.Count > 0)
                {
                    ExecuteStatement(loopContext);
                }
            }
        }

        private void ExecuteIf(ExecutionContext context)
        {
            var condition = ExpressionEvaluator.EvaluateBoolCondition(context);

            if (context.Tokens.Pop().Value != "{")
                throw new InvalidOperationException("Expected '{' to start if body.");

            var ifBody = ExtractBlock(context);

            List<Token> elseBody = null;
            if (context.Tokens.Count > 0 && context.Tokens.Peek().Value == "else")
            {
                context.Tokens.Pop();

                if (context.Tokens.Pop().Value != "{")
                    throw new InvalidOperationException("Expected '{' to start else body.");

                elseBody = ExtractBlock(context);
            }

            var executionBody = condition ? ifBody : elseBody;

            if (executionBody != null)
            {
                var conditionalContext = new ExecutionContext(executionBody, context.Variables);
                while (conditionalContext.Tokens.Count > 0)
                {
                    ExecuteStatement(conditionalContext);
                }
            }
        }

        private List<Token> ExtractBlock(ExecutionContext context)
        {
            var blockTokens = new List<Token>();
            var openBraces = 1;

            while (openBraces > 0 && context.Tokens.Count > 0)
            {
                var token = context.Tokens.Pop();

                if (token.Value == "{") openBraces++;
                if (token.Value == "}") openBraces--;

                if (openBraces > 0) blockTokens.Add(token);
            }

            if (openBraces > 0)
                throw new InvalidOperationException("Missing closing '}' in block.");

            //blockTokens.Reverse();
            return blockTokens;
        }

        private void EnsureTokensExist(ExecutionContext context)
        {
            if (context.Tokens.Count == 0)
                throw new InvalidOperationException("Unexpected end of input.");
        }
    }

    public class ExecutionContext
    {
        public Dictionary<string, int> Variables { get; set; }
        public Stack<Token> Tokens { get; set; }

        public ExecutionContext(IEnumerable<Token> tokens, Dictionary<string, int> variables = null)
        {
            Tokens = new Stack<Token>(tokens.Reverse());
            Variables = variables ?? new Dictionary<string, int>();
        }
    }

    public static class ExpressionEvaluator
    {
        public static int Evaluate(ExecutionContext context, out Token curToken)
        {
            curToken = context.Tokens.Pop();

            if (curToken.Type == TokenType.Number)
                return int.Parse(curToken.Value);

            if (curToken.Type == TokenType.Identifier && context.Variables.ContainsKey(curToken.Value))
                return context.Variables[curToken.Value];

            throw new InvalidOperationException($"Invalid expression: {curToken.Value}");
        }

        public static bool EvaluateBoolCondition(ExecutionContext context)
        {
            var left = Evaluate(context, out Token curTokenLeft);
            var op = context.Tokens.Pop();
            var right = Evaluate(context, out Token curTokenRight);

            if (op.Type == TokenType.Operator)
                return op.Value switch
                {
                    "==" => left == right,
                    "!=" => left != right,
                    "<" => left < right,
                    ">" => left > right,
                    "<=" => left <= right,
                    ">=" => left >= right,
                    _ => throw new InvalidOperationException($"Invalid operator: {op.Value}")
                };

            context.Tokens.Push(curTokenRight);
            context.Tokens.Push(op);
            context.Tokens.Push(curTokenLeft);

            throw new InvalidOperationException($"Invalid operator: {op.Value}");
        }
        
        public static double EvaluateCondition(ExecutionContext context)
        {
            if (context.Tokens.Count > 2)
            {
                var left = Evaluate(context, out Token curTokenLeft);
                var op = context.Tokens.Pop();

                if (op.Type == TokenType.Operator)
                {
                    var right = Evaluate(context, out Token curTokenRight);
                    return op.Value switch
                    {
                        "*" => left * right,
                        "+" => left + right,
                        "-" => left - right,
                        "/" => left / right,
                        _ => throw new InvalidOperationException($"Invalid operator: {op.Value}")
                    };
                }

                context.Tokens.Push(op);
                context.Tokens.Push(curTokenLeft);
            }

            return Evaluate(context, out Token curToken);
        }
    }

    public static class PrintHelper
    {
        public static string Parse(ExecutionContext context)
        {
            var token = context.Tokens.Pop();

            if (token.Type == TokenType.String)
            {
                var bufStr = token.Value.Trim('"');
                var checkToken = context.Tokens.Pop();
                if (checkToken.Value == ",")
                    bufStr += ExpressionEvaluator.EvaluateCondition(context);
                else
                    context.Tokens.Push(checkToken);
                return bufStr;
            }

            context.Tokens.Push(token);
            return ExpressionEvaluator.EvaluateCondition(context).ToString();
        }
    }

    public class Tokenizer
    {
        private static readonly Regex TokenPattern = new Regex(
            @"(?<Number>\d+)|(?<Identifier>[a-zA-Z_][a-zA-Z0-9_]*)|(?<String>\"".*?\"")|(?<Equal>=)|(?<Operator>[+\-*/<>=!]+)|(?<Punctuation>[{}();,])|(?<Space>\s+)");

        public IEnumerable<Token> Tokenize(string code)
        {
            var matches = TokenPattern.Matches(code);

            foreach (Match match in matches)
            {
                if (match.Groups["Number"].Success)
                    yield return new Token(match.Value, TokenType.Number);
                else if (match.Groups["Identifier"].Success)
                    yield return new Token(match.Value, TokenType.Identifier);
                else if (match.Groups["String"].Success)
                    yield return new Token(match.Value, TokenType.String);
                else if (match.Groups["Equal"].Success)
                    yield return new Token(match.Value, TokenType.Equal);
                else if (match.Groups["Operator"].Success)
                    yield return new Token(match.Value, TokenType.Operator);
                else if (match.Groups["Punctuation"].Success)
                    yield return new Token(match.Value, TokenType.Punctuation);
                else if (match.Groups["Space"].Success)
                    yield return new Token(match.Value, TokenType.Space);
            }
        }
    }

    public class Token
    {
        public string Value { get; }
        public TokenType Type { get; }

        public Token(string value, TokenType type)
        {
            Value = value;
            Type = type;
        }
    }

    public enum TokenType
    {
        Identifier,
        Keyword,
        Operator,
        Number,
        String,
        Equal,
        Punctuation,
        Space
    }
}
