namespace GamerScript.Core;

public abstract record AstNode;

public abstract record Expression : AstNode;

public abstract record Statement : AstNode;

// Expressions

public record BinaryExpression(Expression Left, Token Operator, Expression Right) : Expression;

public record UnaryExpression(Token Operator, Expression Right) : Expression;

public record LiteralExpression(Token Token) : Expression;

public record VariableExpression(Token Name) : Expression;

public record AssignmentExpression(Token Name, Expression Value) : Expression;

public record CallExpression(Token Identifier, IList<Expression> Arguments) : Expression;

public record ExpressionStatement(Expression Expression) : Statement;

public record VariableStatement(Token Name, Expression? Initializer) : Statement;

// Statements

public record BlockStatement(IList<Statement> Statements) : Statement;

public record IfStatement(
    Expression Condition,
    Statement ThenBranch,
    List<(Expression condition, Statement thenBranch)> ElifBranches,
    Statement? ElseBranch) : Statement;

public record WhileStatement(Expression Condition, Statement Body) : Statement;

public record FunctionStatement(Token Name, IList<Token> Parameters, BlockStatement Body) : Statement;

public record ReturnStatement(Expression? Value) : Statement;

public record AssignmentStatement(VariableExpression Variable, Expression? Value) : Statement;

public record EndOfFileStatement : Statement;