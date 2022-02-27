using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using MathNet.Symbolics;
using Expr = MathNet.Symbolics.SymbolicExpression;
using Quaternion = UnityEngine.Quaternion;

public class ComparisonDraw: MonoBehaviour
{
    public int NInt; //���������� ����������
    public int Method; //������ �����
    public int Method2; //������ �����
    int NRoots1; //���������� ������������ �������� ��� ������� ������
    int NRoots2; //���������� ������������ �������� ��� ������� ������
    int N; //���������� �������
    public double J; //���������� ��� ������� �� eps
    double YN; //������ y ������� ���������
    double YV; //������� y ������� ���������
    double XP; //������ x ������� ���������
    double XL; //����� x ������� ���������
    double KMax; //������������ k �� eps
    double KMaxMax = 0; //������������ k �� �������
    public double mineps; //����������� eps

    public double MinX; //����� �������� ��������� �� x
    public double MaxX; //������ �������� ��������� �� x
    public double MinY; //������ �������� ��������� �� y
    public double MaxY; //������� �������� ��������� �� y
    public double step; //��� �� x
    public double eps; //��������
    List<List<double>> intervals; //���� ����������
    public string func; //������ �������
    Expr f; //�������
    Expr FirstDerivative; //������ �����������
    Expr SecondDerivative; //������ �����������
    Dictionary<string, FloatingPoint> symbols; //�������� �������
    Dictionary<string, FloatingPoint> symbolsd1; //������� ��� ������ �����������
    Dictionary<string, FloatingPoint> symbolsd2; //������� ��� ������ �����������

    public List<double> Tops; //���� ������������ k
    LineRenderer function; //����� �������

    public GameObject c; //������� ���������
    public GameObject x; //prefab ����
    public GameObject XDivision; //prefab ������� �� x
    public GameObject YDivision; //prefab ������� �� y
    public GameObject LinePrefab; //prefab �����
    public ErrorS er; //���������� ������
    public Text Output; //������� ������


    //������ ���������
    public void Go()
    {
        DestroyObjects(); //������� ����� � ������ ��� �������, ����� ��������� �� ������
        SetUp(); //������ ������ ����������
        J = mineps; //����������� eps 

        //������� � ����������� �� �������
        {
            switch (Method)
            {
                case 1:
                    c.GetComponentInChildren<TextMesh>().text += "Blue color is Newton's Method\n";
                    break;
                case 2:
                    c.GetComponentInChildren<TextMesh>().text += "Blue color is The Dichotomy Method\n";
                    break;
                default:
                    break;
            }
            switch (Method2)
            {
                case 1:
                    c.GetComponentInChildren<TextMesh>().text += "Red color is Newton's Method\n";
                    break;
                case 2:
                    c.GetComponentInChildren<TextMesh>().text += "Red color is The Dichotomy Method\n";
                    break;
                default:
                    break;
            }
        }

        Tops = new List<double>(); //������� ����� ��������

        //���� �� ��������� ��� ������� ������
        for (int i = 0; i < N; i++)
        {
            switch (Method)
            {
                case 1:
                    Output.text += "Newton's Method\n";
                    NewtonsMethod();
                    break;
                case 2:
                    Output.text += "The Dichotomy Method\n";
                    TheDichotomyMethod();
                    break;
                default:
                    break;
            }
            J *= 10;
        }

        J = mineps; //����� ���� ����������� eps

        //���� �� ��������� ��� ������� ������
        for (int i = 0; i < N; i++)
        {
            switch (Method2)
            {
                case 1:
                    Output.text += "Newton's Method\n";
                    NewtonsMethod();
                    break;
                case 2:
                    Output.text += "The Dichotomy Method\n";
                    TheDichotomyMethod();
                    break;
                default:
                    break;
            }
            J *= 10;
        }


        if (KMaxMax > 0) //���� �� ������ �������
        {
            CoordinateSystem(); //������ ������� ���������
            Points(new Color(0,0,1,1), new Color(0, 0, 1, 1), new Color(1, 0, 0, 1), new Color(1, 0, 0, 1)); //������ �������
        }
    }

