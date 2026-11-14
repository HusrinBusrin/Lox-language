using System;
using System.Collections.Generic;
using System.IO;

namespace Interpreter.Lox;

public class Lox {
    private static bool hadError = false;
    public static bool hadRuntimeError = false;
    private static readonly Interpreter interpreter = new Interpreter();
    public static void Main(string[] args){
    if (args.Length > 1) {
      Console.WriteLine("Usage: jlox [script]");
      Environment.Exit(64); 
    } else if (args.Length == 1) {
      RunFile(args[0]);
    } else {
      RunPrompt();
    }
  }

  
  private static void RunFile(string path)
{
    byte[] bytes = File.ReadAllBytes(path);
    Run(System.Text.Encoding.Default.GetString(bytes));
    if (hadRuntimeError) Environment.Exit(70);
}

private static void RunPrompt()
{
    for (;;)
    {
        Console.Write("> ");

        string? line = Console.ReadLine();

        if (line == null) break;

        Run(line);
        hadError = false;
    }
}

  private static void Run(String source) {
    if (hadError){
        Environment.Exit(65);
    }
    Scanner scanner = new Scanner(source);
    List<Token> tokens = scanner.ScanTokens();

    Parser parser = new Parser(tokens);
    List<Stmt> statements = parser.parse();

    // Stop if there was a syntax error.
    if (hadError) return;

    interpreter.Interpret(statements);
  }

  public static void Error(int line, string message) {
    Report(line, "", message);
  }

  private static void Report(int line, string where,
                             string message) {
    Console.Error.WriteLine(
        $"[line {line}] Error{where}: {message}");
    hadError = true;
  }
  public static void Error(Token token, String message) {
    if (token.Type == TokenType.EOF) {
      Report(token.Line, " at end", message);
    } else {
      Report(token.Line, " at '" + token.Lexeme + "'", message);
    }
  }
  public static void RuntimeError(RuntimeError error)
  {
    Console.Error.WriteLine(
        error.Message + "\n[line " + error.Token.Line + "]");

    hadRuntimeError = true;
  }

}