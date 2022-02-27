using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MathNet.Symbolics;
using Expr = MathNet.Symbolics.SymbolicExpression;

public class Lab11GetIntervals : MonoBehaviour
{
    Expr exp;
    public double a;
    public double b;
    public InputField A;
    public InputField B;
    public ErrorS er;
    public bool Get()
    {
        var symbols = new Dictionary<string, FloatingPoint> { { "x", 2.0 } };
        if (A.text != "")
        {
            exp = Expr.Parse(A.text);
            a = exp.Evaluate(symbols).RealValue;
        }
        else
        {
            er.SetUp("Enter a!");
            return false;
        }
        if (B.text != "")
        {
            exp = Expr.Parse(B.text);
            b = exp.Evaluate(symbols).RealValue;
        }
        else
        {
            er.SetUp("Enter b!");
            return false;
        }
        return true;
    }

}
