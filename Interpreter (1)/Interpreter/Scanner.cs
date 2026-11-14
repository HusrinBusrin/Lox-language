using System.Collections.Generic;

namespace Interpreter.Lox;

public class Scanner
{
    private readonly string source;
    private readonly List<Token> tokens = new List<Token>();
      private int start = 0;
    private int current = 0;
    private int line = 1;
    private static readonly Dictionary<string, TokenType> keywords =
    new Dictionary<string, TokenType>
    {
        { "and", TokenType.AND },
        { "class", TokenType.CLASS },
        { "else", TokenType.ELSE },
        { "false", TokenType.FALSE },
        { "for", TokenType.FOR },
        { "fun", TokenType.FUN },
        { "if", TokenType.IF },
        { "nil", TokenType.NIL },
        { "or", TokenType.OR },
        { "print", TokenType.PRINT },
        { "return", TokenType.RETURN },
        { "super", TokenType.SUPER },
        { "this", TokenType.THIS },
        { "true", TokenType.TRUE },
        { "var", TokenType.VAR },
        { "while", TokenType.WHILE }
    };
    public Scanner(string source)
    {
        this.source = source;
    }
    public  List<Token> ScanTokens() {
    while (!isAtEnd()) {
      // We are at the beginning of the next lexeme.
      start = current;
      ScanToken();
    }

    tokens.Add(new Token(TokenType.EOF, "", null, line));
    return tokens;
    }
    private bool isAtEnd() {
        return current >= source.Length;
    }
    private char Advance()
    {
        return source[current++];
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, object? literal)
    {
        string text = source.Substring(start, current - start);
        tokens.Add(new Token(type, text, literal, line));
    }
        private void ScanToken()
    {
        char c = Advance();

        switch (c)
        {
        case '(':
            AddToken(TokenType.LEFT_PAREN);
            break;
        case ')':
            AddToken(TokenType.RIGHT_PAREN);
            break;
        case '{':
            AddToken(TokenType.LEFT_BRACE);
            break;
        case '}':
            AddToken(TokenType.RIGHT_BRACE);
            break;
        case ',':
            AddToken(TokenType.COMMA);
            break;
        case '.':
            AddToken(TokenType.DOT);
            break;
        case '-':
            AddToken(TokenType.MINUS);
            break;
        case '+':
            AddToken(TokenType.PLUS);
            break;
        case ';':
            AddToken(TokenType.SEMICOLON);
            break;
        case '*':
            AddToken(TokenType.STAR);
            break;
        case '!':
            AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
        break;
        case '=':
            AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
            break;

        case '<':
            AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
            break;

        case '>':
            AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
            break;
        case '/':
            if (Match('/')) {
            // A comment goes until the end of the line.
            while (Peek() != '\n' && !isAtEnd()) Advance();
        } else {
          AddToken(TokenType.SLASH);
        }
            break;
        
        case ' ':
        case '\r':
        case '\t':
        // Ignore whitespace.
            break;

        case '\n':
            line++;
            break;
        case '"': ScanString(); break;
        default:
        if (isDigit(c)) {
          number();
        } else if (isAlpha(c)) {
          identifier();
        }  else {
          Lox.Error(line, "Unexpected character.");
        }
        break;
        }
    }
    // private void identifier() {
    // while (isAlphaNumeric(Peek())) Advance();
    // string text = source.Substring(start, current - start);
    // TokenType? type = keywords.GetValueOrDefault(text);

    // if (type == null)
    //     type = TokenType.IDENTIFIER;

    // AddToken(type.Value);
    // AddToken(TokenType.IDENTIFIER);
    // }
    private void identifier()
{
    while (isAlphaNumeric(Peek()))
        Advance();

    string text = source.Substring(start, current - start);

    TokenType type = keywords.GetValueOrDefault(
        text, TokenType.IDENTIFIER
    );

    AddToken(type);
}
    private void number() {
    while (isDigit(Peek())) Advance();
    
    // Look for a fractional part.
    if (Peek() == '.' && isDigit(PeekNext())) {
      // Consume the "."
      Advance();

      while (isDigit(Peek())) Advance();
    }

    AddToken(TokenType.NUMBER, double.Parse(source.Substring(start, current - start)));
    }
    private void ScanString()
    {
    while (Peek() != '"' && !isAtEnd())
    {
        if (Peek() == '\n')
            line++;

        Advance();
    }

    if (isAtEnd())
    {
        Lox.Error(line, "Unterminated string.");
        return;
    }

    // The closing ".
    Advance();

    // Trim the surrounding quotes.
    string value = source.Substring(start + 1, current - start - 2);
    AddToken(TokenType.STRING, value);
}
     private bool Match(char expected) {
    if (isAtEnd()) 
        return false;
    if (source[current] != expected) 
        return false;

    current++;
    return true;
    }
    private char Peek() {
        if (isAtEnd()) 
            return '\0';
        return source[current];
    }
    private char PeekNext() {
    if (current + 1 >= source.Length) return '\0';
    return source[current+1];
    } 
    private bool isDigit(char c) {
    return c >= '0' && c <= '9';
  } 
   private bool isAlpha(char c) {
    return (c >= 'a' && c <= 'z') ||
           (c >= 'A' && c <= 'Z') ||
            c == '_';
  }

  private bool isAlphaNumeric(char c) {
    return isAlpha(c) || isDigit(c);
  }
}

