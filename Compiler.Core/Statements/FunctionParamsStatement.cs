using Compiler.Core.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core.Expressions {
    public class FunctionParamsStatement : Statement {
        public FunctionParamsStatement(Token token, Id id) {
            Id = id;
            Token = token;
        }

        public Id Id { get; }
        public Token Token { get; }

        public override string Generate() {
         return Token.Lexeme;
        }

        public override void Interpret() {
            throw new NotImplementedException();
        }

        public override void ValidateSemantic() {
            throw new NotImplementedException();
        }
    }
}
