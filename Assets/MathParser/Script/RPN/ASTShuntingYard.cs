using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MathExpParser
{
    /// <summary>
    /// Detail here https://en.wikipedia.org/wiki/Shunting-yard_algorithm
    /// </summary>
    public class ASTShuntingYard
    {
        #region Parameters
        //private List<Token> outputQueue;
        private Stack<Token> operatorStack;

        private OutStack outputStack;


        private readonly Dictionary<string, string> AssociativityDict = new Dictionary<string, string> {
            { "^", "right"},
            { "*", "left"},
            { "/", "left"},
            { "+", "left"},
            { "-", "left"}
        };

        private readonly Dictionary<string, int> PrecedenceDict = new Dictionary<string, int> {
            { "^", 4},
            { "*", 3},
            { "/", 3},
            { "+", 2},
            { "-", 2}
        };
        #endregion

        public ASTShuntingYard() {
            //outputQueue = new List<Token>();
            operatorStack = new Stack<Token>();
            outputStack = new OutStack();
        }

        public ASTNode Parse(List<Token> p_tokens)
        {
            Clear();
            int tokenLength = p_tokens.Count;

            for (int i = 0; i < tokenLength; i++)
            {
                var t = p_tokens[i];
                //if the token is a number, then:
                //push it to the output queue.
                if (t._type == Token.Types.Number || t._type == Token.Types.Variable)
                {
                    outputStack.stacks.Push( new ASTNode(t));
                }

                //if the token is a function then:
                //push it onto the operator stack
                else if (t._type == Token.Types.Function)
                {
                    operatorStack.Push(t);
                }

                //If the token is a function argument separator 
                else if (t._type == Token.Types.ArgumentSeperator)
                {
                    //Until the token at the top of the stack is a left parenthesis
                    //pop operators off the stack onto the output queue.
                    while (operatorStack.Count > 0
                        && operatorStack.Peek()._type != Token.Types.LeftParenthesis)
                    {
                        outputStack.AddNode(operatorStack.Pop());
                    }

                }

                //if the token is an operator, then:
                else if (t._type == Token.Types.Operator)
                {
                    //while there is an operator token o2, at the top of the operator stack and either
                    while (operatorStack.Count > 0 && (operatorStack.Peek()._type == Token.Types.Operator)
                      //o1 is left-associative and its precedence is less than or equal to that of o2, or
                      && ((GetAssociativity(t._value) == "left" && GetPrecedence(t._value) <= GetPrecedence(operatorStack.Peek()._value))
                          //o1 is right associative, and has precedence less than that of o2,
                          || (GetAssociativity(t._value) == "right" && GetPrecedence(t._value) < GetPrecedence(operatorStack.Peek()._value))))
                    {
                        outputStack.AddNode(operatorStack.Pop());
                    }

                    //at the end of iteration push o1 onto the operator stack
                    operatorStack.Push(t);
                }


                //if the token is a left paren (i.e. "("), then:
                else if (t._type == Token.Types.LeftParenthesis)
                {
                    operatorStack.Push(t);
                }

                //if the token is a right paren(i.e. ")"), then:
                else if (t._type == Token.Types.RightParenthesis)
                {
                    //while the operator at the top of the operator stack is not a left paren:
                    //pop the operator from the operator stack onto the output queue.
                    while (operatorStack.Count > 0 && operatorStack.Peek()._type != Token.Types.LeftParenthesis)
                    {
                        outputStack.AddNode(operatorStack.Pop());
                    }

                    //if there is a left paren at the top of the operator stack, then:
                    //pop the operator from the operator stack and discard it
                    //if (operatorStack.Count > 0 && operatorStack.Peek()._type == Token.Types.LeftParenthesis)
                    operatorStack.Pop();

                    //after while loop, if operator stack not null, pop everything to output queue
                    if (operatorStack.Count > 0 && operatorStack.Peek()._type == Token.Types.Function)
                        outputStack.AddNode(operatorStack.Pop());
                }
            }

            //if there are no more tokens to read:
            //while there are still operator tokens on the stack:
            //pop the operator from the operator stack onto the output queue.
            while (operatorStack.Count > 0)
                outputStack.AddNode(operatorStack.Pop());

            return outputStack.stacks.Pop();
        }

        private string GetAssociativity(string p_operator) {
            if (AssociativityDict.ContainsKey(p_operator)) {
                return AssociativityDict[p_operator];
            }

            return "right";
        }

        private int GetPrecedence(string p_operator)
        {
            if (PrecedenceDict.ContainsKey(p_operator))
            {
                return PrecedenceDict[p_operator];
            }

            return 5;
        }


        private void Clear()
        {
            outputStack.stacks.Clear();
            operatorStack.Clear();
        }

        private class OutStack {
            public Stack<ASTNode> stacks = new Stack<ASTNode>();

            public void AddNode(Token token) {
                ASTNode rightNode = (stacks.Count > 0) ? stacks.Pop() : null;
                ASTNode leftNode = (stacks.Count > 0) ? stacks.Pop() : null;

                stacks.Push(new ASTNode(token, leftNode, rightNode));
            }

        }

    }
}