using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MathExpParser
{
    public class Tokenizer
    {
        public List<Token> Parse(string p_raw_expression) {
            List<Token> tokens = new List<Token>();
            List<string> numberBuffer = new List<string>();
            List<string> letterBuffer = new List<string>();

            p_raw_expression = Regex.Replace(p_raw_expression, StaticDataSet.RegexSyntax.IgnoreSpace, "");

            for (int i = 0; i < p_raw_expression.Length; i++) {
                string part = p_raw_expression[i].ToString();

                if (IsNumber(part))
                {
                    numberBuffer.Add(part);
                }

                else if (part == ".") {
                    numberBuffer.Add(part);
                }

                else if (IsVariable(part))
                {
                    if (numberBuffer.Count > 0) {
                        tokens.AddRange(RetrieveNumberBuffer(numberBuffer));
                        tokens.Add(new Token("*", Token.Types.Operator));
                    }
                    letterBuffer.Add(part);
                    //tokens.Add(new Token(part, Token.Types.Variable));
                }
                else if (IsOperator(part))
                {
                    //Handle special case for "-"
                    if (part == "-")
                    {
                        var tokenCount = (tokens.Count);

                        if (tokenCount == 0 || tokens[tokenCount - 1]._type == Token.Types.LeftParenthesis || (tokens[tokenCount - 1]._type == Token.Types.Operator))
                        {

                            tokens.Add(new Token("-1", Token.Types.Number));
                            tokens.Add(new Token("*", Token.Types.Operator));
                            continue;
                        }
                    }

                    tokens.AddRange(RetrieveNumberBuffer(numberBuffer));
                    tokens.AddRange(RetrieveLetterBuffer(letterBuffer));

                    tokens.Add(new Token(part, Token.Types.Operator));
                    
                }
                else if (isLeftParenthesis(part))
                {
                    //If the char before leftParenthesis is letter, its  a function
                    if (letterBuffer.Count > 0)
                    {
                        tokens.Add(new Token(GetFullLetterString(letterBuffer), Token.Types.Function));
                        letterBuffer.Clear();
                    }
                    else if (numberBuffer.Count > 0) {
                        tokens.AddRange(RetrieveNumberBuffer(numberBuffer));
                        tokens.Add(new Token("*", Token.Types.Operator));
                    }

                    tokens.Add(new Token(part, Token.Types.LeftParenthesis));
                }
                else if (isRightParenthesis(part))
                {
                    tokens.AddRange(RetrieveLetterBuffer(letterBuffer));
                    tokens.AddRange(RetrieveNumberBuffer(numberBuffer));

                    tokens.Add(new Token(part, Token.Types.RightParenthesis));
                }
                else if (IsComma(part))
                {
                    tokens.AddRange(RetrieveNumberBuffer(numberBuffer));
                    tokens.AddRange(RetrieveLetterBuffer(letterBuffer));

                    tokens.Add(new Token(part, Token.Types.ArgumentSeperator));
                }
            }

            tokens.AddRange(RetrieveNumberBuffer(numberBuffer));
            tokens.AddRange(RetrieveLetterBuffer(letterBuffer));

            //Clear
            letterBuffer = null;
            numberBuffer = null;

            return tokens;
        }

        private string GetFullLetterString(List<string> letterBuffer)
        {
            return System.String.Join("", letterBuffer.ToArray());
        }

        private List<Token> RetrieveNumberBuffer(List<string> numberBuffer) {
            var r_tokens = new List<Token>();
            if (numberBuffer.Count > 0) {
                string fullDigitalString = System.String.Join("", numberBuffer.ToArray());

                r_tokens.Add(new Token(fullDigitalString, Token.Types.Number));
            }

            numberBuffer.Clear();
            return r_tokens;
        }

        private List<Token> RetrieveLetterBuffer(List<string> letterBuffer) {
            int length = letterBuffer.Count;
            var r_tokens = new List<Token>();
            for (int i = 0; i < length; i++) {
                r_tokens.Add(new Token(letterBuffer[i], Token.Types.Variable));

                if (i < length - 1) {
                    r_tokens.Add(new Token("*", Token.Types.Operator));
                }
            }

            letterBuffer.Clear();

            return r_tokens;
        }

        #region Qualifier Method
        private bool IsComma(string p_char)
        {
            return (p_char == ",");
        }

        private bool IsNumber(string p_char)
        {
            return Regex.IsMatch(p_char, StaticDataSet.RegexSyntax.IsNumber);
        }

        private bool IsVariable(string p_char)
        {
            return Regex.IsMatch(p_char, StaticDataSet.RegexSyntax.IsVariable);
        }

        private bool IsOperator(string p_char)
        {
            return Regex.IsMatch(p_char, StaticDataSet.RegexSyntax.IsOperator);
        }

        private bool isRightParenthesis(string p_char)
        {
            return (p_char == ")");
        }

        private bool isLeftParenthesis(string p_char)
        {
            return (p_char == "(");
        }

        #endregion

    }
}