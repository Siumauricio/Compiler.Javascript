using Compiler.Core;
using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using Compiler.Core.Lists;
using Compiler.Core.Statements;
using System;
using System.Collections.Generic;
using Type = Compiler.Core.Type;
using System.Linq;
namespace Compiler.Parser {
    public class Parser : IParser {
        private readonly IScanner scanner;
        private Token lookAhead;
        private List<SymbolList> lst =new List<SymbolList>();
        string buffer = "";
        string bff = "";
        List<Statement> functions = new List<Statement>();
        Tuple<Token, TokenType,Id> tuple;
        List<Expression> expressions = new List<Expression>();
        string paramsAssignment = "";
        public Parser(IScanner scanner) {

            this.scanner = scanner;
            this.Move();

        }

        public Statement Parse() {
            return Program();
        }

        private Statement Program() {

            DeclsUsing();
            NamespaceStmt();
            EnvironmentManager.PushContext();
            //EnvironmentManager.AddMethod("print", new Id(new Token { Lexeme = "print", }, Type.Void), new ArgumentExpression(new Token { Lexeme = "" }, new Id(new Token { Lexeme = "arg1" }, Type.String)));
            

            FunctionStmt();

            var block = Block();
            Match(TokenType.CloseBrace);

            //block.ValidateSemantic();
            //code = code.Replace($"else:{Environment.NewLine}\tif", "elif");
            //var code = block.Generate();
            return block;
        }

        private void DeclsUsing() {
            if (this.lookAhead.TokenType == TokenType.UsingKeyword) {
                switch (this.lookAhead.TokenType) {
                    case TokenType.UsingKeyword:
                        Match(TokenType.UsingKeyword);
                        Match(TokenType.Identifier);
                        Match(TokenType.SemiColon);
                        break;
                }
                DeclsUsing();
            }
        }

        private void NamespaceStmt() {
            if (this.lookAhead.TokenType == TokenType.NamespaceKeyword) {
                Match(TokenType.NamespaceKeyword);
                Match(TokenType.Identifier);
                Match(TokenType.OpenBrace);
                Match(TokenType.ClassKeyword);
                Match(TokenType.Identifier);
                Match(TokenType.OpenBrace);
            }

        }

        private Statement Block() {

            if (this.lookAhead.TokenType == TokenType.OpenBrace) {
                Match(TokenType.OpenBrace);
            }
            FunctionStmt();
            Decls();//son las variables
            var statements = Stmts();//statements como ifs, whiles,foreach,asignaciones
            Match(TokenType.CloseBrace);
            bff += "}"+Environment.NewLine;

            Decls();
            return statements;
        }

