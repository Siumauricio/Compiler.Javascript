using Compiler.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core.Statements
{
    public class FunctionStatement : Statement
    {
        public FunctionStatement(TypedExpression expression, Statement firsStatement, Statement nextStatement)
        {
            Expression = expression;
            FirstStatement = firsStatement;
            NextStatement = nextStatement;
        }

        public TypedExpression Expression { get; }
        public Statement FirstStatement { get; }
        public Statement NextStatement { get; }
        public override string Generate()
        {
            throw new NotImplementedException();
        }

        public override void Interpret()
        {
            throw new NotImplementedException();
        }

        public override void ValidateSemantic()
        {
            throw new NotImplementedException();
        }
    }
}
