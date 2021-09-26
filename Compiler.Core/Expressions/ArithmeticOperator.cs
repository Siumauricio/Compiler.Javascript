using System;
using System.Collections.Generic;

namespace Compiler.Core.Expressions
{
    public class ArithmeticOperator : TypedBinaryOperator
    {
        private readonly Dictionary<(Type, Type), Type> _typeRules;
        public ArithmeticOperator(Token token, TypedExpression leftExpression, TypedExpression rightExpression)
            : base(token, leftExpression, rightExpression, null)
        {
            _typeRules = new Dictionary<(Type, Type), Type>
            {
                { (Type.Float, Type.Float), Type.Float },
                { (Type.Int, Type.Int), Type.Int },
                { (Type.String, Type.String), Type.String },
                { (Type.Float, Type.Int), Type.Float },
                { (Type.Int, Type.Float), Type.Float },
                { (Type.String, Type.Int), Type.String  },
                { (Type.String, Type.Float), Type.String  }
            };
        }

        public override dynamic Evaluate()
        {
            return Token.TokenType switch
            {
                TokenType.Plus => LeftExpression.Evaluate() + RightExpression.Evaluate(),
                TokenType.Minus => LeftExpression.Evaluate() - RightExpression.Evaluate(),
                TokenType.Asterisk => LeftExpression.Evaluate() * RightExpression.Evaluate(),
                TokenType.Division => LeftExpression.Evaluate() / RightExpression.Evaluate(),
                _ => throw new NotImplementedException()
            };
        }

        public override string Generate()
        {
            if (LeftExpression.GetExpressionType() == Type.String &&
                RightExpression.GetExpressionType() != Type.String)
            {
                return $"{LeftExpression.Generate()} , {RightExpression.Generate()})";
            }

            return $"{LeftExpression.Generate()} + {RightExpression.Generate()}";
        }

        public override string Generate2() {
            if (LeftExpression.GetExpressionType() == Type.String &&
                RightExpression.GetExpressionType() != Type.String) {
                return $"{LeftExpression.Generate2()} , {RightExpression.Generate2()})";
            }

            return $"{LeftExpression.Generate2()} , {RightExpression.Generate2()}";
        }

        public override Type GetExpressionType()
        {
            if (_typeRules.TryGetValue((LeftExpression.GetExpressionType(), RightExpression.GetExpressionType()), out var resultType))
            {
                return resultType;
            }

            throw new ApplicationException($"Cannot perform arithmetic operation on {LeftExpression.GetExpressionType()}, {RightExpression.GetExpressionType()}");
        }
    }
}
