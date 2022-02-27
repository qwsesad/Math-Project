using System.Collections.Generic;
using UnityEngine;

namespace MathExpParser
{
    public class ShuntingYardParser
    {
        #region Parameter
        /// <summary>
        /// string = function value
        /// int = needed input length
        /// </summary>
        private Dictionary<string, int> FunctionLookUpTable = new Dictionary<string, int> {
            { "sin", 1},
            { "cos", 1},
            { "tan", 1},
            { "arcsine", 1},
            { "arccos", 1},
            { "atan", 1},
            { "sqrt", 1},
            { "abs", 1},
            { "floor", 1},
            { "ceil", 1},
            { "round", 1},
            { "sign", 1},

            { "step" , 2},
            { "min", 2},
            { "max", 2},
            { "rand", 2},
            { "atan2", 2},
            { "pow", 2},

            { "clamp", 3},
            { "lerp" , 3},
            { "smoothstep", 3}
        };

        #endregion

        public float Parse(List<Token> shuntingYard_tokens) {
            Stack<float> outputStack = new Stack<float>();

            try


            {
                foreach (Token t in shuntingYard_tokens)
                {
                    if (t._type == Token.Types.Number)
                        outputStack.Push(float.Parse(t._value));

                    else if (t._type == Token.Types.Operator)
                    {
                        float rightInput = outputStack.Pop(),
                            leftInput = outputStack.Pop();

                        outputStack.Push(ComputeOperatorToken(t, leftInput, rightInput));
                    }
                    else if (t._type == Token.Types.Function) {
                        if (FunctionLookUpTable.ContainsKey(t._value))
                        {
                            float[] inputArray = new float[FunctionLookUpTable[t._value]];
                            for (int i = 0; i < inputArray.Length; i++)
                                inputArray[i] = outputStack.Pop();

                            System.Array.Reverse(inputArray);
                            outputStack.Push(ComputeFunctionToken(t, inputArray ));
                        }
                        else {
                            Debug.LogError("Function " + t._value + " is not define");
                            break;
                        }
                    }
                }
            }
            catch {
                Debug.LogError("Encounter incorrect syntax, have you assign value to variable?");
            }

            if (outputStack.Count <= 0)
                return 0;

            return outputStack.Pop();
        }

        private float ComputeOperatorToken(Token token, float leftInput, float rightInput) {
            switch (token._value)
            {
                case "+":
                    return leftInput + rightInput;

                case "-":
                    return leftInput - rightInput;

                case "/":
                    return leftInput / rightInput;

                case "*":
                    return leftInput * rightInput;

                case "^":
                    return (float)System.Math.Pow(leftInput, rightInput);

                default:
                    return 0;
            }
        }

        private float ComputeFunctionToken(Token token, float[] input)
        {
            switch (token._value)
            {
                #region Function with ONE input
                case "sin":
                    return Mathf.Sin(input[0]);

                case "cos":
                    return Mathf.Cos(input[0]);

                case "tan":
                    return Mathf.Tan(input[0]);

                case "arcsine":
                    return Mathf.Asin(input[0]);

                case "arccos":
                    return Mathf.Acos(input[0]);

                case "atan":
                    return Mathf.Atan(input[0]);

                case "sqrt":
                    return Mathf.Sqrt(input[0]);

                case "abs":
                    return Mathf.Abs(input[0]);

                case "floor":
                    return Mathf.FloorToInt(input[0]);

                case "ceil":
                    return Mathf.CeilToInt(input[0]);

                case "round":
                    return Mathf.RoundToInt(input[0]);

                case "sign":
                    return Mathf.Sign(input[0]);
                #endregion

                #region Function with Two inputs
                case "rand":
                    return Random.Range(input[0], input[1]);

                case "min":
                    return Mathf.Min(input);

                case "max":
                    return Mathf.Max(input);

                case "atan2":
                    return Mathf.Atan2(input[0], input[1]);

                case "pow":
                    return Mathf.Pow(input[0], input[1]);

                case "step":
                    return input[1] >= input[0] ? 1 : 0;
                #endregion

                #region Function with THREE input
                case "clamp":
                    return Mathf.Clamp(input[0], input[1], input[2]);

                case "lerp":
                    return input[0] + (input[2] * (input[1] - input[0]));

                case "smoothstep":
                    return Mathf.SmoothStep(input[0], input[1] , input[2]);
                    #endregion

                default:
                    return 0;
            }
        }

    }
}