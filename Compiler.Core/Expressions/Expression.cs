namespace Compiler.Core.Expressions
{
    public abstract class Expression : Node
    {
        public readonly Type type;

        public Token Token { get; }

        public Expression(Token token, Type type)
        {
            Token = token;
            this.type = type;
        }

        public abstract string Generate();
        public abstract string Generate2();
    }
}