    //�������������� ����������
    void SetUp()
    {
        symbols = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //������� ��������� �������
        symbolsd1 = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //������� ������� ��� ������ �����������
        symbolsd2 = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //������� ������� ��� ������ �����������
        intervals = new List<List<double>>(); //������� ����� ����������

        f = Expr.Parse(func); //������� �������
        var x_ = Expr.Variable("x"); //������� ����������, �� ������� ����� ����������� �����������
        FirstDerivative = f.Differentiate(x_); //������� ������ �����������
        SecondDerivative = FirstDerivative.Differentiate(x_); //������� ������ �����������

        NInt = 0; //�������� ���������� ����������
        NRoots1 = 0; //�������� ���������� ������������ �������� ��� ������� ������
        NRoots2 = 0; //�������� ���������� ������������ �������� ��� ������� ������
        N = 5; //���������� �������

        //����������� eps � ����������� �� ���������� �������
        mineps = eps;
        for (int i = 1; i < N; i++)
        {
            mineps /= 10;
        }

        XYPLVN(); //���������� ������� ���������
        MinMax(); //����� ����������
    }

    //��������� ������������ �������
    void CoordinateSystem()
    {
        GameObject YLine; //��� y
        GameObject XLine; //��� x
        Quaternion rotation; //�������� ��� ��� y
        Vector3 scalex = new Vector3(transform.localScale.x, 3, 0); //scale ��� x
        Vector3 scaley = new Vector3(transform.localScale.y, 3, 0); //scale ��� y

        double X0 = PreobrX(2.5); //���� ��� ������� ��� y
        double Y0 = PreobrY(KMaxMax / 2); //���� ��� ������� ��� x
        double XJust0 = PreobrX(0); //���� �� x
        double YJust0 = PreobrY(0); //���� �� y
        double YDivisionsStep = KMaxMax / 4; //��� ��� ������� �� y

        rotation = new Quaternion(x.transform.rotation.x, x.transform.rotation.y, 1, x.transform.rotation.w); //��� ��������� ��� x �� 90 ��������, ����� �������� ��� y
        YLine = Instantiate(x, new Vector3((float)XJust0, (float)Y0, 0), x.transform.rotation); //������ ��� y
        YLine.transform.localScale = scaley; //����������� ��� y
        YLine.transform.rotation = rotation; //������������ ��� y
        YLine.tag = "Line2"; //����������� tag ��� �������

        //������ �������
        double i = YDivisionsStep;
        while (i <= KMaxMax)
        {
            GameObject Division; //����� �������
            Division = Instantiate(YDivision, new Vector3((float)XJust0, (float)PreobrY(i), 0), YDivision.transform.rotation); //������ �������
            Division.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (i).ToString(); //������ �������
            Division.tag = "Division2"; //����������� tag ��� �������
            i += YDivisionsStep; //��� � ���������� �������
        }

        XLine = Instantiate(x, new Vector3((float)X0, (float)YJust0, 0), x.transform.rotation); //������ ��� x
        XLine.transform.localScale = scalex; //����������� ��� x
        XLine.tag = "Line2"; //����������� tag ��� �������

        //������ �������
        i = mineps;
        for (int j = 1; j <= N; j++)
        {

            GameObject Division; //����� �������
            Division = Instantiate(XDivision, new Vector3((float)PreobrX(j), (float)YJust0, 0), XDivision.transform.rotation); //������ �������
            Division.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (i).ToString(); //������ �������
            Division.tag = "Division2"; //����������� tag ��� �������
            i *= 10; //��� � ���������� �������
        }
    }