        private void FunctionStmt() {
            if (this.lookAhead.TokenType == TokenType.PublicKeyword)//esto si es una funcion
            {
                Match(TokenType.PublicKeyword);
                functions.Add(Function());
                GenerateCodeFunction();
                FunctionStmt();
            } else if (this.lookAhead.TokenType == TokenType.StaticKeyword)//esto si es el Main
            {
                Match(TokenType.StaticKeyword);
                functions.Add(MaintStmt());
                GenerateCodeFunctionMain();
                FunctionStmt();
            }
        }
        private Statement MaintStmt() {
            Match(TokenType.VoidKeyword);
            Match(TokenType.MainKeyword);
            Match(TokenType.LeftParens);
            Match(TokenType.StringKeyword);
            Match(TokenType.LeftBracket);
            Match(TokenType.RightBracket);
            Match(TokenType.Identifier);
            Match(TokenType.RightParens);
            var stms = Block();
            return stms;
        }
        private void GenerateCodeFunction() {
            var function = EnvironmentManager.GetSymbol(tuple.Item1.Lexeme);
            buffer += "function " + tuple.Item1.Lexeme + "(";
            if (function.Parameters.Count == 0) {
                buffer += ") {"+Environment.NewLine;
            }
            for (int i = 0; i < function.Parameters.Count; i++) {
                buffer += function.Parameters[i].Generate();
                if ((i+1) != function.Parameters.Count ) {
                    buffer += ", ";
                } else {
                    buffer += ") {"+Environment.NewLine;
                }
            }
            buffer += bff;
            bff = ""+Environment.NewLine;
        }
        private void GenerateCodeFunctionMain() {
            buffer += bff;
            buffer = buffer.Substring(0, buffer.Length-3);
            Console.WriteLine(buffer);
        }
        private Statement Function() {
            if (this.lookAhead.TokenType == TokenType.VoidKeyword || this.lookAhead.TokenType == TokenType.IntKeyword || this.lookAhead.TokenType == TokenType.BoolKeyword ||   this.lookAhead.TokenType == TokenType.FloatKeyword ||this.lookAhead.TokenType == TokenType.DateTimeKeyword) {
                TokenType tokenType = this.lookAhead.TokenType;
                Match(this.lookAhead.TokenType);
                var token = this.lookAhead;
                Match(TokenType.Identifier);
                Match(TokenType.LeftParens);
                Id id = null;
                if (tokenType == TokenType.IntKeyword) {
                    id =  new Id(token, Type.Int);
                } else if(tokenType == TokenType.FloatKeyword) {
                    id = new Id(token, Type.Float);

                } else if (tokenType == TokenType.DateTimeKeyword) {
                    id = new Id(token, Type.DateTime);

                } else if (tokenType == TokenType.BoolKeyword) {
                    id = new Id(token, Type.Bool);
                }
                List<FunctionParamsStatement> symbols = new List<FunctionParamsStatement>();
                ParamsFunction(symbols);
                tuple = new Tuple<Token, TokenType,Id>(token, tokenType,id);
                EnvironmentManager.AddMethod2(token.Lexeme, id, symbols) ;
                Match(TokenType.RightParens);
            }
            var stmts = Block();
            return stmts;
        }
   
        private void ParamsFunction(List<FunctionParamsStatement> symbols) {
           
            if (this.lookAhead.TokenType == TokenType.IntKeyword || this.lookAhead.TokenType == TokenType.FloatKeyword || this.lookAhead.TokenType == TokenType.StringKeyword || this.lookAhead.TokenType == TokenType.BoolKeyword || this.lookAhead.TokenType == TokenType.DateTimeKeyword)//aca voy a leer las variables de los parametros
            {
                TokenType tokenType = this.lookAhead.TokenType;
                Match(this.lookAhead.TokenType);
                var declaration = this.lookAhead; //aca extraigo la variable
                Match(TokenType.Identifier);
                Id id =null;
                if (this.lookAhead.TokenType == TokenType.Comma) {
                    Match(TokenType.Comma);
                }
                if (tokenType == TokenType.IntKeyword) {
                    id = new Id(declaration, Type.Int);
                } else if (tokenType == TokenType.FloatKeyword) {
                    id = new Id(declaration, Type.Float);

                } else if (tokenType == TokenType.DateTimeKeyword) {
                    id = new Id(declaration, Type.DateTime);

                } else if (tokenType == TokenType.BoolKeyword) {
                    id = new Id(declaration, Type.Bool);
                }
                EnvironmentManager.AddVariable(declaration.Lexeme, id);
                symbols.Add(new FunctionParamsStatement(declaration,id));
                ParamsFunction(symbols); 
            }
        }

