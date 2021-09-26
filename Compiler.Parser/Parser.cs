﻿using Compiler.Core;
using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using Compiler.Core.Lists;
using Compiler.Core.Statements;
using System;
using System.Collections.Generic;
using Type = Compiler.Core.Type;

namespace Compiler.Parser
{
    public class Parser : IParser
    {
        private readonly IScanner scanner;
        private Token lookAhead;
        private List<SymbolList> lst =new List<SymbolList>();
        public Parser(IScanner scanner)
        {
            this.scanner = scanner;
            this.Move();
        }

        public Statement Parse()
        {

            return Program();
        }

       
        private Statement Program()
        {
            EnvironmentManager.PushContext();
            //EnvironmentManager.AddMethod("print", new Id(new Token { Lexeme = "print",}, Type.Void),new ArgumentExpression(new Token  { Lexeme = "" }, new Id(new Token {Lexeme = "arg1"}, Type.String)));
            DeclsUsing();
            NamespaceStmt();
            EnvironmentManager.PopContext();

            EnvironmentManager.PushContext();
            var block = Block();
            Match(TokenType.CloseBrace);
            //block.ValidateSemantic();
            //code = code.Replace($"else:{Environment.NewLine}\tif", "elif");
            return block;
        }

        private void DeclsUsing()
        {
            if (this.lookAhead.TokenType == TokenType.UsingKeyword)
            {
                DeclUsing();
                DeclsUsing();
            }
        }
        private void DeclUsing()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.UsingKeyword:
                    Match(TokenType.UsingKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.Void);
                    EnvironmentManager.AddLibrary(token.Lexeme, id);
                    break;
            }
        }

        private void NamespaceStmt()
        {
            if (this.lookAhead.TokenType == TokenType.NamespaceKeyword)
            {
                Match(TokenType.NamespaceKeyword);
                Match(TokenType.Identifier);
                Match(TokenType.OpenBrace);
                Match(TokenType.ClassKeyword);
                Match(TokenType.Identifier);
                Match(TokenType.OpenBrace);
            }

        }

        private Statement Block()
        {

            if (this.lookAhead.TokenType == TokenType.OpenBrace )
            {
                Match(TokenType.OpenBrace);
                EnvironmentManager.PushContext();
            }
            FunctionStmt();
            Decls();
            var statements = Stmts();
            Match(TokenType.CloseBrace);
            EnvironmentManager.PopContext();
            Decls();
            return statements;
        }

        private void FunctionStmt()
        {
            if (this.lookAhead.TokenType == TokenType.PublicKeyword)
            {
                Match(TokenType.PublicKeyword);
                TypeFunction();
                FunctionStmt();
            }
            else if (this.lookAhead.TokenType == TokenType.StaticKeyword)
            {
                Match(TokenType.StaticKeyword);
                MaintStmt();
                FunctionStmt();
            }
        }
        private void MaintStmt()
        {
            Match(TokenType.VoidKeyword);
            Match(TokenType.MainKeyword);
            Match(TokenType.LeftParens);
            Match(TokenType.StringKeyword);
            Match(TokenType.LeftBracket);
            Match(TokenType.RightBracket);
            Match(TokenType.Identifier);
            Match(TokenType.RightParens);
            Block();
        }

        private void TypeFunction()
        {
            if (this.lookAhead.TokenType == TokenType.VoidKeyword ||
                    this.lookAhead.TokenType == TokenType.IntKeyword ||
                    this.lookAhead.TokenType == TokenType.BoolKeyword ||
                    this.lookAhead.TokenType == TokenType.FloatKeyword ||
                    this.lookAhead.TokenType == TokenType.DateTimeKeyword)
            {
                MatchingTypesFunctions(this.lookAhead.TokenType);
            }
        }

        private void MatchingTypesFunctions(TokenType tokentype)
        {
            Id id = null;
            Match(tokentype);
            var token = lookAhead;
            Match(TokenType.Identifier);
           
            if (tokentype == TokenType.DateTimeKeyword )
            {
                id = new Id(token, Type.DateTime);
            }
            else if (tokentype == TokenType.IntKeyword)
            {
                id = new Id(token, Type.Int);
            }
            else if (tokentype == TokenType.BoolKeyword)
            {
                id = new Id(token, Type.Bool);
            }
            else if (tokentype == TokenType.FloatKeyword)
            {
                id = new Id(token, Type.Float);
            }
            Match(TokenType.LeftParens);
            EnvironmentManager.AddMethod(token.Lexeme, id, null);
            ParamsFunction();
            Match(TokenType.RightParens);
            Block();
        }


        private void ParamsFunction()
        {
            if (this.lookAhead.TokenType == TokenType.IntKeyword ||
                 this.lookAhead.TokenType == TokenType.FloatKeyword ||
                 this.lookAhead.TokenType == TokenType.StringKeyword ||
                 this.lookAhead.TokenType == TokenType.BoolKeyword ||
                 this.lookAhead.TokenType == TokenType.DateTimeKeyword)
            {
               ParamFunction();
                ParamsFunction();
            }
        }

        private void ParamFunction()
        {
            if (this.lookAhead.TokenType == TokenType.IntKeyword ||
               this.lookAhead.TokenType == TokenType.FloatKeyword ||
               this.lookAhead.TokenType == TokenType.StringKeyword ||
               this.lookAhead.TokenType == TokenType.BoolKeyword ||
               this.lookAhead.TokenType == TokenType.DateTimeKeyword)
            {
                Id id = null;
                var tokenType = this.lookAhead.TokenType;
                Match(this.lookAhead.TokenType);
                var Token = this.lookAhead;

                if (tokenType == TokenType.DateTimeKeyword)
                {
                    id = new Id(Token, Type.DateTime);
                }
                else if (tokenType == TokenType.IntKeyword)
                {
                    id = new Id(Token, Type.Int);
                }
                else if (tokenType == TokenType.BoolKeyword)
                {
                    id = new Id(Token, Type.Bool);
                }
                else if (tokenType == TokenType.FloatKeyword)
                {
                    id = new Id(Token, Type.Float);
                }
                Match(TokenType.Identifier);
                EnvironmentManager.AddVariable(Token.Lexeme, id);
                if (this.lookAhead.TokenType == TokenType.Comma)
                {
                    Match(TokenType.Comma);
                }
            }
        }



        private Statement Stmts()
        {
            if (this.lookAhead.TokenType == TokenType.CloseBrace)
            {//{}
                return null;
            }
            return new SequenceStatement(Stmt(), Stmts());
        }


        private void ValidateConstant(Token token, TokenType validate) {
            if (token.TokenType == TokenType.IntConstant && validate == TokenType.IntKeyword)
            {
                var constant = new Constant(token, Type.Int);
                Match(TokenType.IntConstant);
             

            }
            else if (token.TokenType == TokenType.FloatConstant && validate == TokenType.FloatKeyword)
            {
                var constant = new Constant(token, Type.Float);

                Match(TokenType.FloatConstant);
             

            }
            else if (token.TokenType == TokenType.TrueConstant && validate == TokenType.BoolKeyword)
            {
                var constant = new Constant(token, Type.Bool);

                Match(TokenType.TrueConstant);
            
            }
            else if (token.TokenType == TokenType.DateTimeConstant && validate == TokenType.DateTimeKeyword)
            {
                var constant = new Constant(token, Type.DateTime);

                Match(TokenType.DateTimeConstant);
             
            }
            else if (token.TokenType == TokenType.FalseConstant && validate == TokenType.BoolKeyword)
            {
                var constant = new Constant(token, Type.Bool);

                Match(TokenType.FalseConstant);
            }
            else {
                throw new ApplicationException($"Syntax error! expected another type variable but found {token.TokenType}. Line: {token.Line}, Column: {token.Column}");

            }

        }

        private Statement Stmt()
        {
            Expression expression;
            Statement statement1, statement2;
            switch (this.lookAhead.TokenType)
            {
                case TokenType.Identifier: ////////////// aqui es
                    {
                        string[] resultSplit;
                        if (lookAhead.Lexeme.Contains(".add") && lookAhead.Lexeme[lookAhead.Lexeme.Length - 1] == 'd')
                        {
                            resultSplit = lookAhead.Lexeme.Split(".");
                            //  EnvironmentManager.GetSymbol(resultSplit[0]);
                            this.lookAhead.Lexeme = resultSplit[0];
                        }
                        var symbol = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                        var variable = lookAhead;
                        Match(TokenType.Identifier);
                        if (lookAhead.TokenType == TokenType.LeftParens) {
                            // Match(TokenType.AddKeyword);
                            Match(TokenType.LeftParens);
                            var data = GetSymbolListByLexeme(variable.Lexeme);
                            //  Match(TokenType.Identifier);
                            if (data == null) {
                                throw new ApplicationException($"Syntax error! expected type variable list but found {variable}. Line: {variable.Line}, Column: {variable.Column}");
                            }
                            ValidateConstant(lookAhead, data.typeVariable);
                            // var constant = new Constant(lookAhead, Type.Int);
                            // Match(data.typeVariable);
                            Match(TokenType.RightParens);
                            Match(TokenType.SemiColon);
                            return new ListStatement();

                        }
                        var @operator = lookAhead;
                        if (this.lookAhead.TokenType == TokenType.Increment)
                        {
                            Match(TokenType.Increment);
                            Match(TokenType.SemiColon);
                            return new IncrementStatement(variable, @operator);
                        }
                        else if (this.lookAhead.TokenType == TokenType.Decrement) {
                            Match(TokenType.Decrement);
                            Match(TokenType.SemiColon);
                            return new DecrementStatement(variable, @operator);
                        }

                        if (this.lookAhead.TokenType == TokenType.Assignation)
                        {
                            return AssignStmt(symbol.Id);
                        }

                        return CallStmt(symbol);
                    }
                case TokenType.IfKeyword:
                    {
                        Match(TokenType.IfKeyword);
                        Match(TokenType.LeftParens);
                        List<TypedExpression> data = new List<TypedExpression>();
                        List<Token> Logics = new List<Token>();
                        expression = Eq();
                        data.Add(expression as TypedExpression);
                        for (int i = 0; i < data.Count; i++)
                        {
                            if (lookAhead.TokenType == TokenType.AndOperator)
                            {
                                Logics.Add(lookAhead);
                                Match(TokenType.AndOperator);
                                expression = Eq();
                                data.Add(expression as TypedExpression);
                            }
                            else if (lookAhead.TokenType == TokenType.OrOperator)
                            {
                                Logics.Add(lookAhead);
                                Match(TokenType.OrOperator);
                                expression = Eq();
                                data.Add(expression as TypedExpression);
                            }
                        }
                        Match(TokenType.RightParens);

                        statement1 = Stmt();
                        if (this.lookAhead.TokenType != TokenType.ElseKeyword)
                        {
                            return new IfStatement(data, statement1,Logics);
                        }
                        Match(TokenType.ElseKeyword);
                        statement2 = Stmt();
                        return new ElseStatement(data, statement1, statement2);
                    }

                case TokenType.WhileKeyword:
                    {
                        Match(TokenType.WhileKeyword);
                        Match(TokenType.LeftParens);
                        List<TypedExpression> data = new List<TypedExpression>();
                        List<Token> Logics = new List<Token>();
                        expression = Eq();
                        data.Add(expression as TypedExpression);
                        for (int i = 0; i < data.Count; i++)
                        {
                            if (lookAhead.TokenType == TokenType.AndOperator)
                            {
                                Logics.Add(lookAhead);
                                Match(TokenType.AndOperator);
                                expression = Eq();
                                data.Add(expression as TypedExpression);
                            }
                            else if (lookAhead.TokenType == TokenType.OrOperator)
                            {
                                Logics.Add(lookAhead);
                                Match(TokenType.OrOperator);
                                expression = Eq();
                                data.Add(expression as TypedExpression);
                            }
                        }
                        Match(TokenType.RightParens);
                        statement1 = Stmt();
                        return new WhileStatement(data, statement1,Logics);
                    }
                case TokenType.ForeachKeyword:
                    {
                        Match(TokenType.ForeachKeyword);
                        Match(TokenType.LeftParens);
                        var result = lookAhead;
                        verifyType(lookAhead);
                        Match(TokenType.Identifier);
                        Match(TokenType.InKeyword);
                        var variableList = lookAhead;
                        var variableExists = EnvironmentManager.GetSymbol(variableList.Lexeme);
                        MatchVariableType(result.TokenType, lookAhead);
                        Match(TokenType.RightParens);
                        statement1 = Stmt();
                        return new ForEachStatement(result, variableList, statement1);
                        //FALTA RETORNAR UNA CLASE TIPO FOREACH
                        //return Block();
                    }
                case TokenType.WriteLineKeyword:
                    Match(TokenType.WriteLineKeyword);
                    Match(TokenType.LeftParens);
                    var isIdentifier = lookAhead;
                    var dataVariable = new List<Symbol>();
                    if (isIdentifier.TokenType == TokenType.Identifier)
                    {
                        var symbolToPrint = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                     
                        Match(TokenType.Identifier);
                        dataVariable.Add(symbolToPrint);
                        for (int i = 0; i < dataVariable.Count; i++)
                        {
                            if (lookAhead.TokenType == TokenType.Plus)
                            {
                                Match(TokenType.Plus);
                                symbolToPrint = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                                Match(TokenType.Identifier);
                                dataVariable.Add(symbolToPrint);
                            }
                        }
                        Match(TokenType.RightParens);
                        Match(TokenType.SemiColon);
                        return new WriteLineStatement(dataVariable);
                    }
                    Match(TokenType.RightParens);
                    Match(TokenType.SemiColon);
                    return new WriteLineStatement();

                case TokenType.ReadLineKeyword:
                    Match(TokenType.ReadLineKeyword);
                    Match(TokenType.LeftParens);
                    Match(TokenType.RightParens);
                    Match(TokenType.SemiColon);
                    return new ReadLineStatement();
                case TokenType.OpenBrace:
                    return Block();
            }
            return null;
        }

        private void verifyType(Token token) {

            if (token.TokenType == TokenType.IntKeyword)
            {
                Match(TokenType.IntKeyword);
                return;
            }
            else if (token.TokenType == TokenType.DateTimeKeyword)
            {
                Match(TokenType.DateTimeKeyword);
                return;
            }
            else if (token.TokenType == TokenType.BoolKeyword)
            {
                Match(TokenType.BoolKeyword);
                return;
            }
            else if (token.TokenType == TokenType.FloatKeyword)
            {
                Match(TokenType.FloatKeyword);
                return;
            }
            else {
                throw new ApplicationException($"Syntax error! expected variable but found {token.TokenType}. Line: {token.Line}, Column: {token.Column}");
            }

        }

        private Expression Eq()
        {
            var expression = Rel();
            while (this.lookAhead.TokenType == TokenType.Equal || this.lookAhead.TokenType == TokenType.DifferentThan)
            {
                var token = lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Rel() as TypedExpression);
            }

            return expression;
        }



        private Expression Rel()
        {
            var expression = Expr();
            if (this.lookAhead.TokenType == TokenType.LessThan
                || this.lookAhead.TokenType == TokenType.GreaterThan
                || this.lookAhead.TokenType == TokenType.LessOrEqualThan
                || this.lookAhead.TokenType == TokenType.GreaterOrEqualThan)
            {
                var token = lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Expr() as TypedExpression);
            }
            return expression;
        }

        private Expression Expr()
        {
            var expression = Term();
            while (this.lookAhead.TokenType == TokenType.Plus || this.lookAhead.TokenType == TokenType.Minus)
            {
                var token = lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Term() as TypedExpression);
            }
            return expression;
        }

        private Expression Term()
        {
            var expression = Factor();
            while (this.lookAhead.TokenType == TokenType.Asterisk || this.lookAhead.TokenType == TokenType.Division)
            {
                var token = lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Factor() as TypedExpression);
            }
            return expression;
        }

        private Expression Factor()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.LeftParens:
                    {
                        Match(TokenType.LeftParens);
                        var expression = Eq();
                        Match(TokenType.RightParens);
                        return expression;
                    }
                case TokenType.IntConstant:
                    var constant = new Constant(lookAhead, Type.Int);
                    Match(TokenType.IntConstant);
                    return constant;
                case TokenType.FloatConstant:
                    constant = new Constant(lookAhead, Type.Float);
                    Match(TokenType.FloatConstant);
                    return constant;
                case TokenType.StringConstant:
                    constant = new Constant(lookAhead, Type.String);
                    Match(TokenType.StringConstant);
                    return constant;
                case TokenType.TrueConstant:
                    constant = new Constant(lookAhead, Type.Bool);
                    Match(TokenType.TrueConstant);
                    return constant;
                case TokenType.FalseConstant:
                    constant = new Constant(lookAhead, Type.Bool);
                    Match(TokenType.FalseConstant);
                    return constant;
                case TokenType.DateTimeConstant:
                    constant = new Constant(lookAhead, Type.DateTime);
                    Match(TokenType.DateTimeConstant);
                    return constant;
                default:
                    var symbol = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                    Match(TokenType.Identifier);
                    return symbol.Id;
            }
        }

        private Statement CallStmt(Symbol symbol)
        {
            Match(TokenType.LeftParens);
            var @params = OptParams();
            Match(TokenType.RightParens);
            Match(TokenType.SemiColon);
            return new CallStatement(symbol.Id, @params, symbol.Attributes);
        }

        private Expression OptParams()
        {
            if (this.lookAhead.TokenType != TokenType.RightParens)
            {
                return Params();
            }
            return null;
        }

        private Expression Params()
        {
            var expression = Eq();
            if (this.lookAhead.TokenType != TokenType.Comma)
            {
                return expression;
            }
            Match(TokenType.Comma);
            expression = new ArgumentExpression(lookAhead, expression as TypedExpression, Params() as TypedExpression);
            return expression;
        }

        private Statement AssignStmt(Id id)
        {
            Match(TokenType.Assignation);
            var expression = Eq();
            Match(TokenType.SemiColon); 
            return new AssignationStatement(id, expression as TypedExpression);
        }

        private void Decls()
        {
            if (this.lookAhead.TokenType == TokenType.IntKeyword ||
                this.lookAhead.TokenType == TokenType.FloatKeyword ||
                this.lookAhead.TokenType == TokenType.StringKeyword ||
                this.lookAhead.TokenType == TokenType.BoolKeyword ||
                this.lookAhead.TokenType == TokenType.DateTimeKeyword||
                this.lookAhead.TokenType==TokenType.ListKeyword)
            {
                Decl();
                Decls();
            }
        }

        private void Decl()
        {
            Id id;
            switch (this.lookAhead.TokenType)
            {
                case TokenType.FloatKeyword:
                    Match(TokenType.FloatKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    var isInitialize = lookAhead;

                    if (isInitialize.TokenType == TokenType.Assignation)
                    {
                        id = new Id(token, Type.Float);
                        var assignation = AssignStmt(id) as AssignationStatement;
                        assignation.ValidateSemantic();
                        EnvironmentManager.AddVariableWithValue(token.Lexeme, id, assignation.Expression.Token.Lexeme);
                        break;
                    }
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Float);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.StringKeyword:
                    Match(TokenType.StringKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.String);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.IntKeyword:
                    Match(TokenType.IntKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    isInitialize = lookAhead;

                    if (isInitialize.TokenType == TokenType.Assignation)
                    {
                        id = new Id(token, Type.Int);
                        var assignation = AssignStmt(id) as AssignationStatement;
                        assignation.ValidateSemantic();
                        EnvironmentManager.AddVariableWithValue(token.Lexeme, id, assignation.Expression.Token.Lexeme);
                        break;
                    }

                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Int);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.DateTimeKeyword:
                    Match(TokenType.DateTimeKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    isInitialize = lookAhead;
                    if (isInitialize.TokenType == TokenType.Assignation)
                    {
                        id = new Id(token, Type.DateTime);
                        var assignation = AssignStmt(id) as AssignationStatement;
                        assignation.ValidateSemantic();
                        EnvironmentManager.AddVariableWithValue(token.Lexeme, id, assignation.Expression.Token.Lexeme);
                        break;
                    }
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.DateTime);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.BoolKeyword:
                    Match(TokenType.BoolKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    isInitialize = lookAhead;
                    if (isInitialize.TokenType == TokenType.Assignation)
                    {
                        id = new Id(token, Type.Bool);
                        var assignation = AssignStmt(id) as AssignationStatement;
                        assignation.ValidateSemantic();
                        EnvironmentManager.AddVariableWithValue(token.Lexeme, id, assignation.Expression.Token.Lexeme);
                        break;
                    }
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Bool);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;

                case TokenType.ListKeyword:
                    Match(TokenType.ListKeyword);
                    Match(TokenType.LessThan);
                    token = lookAhead;
                    if (token.TokenType == TokenType.IntKeyword)
                    {
                        AddListData(Type.Int, TokenType.IntKeyword);
                    }
                    else if (token.TokenType == TokenType.FloatKeyword)
                    {

                        AddListData(Type.Float, TokenType.FloatKeyword);
                    }
                    else if (token.TokenType == TokenType.BoolKeyword)
                    {
                        AddListData(Type.Bool, TokenType.BoolKeyword);
                    }
                    else if (token.TokenType == TokenType.DateTimeKeyword)
                    {
                        AddListData(Type.DateTime, TokenType.DateTimeKeyword);
                    }

                    break;

                default:
                    break;
            }
        }


        private void AddListData(Type type, TokenType tokenType)
        {
            Match(tokenType);
            Match(TokenType.GreaterThan);
            var token = lookAhead;
            Match(TokenType.Identifier);
            Match(TokenType.Assignation);
            Match(TokenType.NewKeyword);
            Match(TokenType.ListKeyword);
            Match(TokenType.LessThan);
            Match(tokenType);
            Match(TokenType.GreaterThan);
            Match(TokenType.LeftParens);
            Match(TokenType.RightParens);
            Match(TokenType.SemiColon);
            var id = new Id(token, type);
            var simbolList = new SymbolList
            {
                lexeme = token.Lexeme,
                typeVariable = tokenType
            };
            this.lst.Add(simbolList);
            EnvironmentManager.AddVariable(token.Lexeme, id);

        }

        private void Move()
        {
            this.lookAhead = this.scanner.GetNextToken();
        }

        private void MatchVariableType(TokenType variablefirst, Token variable)
        {
           
            if (!this.lst.Exists(x => x.lexeme == variable.Lexeme && x.typeVariable == variablefirst))
            {
                throw new ApplicationException($"Syntax error! expected type variable but found {variablefirst}. Line: {variable.Line}, Column: {variable.Column}");
            }
            this.Move();
        }

        private SymbolList GetSymbolListByLexeme(string lexeme) {

            return this.lst.Find(x => x.lexeme == lexeme);
        }


        private void Match(TokenType tokenType)
        {
            if (this.lookAhead.TokenType != tokenType)
            {
                throw new ApplicationException($"Syntax error! expected token {tokenType} but found {this.lookAhead.TokenType}. Line: {this.lookAhead.Line}, Column: {this.lookAhead.Column}");
            }
            this.Move();
        }
    }
}
