using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;

namespace CoverageAnalysis.Rules
{
    public static class Settings
    {
        public static Rules Rules = new Rules();

        public static void Init()
        {
            var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "appsettings.json"));
            Settings.Rules = JsonConvert.DeserializeObject<Rules>(json);
        }
    }
    public class Rules
    {
        public Source Source { get; set; }
        public Target[] Targets { get; set; }
    }

    public class Source
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public SyntaxType SyntaxType { get; set; }
        public string[] BaseTypes { get; set; }
        public bool IgnoreAbstract { get; set; }
    }

    public class Target
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public SyntaxType SyntaxType { get; set; }
        public string[] BaseTypes { get; set; }
        public string[] Attributes { get; set; }
    }

    public enum SyntaxType
    { 
        ClassDeclaration,
        MethodDeclaration
    }
}



