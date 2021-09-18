using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using System;

namespace Compiler.Core.Statements
{
    public class ForEachStatement : Statement
    {
        public ForEachStatement(Statement statement)
        {
            Statement = statement;
        }


        public ForEachStatement()
        {

        }

        public TypedExpression Expression { get; }
        public Statement Statement { get; }

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

        public override string Generate(int tabs)
        {
            throw new NotImplementedException();
        }
    }
}