using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using System;

namespace Compiler.Core.Statements
{
    public class ElseStatement : Statement
    {
        public ElseStatement(TypedExpression expression, Statement trueStatement, Statement falseStatement)
        {
            Expression = expression;
            TrueStatement = trueStatement;
            FalseStatement = falseStatement;
        }

        public TypedExpression Expression { get; }
        public Statement TrueStatement { get; }
        public Statement FalseStatement { get; }

        public override string Generate()
        {
            var code = GetCodeInit();
            code += $"if({Expression.Generate()}):{Environment.NewLine}";
            code += $"{TrueStatement.Generate()}{Environment.NewLine}";
            code += $"else:{Environment.NewLine}";
            code += $"{FalseStatement.Generate()}{Environment.NewLine}";
            return code;
        }

        public override void Interpret()
        {
            if (Expression.Evaluate())
            {
                TrueStatement.Interpret();
            }
            else
            {
                FalseStatement.Interpret();
            }
        }

        public override void ValidateSemantic()
        {
            if (Expression.GetExpressionType() != Type.Bool)
            {
                throw new ApplicationException("A boolean is required in ifs");
            }
        }
    }
}
