namespace GamerScript.Core;

public interface IVisitor<out T>
{
    T VisitBinaryExpression(BinaryExpression expr);
    T VisitUnaryExpression(UnaryExpression expr);
    T VisitLiteralExpression(LiteralExpression expr);
    T VisitVariableExpression(VariableExpression expr);
    T VisitCallExpression(CallExpression expr);
    T VisitExpressionStatement(ExpressionStatement stmt);
    T VisitVariableStatement(VariableStatement stmt);
    T VisitBlockStatement(BlockStatement stmt);
    T VisitIfStatement(IfStatement stmt);
    T VisitWhileStatement(WhileStatement stmt);
    T VisitFunctionStatement(FunctionStatement stmt);
    T VisitReturnStatement(ReturnStatement stmt);
    T VisitAssignmentStatement(AssignmentStatement stmt);
    T VisitEndOfFileStatement(EndOfFileStatement stmt);
}

public abstract record AstNode
{
    public abstract T Accept<T>(IVisitor<T> visitor);
}

public abstract record Expression : AstNode;

public abstract record Statement : AstNode;

public record BinaryExpression(Expression Left, Token Operator, Expression Right) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitBinaryExpression(this);
    }
}

public record UnaryExpression(Token Operator, Expression Right) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitUnaryExpression(this);
    }
}

public record LiteralExpression(Token Token) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitLiteralExpression(this);
    }
}

public record VariableExpression(Token Name) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitVariableExpression(this);
    }
}

public record CallExpression(Token Identifier, IList<Expression> Arguments) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitCallExpression(this);
    }
}

public record ExpressionStatement(Expression Expression) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitExpressionStatement(this);
    }
}

public record VariableStatement(Token Name, Expression? Initializer) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitVariableStatement(this);
    }
}

public record BlockStatement(IList<Statement> Statements) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitBlockStatement(this);
    }
}

public record IfStatement(
    Expression Condition,
    Statement ThenBranch,
    List<(Expression condition, Statement thenBranch)> ElifBranches,
    Statement? ElseBranch) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitIfStatement(this);
    }
}

public record WhileStatement(Expression Condition, Statement Body) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitWhileStatement(this);
    }
}

public record FunctionStatement(Token Name, IList<Token> Parameters, BlockStatement Body) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitFunctionStatement(this);
    }
}

public record ReturnStatement(Expression? Value) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitReturnStatement(this);
    }
}

public record AssignmentStatement(VariableExpression Variable, Expression? Value) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitAssignmentStatement(this);
    }
}

public record EndOfFileStatement : Statement
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitEndOfFileStatement(this);
    }
}