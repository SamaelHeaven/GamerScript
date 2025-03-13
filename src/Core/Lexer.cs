using System.Globalization;

namespace GamerScript.Core;

public class Lexer(string source)
{
    private readonly Dictionary<string, TokenType> _keywords = new()
    {
        { "loot", TokenType.Var },
        { "dlc", TokenType.Function },
        { "clutch", TokenType.If },
        { "retry", TokenType.Elif },
        { "ragequit", TokenType.Else },
        { "buffed", TokenType.True },
        { "nerfed", TokenType.False },
        { "farm", TokenType.While },
        { "spawn", TokenType.Return },
        { "buff", TokenType.Increment },
        { "nerf", TokenType.Decrement }
    };

    private readonly string _source = source.Replace("\r", "").Replace("\v", "").Replace("\f", "");
    private int _line = 1;
    private int _position;
    private int _start;

    public IList<Token> Tokenize()
    {
        var tokens = new List<Token>();
        while (_position < _source.Length)
        {
            var current = _source[_position];
            _start = _position;
            _position++;
            tokens.Add(current switch
            {
                '\n' =>
                    LexNewLine(),
                ' ' =>
                    Lex(TokenType.WhiteSpace, " "),
                '\t' =>
                    Lex(TokenType.WhiteSpace, "    "),
                ',' =>
                    Lex(TokenType.Comma, ","),
                '+' =>
                    Lex(TokenType.Plus, "+"),
                '-' =>
                    Lex(TokenType.Minus, "-"),
                '*' =>
                    Lex(TokenType.Asterisk, "*"),
                '/' =>
                    Lex(TokenType.Slash, "/"),
                '=' =>
                    LexSign(TokenType.Assignment, '=', TokenType.Equals, '='),
                '>' =>
                    LexSign(TokenType.GreaterThan, '>', TokenType.GreaterThanOrEqual, '='),
                '<' =>
                    LexSign(TokenType.LessThan, '<', TokenType.LessThanOrEqual, '='),
                '(' =>
                    Lex(TokenType.LeftParen, "("),
                ')' =>
                    Lex(TokenType.RightParen, ")"),
                '{' =>
                    Lex(TokenType.LeftBrace, "{"),
                '}' =>
                    Lex(TokenType.RightBrace, "}"),
                >= 'A' and <= 'z' =>
                    LexIdentifierOrKeywordOrComment(),
                >= '0' and <= '9' =>
                    LexNumber(),
                '\"' =>
                    LexString(),
                _ =>
                    throw new GsException($"Unexpected character: '{current}' at line {_line}.")
            });
        }

        tokens.Add(Lex(TokenType.EndOfFile, ""));
        _line = 1;
        _position = 0;
        _start = 0;
        return tokens;
    }

    private Token Lex(TokenType type, string lexeme, int? line = null)
    {
        return new Token(type, lexeme, line ?? _line);
    }

    private Token LexComment()
    {
        var line = _line;
        _position++;
        var start = _position;
        while (_position + 1 < _source.Length && !(_source[_position] == 'X' && _source[_position + 1] == 'x'))
        {
            if (_source[_position] == '\n')
                _line++;
            _position++;
        }

        if (_position + 1 >= _source.Length)
            throw new GsException($"Unterminated comment at line {_line}.");
        var lexeme = $"xX{_source[start.._position]}Xx";
        _position += 2;
        return Lex(TokenType.Comment, lexeme, line);
    }

    private Token LexNewLine()
    {
        var line = _line;
        _line++;
        return Lex(TokenType.NewLine, "\n", line);
    }

    private Token LexSign(TokenType type, char lexeme, TokenType otherType, char otherLexeme)
    {
        if (_position >= _source.Length || _source[_position] != otherLexeme)
            return new Token(type, lexeme.ToString(), _line);
        _position++;
        return Lex(otherType, $"{lexeme}{otherLexeme}");
    }

    private Token LexNumber()
    {
        while (_position < _source.Length && (char.IsDigit(_source[_position]) || _source[_position] == '.'))
            _position++;
        var lexeme = _source[_start.._position];
        if (!double.TryParse(lexeme, CultureInfo.InvariantCulture, out _))
            throw new GsException($"Invalid number format: '{lexeme}' at line {_line}.");
        return Lex(TokenType.Number, lexeme);
    }

    private Token LexString()
    {
        var line = _line;
        _position++;
        while (_position < _source.Length && _source[_position] != '\"')
        {
            if (_source[_position] == '\n')
                _line++;
            if (_source[_position] == '\\' && _position + 1 < _source.Length && _source[_position + 1] == '\"')
                _position++;
            _position++;
        }

        if (_position >= _source.Length || _source[_position] != '\"')
            throw new GsException($"Unterminated string literal at line {_line}.");
        var lexeme = $"\"{_source[(_start + 1).._position]}\"";
        _position++;
        return Lex(TokenType.String, lexeme, line);
    }

    private Token LexIdentifierOrKeywordOrComment()
    {
        if (_source[_start] == 'x' && _start + 1 < _source.Length && _source[_start + 1] == 'X')
            return LexComment();
        while (_position < _source.Length && char.IsLetterOrDigit(_source[_position]))
            _position++;
        var lexeme = _source[_start.._position];
        var type = _keywords.GetValueOrDefault(lexeme, TokenType.Identifier);
        return Lex(type, lexeme);
    }
}