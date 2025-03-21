using System.Globalization;

namespace GamerScript.Core;

public class Interpreter : IVisitor<object?>
{
    private readonly Dictionary<string, FunctionStatement> _functions = new();
    private readonly Dictionary<string, Func<CallExpression, object?>> _stdFunctions;
    private readonly Dictionary<string, object?> _variables = new();

    public Interpreter(TextWriter? stdout = null)
    {
        var outStream = stdout ?? Console.Out;
        _stdFunctions = new Dictionary<string, Func<CallExpression, object?>>
        {
            {
                "taunt", expr =>
                {
                    var strExpr = expr.Arguments.FirstOrDefault();
                    var str = strExpr is null ? "" : Evaluate(strExpr);
                    var newLineExpr = expr.Arguments.ElementAtOrDefault(1);
                    var newLine = newLineExpr is null ? true : Evaluate(newLineExpr);
                    if (IsTruthy(newLine))
                        outStream.WriteLine(str);
                    else
                        outStream.Write(str);
                    if (outStream == Console.Out)
                        outStream.Flush();
                    return null;
                }
            },
            {
                "quest", expr =>
                {
                    if (expr.Arguments.FirstOrDefault() is null)
                        return Console.ReadLine();
                    outStream.WriteLine(Evaluate(expr.Arguments[0]));
                    if (outStream == Console.Out)
                        outStream.Flush();
                    return Console.ReadLine();
                }
            },
            {
                "afk", expr =>
                {
                    var milliseconds = Convert.ToInt32(Evaluate(expr.Arguments[0])) * 1000;
                    Thread.Sleep(milliseconds);
                    return null;
                }
            },
            {
                "lag", expr =>
                {
                    var milliseconds = Convert.ToInt32(Evaluate(expr.Arguments[0]));
                    Thread.Sleep(milliseconds);
                    return null;
                }
            },
            {
                "stat", expr =>
                {
                    var value = Evaluate(expr.Arguments[0]);
                    return value switch
                    {
                        null => 0,
                        string strValue => double.Parse(strValue, CultureInfo.InvariantCulture),
                        bool boolValue => boolValue ? 1.0 : 0.0,
                        _ => (double)value
                    };
                }
            },
            {
                "chat", expr =>
                {
                    var value = Evaluate(expr.Arguments[0]);
                    return value?.ToString() ?? "";
                }
            },
            {
                "patch", expr =>
                {
                    var value = Evaluate(expr.Arguments[0]);
                    return IsTruthy(value);
                }
            },
            {
                "gameover", expr =>
                {
                    var exitCode = Convert.ToInt32(Evaluate(expr.Arguments[0]));
                    Environment.Exit(exitCode);
                    return null;
                }
            }
        };
    }

    public object? VisitUnaryExpression(UnaryExpression expr)
    {
        var right = Evaluate(expr.Right);
        return expr.Operator.Type switch
        {
            TokenType.Minus => -(dynamic?)right,
            _ => throw new GsException($"Unknown unary operator at line {expr.Operator.Line}.")
        };
    }

    public object VisitLiteralExpression(LiteralExpression expr)
    {
        return expr.Token.Type switch
        {
            TokenType.Number => double.Parse(expr.Token.Lexeme, CultureInfo.InvariantCulture),
            TokenType.String => expr.Token.Lexeme.Substring(1, expr.Token.Lexeme.Length - 2)
                .Replace(@"\\", "\\")
                .Replace("\\\"", "\"")
                .Replace("\\n", "\n")
                .Replace("\\t", "\t")
                .Replace("\\r", "\r"),
            TokenType.True => true,
            TokenType.False => false,
            _ => throw new GsException($"Invalid literal at line {expr.Token.Line}.")
        };
    }

    public object? VisitVariableExpression(VariableExpression expr)
    {
        if (_variables.TryGetValue(expr.Name.Lexeme, out var value))
            return value;

        throw new GsException($"Undefined variable '{expr.Name.Lexeme}' at line {expr.Name.Line}.");
    }

