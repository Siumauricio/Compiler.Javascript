using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using System;

namespace Compiler.Core.Statements
{
    public class ForStatement : Statement
    {
        public ForStatement(TypedExpression expression, Statement statement)
        {
            Expression = expression;
            Statement = statement;
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
    }
}
