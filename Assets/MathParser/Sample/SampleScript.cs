using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MathExpParser
{
    public class SampleScript : MonoBehaviour
    {
        public string raw_math_expression;

        Dictionary<string, float> demoLookupTable = new Dictionary<string, float>
            {
                {"p", 5 },
                { "t", 3}
            };

        MathParser mathParser;

        public void Execute() {
            mathParser = new MathParser();

            TestSyncMethod();
            TestAsyncMethod();
        }

        private void Start()
        {
            mathParser = new MathParser();
        }

        private void TestSyncMethod() {
            if (mathParser == null) return;

            mathParser.SetVariableLookUpTable(demoLookupTable);

            float syncAnswer = mathParser.Calculate(raw_math_expression);

            Debug.Log("Sync Answer " + syncAnswer);
        }

        private void TestAsyncMethod() {
            if (mathParser == null) return;

            mathParser.SetVariableLookUpTable(demoLookupTable);

            mathParser.CalculateAsyn(raw_math_expression, (float answer) =>
            {
                Debug.Log("Async Answer " + answer);
            });
        }

        private void TestThreadingMethod(int pressureVolum) {
            if (mathParser == null) return;

            for (int i = 0; i < pressureVolum; i++)
            {
                MathParserThreading.Instance.CalculateAsyn(raw_math_expression, (MathParserThreading.ParseResult result) =>
                {
                    Debug.Log("Threading Answer " + result.answer);
                }, demoLookupTable);
            }
        }

    }
}