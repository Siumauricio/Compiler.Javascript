using Compiler.Core;
using Compiler.Lexer;
using System;
using System.IO;

namespace Compiler.Console
{
    class Program {
        static void Main(string[] args) {
            var code = File.ReadAllText("D:\\GitHub\\Compiler.Javascript\\Compiler.Console\\Code.txt").Replace(Environment.NewLine, "\n");
            var input = new Input(code);
            var scanner = new Scanner(input);//esto le devuelve los tokens digamos 
            var parser = new Parser.Parser(scanner);
            var ast = parser.Parse();
            //var engine = new CompilerEngine(parser);
            //engine.Run();
        }
    }
}

//    while (true) {
//        var token = scanner.GetNextToken();
//        System.Console.WriteLine(token.ToString());
//        if (token.Lexeme == "\0") {
//            System.Console.WriteLine("TODOS LOS TOKENS INGRESADOS SON VALIDOzsssS1");
//            break;
//        }
//    }
//}