using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Compiler.Core
{
    public class Type : IEquatable<Type>
    {
        public string Lexeme { get; private set; }

        public TokenType TokenType { get; private set; }
        public Type(string lexeme, TokenType tokenType)
        {
            Lexeme = lexeme;
            TokenType = tokenType;
        }

        public static Type Int => new Type("int", TokenType.BasicType);
        public static Type Float => new Type("float", TokenType.BasicType);
        public static Type Bool => new Type("bool", TokenType.BasicType);
        public static Type String => new Type("string", TokenType.BasicType);
        public static Type Void => new Type("void", TokenType.BasicType);
        public static Type DateTime => new Type("datetime", TokenType.BasicType);
        public static Type List => new Type("List", TokenType.BasicType);
        public static Type Class => new Type("Class", TokenType.BasicType);
        public static Type ReadLine => new Type("ConsoleReadLine", TokenType.ReadLineKeyword);
        public bool Equals(Type other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Lexeme == other.Lexeme && TokenType == other.TokenType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Type)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Lexeme, (int)TokenType);
        }

        public static bool operator ==(Type a, Type b) => a.Equals(b);

        public static bool operator !=(Type a, Type b) => !a.Equals(b);
        public override string ToString()
        {
            return Lexeme;
        }
    }
}