    //����� ����������
    void MinMax()
    {
        double Min = 0; //����������� �������� y �� ���������
        double Max = 0; //������������ �������� y �� ���������
        double prom = 0; //���������� �������� �������
        double next = 0; //��������� �������� �������
        double x1 = MinX; //��������� �������� ���������
        double x0 = MinX; //���������� �������� ���������
        bool check = false; //�������� ����������� �������

        //���� ������� �������� (�������� ������� � ������ ���������)
        while (!check && x1 <= MaxX)
        {
            try //���� ������� �� ����� �����������, �� ������������ ������� � catch
            {
                symbols.Remove("x");
                symbols.Add("x", x1);
                Min = Max = next = f.Evaluate(symbols).RealValue; //���������� �������� ������� � ������� ������ �������� ��� �������� � ���������

                check = true;
                prom = next;
                x0 = x1;
                x1 += step;
            }
            catch
            {
                x1 += step;
            }
        }

        // �������� ���� ������ ����������
        while (x1 <= MaxX)
        {
            try //���� ������� �� ����� �����������, �� ������������ ������� � catch
            {
                symbols.Remove("x");
                symbols.Add("x", x1);
                next = f.Evaluate(symbols).RealValue; //���������� �������� �������

                //����� ����������� � ������������ ��������
                if (next > Max)
                    Max = next;
                else if (next < Min)
                    Min = next;

                try //���� ����������� �� ����� �����������, �� �������� �� ������ � ���������
                {
                    //�������� ����������� ������� ���������
                    if ((prom * next) <= 0 && (Method == 1 || Method2 == 1)) //�������� ������� ������ ������ � ���������� ��������, ���� ������������ ����� �������
                    {
                        double d1_1;
                        double d1_2;

                        symbolsd1.Remove("x");
                        symbolsd1.Add("x", x0);
                        d1_1 = FirstDerivative.Evaluate(symbols).RealValue; //������ ����������� �� ����� �������� ���������

                        symbolsd1.Remove("x");
                        symbolsd1.Add("x", x1);
                        d1_2 = FirstDerivative.Evaluate(symbols).RealValue; //������ ����������� �� ������ �������� ���������

                        //���� ������ ����������� � �� ����� �������� ���������, � �� ������ �� ����� ����
                        if (d1_1 * d1_2 != 0)
                        {
                            double d2_1;
                            double d2_2;

                            symbolsd2.Remove("x");
                            symbolsd2.Add("x", x0); 
                            d2_1 = SecondDerivative.Evaluate(symbols).RealValue; //������ ����������� �� ����� �������� ���������

                            symbolsd1.Remove("x");
                            symbolsd1.Add("x", x1);
                            d2_2 = SecondDerivative.Evaluate(symbols).RealValue; //������ ����������� �� ������ �������� ���������

                            //���� ������ ����������� � �� ����� �������� ���������, � �� ������ �� ����� ����
                            if (d2_1 * d2_2 != 0)
                            {
                                //��������� ��������
                                intervals.Add(new List<double>());
                                intervals[NInt].Add(x0);
                                intervals[NInt].Add(x1);
                                NInt++;
                            }
                        }
                    }
                    else if ((prom * next) <= 0) //���� ������ ����� � ������������ �� ����� �������
                    {
                        //��������� ��������
                        intervals.Add(new List<double>());
                        intervals[NInt].Add(x0);
                        intervals[NInt].Add(x1);
                        NInt++;
                    }
                }
                catch 
                {
                    //���� ���-�� ����������� �� ����������� ��� ������ �������, �� �������� ���������� �� ������ � ��������
                }

                x0 = x1;
                x1 += step;
                prom = next;
            }
            catch //���� � ����� ������ �������, ���������� ������
            {
                x1 += step;
                check = false;
                while (!check && x1 <= MaxX)
                {
                    try
                    {
                        symbols.Remove("x");
                        symbols.Add("x", x1);
                        next = f.Evaluate(symbols).RealValue;
                        check = true;
                        prom = next;
                        x0 = x1;
                        x1 += step;
                    }
                    catch
                    {
                        x1 += step;
                    }
                }
            }
        }

        //������ ���������� ��� y, ���� ��� �� ���� ������ ����������
        if (MinY == -0.0123455)
            MinY = Min;
        if (MaxY == 0.0123455)
            MaxY = Max;
    }

