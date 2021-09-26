using Compiler.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core.Statements {
    public class ParamsValueFunction : Statement {

        public ParamsValueFunction(Id id, List<FunctionParamsStatement> @params, List<Expression> attributes) {
            Id = id;
            Params = @params;
            Attributes = attributes;
        }

        public Id Id { get; }
        public List<FunctionParamsStatement> Params { get; }
        public List<Expression> Attributes { get; }

        public override void Interpret() {
            //var method = EnvironmentManager.GetSymbolForEvaluation(Id.Token.Lexeme);
            //if (method.Id.Token.Lexeme == "print") {
            //    InnerEvaluate(Arguments);
            //}
        }

        private void InnerEvaluate(Expression arguments) {
            if (arguments is BinaryOperator binary) {
                InnerEvaluate(binary.LeftExpression);
                InnerEvaluate(binary.RightExpression);
            } else {
                var typedExpression = arguments as TypedExpression;
                Console.WriteLine(typedExpression.Evaluate());
            }
        }

        public override void ValidateSemantic() {
            //ValidateArguments(Attributes, Arguments);
        }

        private void ValidateArguments(Expression attributes, Expression arguments) {
            //if (attributes == null && arguments == null) {
            //    return;
            //}

            //if (attributes is BinaryOperator binary && binary.RightExpression == null && (arguments is BinaryOperator)) {
            //    throw new ApplicationException("Incorrect amount of arguments supplied");
            //}

            //if (attributes is BinaryOperator attr && arguments is BinaryOperator arg) {
            //    ValidateArguments(attr.LeftExpression, arg.LeftExpression);
            //    ValidateArguments(attr.RightExpression, arg.RightExpression);
            //} else if (attributes is TypedExpression typedAttr && arguments is TypedExpression typedArg && typedAttr.GetExpressionType() != typedArg.GetExpressionType()) {
            //    throw new ApplicationException($"Expected {typedAttr.GetExpressionType()} but received {typedArg.GetExpressionType()}");
            //}

        }

        public override string Generate() {
            var code = GetCodeInit();
            //var innerCode = InnerCodeGenerateCode(Arguments);
            //code += $"{Id.Generate()}({innerCode}){Environment.NewLine}";
            return code;
        }

        private string InnerCodeGenerateCode(Expression arguments) {
            var code = string.Empty;
            if (arguments is BinaryOperator binary) {
                code += InnerCodeGenerateCode(binary.LeftExpression);
                code += InnerCodeGenerateCode(binary.RightExpression);
            } else {
                code += arguments.Generate();
            }
            return code;
        }
    }
}
