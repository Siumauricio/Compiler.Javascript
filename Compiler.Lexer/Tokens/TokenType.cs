using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Lexer.Tokens
{
    public enum TokenType
    {
        Asterisk,
        Plus,
        Minus,
        Percentaje,
        LeftParens,
        RightParens,
        SemiColon,
        Equal,
        Division,
        Negation,
        DifferentThan,
        LessThan,
        LessOrEqualThan,
        NotEqual,
        GreaterThan,
        GreaterOrEqualThan,
        IntKeyword,
        IfKeyword,
        ElseKeyword,
        Identifier,
        IntConstant,
        FloatConstant,
        Assignation,
        StringConstant,
        EOF,
        OpenBrace,
        CloseBrace,
        Comma,
        BasicType,
        FloatKeyword,
        StringKeyword,
        BoolKeyword,
        DateTimeKeyword,
        ClassKeyword,
        Ampersand,
        VerticalLine,
        AndOperator,
        OrOperator,
        Increment,
        Decrement,
        ForKeyword,
        ForeachKeyword,
        WhileKeyword,
        WriteLineKeyword,
        ReadLineKeyword

    }
}
