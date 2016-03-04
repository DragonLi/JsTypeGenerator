using System;
using System.Collections.Generic;
using System.IO;
using Corex.IO.Tools;

namespace JsTypeGenerator
{
    internal class JsTypeGeneratorArgs
    {
        private static readonly ToolArgsInfo<JsTypeGeneratorArgs> Info = new ToolArgsInfo<JsTypeGeneratorArgs>
        {
            Error = Console.WriteLine
        };

        public JsTypeGeneratorArgs()
        {
            Files = new List<string>();
        }

        [ToolArgCommand]
        public List<string> Files { get; private set; }

        public string Service { get; set; }

        [ToolArgSwitch("?")]
        public bool Help { get; set; }

        /// <summary>
        ///     designates the current directory that all paths are relative to
        /// </summary>
        [ToolArgSwitch("dir")]
        public string CurrentDirectory { get; set; }

        [ToolArgSwitch("out")]
        public string Output { get; set; }

        [ToolArgSwitch("template")]
        public string Template { get; set; }

        [ToolArgSwitch("format")]
        public string Format { get; set; }

        public static JsTypeGeneratorArgs Parse(string[] args)
        {
            return Info.Parse(args);
        }

        public static void GenerateHelp(TextWriter writer)
        {
            Info.HelpGenerator.Generate(writer);
        }
    }
}