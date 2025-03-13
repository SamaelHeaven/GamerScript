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
    KeywordsEnd,
    Decrement = KeywordsEnd,

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
    OperatorsEnd,
    LessThanOrEqual = OperatorsEnd,

    // Delimiters
    DelimitersBegin,
    LeftParen = DelimitersBegin,
    RightParen,
    LeftBrace,
    RightBrace,
    DelimitersEnd,
    Comma = DelimitersEnd,

    // Others
    Identifier,
    Number,
    String,
    Comment,
    NewLine,
    WhiteSpace,
    EndOfFile
}