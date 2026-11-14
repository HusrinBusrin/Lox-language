namespace Interpreter.Lox;

public abstract class Stmt
{
    public interface Visitor<R>
    {
        R VisitBlockStmt(Block stmt);
        R VisitExpressionStmt(Expression stmt);
        R VisitIfStmt(If stmt);
        R VisitPrintStmt(Print stmt);
        R VisitVarStmt(Var stmt);
        R VisitWhileStmt(While stmt);
    }

    public class Block : Stmt
    {
        public Block(List<Stmt> statements)
        {
            Statements = statements;
        }

        public List<Stmt> Statements { get; }

        public override R Accept<R>(Visitor<R> visitor)
        {
            return visitor.VisitBlockStmt(this);
        }
    }

    public class Expression : Stmt
    {
        public Expression(Expr expression)
        {
            expr = expression;
        }

        public Expr expr { get; }

        public override R Accept<R>(Visitor<R> visitor)
        {
            return visitor.VisitExpressionStmt(this);
        }
    }

    public class If : Stmt
    {
        public If(Expr condition, Stmt thenBranch, Stmt? elseBranch)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }

        public Expr Condition { get; }
        public Stmt ThenBranch { get; }
        public Stmt? ElseBranch { get; }

        public override R Accept<R>(Visitor<R> visitor)
        {
            return visitor.VisitIfStmt(this);
        }
    }

    public class Print : Stmt
    {
        public Print(Expr expression)
        {
            expr = expression;
        }

        public Expr expr { get; }

        public override R Accept<R>(Visitor<R> visitor)
        {
            return visitor.VisitPrintStmt(this);
        }
    }

    public class Var : Stmt
    {
        public Var(Token name, Expr? initializer)
        {
            Name = name;
            Initializer = initializer;
        }

        public Token Name { get; }
        public Expr? Initializer { get; }

        public override R Accept<R>(Visitor<R> visitor)
        {
            return visitor.VisitVarStmt(this);
        }
    }

    public class While : Stmt
    {
        public While(Expr condition, Stmt body)
        {
            Condition = condition;
            Body = body;
        }

        public Expr Condition { get; }
        public Stmt Body { get; }

        public override R Accept<R>(Visitor<R> visitor)
        {
            return visitor.VisitWhileStmt(this);
        }
    }

    public abstract R Accept<R>(Visitor<R> visitor);
}