    //��������� ��������
    void Points(Color start, Color end, Color start2, Color end2)
    {
        int i = 0;
        if (NRoots1 > 0) //���� ���� ���������� � ������� ������
        {
            GameObject newline = Instantiate(LinePrefab, Vector3.zero, Quaternion.identity); //������ ����� �����
            function = newline.GetComponent<LineRenderer>(); //����� ����� ���������
            function.positionCount = NRoots1; //���������� ����� ��� �����
            function.startColor = start; //��������� ����
            function.endColor = end; //�������� ����
            newline.tag = "Line2";

            //������ �����
            for (; i < NRoots1; i++)
            {
                function.SetPosition(i, new Vector3((float)PreobrX(i + 1), (float)PreobrY(Tops[i]), 0));
            }
        }
        if (NRoots2 > 0) //���� ���� ���������� � ������� ������
        {
            GameObject newline = Instantiate(LinePrefab, Vector3.zero, Quaternion.identity); //������ ����� �����
            function = newline.GetComponent<LineRenderer>(); //����� ����� ���������
            function.positionCount = NRoots2; //���������� ����� ��� �����
            function.startColor = start2; //��������� ����
            function.endColor = end2; //�������� ����
            newline.tag = "Line2";

            //������ �����
            for (int t = 0; t < NRoots2; t++)
            {
                function.SetPosition(t, new Vector3((float)PreobrX(t + 1), (float)PreobrY(Tops[i]), 0));
                i++;
            }
        }
    }