    public object? VisitBinaryExpression(BinaryExpression expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);
        return expr.Operator.Type switch
        {
            TokenType.Plus => (dynamic?)left + (dynamic?)right,
            TokenType.Minus => (dynamic?)left - (dynamic?)right,
            TokenType.Asterisk => (dynamic?)left * (dynamic?)right,
            TokenType.Slash => (dynamic?)left / (dynamic?)right,
            TokenType.GreaterThan => (dynamic?)left > (dynamic?)right,
            TokenType.GreaterThanOrEqual => (dynamic?)left >= (dynamic?)right,
            TokenType.LessThan => (dynamic?)left < (dynamic?)right,
            TokenType.LessThanOrEqual => (dynamic?)left <= (dynamic?)right,
            TokenType.Equals => Equals(left, right),
            _ => throw new GsException($"Unknown binary operator at line {expr.Operator.Line}.")
        };
    }

    public object? VisitCallExpression(CallExpression expr)
    {
        var functionName = expr.Identifier.Lexeme;
        if (_stdFunctions.TryGetValue(functionName, out var stdFunction))
            return stdFunction(expr);
        if (!_functions.TryGetValue(functionName, out var function))
            throw new GsException($"Undefined function '{functionName}' at line {expr.Identifier.Line}.");
        var scope = new Dictionary<string, object?>();
        for (var i = 0; i < function.Parameters.Count; i++)
            scope[function.Parameters[i].Lexeme] = Evaluate(expr.Arguments[i]);
        return ExecuteFunction(function, scope);
    }

    public object? VisitVariableStatement(VariableStatement stmt)
    {
        var value = stmt.Initializer is not null ? Evaluate(stmt.Initializer) : null;
        _variables[stmt.Name.Lexeme] = value;
        return null;
    }

    public object? VisitExpressionStatement(ExpressionStatement stmt)
    {
        Evaluate(stmt.Expression);
        return null;
    }

    public object? VisitBlockStatement(BlockStatement stmt)
    {
        foreach (var statement in stmt.Statements)
            Execute(statement);
        return null;
    }

    public object? VisitIfStatement(IfStatement stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.ThenBranch);
            return null;
        }

        var executed = false;
        foreach (var (condition, branch) in stmt.ElifBranches)
            if (IsTruthy(Evaluate(condition)))
            {
                Execute(branch);
                executed = true;
                break;
            }

        if (!executed && stmt.ElseBranch is not null)
            Execute(stmt.ElseBranch);
        return null;
    }

    public object? VisitWhileStatement(WhileStatement stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
            Execute(stmt.Body);
        return null;
    }

    public object? VisitFunctionStatement(FunctionStatement stmt)
    {
        _functions[stmt.Name.Lexeme] = stmt;
        return null;
    }

    public object VisitReturnStatement(ReturnStatement stmt)
    {
        var returnValue = stmt.Value is not null ? Evaluate(stmt.Value) : null;
        throw new ReturnException(returnValue);
    }


    public object? VisitDecrementStatement(DecrementStatement stmt)
    {
        _variables[stmt.Variable.Lexeme] = (dynamic?)_variables[stmt.Variable.Lexeme] - 1;
        return _variables[stmt.Variable.Lexeme];
    }

    public object? VisitEndOfFileStatement(EndOfFileStatement stmt)
    {
        return null;
    }

    public object? VisitAssignmentStatement(AssignmentStatement stmt)
    {
        var value = stmt.Value is not null ? Evaluate(stmt.Value) : null;
        _variables[stmt.Variable.Name.Lexeme] = value;
        return null;
    }

    public object? VisitIncrementStatement(IncrementStatement stmt)
    {
        _variables[stmt.Variable.Lexeme] = (dynamic?)_variables[stmt.Variable.Lexeme] + 1;
        return _variables[stmt.Variable.Lexeme];
    }

    public void Interpret(IEnumerable<Statement> program)
    {
        foreach (var stmt in program)
            Execute(stmt);
    }

    public void Interpret(string sourceCode)
    {
        var lexer = new Lexer(sourceCode);
        var parser = new Parser(lexer.Tokenize());
        var program = parser.Parse();
        Interpret(program);
    }

    private void Execute(AstNode stmt)
    {
        stmt.Accept(this);
    }

    private object? Evaluate(AstNode expr)
    {
        return expr.Accept(this);
    }

    private static bool IsTruthy(object? obj)
    {
        return obj switch
        {
            null => false,
            bool b => b,
            double d => d != 0,
            int i => i != 0,
            string s => s != "",
            _ => true
        };
    }

    private object? ExecuteFunction(FunctionStatement function, Dictionary<string, object?> scope)
    {
        var previousVariables = new Dictionary<string, object?>(_variables);
        try
        {
            foreach (var (key, value) in scope)
                _variables[key] = value;

            object? result = null;
            try
            {
                foreach (var statement in function.Body.Statements)
                    result = ExecuteAndReturn(statement);
            }
            catch (ReturnException ex)
            {
                result = ex.ReturnValue;
            }

            return result;
        }
        finally
        {
            _variables.Clear();
            foreach (var (key, value) in previousVariables)
                _variables[key] = value;
        }
    }


    private object? ExecuteAndReturn(AstNode stmt)
    {
        stmt.Accept(this);
        return null;
    }

    private class ReturnException(object? returnValue) : Exception
    {
        public object? ReturnValue { get; } = returnValue;
    }
}