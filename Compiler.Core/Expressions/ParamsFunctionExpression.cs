using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core.Expressions
{
   public class ParamsFunctionExpression : Expression
    {
        public ParamsFunctionExpression(Token token, Type type) : base(token, type)
        {
        }



        public override string Generate()
        {
            throw new NotImplementedException();
        }
    }
}