    void NewtonsMethod()
    {
        int i;
        double S; //�������� �������
        double S1; //�������� ������ �����������
        double S2; //�������� ������ �����������
        double x0; //������
        bool check; //�������� ����������� �������
        int k; //���������� ��������
        double H; //f(x)/f'(x)
        double Min; //����������� �������� ������ ����������� �� ���������
        double Max; //������������ �������� ������ ����������� �� ���������
        double A = 0; //���������� �������� �������
        KMax = 0; //������������ ���������� �������� ��� ������ ���������

        //���� �� ����������
        for (i = 0; i < NInt; i++)
        {
            k = 1;
            check = true;
            H = 0;
            x0 = 0;
            Max = 0;
            Min = 0;
            try
            {
                double S11; //������ ����������� �� ����� �������� ����������
                double S21; //������ ����������� �� ����� �������� ����������
                double S12; //������ ����������� �� ������ �������� ����������
                double S22; //������ ����������� �� ������ �������� ����������
                symbols.Remove("x");
                symbols.Add("x", intervals[i][0]);
                S = f.Evaluate(symbols).RealValue; //���������� ������� �� ����� �������� ���������
                S2 = SecondDerivative.Evaluate(symbols).RealValue; //���������� ������ ����������� �� ����� �������� ���������
                S1 = FirstDerivative.Evaluate(symbols).RealValue; //���������� ������ ����������� �� ����� �������� ���������
                S11 = Math.Abs(S1);
                S21 = Math.Abs(S2);

                //����� ��������������� �����������
                if (S * S2 > 0) //���� �������� ������� � ������ ����������� ������ ���� �� ����� �������� ���������
                    x0 = intervals[i][0]; //����� ��������
                else //���� �������� ������� � ������ ����������� �� ������ ���� �� ����� �������� ���������
                {
                    symbols.Remove("x");
                    symbols.Add("x", intervals[i][1]);
                    S = f.Evaluate(symbols).RealValue; //���������� ������� �� ������ �������� ���������
                    S2 = SecondDerivative.Evaluate(symbols).RealValue; //���������� ������ ����������� �� ������ �������� ���������

                    if (S * S2 > 0) //���� �������� ������� � ������ ����������� ������ ���� �� ������ �������� ��������� 
                    {
                        x0 = intervals[i][1]; //������ ����������� �����
                    }
                    else //���� �������� ������� � ������ ����������� ����� �� ������ ����
                        check = false; //����������� ������� �� �����������
                }

                S1 = FirstDerivative.Evaluate(symbols).RealValue; //���������� ������ ����������� �� �������������� �����������
                if (S1 != 0) //���� ������ ����������� �� ����� ����
                {
                    A = S = f.Evaluate(symbols).RealValue; //���������� �������
                    H = S / S1;
                }
                else //���� ������ ����������� ����� ����
                    check = false; //����������� ������� �� �����������
                x0 -= H; //������ ��������

                symbols.Remove("x");
                symbols.Add("x", intervals[i][1]);
                S12 = Math.Abs(FirstDerivative.Evaluate(symbols).RealValue); //���������� ������ ����������� �� ������ �������� ���������
                S22 = Math.Abs(SecondDerivative.Evaluate(symbols).RealValue); //���������� ������ ����������� �� ������ �������� ���������

                //����� ������� �������� ��� ������ �����������
                if (S11 < S12)
                    Min = S11;
                else
                    Min = S12;

                //����� ������� ��������� ��� ������ �����������
                if (S22 > S21)
                    Max = S22;
                else
                    Max = S21;

                //���� �������� � ��������� ����������� ������� � ������� ��������
                while (check && (Math.Abs(Max / (Min * 2) * H * H) >= J))
                {
                    k++;
                    symbols.Remove("x");
                    symbols.Add("x", x0);
                    S1 = FirstDerivative.Evaluate(symbols).RealValue; //���������� ������ �����������
                    S2 = SecondDerivative.Evaluate(symbols).RealValue; //���������� ������ �����������
                    if (S1 != 0) //���� ������ ����������� �� ����� ���� 
                    {
                        A = S;
                        S = f.Evaluate(symbols).RealValue; //���������� �������
                        H = S / S1;
                    }
                    else //���� ������ ����������� ����� ����
                        check = false; //����������� ������� �� �����������
                    x0 -= H; //��������� ����������� �����

                    //����� �������� ��� ������ �����������
                    S11 = Math.Abs(S1);
                    if (S11 < Min)
                        Min = S11;

                    //����� ��������� ��� ������ �����������
                    S22 = Math.Abs(S2);
                    if (S22 > Max)
                        Max = S22;

                    if (S2 == 0)
                        check = false;

                    if (Math.Abs(A) < Math.Abs(S)) //���� ���� ������
                        check = false; //����������� ������� �� �����������

                    if (x0 > intervals[i][1] || x0 < intervals[i][0]) //���� ������ ����� �� ���������
                        check = false; //����������� ������� �� �����������

                    if (k > 1000) //���� ����� �������� ������ 1000
                    {
                        er.SetUp("Sorry( K > 1000. We have to stop it"); //����� ������
                        check = false; //���������� ����� �� ��������� ����������� ��� ����� ������ ����������
                    }
                }

                //����� ���������� ����� � ������ ��������� ���� �� ��������� ����������� ������� �� ���������
                if (check) //���� ������� ���������
                {
                    symbols.Remove("x");
                    symbols.Add("x", x0);
                    S = f.Evaluate(symbols).RealValue; //���������� ������� � �����

                    //�����
                    Output.text += "Root: \n";
                    Output.text += "eps = " + J.ToString() + "\n";
                    Output.text += "a = " + intervals[i][0].ToString() + "\n";
                    Output.text += "b = " + intervals[i][1].ToString() + "\n";
                    Output.text += "k = " + k.ToString() + "\n";
                    Output.text += "x = " + x0.ToString() + "\n";
                    Output.text += "y(x) = " + S.ToString() + "\n\n";

                    //����� ������������� k
                    if (k > KMax)
                        KMax = k;
                }
            }
            catch { }

        }
        //����� ������������� k ��� ������ �������
        if (KMax > KMaxMax) 
            KMaxMax = KMax;
        Tops.Add(KMax);
        NRoots1++;
    }

