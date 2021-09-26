using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Compiler.Core.Statements
{
    public class WriteLineStatement : Statement
    {
        public WriteLineStatement(List<Symbol> expression)
        {
            Expression = expression;
        }

        public WriteLineStatement()
        {


        }

        public List<Symbol> Expression { get; }


        public override void ValidateSemantic()
        {
            foreach (var data in Expression) {
                if (data.Id.GetExpressionType() != Type.Int || data.Id.GetExpressionType() != Type.Bool || data.Id.GetExpressionType() != Type.DateTime
                 || data.Id.GetExpressionType() != Type.Float)
                {
                    throw new ApplicationException("Parameter is required for WriteLine");
                }

            }

        }

        public override void Interpret()
        {

        }

        public override string Generate()
        {
            int count = 0;
            var code = "console.log(";
            foreach (var data in Expression)
            {
                code += $"{data.Id.Token.Lexeme}";
                if (count < Expression.Count-1) {
                    code += "+";
                    count++;
                }
            }
            code += ")";
            code += $"{Environment.NewLine}";
            return code;

        }
    }
}
