using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem;

namespace JsTypeGenerator
{
    internal class JsTypeGenerator
    {
        public JsTypeGeneratorArgs Args { get; set; }
        public string[] CommandLineArguments { get; set; }
        public List<IUnresolvedTypeDefinition> TypeDefinitions { get; set; }

        public int Run()
        {
            int x = InternalRun();
            return x;
        }

        private int InternalRun()
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
#if DEBUG
                PrintArgs();
#endif
                Time(ParseArgs);

                if (Help())
                    return 0;

                Time(ParseCSharpCode);
                Time(GenerateJsTypeConfig);

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
        }

        private void ParseArgs()
        {
            Args = JsTypeGeneratorArgs.Parse(CommandLineArguments);
            if (Args.CurrentDirectory.IsNotNullOrEmpty())
                Directory.SetCurrentDirectory(Args.CurrentDirectory);
        }

        private void ParseCSharpCode()
        {
            var parser = new CSharpParser();
            TypeDefinitions = new List<IUnresolvedTypeDefinition>();

            foreach (string file in Args.Files)
            {
                if (File.Exists(file))
                {
                    string code = File.ReadAllText(file);
                    var syntaxTree = parser.Parse(code, file);
                    var unresolvedFile = syntaxTree.ToTypeSystem();
                    if (unresolvedFile.TopLevelTypeDefinitions == null ||
                        unresolvedFile.TopLevelTypeDefinitions.Count == 0)
                    {
                        Console.WriteLine("Warning:<{0}> has nothing type definition", file);
                    }
                    else
                    {
                        RecursiveParseTypeDefinition(unresolvedFile.TopLevelTypeDefinitions);
                    }
                }
                else
                {
                    Console.WriteLine("Error:<{0}> is not found", file);
                }
            }
        }

        private void RecursiveParseTypeDefinition(IList<IUnresolvedTypeDefinition> typeDefinitions)
        {
            if (typeDefinitions.Count == 0)
                return;

            foreach (var type in typeDefinitions)
            {
                TypeDefinitions.Add(type);
                RecursiveParseTypeDefinition(type.NestedTypes);
            }
        }

        private void GenerateJsTypeConfig()
        {
            var typeNameSet = new HashSet<string>();
            var ignoreNameSet = new HashSet<string>();

            //遍历所有类型定义，将带有JsType属性的加入忽略列表中
            foreach (var typeDefinition in TypeDefinitions)
            {
                string typeName = typeDefinition.FullTypeName.ReflectionName;
                typeNameSet.Add(typeName);
                if (typeDefinition.Attributes != null && typeDefinition.Attributes.Count > 0)
                {
                    if (typeDefinition.Attributes.Any(attribute =>
                    {
                        var csAtr = attribute as CSharpAttribute;
                        if (csAtr != null && csAtr.AttributeType.ToString().Contains("JsType"))
                        {
                            return true;
                        }
                        return false;
                    }))
                    {
                        ignoreNameSet.Add(typeName);
                    }
                }
            }

            typeNameSet.ExceptWith(ignoreNameSet);

            string outputDir = Path.GetDirectoryName(Args.Output);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var fileBuilder = new StringBuilder();
            foreach (string typeName in typeNameSet)
            {
                fileBuilder.AppendFormat(Args.Format, typeName);
                fileBuilder.AppendLine();
            }

            string content = string.Format(Args.Template, fileBuilder);
            File.WriteAllText(Args.Output, content);
        }

        [DebuggerStepThrough]
        private void Time(Action action)
        {
            var stopwatch = new Stopwatch();
            Console.WriteLine("{0:HH:mm:ss.fff}: {1}: Start: ", DateTime.Now, action.Method.Name);
            stopwatch.Start();
            action();
            stopwatch.Stop();
            Console.WriteLine("{0:HH:mm:ss.fff}: {1}: End: {2}ms", DateTime.Now, action.Method.Name,
                stopwatch.ElapsedMilliseconds);
        }

        private bool Help()
        {
            if (Args.Help)
            {
                JsTypeGeneratorArgs.GenerateHelp(Console.Out);
                return true;
            }
            return false;
        }

        private void PrintArgs()
        {
            Console.WriteLine(Process.GetCurrentProcess().MainModule.FileName + " " + ArgsToString());
        }

        private string ArgsToString()
        {
            var sb = new StringBuilder();
            CommandLineArguments.ForEachJoin(arg =>
            {
                if (arg.StartsWith("@"))
                    sb.Append(File.ReadAllText(arg.Substring(1)));
                else
                    sb.Append(arg);
            }, () => sb.Append(" "));
            string s = sb.ToString();
            if (!s.Contains("/dir"))
                s = string.Format("/dir:\"{0}\" ", Directory.GetCurrentDirectory()) + s;
            return s;
        }
    }
}