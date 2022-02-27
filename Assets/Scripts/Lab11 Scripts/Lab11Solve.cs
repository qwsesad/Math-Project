using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using MathNet.Symbolics;
using MathNet.Numerics.Integration;
using Expr = MathNet.Symbolics.SymbolicExpression;
using Quaternion = UnityEngine.Quaternion;
using System.IO;

public class Lab11Solve : MonoBehaviour
{
    public double b;
    public double a;
    public int stepnumber;
    public int n; //n
    public int Method; //метод решени€
    public string func; //строка функции
    double[] t;
    Expr f; //функци€
    Dictionary<string, FloatingPoint> symbols; //основной словарь
    public ErrorS er; //обработчик ошибок
    public Text Output; //область вывода
    double rating;
    double step;

    //запуск решени€
    public void Go()
    {
        Output.text = ""; //очищаем вывод дл€ случаев, когда решение не первое
        SetUp(); //делаем первые объ€влени€

        //запускаем выбранный метод решени€
        switch (Method)
        {
            case 1:
                if (n < 2 || n == 8 || n >= 10)
                {
                    er.SetUp("Wrong N");
                    return;
                }
                ReadText();
                Output.text += "Chebyshev's Method\n";
                ChebyshevsMethod();
                break;
            case 2:
                Output.text += "Trapezoid Method\n";
                TrapezoidMethod();
                break;
            default:
                break;
        }
    }

    void SetUp()
    {
        step = (b - a) / stepnumber;
        t = new double[n];
        symbols = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //задание основного словар€
        f = Expr.Parse(func); //парсинг функции
    }

    void ReadText()
    {
        int i;
        double[][] mas = new double[9][];
        mas[0] = new double[2]{ -0.577350, 0.577350 };
        mas[1] = new double[3] { -0.707107, 0, 0.707107 };
        mas[2] = new double[4] { -0.794654, -0.187592, 0.187592, 0.794654 };
        mas[3] = new double[5] { -0.832498, -0.374541, 0, 0.374541, 0.832498 };
        mas[4] = new double[6] { -0.866247, - 0.422519, - 0.266635, 0.266635, 0.422519, 0.866247 };
        mas[5] = new double[7] { -0.883862, - 0.529657, - 0.323912, 0, 0.323912, 0.529657, 0.883862 };
        mas[6] = new double[0] { };
        mas[7] = new double[9] { -0.911589, - 0.601019, - 0.528762, - 0.167906, 0, 0.167906, 0.528762, 0.601019, 0.911589 };

        for (i = 0; i < n; i++)
        {
            t[i] = mas[n-2][i];
        }
    }

    void ChebyshevsMethod()
    {
        double sum = 0;
        double x = 0;
        for (int i = 0; i < n; i++)
        {
            x = ((b + a) / 2) + ((b - a) / 2) * t[i];
            symbols.Remove("x");
            symbols.Add("x", x);
            sum += f.Evaluate(symbols).RealValue;
            Output.text += ("x" + (i + 1).ToString() + " = " + x.ToString() + "\n");
        }
        sum = ((b - a) / n) * sum;
        Output.text += ("Integral = " + sum.ToString() + "\n");
        ChebyshevsRate();
    }

    void ChebyshevsRate()
    {
        double Max, prom;
        double x = a;
        int n2 = (n % 2 == 0) ? n + 2 : n + 1;
        double[] koefs = new double[]
        {
            (1.0 / 135),
            (1.0 / 360),
            (2.0 / 42525),
            (13.0 / 544320),
            (1.0 / 3969000),
            (281.0 / 1959552000),
            0,
            (74747.0 / 11200 / 9 / 11 / 10 / 9 / 8 / 7 / 6 / 5 / 4 / 3 / 2)
        };
        var x_ = Expr.Variable("x");
        var NDerivative = f.Differentiate(x_);
        for (int i = 1; i < n2; i++)
        {
            NDerivative = NDerivative.Differentiate(x_);
        }

        symbols.Remove("x");
        symbols.Add("x", x);
        Max = Math.Abs(NDerivative.Evaluate(symbols).RealValue);
        x += step;
        for (int i = 1; i < stepnumber; i++)
        {
            symbols.Remove("x");
            symbols.Add("x", x);
            prom = Math.Abs(NDerivative.Evaluate(symbols).RealValue);
            if (prom > Max)
                Max = prom;
            x += step;
        }
        rating = koefs[n - 2] * Max;
        Output.text += ("Error = " + rating.ToString() + "\n");
    }

    void TrapezoidMethod()
    {
        double sum = 0;
        double x = a;
        double h = (b - a) / n;
        symbols.Remove("x");
        symbols.Add("x", x);
        sum += f.Evaluate(symbols).RealValue / 2;
        for (int i = 1; i < n - 1; i++)
        {
            x += h;
            symbols.Remove("x");
            symbols.Add("x", x);
            sum += f.Evaluate(symbols).RealValue;
        }
        symbols.Remove("x");
        symbols.Add("x", x);
        sum += f.Evaluate(symbols).RealValue / 2;
        sum = h * sum;
        Output.text += ("Integral = " + sum.ToString() + "\n");
        TrapezoidRate();
    }

    void TrapezoidRate()
    {
        double Max, prom;
        double x = a;
        double h = (b - a) / n;
        var x_ = Expr.Variable("x");
        var NDerivative = f.Differentiate(x_);
        NDerivative = NDerivative.Differentiate(x_);
        Max = Math.Abs(NDerivative.Evaluate(symbols).RealValue);
        x += step;
        for (int i = 1; i < stepnumber; i++)
        {
            symbols.Remove("x");
            symbols.Add("x", x);
            prom = Math.Abs(NDerivative.Evaluate(symbols).RealValue);
            if (prom > Max)
                Max = prom;
            x += step;
        }
        rating = (b - a) * h * h * Max / 12;
        Output.text += ("Error = " + rating.ToString() + "\n");
    }
}
