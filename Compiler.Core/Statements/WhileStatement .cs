using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using System;

namespace Compiler.Core.Statements
{
    public class WhileStatement : Statement
    {
        public WhileStatement(TypedExpression expression, Statement statement)
        {
            Expression = expression;
            Statement = statement;
        }

        public TypedExpression Expression { get; }
        public Statement Statement { get; }

        public override string Generate()
        {

            var code = GetCodeInit();
            code += $"while({Expression.Generate()})";
            code += "{";
            code += $"{Environment.NewLine}";
            code += $"{Statement?.Generate()}{Environment.NewLine}";
            code += "}";
            return code;
        }

        public override void Interpret()
        {
            throw new NotImplementedException();
        }

        public override void ValidateSemantic()
        {
            if (Expression.GetExpressionType() != Type.Bool)
            {
                throw new ApplicationException("A boolean is required in while expression");
            }
        }
    }
}
