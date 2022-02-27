using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MathExpParser
{
    public class ASTNode
    {
        public Token token;
        public ASTNode leftChildNode;
        public ASTNode rightChildNode;

        public ASTNode(Token token, ASTNode leftChildNode = null, ASTNode rightChildNode = null)
        {
            this.token = token;
            this.leftChildNode = leftChildNode;
            this.rightChildNode = rightChildNode;
        }

        public float Solve()
        {
            if (token._type == Token.Types.Number) {
                return float.Parse(token._value);
            }


            float leftNumber = (leftChildNode == null) ? 0 : leftChildNode.Solve();
            float rightNumber = (rightChildNode == null) ? 0 : rightChildNode.Solve();

            Debug.Log(token._value + ", leftNumber" + leftNumber + ", rightNumber" + rightNumber);

            switch (token._value)
            {
                case "+":
                    return leftNumber + rightNumber;

                case "-":
                    return leftNumber - rightNumber;

                case "/":
                    return leftNumber / rightNumber;

                case "*":
                    return leftNumber * rightNumber;

                case "^":
                    return Mathf.Pow(leftNumber, rightNumber);

                case "sin":
                    return Mathf.Sin(rightNumber);

                default:
                    return 0;
            }
        }

        public void Render() {

            string rightNode = (rightChildNode == null) ? "null" : ("rightChildNode.token._type " + rightChildNode.token._type + " : rightChildNode.token._value " + rightChildNode.token._value) ;

            string leftNode = (leftChildNode == null) ? "null" : ("leftChildNode.token._type " + leftChildNode.token._type + " : leftChildNode.token._value " + leftChildNode.token._value);

            string message = string.Format("Token Type {0} : {1},  \n rightNode : {2} \n leftNode : {3}", token._type, token._value, rightNode, leftNode);

            Debug.Log(message);

            if (leftChildNode != null)
                leftChildNode.Render();

            if (rightChildNode != null)
                rightChildNode.Render();

        }
        


    }
}