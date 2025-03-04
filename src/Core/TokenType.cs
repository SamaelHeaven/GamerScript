namespace GamerScript.Core;

public enum TokenType
{
    // Keywords
    KeywordsBegin,
    Var = KeywordsBegin,
    Function,
    If,
    Elif,
    Else,
    True,
    False,
    While,
    Return,
    Increment,
    Decrement,
    KeywordsEnd = Decrement,

    // Operators
    OperatorsBegin,
    Plus = OperatorsBegin,
    Minus,
    Asterisk,
    Slash,
    Assignment,
    Equals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    OperatorsEnd = LessThanOrEqual,

    // Delimiters
    DelimitersBegin,
    LeftParen = DelimitersBegin,
    RightParen,
    LeftBrace,
    RightBrace,
    DelimitersEnd = RightBrace,

    // Others
    Identifier,
    Number,
    String,
    Comment,
    NewLine,
    WhiteSpace,
    EndOfFile
}