using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Compiler.Core.Statements
{
    public class DecrementStatement : Statement
    {
        public DecrementStatement(Token token1, Token token2)
        {
            Token1 = token1;
            Token2 = token2;
        }

        public Token Token1 { get; }
        public Token Token2 { get; }


        public override string Generate()
        {
            var code = $"{Token1.Lexeme}";
            code += $"{Token2.Lexeme};";
            code += $"{Environment.NewLine}";
            return code;
        }



        public override void Interpret()
        {
      
        }

        public override void ValidateSemantic()
        {
          
        }
    }
}
