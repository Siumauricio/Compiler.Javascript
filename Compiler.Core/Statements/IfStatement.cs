using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Compiler.Core.Statements
{
    public class IfStatement : Statement
    {
        public IfStatement(List<TypedExpression> expression, Statement statement)
        {
            Expression = expression;
            Statement = statement;
        }

        public List<TypedExpression> Expression { get; }
        public Statement Statement { get; }

        public override string Generate(int tabs)
        {
            var code = GetCodeInit(tabs);
            code += $"if(";
            foreach (var data in Expression) {
                code += $"{data.Generate()}";
            }
            code += $")";
            code += "{";
            code += $"{Environment.NewLine}";
            code += $"{Statement.Generate(tabs + 1)}{Environment.NewLine}";
            return code;
        }

        public override void Interpret()
        {
            foreach (var data in Expression) {
                if (data.Evaluate())
                {
                    Statement.Interpret();
                }
            }
        }

        public override void ValidateSemantic()
        {

            foreach (var data in Expression) {
                if (data.GetExpressionType() != Type.Bool)
                {
                    throw new ApplicationException("A boolean is required in ifs");
                }

            }
        }
    }
}
