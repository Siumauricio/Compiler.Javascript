using Compiler.Core.Expressions;
using System;
using System.Collections.Generic;

namespace Compiler.Core
{
    public enum SymbolType
    {
        Variable,
        Method,
        Library
    }

    public class Symbol {
        public Symbol(SymbolType symbolType, Id id, dynamic value) {
            SymbolType = symbolType;
            Id = id;
            Value = value;
        }

        public Symbol(SymbolType symbolType, Id id, Expression attributes) {
            Attributes = attributes;
            SymbolType = symbolType;
            Id = id;

        }
        public Symbol(SymbolType symbolType, Id id, List<FunctionParamsStatement> parameters) {
            Parameters = parameters;
            SymbolType = symbolType;
            Id = id;
        }

        public SymbolType SymbolType { get; }
        public Id Id { get; }
        public dynamic Value { get; set; }
        public Expression Attributes { get; }
        public List<FunctionParamsStatement> Parameters { get; set; }
        
    }
}
