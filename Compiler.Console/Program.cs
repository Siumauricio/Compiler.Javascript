﻿using Compiler.Core;
using Compiler.Lexer;
using System;
using System.IO;

namespace Compiler.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = File.ReadAllText("Code.txt").Replace(Environment.NewLine, "\n");
            var input = new Input(code);
            var scanner = new Scanner(input);
            while (true)
            {
                var token = scanner.GetNextToken();
                System.Console.WriteLine(token.ToString());

                if (token.Lexeme == "\0")
                {
                    System.Console.WriteLine("TODOS LOS TOKENS INGRESADOS SON VALIDOzsssS1");
                    break;
                }
            }
            //var scanner = new Scanner(input);
            //var parser = new Parser.Parser(scanner);
            //var engine = new CompilerEngine(parser);
            //engine.Run();
        }

   }
}