        private Statement Stmts() {
            if (this.lookAhead.TokenType == TokenType.CloseBrace) {
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
            Expression expression = null;
            Statement statement1, statement2;
            switch (this.lookAhead.TokenType)
            {
                case TokenType.Identifier: 
                    {
                        var varname = this.lookAhead;
                        string[] resultSplit;
                        bool isList = false;
                        if (lookAhead.Lexeme.Contains(".add") && lookAhead.Lexeme[lookAhead.Lexeme.Length - 1] == 'd') {
                           
                            resultSplit = lookAhead.Lexeme.Split(".");
                            bff += resultSplit[0]+".push(";
                            //  EnvironmentManager.GetSymbol(resultSplit[0]);
                            this.lookAhead.Lexeme = resultSplit[0];
                            isList = true;
                        }
                        var symbol = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                        var variable = lookAhead;
                        Match(TokenType.Identifier);
                        if (lookAhead.TokenType == TokenType.LeftParens && isList) {
                            Match(TokenType.LeftParens);
                            var data = GetSymbolListByLexeme(variable.Lexeme);
                            if (data == null) {////////////////////list
                                throw new ApplicationException($"Syntax error! expected type variable list but found {variable}. Line: {variable.Line}, Column: {variable.Column}");
                            }
                            bff += lookAhead.Lexeme + ");" + Environment.NewLine;

                            ValidateConstant(lookAhead, data.typeVariable);
                            Match(TokenType.RightParens);
                            Match(TokenType.SemiColon);
                            return new ListStatement();

                        }
                        var @operator = lookAhead;
                        if (this.lookAhead.TokenType == TokenType.Increment) {
                            Match(TokenType.Increment);
                            Match(TokenType.SemiColon);
                            return new IncrementStatement(variable, @operator);
                        } else if (this.lookAhead.TokenType == TokenType.Decrement) {
                            Match(TokenType.Decrement);
                            Match(TokenType.SemiColon);
                            return new DecrementStatement(variable, @operator);
                        }

                        //if (this.lookAhead.TokenType == TokenType.Assignation) {
                        //    return AssignStmt(symbol.Id);
                        //}

                        if (symbol.SymbolType == SymbolType.Method) {
                            var Assign = GetFunctionStmt(symbol) as ParamsValueFunction;
                            bff += Assign.Id.Token.Lexeme + " (";
                            if (Assign.Params.Count != Assign.Attributes.Count) {
                                throw new ApplicationException($"Syntax error! Function {Assign.Id.Token.Lexeme} Accept {Assign.Params.Count} parameters and you send {Assign.Attributes.Count} parameters");

                            }
                            for (int i = 0; i < Assign.Params.Count; i++) {
                                Type tipo = Assign.Attributes[i].type;

                                if ((Assign.Params[i].Id.GetExpressionType() != Assign.Attributes[i].type) && tipo?.GetType() != null) {
                                    throw new ApplicationException($"Syntax error! Param Type Expected {Assign.Params[i].Id.GetExpressionType()} but Found {Assign.Attributes[i].type}");
                                } else if (Assign.Attributes[i] is ArithmeticOperator) {
                                    var convert = Assign.Attributes[i] as ArithmeticOperator;
                                    var type = convert.GetExpressionType();
                                    if (type == Assign.Params[i].Id.GetExpressionType()) {
                                        bff += convert.Generate();
                                    } else {
                                        throw new ApplicationException($"Syntax error! Param Type Expected {Assign.Params[i].Id.GetExpressionType()} but Found {type}");
                                    }
                                } else {
                                    bff += Assign.Attributes[i].Token.Lexeme;
                                }
                                if ((i + 1) != Assign.Params.Count) {
                                    bff += ",";
                                }

                            }
                            bff += ");" + Environment.NewLine;
                            expressions.Clear();
                            Decls();
                            return Assign;
                        } else {
                            if (this.lookAhead.TokenType == TokenType.Assignation) {
                                var Assign = AssignStmt(symbol.Id);
                                bff += Assign.Generate();
                                Decls();
                                return Assign;
                            }
                        }
                        return CallStmt(symbol);
                    }


                case TokenType.IfKeyword: {
                        Match(TokenType.IfKeyword);
                        Match(TokenType.LeftParens);
                        List<TypedExpression> data = new List<TypedExpression>();
                        List<Token> Logics = new List<Token>();
                        expression = Eq();
                        if (expression is RelationalExpression) {
                            RelationalExpression result = expression as RelationalExpression;
                            bff += "if(" + result.LeftExpression.Token.Lexeme + " " + result.Token.Lexeme + " " + result.RightExpression.Token.Lexeme;
                        }
                        data.Add(expression as TypedExpression);
                        for (int i = 0; i < data.Count; i++) {
                            if (lookAhead.TokenType == TokenType.AndOperator) {
                                bff += " && ";
                                Logics.Add(lookAhead);
                                Match(TokenType.AndOperator);
                                expression = Eq();
                                RelationalExpression result = expression as RelationalExpression;
                                bff += result.LeftExpression.Token.Lexeme + " " + result.Token.Lexeme + " " + result.RightExpression.Token.Lexeme;
                                data.Add(expression as TypedExpression);
                            } else if (lookAhead.TokenType == TokenType.OrOperator) {
                                bff += " || ";
                                Logics.Add(lookAhead);
                                Match(TokenType.OrOperator);
                                expression = Eq();
                                RelationalExpression result = expression as RelationalExpression;
                                bff += result.LeftExpression.Token.Lexeme + " " + result.Token.Lexeme + " " + result.RightExpression.Token.Lexeme;
                                data.Add(expression as TypedExpression);
                            }
                        }
                            bff += "){" + Environment.NewLine;
                            Match(TokenType.RightParens);

                            statement1 = Stmt();
                            if (this.lookAhead.TokenType != TokenType.ElseKeyword) {
                                return new IfStatement(data, statement1, Logics);
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
                        if (expression is RelationalExpression) {
                            RelationalExpression result = expression as RelationalExpression;
                            bff += "while(" + result.LeftExpression.Token.Lexeme + " " + result.Token.Lexeme + " " + result.RightExpression.Token.Lexeme;
                        }

                        data.Add(expression as TypedExpression);
                        for (int i = 0; i < data.Count; i++)
                        {
                            if (lookAhead.TokenType == TokenType.AndOperator)
                            {
                                bff += " && ";
                                Logics.Add(lookAhead);
                                Match(TokenType.AndOperator);
                                expression = Eq();
                                RelationalExpression result = expression as RelationalExpression;
                                bff += result.LeftExpression.Token.Lexeme + " " + result.Token.Lexeme + " " + result.RightExpression.Token.Lexeme;
                                data.Add(expression as TypedExpression);
                            }
                            else if (lookAhead.TokenType == TokenType.OrOperator)
                            {
                                bff += " || ";
                                Logics.Add(lookAhead);
                                Match(TokenType.OrOperator);
                                expression = Eq();
                                RelationalExpression result = expression as RelationalExpression;
                                bff += result.LeftExpression.Token.Lexeme + " " + result.Token.Lexeme + " " + result.RightExpression.Token.Lexeme;
                                data.Add(expression as TypedExpression);
                            }
                        }
                        bff += "){" + Environment.NewLine;
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
                        var variable = lookAhead;
                      
                        Match(TokenType.Identifier);
                        Match(TokenType.InKeyword);
                        var variableList = lookAhead;
                        var variableExists = EnvironmentManager.GetSymbol(variableList.Lexeme);
                        MatchVariableType(result.TokenType, lookAhead);
                        Match(TokenType.RightParens);
                        bff += variableList.Lexeme+ ".forEach( (" + variable.Lexeme + ") => {"+Environment.NewLine;
                        statement1 = Stmt();
                        bff = bff.Substring(0, bff.Length - 2);
                        bff += ");"+Environment.NewLine;

                        return new ForEachStatement(result, variableList, statement1);
                    }
                case TokenType.WriteLineKeyword:
                    Match(TokenType.WriteLineKeyword);
                    bff += "console.log(";

                    Match(TokenType.LeftParens);
                    var isIdentifier = lookAhead;
                    var dataVariable = new List<Symbol>();
                    if (isIdentifier.TokenType == TokenType.Identifier || isIdentifier.TokenType == TokenType.IntConstant || isIdentifier.TokenType == TokenType.StringConstant || isIdentifier.TokenType == TokenType.FloatConstant || isIdentifier.TokenType == TokenType.BoolConstant)
                    {
                        var symbolToPrint = Eq();
                        bff += symbolToPrint.Generate2();
                        bff += ");"+Environment.NewLine;
                        Match(TokenType.RightParens);
                        Match(TokenType.SemiColon);
                        Decls();

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
                case TokenType.ReturnKeyword:
                    List<RelationalExpression> data2 = new List<RelationalExpression>();
                    Match(TokenType.ReturnKeyword);
                    if (this.lookAhead.TokenType != TokenType.SemiColon) {
                        expression = Eq();
                    }
                    
                    bff += "return ";
                    if (tuple.Item2 == TokenType.VoidKeyword) {
                        if (expression !=null) {
                            throw new ApplicationException($"Syntax error! expected return type {tuple.Item2} but found another type.");
                        }
                        bff += ";" + Environment.NewLine;
                    }
                    else if (expression is ArithmeticOperator && (tuple.Item2 == TokenType.IntKeyword || tuple.Item2 == TokenType.FloatKeyword) ) {
                        ArithmeticOperator arithmeticOperator = (ArithmeticOperator)expression ;
                        bff += arithmeticOperator.Generate()+";" + Environment.NewLine;
                    }
                    else if(expression is RelationalExpression && (tuple.Item2 == TokenType.BoolKeyword)) {
                        RelationalExpression relational = (RelationalExpression)expression;
                        relational.GetExpressionType();
                        bff += relational.LeftExpression.Token.Lexeme + " " + relational.Token.Lexeme + " " + relational.RightExpression.Token.Lexeme ;
                        data2.Add(expression as RelationalExpression);
                        for (int i = 0; i < data2.Count; i++) {
                            if (lookAhead.TokenType == TokenType.AndOperator) {
                                bff += " && ";
                                Match(TokenType.AndOperator);
                                expression = Eq();
                                RelationalExpression result = expression as RelationalExpression;
                                result.GetExpressionType();

                                bff += result.LeftExpression.Token.Lexeme + " " + result.Token.Lexeme + " " + result.RightExpression.Token.Lexeme;
                                data2.Add(expression as RelationalExpression);
                            } else if (lookAhead.TokenType == TokenType.OrOperator) {
                                bff += " || ";
                                Match(TokenType.OrOperator);
                                expression = Eq();
                                RelationalExpression result = expression as RelationalExpression;
                                bff += result.LeftExpression.Token.Lexeme + " " + result.Token.Lexeme + " " + result.RightExpression.Token.Lexeme;
                                data2.Add(expression as RelationalExpression);
                            }
                        }
                        bff += ";"+Environment.NewLine;

                        
                    }
                    else if(expression!=null ) {
                        if (expression.GetType() == typeof(Id)) {
                            if (expression.type == tuple.Item3.type) {
                                bff += expression.Token.Lexeme + ";" + Environment.NewLine;
                            } else {
                                throw new ApplicationException($"Syntax error! expected return type {tuple.Item3.type} but found {expression.type}.");
                            }
                        } else {
                            throw new ApplicationException($"Syntax error! Return Invalid!");
                        }
                    } else {
                        throw new ApplicationException($"Syntax error! expected return type {tuple.Item3.type} but found another");
                    }
                    Match(TokenType.SemiColon);
                    if (this.lookAhead.TokenType != TokenType.CloseBrace) {
                        throw new ApplicationException($"Syntax error! You cant put Statement after a return");
                    }
                    return null;
                default:
                    return Block();
            }

        }

        private void verifyType(Token token) {

            if (token.TokenType == TokenType.IntKeyword)
            {
                
                Match(TokenType.IntKeyword);
                var @var = this.lookAhead;
                Id id = new Id(@var, Type.Int);
                EnvironmentManager.AddVariable(@var.Lexeme, id);
                return;
            }
            else if (token.TokenType == TokenType.DateTimeKeyword)
            {
                Match(TokenType.DateTimeKeyword);
                var @var = this.lookAhead;
                Id id = new Id(@var, Type.DateTime);
                EnvironmentManager.AddVariable(@var.Lexeme, id);
                return;
            }
            else if (token.TokenType == TokenType.BoolKeyword)
            {
                Match(TokenType.BoolKeyword);
                var @var = this.lookAhead;
                Id id = new Id(@var, Type.Bool);
                EnvironmentManager.AddVariable(@var.Lexeme, id);
                return;
            }
            else if (token.TokenType == TokenType.FloatKeyword)
            {
                Match(TokenType.FloatKeyword);
                var @var = this.lookAhead;
                Id id = new Id(@var, Type.Float);
                EnvironmentManager.AddVariable(@var.Lexeme, id);
                return;
            }
            else {
                throw new ApplicationException($"Syntax error! expected variable but found {token.TokenType}. Line: {token.Line}, Column: {token.Column}");
            }
        }

        private Expression Eq()
        {
            var expression = Rel();
            while (this.lookAhead.TokenType == TokenType.Equal || this.lookAhead.TokenType == TokenType.DifferentThan) {
                var token = lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Rel() as TypedExpression);
            }

            return expression;
        }

        private Expression Rel() {

            var expression = Expr();
            if (this.lookAhead.TokenType == TokenType.LessThan
                || this.lookAhead.TokenType == TokenType.GreaterThan
                || this.lookAhead.TokenType == TokenType.LessOrEqualThan
                || this.lookAhead.TokenType == TokenType.GreaterOrEqualThan) {
                var token = lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Expr() as TypedExpression);
            }
            return expression;
        }

        private Expression Expr() {
            var expression = Term();
            while (this.lookAhead.TokenType == TokenType.Plus || this.lookAhead.TokenType == TokenType.Minus) {
                var token = lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Term() as TypedExpression);
            }
            return expression;
        }

