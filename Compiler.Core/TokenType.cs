using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core
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
        GreaterThan,
        GreaterOrEqualThan,
        IntKeyword,
        IfKeyword,
        ElseKeyword,
        Identifier,
        IntConstant,
        FloatConstant,
        BoolConstant,
        DateTimeConstant,
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
        ReadLineKeyword,
        NamespaceKeyword,
        StaticKeyword,
        VoidKeyword,
        MainKeyword,
        LeftBracket,
        RightBracket,
        UsingKeyword,
        FalseConstant,
        TrueConstant
        
        
    }
}
