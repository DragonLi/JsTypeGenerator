using System;
using System.Diagnostics;
using System.IO;
using Corex.IO.Tools;

namespace JsTypeGenerator
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments!");
                return 0;
            }

            string[] resolvedArgs = null;
            string paramFileTag = "/paramFile:";
            if (args.Length == 1 && args[0].StartsWith(paramFileTag))
            {
                string paramFile = args[0].Replace(paramFileTag, "");
                if (File.Exists(paramFile))
                {
                    string longArgs = File.ReadAllText(paramFile);
                    var tokenizer = new ToolArgsTokenizer();
                    resolvedArgs = tokenizer.Tokenize(longArgs);
                }
                else
                {
                    Console.WriteLine("Error:<{0}> is not found", paramFile);
                    return 0;
                }
            }

            if (resolvedArgs == null)
            {
                resolvedArgs = args;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            //Console.AutoFlush = true;
            var generator = new JsTypeGenerator {CommandLineArguments = resolvedArgs};
            int res = generator.Run();
            stopwatch.Stop();
            Console.WriteLine("Total: {0}ms", stopwatch.ElapsedMilliseconds);
#if DEBUG
            Console.ReadLine();
#endif
            return res;
        }
    }
}