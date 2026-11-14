using System.Collections.Generic;

namespace Interpreter.Lox;




public class Parser {
     private class ParseError : Exception
    {
    }
  private readonly List<Token> tokens;
  private int current = 0;

  public Parser(List<Token> tokens) {
    this.tokens = tokens;
  }
 public List<Stmt> parse()
{
    List<Stmt> statements = new List<Stmt>();

    while (!isAtEnd())
    {
        Stmt? statement = declaration();

        if (statement != null)
        {
            statements.Add(statement);
        }
    }

    return statements;
}

   private Expr expression() {
    return assignment();
  }
  private Stmt? declaration()
{
    try
    {
        if (Match(TokenType.VAR))
            return varDeclaration();

        return statement();
    }
    catch (ParseError)
    {
        synchronize();
        return null;
    }
}
  private Stmt statement() {
    if (Match(TokenType.FOR)) return forStatement();
    if (Match(TokenType.IF)) return ifStatement();
    if (Match(TokenType.PRINT)) return printStatement();
    if (Match(TokenType.WHILE)) return whileStatement();
    if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(block());
    return expressionStatement();
  }
  private Stmt forStatement() {
    consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

    Stmt? initializer;
    if (Match(TokenType.SEMICOLON)) {
      initializer = null;
    } else if (Match(TokenType.VAR)) {
      initializer = varDeclaration();
    } else {
      initializer = expressionStatement();
    }
    Expr? condition = null;
    if (!check(TokenType.SEMICOLON)) {
      condition = expression();
    }
    consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");
    Expr? increment = null;
    if (!check(TokenType.RIGHT_PAREN)) {
      increment = expression();
    }
    consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");
    Stmt body = statement();
    if (increment != null)
    {
    body = new Stmt.Block(
        new List<Stmt>
        {
            body,
            new Stmt.Expression(increment)
        }
    );
    }
    if (condition == null) condition = new Expr.Literal(true);
    body = new Stmt.While(condition, body);
    if (initializer != null)
    {
    body = new Stmt.Block(new List<Stmt>
        {
            initializer,
            body
        }
    );
    }
    return body;
  }
  private Stmt ifStatement()
{
    consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");

    Expr condition = expression();

    consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

    Stmt thenBranch = statement();
    Stmt? elseBranch = null;

    if (Match(TokenType.ELSE))
    {
        elseBranch = statement();
    }

    return new Stmt.If(condition, thenBranch, elseBranch);
}
  private Stmt printStatement() {
    Expr value = expression();
    consume(TokenType.SEMICOLON, "Expect ';' after value.");
    return new Stmt.Print(value);
  }
  private Stmt varDeclaration()
{
    Token name = consume(TokenType.IDENTIFIER, "Expect variable name.");

    Expr? initializer = null;

    if (Match(TokenType.EQUAL))
    {
        initializer = expression();
    }

    consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");

    return new Stmt.Var(name, initializer);
}
private Stmt whileStatement() {
    consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
    Expr condition = expression();
    consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
    Stmt body = statement();

    return new Stmt.While(condition, body);
  }
  private Stmt expressionStatement() {
    Expr expr = expression();
    consume(TokenType.SEMICOLON, "Expect ';' after expression.");
    return new Stmt.Expression(expr);
  }
   private List<Stmt> block() {
    List<Stmt> statements = new List<Stmt>();

    while (!check(TokenType.RIGHT_BRACE) && !isAtEnd()) {
      Stmt? statement = declaration();

  if (statement != null)
  {
    statements.Add(statement);
  }
  }

    consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
    return statements;
  }
  private Expr assignment()
  {
    Expr expr = or();

    if (Match(TokenType.EQUAL))
    {
        Token equals = previous();
        Expr value = assignment();

        if (expr is Expr.Variable variable)
        {
            Token name = variable.Name;
            return new Expr.Assign(name, value);
        }

        error(equals, "Invalid assignment target.");
    }

    return expr;
  }
   private Expr or() {
    Expr expr = and();

    while (Match(TokenType.OR)) {
      Token op = previous();
      Expr right = and();
      expr = new Expr.Logical(expr, op, right);
    }

    return expr;
  }
   private Expr and() {
    Expr expr = equality();

    while (Match(TokenType.AND)) {
      Token op = previous();
      Expr right = equality();
      expr = new Expr.Logical(expr, op, right);
    }

    return expr;
  }
    private Expr equality() {
    Expr expr = comparison();

    while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL)) {
      Token op = previous();
      Expr right = comparison();
      expr = new Expr.Binary(expr, op, right);
    }

    return expr;
  }
   private Expr comparison() {
    Expr expr = term();

    while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)) {
      Token op = previous();
      Expr right = term();
      expr = new Expr.Binary(expr, op, right);
    }

    return expr;
  }
  private Expr term() {
    Expr expr = factor();

    while (Match(TokenType.MINUS, TokenType.PLUS)) {
      Token op = previous();
      Expr right = factor();
      expr = new Expr.Binary(expr, op, right);
    }

    return expr;
  }
  private Expr factor() {
    Expr expr = unary();

    while (Match(TokenType.SLASH, TokenType.STAR)) {
      Token op = previous();
      Expr right = unary();
      expr = new Expr.Binary(expr, op, right);
    }

    return expr;
  }
   private Expr unary() {
    if (Match(TokenType.BANG, TokenType.MINUS)) {
      Token op = previous();
      Expr right = unary();
      return new Expr.Unary(op, right);
    }

    return primary();
  }
  private Expr primary() {
    if (Match(TokenType.FALSE)) return new Expr.Literal(false);
    if (Match(TokenType.TRUE)) return new Expr.Literal(true);
    if (Match(TokenType.NIL)) return new Expr.Literal(null);

    if (Match(TokenType.NUMBER, TokenType.STRING)) {
      return new Expr.Literal(previous().Literal);
    }
    if (Match(TokenType.IDENTIFIER)) {
      return new Expr.Variable(previous());
    }

    if (Match(TokenType.LEFT_PAREN)) {
      Expr expr = expression();
      consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
      return new Expr.Grouping(expr);
    }
    throw error(peek(), "Expect expression.");
  }
   private bool Match(params TokenType[] types) {
    foreach (TokenType type in types) {
      if (check(type)) {
        advance();
        return true;
      }
    }

    return false;
  }
  private Token consume(TokenType type, string message) {
    if (check(type)) return advance();

    throw error(peek(), message);
  }
  private bool check(TokenType type) {
    if (isAtEnd()) return false;
    return peek().Type == type;
  }
   private Token advance() {
    if (!isAtEnd()) current++;
    return previous();
  }
  private bool isAtEnd() {
    return peek().Type == TokenType.EOF;
  }

  private Token peek() {
    return tokens[current];
  }

  private Token previous() {
    return tokens[current - 1];
  }
  private ParseError error(Token token, String message) {
    Lox.Error(token, message);
    return new ParseError();
  }
  private void synchronize()
{
    advance();

    while (!isAtEnd())
    {
        if (previous().Type == TokenType.SEMICOLON)
            return;

        switch (peek().Type)
        {
            case TokenType.CLASS:
            case TokenType.FUN:
            case TokenType.VAR:
            case TokenType.FOR:
            case TokenType.IF:
            case TokenType.WHILE:
            case TokenType.PRINT:
            case TokenType.RETURN:
                return;
        }

        advance();
    }
}
}