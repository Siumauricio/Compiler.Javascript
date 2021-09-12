using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using System;

namespace Compiler.Core.Statements
{
    public class WriteLineStatement : Statement
    {
        public WriteLineStatement(TypedExpression expression)
        {
            Expression = expression;
        }

        public WriteLineStatement()
        {

        }

        public TypedExpression Expression { get; }

        public override void ValidateSemantic()
        {
            if (Expression.GetExpressionType() != Type.Int || Expression.GetExpressionType() != Type.Bool || Expression.GetExpressionType() != Type.DateTime
                || Expression.GetExpressionType() != Type.Float)
            {
                throw new ApplicationException("Parameter is required for WriteLine");
            }
        }
    }
}
