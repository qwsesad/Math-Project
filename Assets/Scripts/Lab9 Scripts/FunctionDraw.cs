using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using MathNet.Symbolics;
using Expr = MathNet.Symbolics.SymbolicExpression;
using Quaternion = UnityEngine.Quaternion;

public class FunctionDraw : MonoBehaviour
{
    public int N; //���������� ����� ��� �������
    public int NInt; //���������� ����������
    public int Method; //����� �������
    int NRoots; //���������� ������
    double YN; //������ y ������� ���������
    double YV; //������� y ������� ���������
    double XP; //������ x ������� ���������
    double XL; //����� x ������� ���������
    public bool MouseDown = false; //�������� ������� ����� ������ ���� � ������� ���������
    bool inside = false; //�������� ��������� ������� �� ������� ���������
    bool shift = false; //�������� ������� �� shift

    public double MinX; //����� �������� ��������� �� x
    public double MaxX; //������ �������� ��������� �� x
    public double MinY; //������ �������� ��������� �� y
    public double MaxY; //������� �������� ��������� �� y
    public double step; //��� �� x 
    public double eps; //��������
    public List<double> roots; //���� ������
    List<List<double>> intervals; //���� ����������
    public string func; //������ �������
    Expr f; //�������
    Expr FirstDerivative; //������ �����������
    Expr SecondDerivative; //������ �����������
    Vector3 MouseStart; //���������� �����, ��� ������ ���� ���� ������
    Dictionary<string, FloatingPoint> symbols; //�������� �������
    Dictionary<string, FloatingPoint> symbolsd1; //������� ��� ������ �����������
    Dictionary<string, FloatingPoint> symbolsd2; //������� ��� ������ �����������

    List<Vector3> Tops; //���� ����� ��� �������
    LineRenderer function; //����� �������

    public GameObject x; //prefab ����
    public GameObject XDivision; //prefab ������� �� x
    public GameObject YDivision; //prefab ������� �� y
    public GameObject Circle; //prefab �����
    public GameObject LinePrefab; //prefab �����
    public GameObject Land; //������� ���������
    public ErrorS er; //���������� ������
    public Camera _camera; //������
    public Text Output; //������� ������

    //������ �������
    public void Go()
    {
        DestroyObjects(); //������� ����� � ������ ��� �������, ����� ������� �� ������
        SetUp(); //������ ������ ����������
        CoordinateSystem(); //������ ������� ���������

        //��������� ��������� ����� �������
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
        Output.text += "Number of uniq roots:" + NRoots.ToString() + "\n";
        SetRoots(); //����������� ����� �� �������
    }

    //�������������� ����������
    void SetUp ()
    { 
        symbols = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //������� ��������� �������
        symbolsd1 = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //������� ������� ��� ������ �����������
        symbolsd2 = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //������� ������� ��� ������ �����������
        roots = new List<double>(); //������� ����� ������
        intervals = new List<List<double>>(); //������� ����� ����������

        f = Expr.Parse(func); //������� �������
        var x_ = Expr.Variable("x"); //������� ����������, �� ������� ����� ����������� �����������
        FirstDerivative = f.Differentiate(x_); //������� ������ �����������
        SecondDerivative = FirstDerivative.Differentiate(x_); //������� ������ �����������

        NInt = 0; //�������� ���������� ����������
        NRoots = 0; //�������� ���������� ������
        N = 0; //�������� ���������� ����� ��� �����

        XYPLVN(); //���������� ������� ���������
        MinMaxY(); //����� ����������
        Points(); //��������� �����
    }

