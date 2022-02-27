using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MathExpParser
{
    public class MathParser
    {
        public delegate void MathParserCallback(float answer);
        public bool debugMode = false;
        public bool cacheMode = true;

        Tokenizer _tokenizer;
        ShuntingYard _shuntinYard;
        ShuntingYardParser _shuntinYardParser;
        Dictionary<string, float> _customLookupTable;
        Dictionary<string, List<Token>> _cache;

        #region Public API
        public MathParser()
        {
            _tokenizer = new Tokenizer();
            _shuntinYard = new ShuntingYard();
            _shuntinYardParser = new ShuntingYardParser();
            _customLookupTable = new Dictionary<string, float>();
            _cache = new Dictionary<string, List<Token>>();
        }

        /// <summary>
        /// Global Variable lookuptable
        /// </summary>
        /// <param name="p_lookupTable"></param>
        public void SetVariableLookUpTable(Dictionary<string, float> p_lookupTable)
        {
            _customLookupTable = p_lookupTable;
        }

        /// <summary>
        /// Calculate in main thread
        /// </summary>
        /// <param name="raw_syntax"></param>
        /// <param name="p_varaibleLookupTable"></param>
        /// <returns></returns>
        public float Calculate(string raw_syntax, Dictionary<string, float> p_varaibleLookupTable = null)
        {
            string preparedSyntax = PrepareExpressionSyntax(raw_syntax);

            //Check cache
            if (_cache.TryGetValue(preparedSyntax, out List<Token> cacheToken))
            {
                return ParseShuntingYardToken(cacheToken, p_varaibleLookupTable);
            }

            return PureCalculate(preparedSyntax, p_varaibleLookupTable);
        }

        /// <summary>
        /// Do it in asyn Task
        /// </summary>
        /// <param name="raw_syntax"></param>
        /// <param name="mathParserCallback"></param>
        /// <param name="p_varaibleLookupTable"></param>
        public async void CalculateAsyn(string raw_syntax, MathParserCallback mathParserCallback, Dictionary<string, float> p_varaibleLookupTable = null)
        {
            string preparedSyntax = PrepareExpressionSyntax(raw_syntax);

            //Check cache
            if (_cache.TryGetValue(preparedSyntax, out List<Token> cacheToken))
            {
                if (mathParserCallback != null)
                    mathParserCallback(ParseShuntingYardToken(cacheToken, p_varaibleLookupTable));

                return;
            }

            float answer = await Task.Run(() => {
                return PureCalculate(preparedSyntax);
            });

            if (mathParserCallback != null)
                mathParserCallback(answer);
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Clean up raw data, to what parser can read
        /// </summary>
        /// <param name="raw_syntax"></param>
        /// <returns></returns>
        private string PrepareExpressionSyntax(string raw_syntax) {
            string prepareSyntax = ReplaceVariable(StaticDataSet.PredefineVariableTable, raw_syntax);
            return prepareSyntax.ToLower();
        }

        private float PureCalculate(string math_expression, Dictionary<string, float> p_varaibleLookupTable = null)
        {
            List<Token> parsedToken = ParseMathExpression(math_expression);

            return ParseShuntingYardToken(parsedToken, p_varaibleLookupTable);
        }

        /// <summary>
        /// Return ShuntingYard symbol for later use.
        /// </summary>
        /// <param name="math_expression">string of math expression</param>
        /// <returns></returns>
        public List<Token> ParseMathExpression(string math_expression)
        {
            var tokens = _tokenizer.Parse(math_expression);

            var tokenList = _shuntinYard.Parse(tokens);

            if (debugMode)
                TokenToStringLog(tokens);

            if (cacheMode) {
                lock (_cache) {
                    _cache = UtilityFunc.SaveDictionary<List<Token>>(_cache, math_expression, tokenList);
                }
            }

            return tokenList;
        }

        /// <summary>
        /// Solve for answer
        /// </summary>
        /// <param name="p_shuntinYardTokens">Parsed ShuntinYard Tokens</param>
        /// <param name="p_varaibleLookupTable">(Optional) LookupTable for varaible, will override the previous lookuptable data</param>
        /// <returns></returns>
        public float ParseShuntingYardToken(List<Token> p_shuntinYardTokens, Dictionary<string, float> p_varaibleLookupTable = null)
        {
            if (p_varaibleLookupTable != null)
                _customLookupTable = p_varaibleLookupTable;

            p_shuntinYardTokens = ReplaceVariable(_customLookupTable, p_shuntinYardTokens);

            return _shuntinYardParser.Parse(p_shuntinYardTokens);
        }

        /// <summary>
        /// Replace the rest of symbol into number
        /// </summary>
        /// <param name="lookupTable"></param>
        /// <param name="tokens"></param>
        /// <returns></returns>
        private List<Token> ReplaceVariable(Dictionary<string, float> lookupTable, List<Token> tokens)
        {
            if (lookupTable == null)
                return tokens;

            for (int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];
                if (token._type == Token.Types.Variable && lookupTable.ContainsKey(token._value))
                {
                    tokens[i] = new Token(lookupTable[token._value].ToString(), Token.Types.Number);
                }
            }

            return tokens;
        }

        /// <summary>
        /// Replace symbol from character to number according to definition
        /// </summary>
        /// <param name="lookupTable"></param>
        /// <param name="p_raw_input"></param>
        /// <returns></returns>
        private string ReplaceVariable(Dictionary<string, float> lookupTable, string p_raw_input)
        {
            foreach (KeyValuePair<string, float> predefineTable in lookupTable)
                p_raw_input = p_raw_input.Replace(predefineTable.Key, predefineTable.Value.ToString());

            return p_raw_input;
        }
        #endregion

        #region Debug Functions
        private void TokenToStringLog(List<Token> tokens)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                Debug.Log(i + " => " + tokens[i]._type + "(" + tokens[i]._value + ")");
            }
        }

        private void RPNToStringLog(List<Token> tokens)
        {
            string groupString = "";
            foreach (Token t in tokens)
            {
                groupString += t._value + " ";
            }

            Debug.Log(groupString);
        }
        #endregion
    }
}