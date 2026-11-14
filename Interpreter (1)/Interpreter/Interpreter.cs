namespace Interpreter.Lox;

public class Interpreter : Expr.Visitor<object?>, Stmt.Visitor<object?>
{
    private Environment1 environment = new();
    public void Interpret(List<Stmt> statements)
    {
    try
    {
        foreach (Stmt statement in statements)
        {
            execute(statement);
        }
    }
    catch (RuntimeError error)
    {
        Lox.RuntimeError(error);
    }
    }
    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }
    public object? VisitLogicalExpr(Expr.Logical expr)
{
    object? left = evaluate(expr.Left);

    if (expr.Operator.Type == TokenType.OR)
    {
        if (isTruthy(left))
            return left;
    }
    else
    {
        if (!isTruthy(left))
            return left;
    }

    return evaluate(expr.Right);
}
    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        return evaluate(expr.Expression);
    }
    public object? VisitUnaryExpr(Expr.Unary expr)
    {
    object? right = evaluate(expr.Right);

    switch (expr.Operator.Type)
    {
        case TokenType.BANG:
            return !isTruthy(right);
        case TokenType.MINUS:
            return -(double)right!;

    }
    

    // Unreachable.
    return null;
    }
    public object? VisitVariableExpr(Expr.Variable expr)
    {
    return environment.Get(expr.Name);
    }   
    private void checkNumberOperand(Token op, object? operand)
    {
    if (operand is double) return;

    throw new RuntimeError(op, "Operand must be a number.");
    }
    private void checkNumberOperands(Token op, object? left, object? right)
    {
    if (left is double && right is double) return;

    throw new RuntimeError(op, "Operands must be numbers.");
    }
    private bool isTruthy(object? ob) {
    if (ob == null) return false;
    if (ob is bool) return (bool)ob;
    return true;
    }
    private bool isEqual(object? a, object? b) {
    if (a == null && b == null) return true;
    if (a == null) return false;

    return a.Equals(b);
  }
  private string Stringify(object? value)
    {
    if (value == null) return "nil";

    if (value is double)
    {
        string text = value.ToString()!;

        if (text.EndsWith(".0"))
        {
            text = text.Substring(0, text.Length - 2);
        }

        return text;
    }

    return value.ToString()!;
    }
    private object? evaluate(Expr expr) {
        return expr.Accept(this);
    }
    private void execute(Stmt stmt) {
    stmt.Accept(this);
    }
    private void ExecuteBlock(
    List<Stmt> statements,
    Environment1 environment)
    {
    Environment1 previous = this.environment;

    try
    {
        this.environment = environment;

        foreach (Stmt statement in statements)
        {
            execute(statement);
        }
    }
    finally
    {
        this.environment = previous;
    }
    }
    public object? VisitBlockStmt(Stmt.Block stmt)
    {
    ExecuteBlock(stmt.Statements, new Environment1(environment));
    return null;
    }
    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
    evaluate(stmt.expr);
    return null;
    }
    public object? VisitIfStmt(Stmt.If stmt)
    {
    if (isTruthy(evaluate(stmt.Condition)))
    {
        execute(stmt.ThenBranch);
    }
    else if (stmt.ElseBranch != null)
    {
        execute(stmt.ElseBranch);
    }

    return null;
    }
    public object? VisitPrintStmt(Stmt.Print stmt) {
    object? value = evaluate(stmt.expr);
    Console.WriteLine(Stringify(value));
    return null;
    }
    public object? VisitAssignExpr(Expr.Assign expr)
    {
    object? value = evaluate(expr.Value);

    environment.Assign(expr.Name, value);

    return value;
    }   
    public object? VisitVarStmt(Stmt.Var stmt)
    {   
    object? value = null;

    if (stmt.Initializer != null)
    {
        value = evaluate(stmt.Initializer);
    }

    environment.Define(stmt.Name.Lexeme, value);

    return null;
    }
    public object? VisitWhileStmt(Stmt.While stmt)
    {
    while (isTruthy(evaluate(stmt.Condition)))
    {
        execute(stmt.Body);
    }

    return null;
    }
    public object? VisitBinaryExpr(Expr.Binary expr)
    {
    object? left = evaluate(expr.Left);
    object? right = evaluate(expr.Right);

    switch (expr.Operator.Type)
    {
        case TokenType.GREATER:
            checkNumberOperands(expr.Operator, left, right);
            return (double)left! > (double)right!;
        case TokenType.GREATER_EQUAL:
            checkNumberOperands(expr.Operator, left, right);
            return (double)left! >= (double)right!;
        case TokenType.LESS:
            checkNumberOperands(expr.Operator, left, right);
            return (double)left! < (double)right!;
        case TokenType.LESS_EQUAL:
            checkNumberOperands(expr.Operator, left, right);
            return (double)left! <= (double)right!;
        case TokenType.MINUS:
            checkNumberOperands(expr.Operator, left, right);
            return (double)left! - (double)right!;
        case TokenType.PLUS:
        if (left is double && right is double)
        {
            return (double)left + (double)right;
        }

        if (left is string && right is string)
        {
            return (string)left + (string)right;
        }

        throw new RuntimeError(expr.Operator,
            "Operands must be two numbers or two strings.");
        case TokenType.SLASH:
            checkNumberOperands(expr.Operator, left, right);
            return (double)left! / (double)right!;

        case TokenType.STAR:
            checkNumberOperands(expr.Operator, left, right);
            return (double)left! * (double)right!;
        case TokenType.BANG_EQUAL: return !isEqual(left, right);
        case TokenType.EQUAL_EQUAL: return isEqual(left, right);
    }

    // Unreachable.
    return null;
    }
}