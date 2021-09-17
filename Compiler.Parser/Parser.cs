using Compiler.Core;
using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using Compiler.Core.Statements;
using System;
using Type = Compiler.Core.Type;

namespace Compiler.Parser
{
    public class Parser : IParser
    {
        private readonly IScanner scanner;
        private Token lookAhead;

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
            // var code = block.Generate(0);
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

        private Statement Stmt()
        {
            Expression expression;
            Statement statement1, statement2;
            switch (this.lookAhead.TokenType)
            {
                case TokenType.Identifier:
                    {
                        var symbol = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                        Match(TokenType.Identifier);
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
                        expression = Eq();
                        Match(TokenType.RightParens);
                        statement1 = Stmt();
                        if (this.lookAhead.TokenType != TokenType.ElseKeyword)
                        {
                            return new IfStatement(expression as TypedExpression, statement1);
                        }
                        Match(TokenType.ElseKeyword);
                        statement2 = Stmt();
                        return new ElseStatement(expression as TypedExpression, statement1, statement2);
                    }
                case TokenType.WhileKeyword:
                    {
                        Match(TokenType.WhileKeyword);
                        Match(TokenType.LeftParens);
                        expression = Eq();
                        Match(TokenType.RightParens);
                        statement1 = Stmt();
                        return new WhileStatement(expression as TypedExpression, statement1);
                    }
                case TokenType.ForeachKeyword:
                    {
                        Match(TokenType.ForeachKeyword);
                        Match(TokenType.LeftParens);
                        var result = lookAhead;
                        if (result.TokenType == TokenType.IntKeyword || result.TokenType == TokenType.FloatConstant || result.TokenType == TokenType.BoolConstant
                            || result.TokenType == TokenType.DateTimeKeyword) {
                            Match(result.TokenType);
                        }
                        Match(TokenType.Identifier);
                        Match(TokenType.InKeyword);
                        var variableSaved = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                        Match(TokenType.Identifier);
                        Match(TokenType.RightParens);
                        //statement1 = Stmt();

                        //FALTA RETORNAR UNA CLASE TIPO FOREACH
                        return Block();
                    }
                case TokenType.WriteLineKeyword:
                    Match(TokenType.WriteLineKeyword);
                    Match(TokenType.LeftParens);
                    var isIdentifier = lookAhead;
                    if (isIdentifier.TokenType == TokenType.Identifier)
                    {
                        var symbolToPrint = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                        Match(isIdentifier.TokenType);
                        Match(TokenType.RightParens);
                        Match(TokenType.SemiColon);
                        return new WriteLineStatement(symbolToPrint.Id);
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
                this.lookAhead.TokenType == TokenType.DateTimeKeyword)
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
                default:
                    break;
            }
        }

        private void Move()
        {
            this.lookAhead = this.scanner.GetNextToken();
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
