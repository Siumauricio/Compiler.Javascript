using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using System;

namespace Compiler.Core.Statements
{
    public class ForEachStatement : Statement
    {
        public ForEachStatement(Token token1, Token token2, Statement statement)
        {
            Token1 = token1;
            Token2 = token2;
            Statement = statement;
        }
  
        public TypedExpression Expression { get; }
        public Statement Statement { get; }
        public Token Token1 { get; }
        public Token Token2 { get; }

        public override void ValidateSemantic()
        {
            if (Expression.GetExpressionType() != Type.Bool)
            {
                throw new ApplicationException("Parameters are required in for bucle");
            }
        }

        public override void Interpret()
        {
            throw new NotImplementedException();
        }

        public override string Generate()
        {
            var code = $"{Token2.Lexeme}.foreach(";
            code += $"function({Token1.Lexeme})";
            code += "{";
            code += $"{Statement.Generate()}";
            code += "}";
            return code;
        }
    }
}