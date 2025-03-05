using System.Diagnostics;

namespace GamerScript.Core;

public class Parser(IEnumerable<Token> tokens)
{
    private readonly List<Token> _tokens = tokens
        .Where(t => t.Type != TokenType.WhiteSpace && t.Type != TokenType.Comment).ToList();

    private int _current;

    public IList<Statement> Parse()
    {
        var statements = new List<Statement>();
        while (!IsAtEnd())
            statements.Add(ParseDeclaration());

        _current = 0;
        return statements;
    }

    private Statement ParseDeclaration()
    {
        SkipNewLines();
        if (Match(TokenType.Function))
            return ParseFunction();
        return Match(TokenType.Var) ? ParseVariableDeclaration() : ParseStatement();
    }

    private VariableStatement ParseVariableDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expected variable name");
        Expression? initializer = null;
        if (Match(TokenType.Assignment))
            initializer = ParseExpression();
        ExpectNewLine("variable declaration");
        return new VariableStatement(name, initializer);
    }

    private FunctionStatement ParseFunction()
    {
        var name = Consume(TokenType.Identifier, "Expected function name");
        Consume(TokenType.LeftParen, "Expected '(' after function name");
        var parameters = new List<Token>();
        if (!Check(TokenType.RightParen))
            do
            {
                parameters.Add(Consume(TokenType.Identifier, "Expected parameter name"));
            } while (Match(TokenType.Comma));

        Consume(TokenType.RightParen, "Expected ')' after parameters");
        Consume(TokenType.LeftBrace, "Expected '{' before function body");
        var body = ParseBlock();
        return new FunctionStatement(name, parameters, body);
    }

    private BlockStatement ParseBlock()
    {
        var statements = new List<Statement>();
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            statements.Add(ParseStatement());
            SkipNewLines();
        }

        Consume(TokenType.RightBrace, "Expected '}' to end the block.");
        return new BlockStatement(statements);
    }


    private Statement ParseStatement()
    {
        SkipNewLines();
        if (Match(TokenType.While))
            return ParseWhileStatement();
        if (Match(TokenType.If))
            return ParseIfStatement();
        if (Match(TokenType.LeftBrace))
            return ParseBlock();
        if (Match(TokenType.Return))
            return ParseReturnStatement();
        if (Check(TokenType.Identifier))
            return ParseExpressionOrAssignment();
        if (Match(TokenType.Increment, TokenType.Decrement))
            return ParseIncrementOrDecrement();
        if (Check(TokenType.EndOfFile))
            return new EndOfFileStatement();
        throw new GsException($"Unexpected token: {Peek().Type} at line {Peek().Line}.");
    }

    private Statement ParseIncrementOrDecrement()
    {
        var previous = Previous();
        var token = Consume(TokenType.Identifier, "Expected identifier after increment or decrement statement");
        return previous.Type switch
        {
            TokenType.Increment => new IncrementStatement(token),
            TokenType.Decrement => new DecrementStatement(token),
            _ => throw new UnreachableException()
        };
    }

    private WhileStatement ParseWhileStatement()
    {
        var condition = ParseExpression();
        var body = ParseStatement();
        return new WhileStatement(condition, body);
    }

    private IfStatement ParseIfStatement()
    {
        var condition = ParseExpression();
        var thenBranch = ParseStatement();
        var elifBranches = new List<(Expression condition, Statement thenBranch)>();
        while (Match(TokenType.Elif))
        {
            var retryCondition = ParseExpression();
            var retryThenBranch = ParseStatement();
            elifBranches.Add((retryCondition, retryThenBranch));
        }

        Statement? elseBranch = null;
        if (Match(TokenType.Else))
            elseBranch = ParseStatement();
        return new IfStatement(condition, thenBranch, elifBranches, elseBranch);
    }

    private ReturnStatement ParseReturnStatement()
    {
        Expression? value = null;
        if (!Check(TokenType.NewLine))
            value = ParseExpression();
        Consume(TokenType.NewLine, "Expected new line after return statement");
        return new ReturnStatement(value);
    }

    private Statement ParseExpressionOrAssignment()
    {
        var expr = ParseExpression();
        switch (expr)
        {
            case VariableExpression variable when Match(TokenType.Assignment):
            {
                var value = ParseExpression();
                ExpectNewLine("assignment");
                return new AssignmentStatement(variable, value);
            }
            case CallExpression or VariableExpression:
                ExpectNewLine("expression");
                return new ExpressionStatement(expr);
            default:
                throw new GsException($"Invalid expression at line {Previous().Line}.");
        }
    }

    private void SkipNewLines()
    {
        while (Match(TokenType.NewLine)) { }
    }

    private void ExpectNewLine(string after)
    {
        try
        {
            Consume(TokenType.NewLine, $"Expected new line after {after}");
        }
        catch
        {
            Consume(TokenType.EndOfFile, $"Expected end of file after {after}");
            _current--;
        }
    }


    private Expression ParseExpression()
    {
        return ParseEquality();
    }

    private Expression ParseEquality()
    {
        var expr = ParseComparison();
        while (Match(TokenType.Equals))
        {
            var op = Previous();
            var right = ParseComparison();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    private Expression ParseComparison()
    {
        var expr = ParseTerm();
        while (Match(TokenType.GreaterThan, TokenType.GreaterThanOrEqual, TokenType.LessThan,
                   TokenType.LessThanOrEqual))
        {
            var op = Previous();
            var right = ParseTerm();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    private Expression ParseTerm()
    {
        var expr = ParseFactor();
        while (Match(TokenType.Plus, TokenType.Minus))
        {
            var op = Previous();
            var right = ParseFactor();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    private Expression ParseFactor()
    {
        var expr = ParseUnary();
        while (Match(TokenType.Asterisk, TokenType.Slash))
        {
            var op = Previous();
            var right = ParseUnary();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    private Expression ParseUnary()
    {
        if (!Match(TokenType.Minus))
            return ParsePrimary();
        var op = Previous();
        var right = ParseUnary();
        return new UnaryExpression(op, right);
    }

    private Expression ParsePrimary()
    {
        if (Match(TokenType.Number, TokenType.String, TokenType.True, TokenType.False))
            return new LiteralExpression(Previous());
        if (Match(TokenType.Identifier))
        {
            var identifier = Previous();
            return Match(TokenType.LeftParen) ? ParseCallExpression(identifier) : new VariableExpression(identifier);
        }

        throw new GsException($"Unexpected token: {Peek().Type} at line {Peek().Line}.");
    }

    private CallExpression ParseCallExpression(Token identifier)
    {
        var arguments = new List<Expression>();
        if (!Check(TokenType.RightParen))
            do
            {
                arguments.Add(ParseExpression());
            } while (Match(TokenType.Comma));

        Consume(TokenType.RightParen, "Expected ')' after arguments");
        return new CallExpression(identifier, arguments);
    }

    private bool Match(params TokenType[] types)
    {
        if (!types.Any(Check))
            return false;
        Advance();
        return true;
    }

    private bool Check(TokenType type)
    {
        return Peek().Type == type;
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EndOfFile;
    }

    private Token Peek()
    {
        return _tokens[_current];
    }

    private Token Advance()
    {
        return _tokens[_current++];
    }

    private Token Previous()
    {
        return _tokens[_current - 1];
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type))
            return Advance();
        throw new GsException(message + $" at line {Peek().Line}.");
    }
}