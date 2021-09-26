using Compiler.Core.Interfaces;

namespace Compiler.Core.Statements
{
    public abstract class Statement : Node, ISemanticValidation, IStatementEvaluate
    {
        public abstract void Interpret();

        public abstract void ValidateSemantic();

        public abstract string Generate();

        public virtual string GetCodeInit()
        {

            var code = string.Empty;
   
            return code;
        }
    }
}
