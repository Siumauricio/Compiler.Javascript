using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using System;

namespace Compiler.Core.Statements
{
    public class ListStatement : Statement
    {
        public ListStatement()
        {
       
        }

        private void ValidateArguments(Expression attributes, Expression arguments)
        {
          
        }

        public override string Generate()
        {

            throw new NotImplementedException();
        }

        public override void Interpret()
        {
            throw new NotImplementedException();
        }

        public override void ValidateSemantic()
        {
            throw new NotImplementedException();
        }
    }
}
