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
        private List<Token> lstBooleans = new List<Token>();
        private List<Token> lstIntFloats = new List<Token>();
        List<Statement> functions = new List<Statement>();
        Tuple<Token, TokenType,Id> tuple;
        List<Expression> expressions = new List<Expression>();
        List<Symbol> _functions = new List<Symbol>();
        Dictionary<string,List<Symbol>> classes = new Dictionary<string, List<Symbol>>();
        string paramsAssignment = "";
        string buffer = "";
        string clases = "";
        string bff = "";
        bool isMain = false;
        string actualClass = "";

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
            //FunctionStmt();
            var block = Block();
            Console.WriteLine(buffer);
            //buffer = buffer.Substring(0, buffer.Length - 3);
            //Match(TokenType.CloseBrace);
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
                ClassStmts();

            }

        }
        public void ClassStmts() {
            if (TokenType.ClassKeyword == this.lookAhead.TokenType) {
                EnvironmentManager.PushContext();
                Match(TokenType.ClassKeyword);
                if (this.lookAhead.Lexeme !="MAIN") {
                    actualClass = this.lookAhead.Lexeme;
                    isMain = true;
                    clases = "class " + this.lookAhead.Lexeme + " {" + Environment.NewLine;
                } else {
                    
                    clases = "";
                }


                Match(TokenType.Identifier);
                buffer += clases;
                Match(TokenType.OpenBrace);
                FunctionStmt();
                Block();
                if (isMain) {
                    classes.Add(actualClass, _functions);
                }
                 _functions = new List<Symbol>();

                bff = "";
                if (clases != "") {
                buffer += "}" + Environment.NewLine;
                }
                actualClass = "";
                isMain = false;
                ClassStmts();
            }
          
        }

      
        private Statement Block() {

            if (this.lookAhead.TokenType == TokenType.OpenBrace) {
                Match(TokenType.OpenBrace);
            }
            FunctionStmt();
            Decls();
            var statements = Stmts();
            Match(TokenType.CloseBrace);
            bff += "}"+Environment.NewLine;
            
            Decls();
            return statements;
        }

        private void FunctionStmt() {
            if (this.lookAhead.TokenType == TokenType.PublicKeyword)
            {
                Match(TokenType.PublicKeyword);
                functions.Add(Function());
                GenerateCodeFunction();
                FunctionStmt();
            } else if (this.lookAhead.TokenType == TokenType.StaticKeyword)
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
            if (isMain) {
                buffer +=  tuple.Item1.Lexeme + "(";

            } else {
                buffer += "function " + tuple.Item1.Lexeme + "(";

            }
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
            //Console.WriteLine(buffer);
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
                } else if (tokenType == TokenType.VoidKeyword) {
                    id = new Id(token, Type.Void);
                }
                List<FunctionParamsStatement> symbols = new List<FunctionParamsStatement>();
                ParamsFunction(symbols);
                tuple = new Tuple<Token, TokenType,Id>(token, tokenType,id);
                Symbol symbol = new Symbol(SymbolType.Method, id, symbols);
                _functions.Add(symbol);



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
                        if (lookAhead.Lexeme.Contains(".Add") && lookAhead.Lexeme[lookAhead.Lexeme.Length - 1] == 'd') {
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
                            bff += variable.Lexeme + "++;" + Environment.NewLine;
                            Decls();
                            return new IncrementStatement(variable, @operator);
                        } else if (this.lookAhead.TokenType == TokenType.Decrement) {
                            Match(TokenType.Decrement);
                            Match(TokenType.SemiColon);
                            bff += variable.Lexeme + "--;" + Environment.NewLine;
                            Decls();
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
                            Match(TokenType.SemiColon);

                            Decls();
                            return Assign;
                        } else {
                            if (this.lookAhead.TokenType == TokenType.Assignation) {
                                var Assign = AssignStmt(symbol.Id);
                                if (Assign is AssignationStatement) {
                                    var Read = Assign as AssignationStatement;
                                    if (Read.Expression.GetExpressionType() == Type.ReadLine) {
                                        var Result = Read.Expression.Generate();
                                        if (Result == "Number") {
                                            if (Read.Id.GetExpressionType() != Type.Int) {
                                                throw new ApplicationException($"Syntax error! Invalid Parse and ReadLine");

                                            }
                                        }
                                        if (Result == "Number") {
                                            if (Read.Id.GetExpressionType() != Type.Float) {
                                                throw new ApplicationException($"Syntax error! Invalid Parse and ReadLine");

                                            }
                                        }
                                        if (Result == "Boolean") {
                                            if (Read.Id.GetExpressionType() != Type.Bool) {
                                                throw new ApplicationException($"Syntax error! Invalid Parse and ReadLine");
                                            }
                                        }
                                        bff += Read.Id.Token.Lexeme +" = "+ Result + "(prompt(''));"+Environment.NewLine;

                                    } else {
                                        bff += Assign.Generate();
                                    }
                                } else {
                                    bff += Assign.Generate();

                                }
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
                            var validateRelationalExpression = expression as TypedExpression;
                            validateRelationalExpression.GetExpressionType();
                            RelationalExpression result = expression as RelationalExpression;
                            bff += "if(" + result.LeftExpression.Token.Lexeme + " " + result.Token.Lexeme + " " + result.RightExpression.Token.Lexeme;
                        }
                        data.Add(expression as TypedExpression);
                        if (lookAhead.TokenType == TokenType.Percentaje) {
                            Match(TokenType.Percentaje);
                            if (lookAhead.TokenType == TokenType.Identifier)
                            {
                                var symbol = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                                var resultFind = GetIntFloatsVariable(lookAhead.Lexeme);
                                if (resultFind == null) {
                                    throw new ApplicationException($"Syntax error! Expected variable type int or float but found  variable {lookAhead.Lexeme}");
                                }
                                Match(TokenType.Identifier);
                            }
                            else if (lookAhead.TokenType == TokenType.IntConstant)
                            {
                                Match(TokenType.IntConstant);
                            }
                        }
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
                           //  var validateRelationalExpression = expression as TypedExpression;
                         //   validateRelationalExpression.GetExpressionType();
                            RelationalExpression result = expression as RelationalExpression;
                            bff += "while(" + result.LeftExpression.Token.Lexeme + " " + result.Token.Lexeme + " " + result.RightExpression.Token.Lexeme;
                        }

                        data.Add(expression as TypedExpression);
                        if (lookAhead.TokenType == TokenType.Percentaje)
                        {
                            Match(TokenType.Percentaje);
                            if (lookAhead.TokenType == TokenType.Identifier)
                            {
                                var symbol = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                                var resultFind = GetIntFloatsVariable(lookAhead.Lexeme);
                                if (resultFind == null)
                                {
                                    throw new ApplicationException($"Syntax error! Expected variable type int or float but found  variable {lookAhead.Lexeme}");
                                }
                                Match(TokenType.Identifier);
                            }
                            else if (lookAhead.TokenType == TokenType.IntConstant)
                            {
                                Match(TokenType.IntConstant);
                            }
                        }

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

                        
                    }else if(expression is Constant) {
                        if (expression.type == tuple.Item3.type) {
                            bff += expression.Generate() + ";" + Environment.NewLine;
                        } else {
                            throw new ApplicationException($"Syntax error! expected return type {tuple.Item3.type} but found {expression.type}.");
                        }
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
            bff = "";
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
            var variable = expression.Token;
            if (GetBoolVariable(variable.Lexeme) != null) {
                if (lookAhead.Lexeme == ">" || lookAhead.Lexeme == "<"
                    || lookAhead.Lexeme == ">=" || lookAhead.Lexeme == "<=") {
                    throw new ApplicationException($"Syntax error! operator {lookAhead.Lexeme} cannot be applied o operands of type bool . Line: {variable.Line}, Column: {variable.Column}");
                }
            }

            if (this.lookAhead.TokenType == TokenType.LessThan
                || this.lookAhead.TokenType == TokenType.GreaterThan
                || this.lookAhead.TokenType == TokenType.LessOrEqualThan
                || this.lookAhead.TokenType == TokenType.GreaterOrEqualThan) {
                var token = lookAhead;//el operador
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
            while (this.lookAhead.TokenType == TokenType.Asterisk || this.lookAhead.TokenType == TokenType.Division || this.lookAhead.TokenType==TokenType.Percentaje) {
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
                    if (isSumFunction) {
                        paramsAssignment += constant.Generate();
                    }
                    Match(TokenType.IntConstant);
                    if (TokenType.Plus == this.lookAhead.TokenType && isSumFunction) {
                        paramsAssignment += " + ";
                    }
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
                case TokenType.IntParseKeyword:
                    constant = new Constant(new Token { Lexeme = "Number" }, Type.ReadLine);
                    Match(TokenType.IntParseKeyword);
                    Match(TokenType.LeftParens);
                    Match(TokenType.ReadLineKeyword);
                    Match(TokenType.LeftParens);
                    Match(TokenType.RightParens);
                    Match(TokenType.RightParens);
                    return constant;
                case TokenType.FloatParseKeyword:
                    constant = new Constant(new Token { Lexeme = "Number" }, Type.ReadLine);
                    Match(TokenType.FloatParseKeyword);
                    Match(TokenType.LeftParens);
                    Match(TokenType.ReadLineKeyword);
                    Match(TokenType.LeftParens);
                    Match(TokenType.RightParens);
                    Match(TokenType.RightParens);
                    return constant;
                case TokenType.BoolParseKeyword:
                    constant = new Constant(new Token { Lexeme = "Boolean" }, Type.ReadLine);
                    Match(TokenType.BoolParseKeyword);
                    Match(TokenType.LeftParens);
                    Match(TokenType.ReadLineKeyword);
                    Match(TokenType.LeftParens);
                    Match(TokenType.RightParens);
                    Match(TokenType.RightParens);
                    return constant;
                default:
                    Symbol symbol = null;
                    if (lookAhead.Lexeme.Contains(".")) {
                        int pos = lookAhead.Lexeme.IndexOf('.')+1;
                        string function = lookAhead.Lexeme.Substring(pos);
                        int posVar = pos - 1;
                         string clase = lookAhead.Lexeme.Substring(0, posVar);
                        Match(TokenType.Identifier);
                        return getParameters(clase, function);
                    } else {
                        symbol = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
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
                                } 
                                else if (Assign.Attributes[i] is ArithmeticOperator) {
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
                                isSumFunction = true;
                            } 
                            expressions.Clear();

                            return Assign.Id;
                        }
                        expressions.Clear();
                    }
                    return symbol.Id;
            }
        }
        private Expression getParameters(string clase,string function) {
            var symbol = EnvironmentManager.GetSymbol(clase);
            //for (int i = 0; i < symbol.; i++) {

            //}
            for (int j = 0; j < symbol.Value.Count; j++) {
                if (symbol.Value[j].Id.Token.Lexeme == function) {
                    if (symbol.Value[j].SymbolType == SymbolType.Method) {
                        var AssSymbol = symbol.Value[j] as Symbol;
                        var Assign = GetFunctionStmt(AssSymbol) as ParamsValueFunction;
                        paramsAssignment += clase+"."+Assign.Id.Token.Lexeme + " (";
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
                        } 
                        expressions.Clear();

                        return Assign.Id;
                    }
                    break;
                }
            }
            expressions.Clear();

            return new Id(null, Type.Void);
        }
        bool isSumFunction = false;
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
            if (isSumFunction) {
                isSumFunction = false;
            }
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
           var isClass =  classes.TryGetValue(this.lookAhead.Lexeme, out var found);
            if (this.lookAhead.TokenType == TokenType.IntKeyword ||
                this.lookAhead.TokenType == TokenType.FloatKeyword ||
                this.lookAhead.TokenType == TokenType.StringKeyword ||
                this.lookAhead.TokenType == TokenType.BoolKeyword ||
                this.lookAhead.TokenType == TokenType.DateTimeKeyword||
                this.lookAhead.TokenType==TokenType.ListKeyword ||
                isClass)
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
                    this.lstIntFloats.Add(token);
                    Match(TokenType.Identifier);
                    var isInitialize = lookAhead;
                    if (isInitialize.TokenType == TokenType.Assignation) {
                        id = new Id(token, Type.Float);
                        var assignation = AssignStmt(id) as AssignationStatement;
                        assignation.ValidateSemantic();

                        if (paramsAssignment == "") {
                            bff += "let " + assignation.Generate() + Environment.NewLine;
                        } else {
                            bff += "let " + token.Lexeme + " = " + paramsAssignment + ";"+ Environment.NewLine;

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
                    this.lstIntFloats.Add(token);
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
                                bff += "let " + token.Lexeme + " = " + paramsAssignment +";"+ Environment.NewLine;
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
                            bff += "const "+assignation.Id.Token.Lexeme+"= " +"Date.now();" + Environment.NewLine;
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
                    lstBooleans.Add(token);
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
                case TokenType.Identifier:


                    if (classes.TryGetValue(this.lookAhead.Lexeme, out var found)) {
                        var Token = this.lookAhead;
                        string clase = this.lookAhead.Lexeme;
                        Match(TokenType.Identifier);
                        string variable = this.lookAhead.Lexeme;
                        bff += "let " + this.lookAhead.Lexeme + "  ";
                        Match(TokenType.Identifier);
                        if (lookAhead.TokenType != TokenType.Assignation) {
                            throw new ApplicationException($"Syntax error! You Must be Initialized the Variable Type Object");

                        }
                        Match(TokenType.Assignation);
                        Match(TokenType.NewKeyword);
                        if (Token.Lexeme != lookAhead.Lexeme) {
                            throw new ApplicationException($"Syntax error! Identifier Left Not Equal To Right Identifier!");
                        }
                        Match(TokenType.Identifier);
                        Match(TokenType.LeftParens);
                        Match(TokenType.RightParens);
                        Match(TokenType.SemiColon);
                        id = new Id(Token, Type.Class);
                        bff += " = new " + clase + "();" + Environment.NewLine;

                        EnvironmentManager.AddVariableWithValue(variable, id, found);
                    } 
                    break;
                default:

                    break;
            }
            paramsAssignment = "";
            isSumFunction = false;
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

        private Token GetBoolVariable(string lexeme) {
            return this.lstBooleans.Find(x => x.Lexeme == lexeme);
        }


        private Token GetIntFloatsVariable(string lexeme)
        {
            return this.lstIntFloats.Find(x => x.Lexeme == lexeme);
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
