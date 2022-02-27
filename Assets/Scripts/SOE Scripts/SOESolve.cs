using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using MathNet.Symbolics;
using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using Expr = MathNet.Symbolics.SymbolicExpression;
using Quaternion = UnityEngine.Quaternion;
using TMPro;

public class SOESolve : MonoBehaviour
{
    public TMP_InputField input;
    public ErrorS error;
    public Text Output;
    public int method;
    double eps;
    int N;
    public char[] Symbols;
    Vector<double> X0;
    Vector<double> X;
    public string[] functions;
    Dictionary<string, FloatingPoint> variables;
    Expr[] system;
    Expr[][] W;

    public void Go()
    {
        Output.text = "";
        if (!GetInput())
            return;

        if (!FDParse())
            return;

        try
        {
            switch(method)
            {
                case 1:
                    NewtonsMethod();
                    break;
                case 2:
                    SimpleIterationsMethod();
                    break;
                default:
                    break;
            }
        }
        catch
        {
            error.SetUp("Ошибка при выполнении метода");
        }

    }

    bool GetInput()
    {
        if (input.text == "")
        {
            error.SetUp("Введите систему!");
            return false;
        }

        try
        {
            int inputlength = input.text.Length;
            int i = 0;
            int i0 = i;
            int j;

            while (input.text[i] != '\0' && input.text[i] != '\n' && input.text[i] != ' ' && i != (inputlength - 1))
                i++;
            i++;
            N = int.Parse(input.text.Substring(0, i));

            if (N < 2)
            {
                error.SetUp("N должно быть больше 1!");
                return false;
            }

            i0 = i;
            while (input.text[i] != '\0' && input.text[i] != '\n' && i != (inputlength - 1))
                i++;
            i++;
            eps = double.Parse(input.text.Substring(i0, i-i0));

            if (eps <= 0)
            {
                error.SetUp("eps должно быть больше 0!");
                return false;
            }

            Symbols = new char[N];
            variables = new Dictionary<string, FloatingPoint>();
            for (j = 0; j < N; j++)
            {
                Symbols[j] = input.text[i];
                i++;
                i++;
            }

            i0 = i;
            X0 = Vector<double>.Build.Dense(N);
            X = Vector<double>.Build.Dense(N);
            for (j = 0; j < N; j++)
            {
                while (input.text[i] != ' ' && input.text[i] != '\0' && input.text[i] != '\n' && i != (inputlength - 1))
                    i++;
                X0[j] = double.Parse(input.text.Substring(i0, i - i0));
                X[j] = X0[j];
                variables.Add(Symbols[j].ToString(), X0[j]);
                i++;
                i0 = i;

            }

            functions = new string[N];
            for (j = 0; j < N; j++)
            {
                while (input.text[i] != '\0' && input.text[i] != '\n' && i != (inputlength - 1))
                    i++;
                functions[j] = input.text.Substring(i0, i - i0 + 1);
                i++;
                i0 = i;

            }
            return true;
        }
        catch
        {
            error.SetUp("Ошибка в вводе!");
            return false;
        }
    }

    bool FDParse()
    {
        try
        {
            int i, j;
            system = new Expr[N];
            W = new Expr[N][];
            for (i = 0; i < N; i++)
            {
                system[i] = Expr.Parse(functions[i]);
                W[i] = new Expr[N];
                for (j = 0; j < N; j++)
                {
                    var x_ = Expr.Variable(Symbols[j].ToString());
                    W[i][j] = system[i].Differentiate(x_);
                }
            }
            return true;
        }
        catch
        {
            error.SetUp("Ошибка при парсинге");
            return false;
        }
    }

    void NewtonsMethod()
    {
        Matrix<double> WE = Matrix<double>.Build.Dense(N, N);
        int i, j;
        double delta = 0;
        double Max = 0;
        double prom = 0;
        Vector<double> f = Vector<double>.Build.Dense(N);
        int k = 0;
        do
        {
            for (i = 0; i < N; i++)
            {
                for (j = 0; j < N; j++)
                {
                    WE[i, j] = W[i][j].Evaluate(variables).RealValue;
                }
            }
            WE = WE.Inverse();
            for (i = 0; i < N; i++)
            {
                f[i] = system[i].Evaluate(variables).RealValue;
            }
            for (i = 0; i < N; i++)
            {
                for (j = 0; j < N; j++)
                {
                    delta += WE[i, j] * f[j];
                }
                X[i] = X0[i] - delta;
                delta = 0;
            }
            Max = Math.Abs(X[0] - X0[0]);
            for (i = 1; i < N; i++)
            {
                prom = Math.Abs(X[i] - X0[i]);
                if (prom > Max)
                    Max = prom;
            }
            for (i = 0; i < N; i++)
            {
                variables.Remove(Symbols[i].ToString());
                variables.Add(Symbols[i].ToString(), X[i]);
                X0[i] = X[i];
            }
            k++;
        } while (Max >= eps && k < 1000);
        Output.text += "Newton's Method:\n";
        for (i = 0; i < N; i++)
            Output.text += "X" + (i + 1).ToString() + " = " + X[i].ToString() + "\n";
        Output.text += "k = " + k.ToString();
    }

    void SimpleIterationsMethod()
    {
        Matrix<double> WE = Matrix<double>.Build.Dense(N, N);
        int i, j;
        double Max = 0;
        double prom = 0;
        bool check = true;
        int k = 0;

        do
        {
            for (i = 0; i < N; i++)
            {
                for (j = 0; j < N; j++)
                {
                    WE[i, j] = W[i][j].Evaluate(variables).RealValue;
                }
            }
            Max = Math.Abs(WE[0, 0]);
            for (j = 0; i < N; j++)
            {
                for (i = 0; i < N; i++)
                {
                    prom += Math.Abs(WE[i, j]);
                }
                if (prom > Max)
                    Max = prom;
                prom = 0;
            }
            if (Max > 1)
            {
                Output.text += "Method of simple iteration:\n Достаточное условие не выполняется";
                check = false;
            }
            if (check)
            {
                for (i = 0; i < N; i++)
                {
                    X[i] = system[i].Evaluate(variables).RealValue;
                }
                Max = Math.Abs(X[0] - X0[0]);
                for (i = 1; i < N; i++)
                {
                    prom = Math.Abs(X[i] - X0[i]);
                    if (prom > Max)
                        Max = prom;
                }
                for (i = 0; i < N; i++)
                {
                    variables.Remove(Symbols[i].ToString());
                    variables.Add(Symbols[i].ToString(), X[i]);
                    X0[i] = X[i];
                }
                k++;
            }
        } while (check && Max >= eps && k < 1000);
        if (check)
        {
            Output.text += "Method of simple iteration:\n";
            for (i = 0; i < N; i++)
                Output.text += "X" + (i + 1).ToString() + " = " + X[i].ToString() + "\n";
            Output.text += "k = " + k.ToString();
        }
    }
}