    //��������� ������������ �������
    void CoordinateSystem()
    {
        GameObject YLine; //��� y
        GameObject XLine; //��� x
        Quaternion rotation; //�������� ��� ��� y
        Vector3 scalex = new Vector3(transform.localScale.x, 3, 0); //scale ��� x
        Vector3 scaley = new Vector3(transform.localScale.y, 3, 0); //scale ��� y

        double X0 = PreobrX((MaxX + MinX)/ 2); //���� ��� ������� ��� y
        double Y0 = PreobrY((MaxY + MinY) / 2); //���� ��� ������� ��� x
        double XJust0 = PreobrX(0); //���� �� x
        double YJust0 = PreobrY(0); //���� �� y
        double XDivisionsStep = (MaxX - MinX) / 5; //��� ��� ������� �� x
        double YDivisionsStep = (MaxY - MinY) / 5; //��� ��� ������� �� y

        //YLine
        if (MinX * MaxX <= 0) //���� ��� y �����
        {
            rotation = new Quaternion(x.transform.rotation.x, x.transform.rotation.y, 1, x.transform.rotation.w);  //��� ��������� ��� x �� 90 ��������, ����� �������� ��� y
            YLine = Instantiate(x, new Vector3((float) XJust0, (float) Y0, 0), x.transform.rotation); //������ ��� y
            YLine.transform.localScale = scaley; //����������� ��� y
            YLine.transform.rotation = rotation; //������������ ��� y
            YLine.tag = "Line"; //����������� tag ��� �������

            //������ �������
            double i = MinY;
            while (i <= MaxY)
            {
                if (i != 0) //���� y �� ����� 0
                {
                    GameObject Division; //����� �������
                    Division = Instantiate(YDivision, new Vector3((float) XJust0, (float) PreobrY(i), 0), YDivision.transform.rotation); //������ �������
                    Division.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (((double)((int)(i * 100))) / 100).ToString(); //������ �������
                    Division.tag = "Division"; //����������� tag ��� �������
                    i += YDivisionsStep; //��� � ���������� �������
                }
                else //���� y ����� 0
                    i += YDivisionsStep; //������� �������� �� ����, ��� ������
            }
        }

        //Xline
        if (MinY * MaxY <= 0) //���� ��� x �����
        {
            XLine = Instantiate(x, new Vector3((float) X0, (float) YJust0, 0), x.transform.rotation); //������ ��� x
            XLine.transform.localScale = scalex; //����������� ��� x
            XLine.tag = "Line"; //����������� tag ��� �������

            //������ �������
            double i = MinX;
            while (i <= MaxX)
            {
                if (i != 0) //���� x �� ����� 0
                {
                    GameObject Division; //����� �������
                    Division = Instantiate(XDivision, new Vector3((float) PreobrX(i), (float) YJust0, 0), XDivision.transform.rotation); //������ �������
                    Division.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (((double)((int)(i*100)))/100).ToString(); //������ �������
                    Division.tag = "Division"; //����������� tag ��� �������
                    i += XDivisionsStep; //��� � ���������� �������
                }
                else //���� x ����� 0
                    i += XDivisionsStep; //������� �������� �� ����, ��� ������
            }
        }
    }

