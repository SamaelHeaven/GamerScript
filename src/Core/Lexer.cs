namespace GamerScript.Core;

public class Lexer(string source)
{
    private readonly Dictionary<string, TokenType> _keywords = new()
    {
        { "loot", TokenType.Var },
        { "strat", TokenType.Function },
        { "clutch", TokenType.If },
        { "retry", TokenType.Elif },
        { "ragequit", TokenType.Else },
        { "buffed", TokenType.True },
        { "nerfed", TokenType.False },
        { "taunt", TokenType.Print },
        { "quest", TokenType.Input },
        { "farm", TokenType.While },
        { "spawn", TokenType.Return },
        { "afk", TokenType.Sleep },
        { "buff", TokenType.Increment },
        { "nerf", TokenType.Decrement }
    };

    private int _line;
    private int _position;
    private int _start;

    internal List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        while (_position < source.Length)
        {
            var current = source[_position];
            _start = _position;
            _position++;
            if (current is ' ' or '\r' or '\t')
                continue;
            tokens.Add(current switch
            {
                '\n' =>
                    LexNewLine(),
                '+'
                    =>
                    LexSimple(TokenType.Plus, '+'),
                '-' =>
                    LexSimple(TokenType.Minus, '-'),
                '*' =>
                    LexSimple(TokenType.Asterisk, '*'),
                '/' =>
                    LexSimple(TokenType.Slash, '/'),
                '=' =>
                    LexSign(TokenType.Assignment, '=', TokenType.Equals, '='),
                '>' =>
                    LexSign(TokenType.GreaterThan, '>', TokenType.GreaterThanOrEqual, '='),
                '<' =>
                    LexSign(TokenType.LessThan, '>', TokenType.LessThanOrEqual, '='),
                '(' =>
                    LexSimple(TokenType.LeftParen, '('),
                ')' =>
                    LexSimple(TokenType.RightParen, ')'),
                '{' =>
                    LexSimple(TokenType.LeftBrace, '{'),
                '}' =>
                    LexSimple(TokenType.RightBrace, '}'),
                >= 'A' and <= 'z' =>
                    LexIdentifierOrKeyword(),
                >= '0' and <= '9' =>
                    LexNumber(),
                '\"' =>
                    LexString(),
                _ =>
                    throw new GsException($"Unexpected character: '{current}' at line {_line}.")
            });
        }

        tokens.Add(LexSimple(TokenType.EndOfFile));
        return tokens;
    }

    private Token LexNewLine()
    {
        var result = LexSimple(TokenType.NewLine);
        _line++;
        return result;
    }

    private Token LexSimple(TokenType type, char? lexeme = null)
    {
        return new Token(type, lexeme?.ToString() ?? "", _line);
    }

    private Token LexSign(TokenType type, char lexeme, TokenType otherType, char otherLexeme)
    {
        if (_position >= source.Length || source[_position] != otherLexeme)
            return new Token(type, lexeme.ToString(), _line);
        _position++;
        return new Token(otherType, $"{lexeme}{otherLexeme}", _position);
    }

    private Token LexNumber()
    {
        while (_position < source.Length && (char.IsDigit(source[_position]) || source[_position] == '.'))
            _position++;
        var lexeme = source[_start.._position];
        return new Token(TokenType.Number, lexeme, _line);
    }

    private Token LexString()
    {
        _position++;
        while (_position < source.Length && source[_position] != '\"')
        {
            if (source[_position] == '\\' && _position + 1 < source.Length && source[_position + 1] == '\"')
                _position++;
            _position++;
        }

        if (_position >= source.Length || source[_position] != '\"')
            throw new GsException($"Unterminated string literal at line {_line}.");
        var lexeme = source[(_start + 1).._position];
        _position++;
        return new Token(TokenType.String, lexeme, _line);
    }


    private Token LexIdentifierOrKeyword()
    {
        while (_position < source.Length && char.IsLetterOrDigit(source[_position]))
            _position++;
        var lexeme = source[_start.._position];
        var type = _keywords.GetValueOrDefault(lexeme, TokenType.Identifier);
        return new Token(type, lexeme, _line);
    }
}