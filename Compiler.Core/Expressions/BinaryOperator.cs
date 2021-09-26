namespace Compiler.Core.Expressions
{
    public abstract class BinaryOperator : Expression
    {

        BinaryOperator nextNodo;

        public BinaryOperator(Token token, TypedExpression leftExpression, TypedExpression rightExpression, Type type)
            : base(token, type)
        {
            LeftExpression = leftExpression;
            RightExpression = rightExpression;
        }

        public TypedExpression LeftExpression { get; set; } //a1,a2,a3,a4
        public TypedExpression RightExpression { get; set; } //a2
        //left = a1
        //righ = left  = a2
        //       right = a3
        //               left = a4
        //               right = null
    }
}
