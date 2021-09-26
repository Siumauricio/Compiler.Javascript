using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Compiler.Core.Statements
{
    public class ElseStatement : Statement
    {
        public ElseStatement(List<TypedExpression> expression, Statement trueStatement, Statement falseStatement)
        {
            Expression = expression;
            TrueStatement = trueStatement;
            FalseStatement = falseStatement;
        }

        public List<TypedExpression> Expression { get; }
        public Statement TrueStatement { get; }
        public Statement FalseStatement { get; }

        public override string Generate()
        {
            var code = GetCodeInit();
            code += $"if(";
            foreach (var data in Expression)
            {
                code += $"{data.Generate()}";
            }
            code += $")";
            code += "{";
            code += $"{Environment.NewLine}";
            code += $"{TrueStatement.Generate()}{Environment.NewLine}";
            //for (int i = 0; i < tabs; i++)
            //{
            //    code += "\t";
            //}
            code += "}else{";
            code+=$"{Environment.NewLine}";
            code += $"{FalseStatement.Generate()}{Environment.NewLine}";
            code += "}";
            return code;
        }

        public override void Interpret()
        {
            //if (Expression.Evaluate())
            //{
            //    TrueStatement.Interpret();
            //}
            //else
            //{
            //    FalseStatement.Interpret();
            //}
        }

        public override void ValidateSemantic()
        {
            //if (Expression.GetExpressionType() != Type.Bool)
            //{
            //    throw new ApplicationException("A boolean is required in ifs");
            //}
        }
    }
}
