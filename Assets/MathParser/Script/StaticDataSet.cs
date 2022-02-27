using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MathExpParser
{
    public class StaticDataSet
    {
        public class RegexSyntax {
            public const string IgnoreSpace = "\\s";
            public const string IsNumber = "\\d";
            public const string IsOperator = "\\+|-|\\*|\\/|\\^";
            public const string IsVariable = "[a-z]";
        }

        public static readonly Dictionary<string, float> PredefineVariableTable = new Dictionary<string, float>()
        {
            { "PI", 3.14159265359f},
            { "E", 2.71828f},
        };
    }
}