    //����� ���������
    void TheDichotomyMethod()
    {
        int i;
        double A; //����� �������� �������
        double B; //������ �������� �������
        double FC; //�������� ������� �� ��������
        double c; //�������� �� ��������
        double x1; //����� �������� ���������
        double x2; //������ �������� ���������
        bool check; //�������� ����������� �������
        int k; //���������� ��������
        KMax = 0; //������������ ���������� ��������

        //���� �� ����������
        for (i = 0; i < NInt; i++)
        {
            k = 0;
            check = true;
            c = 0;
            FC = 0;
            try //���� ������� ��� ����������� �� ����� �����������, �� ������������ ������� � catch
            {
                x1 = intervals[i][0];
                x2 = intervals[i][1];
                symbols.Remove("x");
                symbols.Add("x", x1);
                A = f.Evaluate(symbols).RealValue; //���������� ������� �� ����� �������� ���������
                symbols.Remove("x");
                symbols.Add("x", x2); 
                B = f.Evaluate(symbols).RealValue; //���������� ������� �� ������ �������� ���������

                //���� �������� � ��������� ����������� ������� � ������� ��������
                while (check && (Math.Abs(x2 - x1) >= J))
                {
                    c = (x1 + x2) / 2; //���������� �������� ��������� �� ��������
                    symbols.Remove("x");
                    symbols.Add("x", c);
                    FC = f.Evaluate(symbols).RealValue; //���������� �������� ������� �� ��������

                    if (FC * B >= 0 && Math.Abs(FC) <= Math.Abs(B)) //���� ���������� ����� �������� ������� �� �������� � ������� �������� �������, ��� ���� �� �������� ����� ����, � ����� ��� �������
                    {
                        B = FC; //������ �������� ������� ���������� ����� �������� ������� �� ��������
                        x2 = c; //������ �������� ��������� ���������� ����� ��������� �� ��������
                    }
                    else if (FC * A >= 0 && Math.Abs(FC) <= Math.Abs(A)) //���� ���������� ����� �������� ������� �� �������� � ������ �������� �������, ��� ���� �� �������� ����� ����, � ����� ��� �������
                    {
                        A = FC; //����� �������� ������� ���������� ����� �������� ������� �� ��������
                        x1 = c; //����� �������� ��������� ���������� ����� ��������� �� ��������
                    }
                    else //���� �������� ������ ��� �������� � �����������
                        check = false; //����������� ������� �� �����������

                    if (c > intervals[i][1] || c < intervals[i][0]) //���� ������ ����� �� ���������
                        check = false; //����������� ������� �� �����������

                    k++;
                    if (k > 1000) //���� ����� �������� ������ 1000
                    {
                        er.SetUp("Sorry( K > 1000. We have to stop it"); //����� ������
                        check = false; //���������� ����� �� ��������� ����������� ��� ����� ������ ����������
                    } 
                }

                if (check)
                {
                    //�����
                    Output.text += "Root: \n";
                    Output.text += "eps = " + J.ToString() + "\n";
                    Output.text += "a = " + intervals[i][0].ToString() + "\n";
                    Output.text += "b = " + intervals[i][1].ToString() + "\n";
                    Output.text += "k = " + k.ToString() + "\n";
                    Output.text += "x = " + c.ToString() + "\n";
                    Output.text += "y(x) = " + FC.ToString() + "\n\n";

                    //����� ������������� �������� k
                    if (k > KMax)
                        KMax = k;
                }
            }
            catch 
            {
                //���� ���-�� �� ������� ��������� �������, �� �������� ������������
            }


        }

        //����� ������������� k ��� ������ �������
        if (KMax > KMaxMax)
            KMaxMax = KMax;
        Tops.Add(KMax);
        NRoots2++;
    }

    //���������� ��������� ������� ��������� �������
    void XYPLVN()
    {
        XP = transform.position.x + transform.localScale.x / 2;
        XL = transform.position.x - transform.localScale.x / 2;
        YN = transform.position.y - transform.localScale.y / 2;
        YV = transform.position.y + transform.localScale.y / 2;
    }


    double PreobrX(double x)//������������� ���������� x ��� ��
    {
        x = x * (XP - XL) / 5 + XL;

        return x;
    }

    double PreobrY(double y)//������������� ���������� y ��� ��
    {
        y = y * (YN - YV) / (-KMaxMax) + YN;
        return y;
    }


    //������� ������ � �����
    public void DestroyObjects()
    {
        GameObject[] Objects;

        Objects = GameObject.FindGameObjectsWithTag("Line2");
        foreach (GameObject ob in Objects)
        {
            Destroy(ob);
        }

        Objects = GameObject.FindGameObjectsWithTag("Division2");
        foreach (GameObject ob in Objects)
        {
            Destroy(ob);
        }

        Output.text = "";
        c.GetComponentInChildren<TextMesh>().text = "";

        KMax = 0;
        KMaxMax = 0;
    }

}
