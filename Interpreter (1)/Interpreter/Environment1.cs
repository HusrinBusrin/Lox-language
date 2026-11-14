using System.Collections.Generic;

namespace Interpreter.Lox;

public class Environment1
{
    private readonly Dictionary<string, object?> values =
        new Dictionary<string, object?>();
    public readonly Environment1? enclosing;

     public Environment1() {
        enclosing = null;
    }

  public Environment1(Environment1 enclosing) {
    this.enclosing = enclosing;
  }
    public object? Get(Token name)
    {
    if (values.ContainsKey(name.Lexeme))
    {
        return values[name.Lexeme];
    }
    if (enclosing != null) return enclosing.Get(name);

    throw new RuntimeError(name,
        $"Undefined variable '{name.Lexeme}'.");
    }
    public void Assign(Token name, object? value)
    {
    if (values.ContainsKey(name.Lexeme))
    {
        values[name.Lexeme] = value;
        return;
    }

    if (enclosing != null)
    {
        enclosing.Assign(name, value);
        return;
    }

    throw new RuntimeError(name,
        $"Undefined variable '{name.Lexeme}'.");
    }
    public void Define(string name, object? value)
    {
    values[name] = value;
    }

}