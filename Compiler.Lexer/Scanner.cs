using Compiler.Core;
using Compiler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Lexer
{
    public class Scanner : IScanner
    {
        private Input input;
        private readonly Dictionary<string, TokenType> keywords;
        private readonly Dictionary<string, TokenType> Tokens;
        private readonly Dictionary<string, TokenType> Rep;

        public Scanner(Input input)
        {
            this.input = input;
            this.keywords = new Dictionary<string, TokenType>
            {
                { "if", TokenType.IfKeyword  },
                { "else", TokenType.ElseKeyword },
                { "int", TokenType.IntKeyword },
                { "float", TokenType.FloatKeyword },
                { "bool", TokenType.BoolKeyword },
                { "datetime", TokenType.DateTimeKeyword },
                { "for", TokenType.ForKeyword },
                { "foreach", TokenType.ForeachKeyword },
                { "while", TokenType.WhileKeyword },
                { "Console.WriteLine" ,TokenType.WriteLineKeyword},
                { "Console.ReadLine" ,TokenType.ReadLineKeyword},
                { "namespace" ,TokenType.NamespaceKeyword},
                { "Main" ,TokenType.MainKeyword},
                { "void" ,TokenType.VoidKeyword},
                { "static" ,TokenType.StaticKeyword},
                { "string" ,TokenType.StringKeyword},
                {"using" ,TokenType.UsingKeyword},
                {"class",TokenType.ClassKeyword },
                {"false",TokenType.FalseConstant },
                {"true",TokenType.TrueConstant },
            };

            this.Tokens = new Dictionary<string, TokenType>
            {
                { "+", TokenType.Plus },
                { "-", TokenType.Minus },
                { "*", TokenType.Asterisk },
                { "(", TokenType.LeftParens },
                { ")", TokenType.RightParens },
                { ";", TokenType.SemiColon },
                { "=", TokenType.Assignation },
                { "{", TokenType.OpenBrace },
                { "}", TokenType.CloseBrace },
                { ",", TokenType.Comma },
                { "/", TokenType.Division },
                { "%", TokenType.Percentaje },
                { "\0", TokenType.EOF},
                { ">", TokenType.GreaterThan },
                { "<", TokenType.LessThan },
                { "!", TokenType.Negation },
                { ">=", TokenType.GreaterOrEqualThan },
                { "<=", TokenType.LessOrEqualThan },
                { "!=", TokenType.DifferentThan },
                { "==", TokenType.Equal },
                { "&", TokenType.Ampersand },
                { "|", TokenType.VerticalLine },
                { "&&", TokenType.AndOperator },
                { "||", TokenType.OrOperator },
                { "++", TokenType.Increment },
                { "--", TokenType.Decrement },
               { "[", TokenType.LeftBracket },
               { "]", TokenType.RightBracket }

            };
            this.Rep = new Dictionary<string, TokenType>
            {
                { "+", TokenType.Plus },
                { "-", TokenType.Minus },
                { "&", TokenType.Ampersand },
                { "|", TokenType.VerticalLine },
                { "=", TokenType.Assignation },
            };
        }

        public Token GetNextToken()
        {
            var lexeme = new StringBuilder();
            var currentChar = GetNextChar();
            while (true)
            {
                while (char.IsWhiteSpace(currentChar) || currentChar == '\n')
                {
                    currentChar = GetNextChar();
                }
                if (char.IsLetter(currentChar))
                {
                    lexeme.Append(currentChar);
                    currentChar = PeekNextChar();
                    while (char.IsLetterOrDigit(currentChar) || currentChar == '.')
                    {
                        currentChar = GetNextChar();
                        lexeme.Append(currentChar);
                        currentChar = PeekNextChar();
                    }

                    if (this.keywords.ContainsKey(lexeme.ToString()))
                    {
                        return GetInfo(this.keywords[lexeme.ToString()], input.Position.Column, input.Position.Line, lexeme.ToString());
                    }
                    return GetInfo(TokenType.Identifier, input.Position.Column, input.Position.Line, lexeme.ToString());
                }
                else if (char.IsDigit(currentChar))
                {
                    lexeme.Append(currentChar);
                    currentChar = PeekNextChar();
                    while (char.IsDigit(currentChar))
                    {
                        currentChar = GetNextChar();
                        lexeme.Append(currentChar);
                        currentChar = PeekNextChar();
                    }

                    if (currentChar != '.')
                    {
                        return GetInfo(TokenType.IntConstant, input.Position.Column, input.Position.Line, lexeme.ToString());
                    }

                    currentChar = GetNextChar();
                    lexeme.Append(currentChar);
                    currentChar = PeekNextChar();
                    while (char.IsDigit(currentChar))
                    {
                        currentChar = GetNextChar();
                        lexeme.Append(currentChar);
                        currentChar = PeekNextChar();
                    }
                    return GetInfo(TokenType.FloatConstant, input.Position.Column, input.Position.Line, lexeme.ToString());
                }
                else
                {
                    if (this.Tokens.ContainsKey(currentChar.ToString()))
                    {
                        lexeme.Append(currentChar);
                        var nextChar = PeekNextChar();
                        if (!this.Rep.ContainsKey(nextChar.ToString()))
                        {
                            return GetInfo(this.Tokens[currentChar.ToString()], input.Position.Column, input.Position.Line, lexeme.ToString().Trim());
                        }
                        else
                        {
                            lexeme.Append(nextChar);
                            GetNextChar();
                            if (this.Tokens.ContainsKey(lexeme.ToString()))
                            {
                                return GetInfo(this.Tokens[lexeme.ToString().Trim()], input.Position.Column, input.Position.Line, lexeme.ToString().Trim());
                            }
                        }
                    }

                    throw new ApplicationException($"Caracter {lexeme} invalido en la columna: {input.Position.Column}, fila: {input.Position.Line}");
                }
            }
        }
        private Token GetInfo(TokenType tokenType, int column, int line, string lexeme)
        {
            return new Token
            {
                TokenType = tokenType,
                Column = column,
                Line = line,
                Lexeme = lexeme
            };
        }
        private char GetNextChar()
        {
            var next = input.NextChar();
            input = next.Reminder;
            return next.Value;
        }
        private char PeekNextChar()
        {
            var next = input.NextChar();
            return next.Value;
        }
    }
}