    //����� ����������
    void MinMaxY()
    {
        double Min = 0; //����������� �������� y �� ���������
        double Max = 0; //������������ �������� y �� ���������
        double prom = 0; //���������� �������� �������
        double next = 0; //��������� �������� �������
        double x1 = MinX; //��������� �������� ���������
        double x0 = MinX; //���������� �������� ���������
        bool check = false; //�������� ����������� �������

        //���� ������� �������� (�������� ������� � ������ ���������)
        while (!check && x1<=MaxX) 
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
                    if ((prom * next) <= 0 && Method == 1) //�������� ������� ������ ������ � ���������� ��������, ���� ������������ ����� ������� 
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
                    else if ((prom * next) <= 0 && Method != 1) //���� ������ ����� � ������������ �� ����� �������
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
            catch //���� � ����� ������ �������
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

    //��������� ������� � ������� ����� ��� �����
    void Points()
    {
        double prom = 0;
        double x0 = MinX;
        double next = 0;

        Tops = new List<Vector3>(); //����� ���� ����� ��� �����

        while (x0 <= MaxX)
        {
            try //���� ������� �� ����� �����������, �� ������������ ������� � catch
            {
                symbols.Remove("x");
                symbols.Add("x", (x0+x0+step)/2);
                next = f.Evaluate(symbols).RealValue; //��������� �������� ������� � �������� ���������
                symbols.Remove("x");
                symbols.Add("x", x0);
                prom = f.Evaluate(symbols).RealValue; //���������� �������� ������� ��� ����� x

                if (prom <= MaxY && prom >= MinY) //���� �������� ������� � �������� ��������� �� y
                {
                    Tops.Add(new Vector3((float)PreobrX(x0), (float)PreobrY(prom), 0));
                    N++;
                }
                if (next < MinY || next > MaxY) //���� ����� �� �������
                    Line(); //������ ����� �� �������
                x0 += step;
            }
            catch //���� � ����� ������ �������
            {
                Line(); //������ ����� �� �������
                x0 += step;
            }
        }
        Line(); //������ �����
    }

    //��������� �����
    void Line()
    {
        if (N != 0)
        {
            GameObject newline = Instantiate(LinePrefab, Vector3.zero, Quaternion.identity); //������ ����� �����
            function = newline.GetComponent<LineRenderer>(); //����� ����� ���������
            function.positionCount = N; //���������� ����� ��� �����

            //������ �����
            int z = 0;
            foreach (Vector3 top in Tops)
            {
                function.SetPosition(z, top);
                z++;
            }
            N = 0; //�������� ���������� ����� ��� �����
            Tops = new List<Vector3>(); //�������� ������ ����� ��� �����
        }
    }

    //����� �������
    void NewtonsMethod()
    {
        int i;
        double S; //�������� �������
        double A = 0; //���������� �������� �������
        double S1; //�������� ������ �����������
        double S2; //�������� ������ ����������� 
        double x0; //������
        bool check; //�������� ����������� �������
        int k; //���������� ��������
        double H; //f(x)/f'(x)
        double Min; //����������� �������� ������ ����������� �� ���������
        double Max; //������������ �������� ������ ����������� �� ���������

        //���� �� ����������
        for (i = 0; i < NInt; i++)
        {
            k = 1;
            check = true;
            H = 0;
            x0 = 0;
            Max = 0;
            Min = 0;
            try //���� ������� ��� ����������� �� ����� �����������, �� ������������ ������� � catch
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
                while (check && (Math.Abs(Max/(Min*2)*H*H) >= eps))
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

                    if (S2 == 0) //���� ������ ����������� ����� ����
                        check = false; //����������� ������� �� �����������

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
                    Output.text += "a = " + intervals[i][0].ToString() + "\n";
                    Output.text += "b = " + intervals[i][1].ToString() + "\n";
                    Output.text += "k = " + k.ToString() + "\n";
                    Output.text += "x = " + x0.ToString() + "\n";
                    Output.text += "y(x) = " + S.ToString() + "\n\n";

                    //���������� �����, ���� ��� ��� ��� � ������
                    if (!roots.Contains(x0))
                    {
                        roots.Add(x0);
                        NRoots++;
                    }
                }
            }
            catch
            { 
                //���� ���-�� �� ������� ��������� ������� ��� �����������, �� �������� ������������
            }

        }
    }

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
                while (check && (Math.Abs(x2 - x1) >= eps))
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
                    Output.text += "a = " + intervals[i][0].ToString() + "\n";
                    Output.text += "b = " + intervals[i][1].ToString() + "\n";
                    Output.text += "k = " + k.ToString() + "\n";
                    Output.text += "x = " + c.ToString() + "\n";
                    Output.text += "y(x) = " + FC.ToString() + "\n\n";

                    //���������� �����, ���� ��� ��� ��� � ������
                    if (!roots.Contains(c))
                    {
                        roots.Add(c);
                        NRoots++;
                    }
                }
            }
            catch { }

        }
    }

    //������������ ������ �� ������� �������� �������
    void SetRoots()
    {
        GameObject Root;
        float Y0 = (float) PreobrY(0);
        foreach (double root in roots)
        {
            Root = Instantiate(Circle, new Vector3((float)PreobrX(root), Y0, 3), Circle.transform.rotation);
        }
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
       x = (x-MinX)*(XP-XL)/(MaxX-MinX)+XL;

       return x;
    }

    double PreobrY(double y)//������������� ���������� y ��� ��
    {
        y = (y - MinY) * (YN - YV) / (MinY - MaxY) + YN;
        return y;
    }

    //������� ������ � �����
    public void DestroyObjects()
    {
        GameObject[] Objects;

        Objects = GameObject.FindGameObjectsWithTag("Line");
        foreach (GameObject ob in Objects)
        {
            Destroy(ob);
        }

        Objects = GameObject.FindGameObjectsWithTag("Division");
        foreach (GameObject ob in Objects)
        {
            Destroy(ob);
        }

        Objects = GameObject.FindGameObjectsWithTag("Roots");
        foreach (GameObject ob in Objects)
        {
            Destroy(ob);
        }

        Output.text = "";
    }

    //�������� ������� �� ����� ������ ���� ������ ������� ���������
    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseDown = true;
            MouseStart = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _camera.transform.position.y));
        }
    }

    //�������� ���������� ����� ������ ���� ������ ������� ���������
    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0))
            MouseDown = false;
    }

    //��������� ������
    public GameObject GetObject()
    {
        return Land;
    }

    //�������� ���������� ������� � ������� ���������
    private void OnMouseEnter()
    {
        inside = true;
    }

    //�������� ������ ������� �� ������� ���������
    private void OnMouseExit()
    {
        inside = false;
    }

    //�������� ������� �� shift � ���������� �������
    private void Shift()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            shift = true;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            shift = false;
    }

    //��������� ������� ���������
    void Update()
    {
        Shift();
        if (MouseDown)
        {
            Vector3 newMousePosition = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _camera.transform.position.z));
            double newx = newMousePosition.x - MouseStart.x;
            double newy = newMousePosition.y - MouseStart.y;
            if (Math.Abs(newx) > 10)
            {
                MinX -= newx/1000;
                MaxX -= newx/1000;
            }
            if (Math.Abs(newy) > 1)
            {
                MinY -= newy / 500;
                MaxY -= newy / 500;
            }
            MouseStart.y = newMousePosition.y;
            Go();
        }
        if (inside)
        {
            float iz = Input.GetAxis("Mouse ScrollWheel");
            if (Math.Abs(iz) > 0)
            {
                double z;
                if (Math.Abs(iz) > 1)
                    iz *= (float)0.1;
                else if (Math.Abs(iz) < 1)
                    iz *= (float)10;
                if (!shift)
                {
                    if (Math.Abs(z = MinX - iz) > step)
                        MinX = z;
                    if (Math.Abs(z = MaxX + iz) > step)
                        MaxX = z;
                }
                else
                {
                    if (Math.Abs(z = MinY - iz) > step)
                        MinY = z;
                    if (Math.Abs(z = MaxY + iz) > step)
                        MaxY = z;
                }
                Go();
            }
        }
    }
}
