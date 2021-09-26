using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Compiler.Core.Statements
{
    public class WhileStatement : Statement
    {
        public WhileStatement(List<TypedExpression> expression, Statement statement, List<Token> logics)
        {
            Expression = expression;
            Statement = statement;
            Logics = logics;
        }

        public List<TypedExpression> Expression { get; }
        public Statement Statement { get; }

        public List<Token> Logics { get; }

        public override string Generate(int tabs)
        {
            var code = GetCodeInit(tabs);
            var index = 0;
            code += $"while(";
            foreach (var data in Expression){
                code += $"{data.Generate()}";
                if (Logics.Count != 0)
                {
                    code += $" {Logics[index].Lexeme} ";
                    Logics.RemoveAt(index);
                    index++;
                }
            }
            code += ")";
            code += "{";
            code += $"{Environment.NewLine}";
            code += $"{Statement.Generate(tabs+1)}{Environment.NewLine}";
            code += "}";
            return code;
        }



        public override void Interpret()
        {
            foreach (var data in Expression)
            {
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
                    throw new ApplicationException("A boolean is required in while expression");
                }
            }
        }
    }
}