        private Expression Term() {
            var expression = Factor();
            while (this.lookAhead.TokenType == TokenType.Asterisk || this.lookAhead.TokenType == TokenType.Division) {
                var token = lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Factor() as TypedExpression);
            }
            return expression;
        }

        private Expression Factor() {
            switch (this.lookAhead.TokenType) {
                case TokenType.LeftParens: {
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

                    if (symbol.SymbolType == SymbolType.Method) {
                        var Assign = GetFunctionStmt(symbol) as ParamsValueFunction;

                        paramsAssignment += Assign.Id.Token.Lexeme + " (";
                        if (Assign.Params.Count != Assign.Attributes.Count) {
                            throw new ApplicationException($"Syntax error! Function {Assign.Id.Token.Lexeme} Accept {Assign.Params.Count} parameters and you send {Assign.Attributes.Count} parameters");

                        }
                        for (int i = 0; i < Assign.Params.Count; i++) {
                            Type tipo = Assign.Attributes[i].type;

                            if ((Assign.Params[i].Id.GetExpressionType() != Assign.Attributes[i].type) && tipo?.GetType() != null) {
                                throw new ApplicationException($"Syntax error! Param Type Expected {Assign.Params[i].Id.GetExpressionType()} but Found {Assign.Attributes[i].type}");
                            } else if (Assign.Attributes[i] is ArithmeticOperator) {
                                var convert = Assign.Attributes[i] as ArithmeticOperator;
                                var type = convert.GetExpressionType();
                                if (type == Assign.Params[i].Id.GetExpressionType()) {
                                    paramsAssignment += convert.Generate();
                                } else {
                                    throw new ApplicationException($"Syntax error! Param Type Expected {Assign.Params[i].Id.GetExpressionType()} but Found {type}");
                                }
                            } else {
                                paramsAssignment += Assign.Attributes[i].Token.Lexeme;
                            }
                            if ((i + 1) != Assign.Params.Count) {
                                paramsAssignment += ",";
                            }

                        }
                        paramsAssignment += ")";
                        var isSum = this.lookAhead;

                        if (isSum.TokenType == TokenType.Plus) {
                            paramsAssignment += "+";
                        } else {
                            paramsAssignment += ";";
                        }
                    }
                    expressions.Clear();
                    return symbol.Id;
            }
        }

        private Statement GetFunctionStmt(Symbol symbol) {
            Match(TokenType.LeftParens);
            var @params = OptParams2();
            Match(TokenType.RightParens);
            return new ParamsValueFunction(symbol.Id,symbol.Parameters ,expressions);
        }
        private Expression OptParams2() {
            if (this.lookAhead.TokenType != TokenType.RightParens) {
                return Params2();
            }
            return null;
        }

        private Expression Params2() {
            var expression = Eq();
            expressions.Add(expression);
            if (this.lookAhead.TokenType != TokenType.Comma) {
                return expression;
            }
            Match(TokenType.Comma);
            expression = new ArgumentExpression(lookAhead, expression as TypedExpression, Params2() as TypedExpression);
            return expression;
        }

        private Statement CallStmt(Symbol symbol) {
            Match(TokenType.LeftParens);
            var @params = OptParams();
            Match(TokenType.RightParens);
            Match(TokenType.SemiColon);
            return new CallStatement(symbol.Id, @params, symbol.Attributes);
        }
        private Expression OptParams() {
            if (this.lookAhead.TokenType != TokenType.RightParens) {
                return Params();
            }
            return null;
        }

        private Expression Params() {
            var expression = Eq();
            if (this.lookAhead.TokenType != TokenType.Comma) {
                return expression;
            }
            Match(TokenType.Comma);
            expression = new ArgumentExpression(lookAhead, expression as TypedExpression, Params() as TypedExpression);
            return expression;
        }
        private Statement AssignStmt(Id id) {
            Match(TokenType.Assignation);
            var expression = Eq();
            Match(TokenType.SemiColon);
            return new AssignationStatement(id, expression as TypedExpression);
        }
      
        private void Decls() {
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

        private void Decl() {
            Id id;
            switch (this.lookAhead.TokenType) {
                case TokenType.FloatKeyword:
                    Match(TokenType.FloatKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    var isInitialize = lookAhead;

                    if (isInitialize.TokenType == TokenType.Assignation) {
                        id = new Id(token, Type.Float);
                        var assignation = AssignStmt(id) as AssignationStatement;
                        assignation.ValidateSemantic();

                        if (paramsAssignment == "") {
                            bff += "let " + assignation.Generate() + Environment.NewLine;
                        } else {
                            bff += "let " + token.Lexeme + " = " + paramsAssignment + Environment.NewLine;

                        }
                        EnvironmentManager.AddVariableWithValue(token.Lexeme, id, assignation.Expression.Token.Lexeme);
                        break;
                    }
                    bff += "let " + token.Lexeme + ";" + Environment.NewLine;
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
                        if (paramsAssignment == "") {
                            bff += "let " + assignation.Generate() + Environment.NewLine;
                        } else {
                            bff += "let " + token.Lexeme + " = " + paramsAssignment + Environment.NewLine;

                        }

                        EnvironmentManager.AddVariableWithValue(token.Lexeme, id, assignation.Expression.Token.Lexeme);
                        break;
                    }
                    bff += "let " + token.Lexeme+";" + Environment.NewLine;
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Int);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.DateTimeKeyword:
                    Match(TokenType.DateTimeKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    isInitialize = lookAhead;
                    if (isInitialize.TokenType == TokenType.Assignation) {
                        id = new Id(token, Type.DateTime);
                        var assignation = AssignStmt(id) as AssignationStatement;

                        assignation.ValidateSemantic();

                        if (paramsAssignment == "") {
                            bff += "let " + assignation.Generate() + Environment.NewLine;
                        } else {
                            bff += "let " + token.Lexeme + " = " + paramsAssignment + Environment.NewLine;

                        }
                        EnvironmentManager.AddVariableWithValue(token.Lexeme, id, assignation.Expression.Token.Lexeme);
                        break;
                    }
                    bff += "let " + token.Lexeme + ";" + Environment.NewLine;
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.DateTime);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.BoolKeyword:
                    Match(TokenType.BoolKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    isInitialize = lookAhead;
                    if (isInitialize.TokenType == TokenType.Assignation) {
                        id = new Id(token, Type.Bool);
                        var assignation = AssignStmt(id) as AssignationStatement;
                        assignation.ValidateSemantic();

                        if (paramsAssignment == "") {
                            bff += "let " + assignation.Generate() + Environment.NewLine;
                        } else {
                            bff += "let " + token.Lexeme + " = " + paramsAssignment + Environment.NewLine;

                        }
                        EnvironmentManager.AddVariableWithValue(token.Lexeme, id, assignation.Expression.Token.Lexeme);
                        break;
                    }
                    bff += "let " + token.Lexeme + ";" + Environment.NewLine;
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
            paramsAssignment = "";
        }


        private void AddListData(Type type, TokenType tokenType)
        {
            Match(tokenType);
            Match(TokenType.GreaterThan);
            var token = lookAhead;
            bff += "const " + token.Lexeme + " = [];"+Environment.NewLine;

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
