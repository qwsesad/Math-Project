using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using MathNet.Symbolics;
using Expr = MathNet.Symbolics.SymbolicExpression;
using Quaternion = UnityEngine.Quaternion;
using System.IO;

public class Lab11Compare : MonoBehaviour
{
    public int Method; //первый метод
    public int Method2; //второй метод
    int NRoots1; //количество максимальных итераций для первого метода
    int NRoots2; //количество максимальных итераций для второго метода
    public int n; //количество делений
    public int stepnumber;
    double YN; //нижний y области рисования
    double YV; //верхний y области рисования
    double XP; //правый x области рисования
    double XL; //левый x области рисования
    double KMax; //максимальный
    double step, rating;
    int s, N, N0;
    double[] t;

    public double a; 
    public double b; 
    List<List<double>> intervals; //лист интервалов
    public string func; //строка функции
    Expr f; //функция
    Dictionary<string, FloatingPoint> symbols; //основной словарь

    public List<double> Tops; //лист максимальных k
    LineRenderer function; //линия графика

    public GameObject c; //область рисования
    public GameObject x; //prefab осей
    public GameObject XDivision; //prefab делений по x
    public GameObject YDivision; //prefab делений по y
    public GameObject LinePrefab; //prefab линии
    public ErrorS er; //обработчик ошибок
    public Text Output; //область вывода

    //запуск сравнения
    public void Go()
    {
        DestroyObjects(); //очищаем форму и память для случаев, когда сравнение не первое
        SetUp(); //делаем первые объявления

        //легенда в зависимости от методов
        {
            switch (Method)
            {
                case 1:
                    Output.text += "Chebyshev's Method\n";
                    c.GetComponentInChildren<TextMesh>().text += "Blue color is Chebyshev's Method\n";
                    break;
                case 2:
                    
                    c.GetComponentInChildren<TextMesh>().text += "Blue color is Trapezoid Method\n";
                    break;
                default:
                    break;
            }
        }

        Tops = new List<double>(); //задание листа итераций

        //цикл по точностям для первого метода
        for (n = N0; n <= N; n+=s)
        {
            Output.text += "N = " + n.ToString() + "\n";
            switch (Method)
            {
                case 1:
                    t = new double[n];
                    ReadText();
                    ChebyshevsMethod();
                    break;
                case 2:
                    TrapezoidMethod();
                    break;
                default:
                    break;
            }
        }

        switch (Method2)
        {
            case 1:
                Output.text += "Chebyshev's Method\n";
                c.GetComponentInChildren<TextMesh>().text += "Red color is Chebyshev's Method\n";
                break;
            case 2:
                Output.text += "Trapezoid Method\n";
                c.GetComponentInChildren<TextMesh>().text += "Red color is Trapezoid Method\n";
                break;
            default:
                break;
        }

        //цикл по точностям для второго метода
        for (n = N0; n <= N; n+=s)
        {
            Output.text += "N = " + n.ToString() + "\n";
            switch (Method2)
            {
                case 1:
                    t = new double[n];
                    ReadText();
                    ChebyshevsMethod();
                    break;
                case 2:
                    TrapezoidMethod();
                    break;
                default:
                    break;
            }
        }


        if (KMax > 0) //если всё прошло успешно
        {
            CoordinateSystem(); //рисуем систему координат
            Points(new Color(0, 0, 1, 1), new Color(0, 0, 1, 1), new Color(1, 0, 0, 1), new Color(1, 0, 0, 1)); //рисуем графики
        }
    }

    void SetUp()
    {
        step = (b - a) / stepnumber;
        symbols = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //задание основного словаря
        f = Expr.Parse(func); //парсинг функции
        if (Method == 1 || Method2 == 1)
        {
            N = 9;
            N0 = 2;
            s = 1;
        }
        else
        {
            N = n;
            N0 = 1;
            s = (N - N0) / 8;
        }

        NRoots1 = 0; //обнуляем количество максимальных 
        NRoots2 = 0; //обнуляем количество максимальных 

        XYPLVN(); //координаты области рисования
    }

    //рисование координатной системы
    void CoordinateSystem()
    {
        GameObject YLine; //ось y
        GameObject XLine; //ось x
        Quaternion rotation; //вращение для оси y
        Vector3 scalex = new Vector3(transform.localScale.x, 3, 0); //scale для x
        Vector3 scaley = new Vector3(transform.localScale.y, 3, 0); //scale для y

        double X0 = PreobrX(4); //ноль для задания оси y
        double Y0 = PreobrY(KMax / 2); //ноль для задания оси x
        double XJust0 = PreobrX(0); //ноль по x
        double YJust0 = PreobrY(0); //ноль по y
        double YDivisionsStep = KMax / 8; //шаг для делений по y

        rotation = new Quaternion(x.transform.rotation.x, x.transform.rotation.y, 1, x.transform.rotation.w); //как повернуть ось x на 90 градусов, чтобы получить ось y
        YLine = Instantiate(x, new Vector3((float)XJust0, (float)Y0, 0), x.transform.rotation); //создаём ось y
        YLine.transform.localScale = scaley; //увеличиваем ось y
        YLine.transform.rotation = rotation; //поварачиваем ось y
        YLine.tag = "Line2"; //присваеваем tag для очистки

        //рисуем деления
        double i = YDivisionsStep;
        while (i <= KMax)
        {
            GameObject Division; //новое деление
            Division = Instantiate(YDivision, new Vector3((float)XJust0, (float)PreobrY(i), 0), YDivision.transform.rotation); //создаём деление
            Division.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (i).ToString(); //делаем подпись
            Division.tag = "Division2"; //присваеваем tag для очистки
            i += YDivisionsStep; //идём к следующему делению
        }

        XLine = Instantiate(x, new Vector3((float)X0, (float)YJust0, 0), x.transform.rotation); //создаём ось x
        XLine.transform.localScale = scalex; //увеличиваем ось x
        XLine.tag = "Line2"; //присваеваем tag для очистки

        //рисуем деления
        int j = 1;
        for (i = N0; i <= N; i+=s)
        {
            GameObject Division; //новое деление
            Division = Instantiate(XDivision, new Vector3((float)PreobrX(j), (float)YJust0, 0), XDivision.transform.rotation); //создаём деление
            Division.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (i).ToString(); //делаем подпись
            Division.tag = "Division2"; //присваеваем tag для очистки
            j += 1;
        }
    }

    void Points(Color start, Color end, Color start2, Color end2)
    {
        int i = 0;
        if (NRoots1 > 0) //если были вычисления у первого метода
        {
            GameObject newline = Instantiate(LinePrefab, Vector3.zero, Quaternion.identity); //создаём новую линию
            function = newline.GetComponent<LineRenderer>(); //задаём линии отрисовку
            function.positionCount = NRoots1; //количество точек для линии
            function.startColor = start; //начальный цвет
            function.endColor = end; //конечный цвет
            newline.tag = "Line2";

            //рисуем линию
            for (; i < NRoots1; i++)
            {
                function.SetPosition(i, new Vector3((float)PreobrX(i + 1), (float)PreobrY(Tops[i]), 0));
            }
        }
        if (NRoots2 > 0) //если были вычисления у второго метода
        {
            GameObject newline = Instantiate(LinePrefab, Vector3.zero, Quaternion.identity); //создаём новую линию
            function = newline.GetComponent<LineRenderer>(); //задаём линии отрисовку
            function.positionCount = NRoots2; //количество точек для линии
            function.startColor = start2; //начальный цвет
            function.endColor = end2; //конечный цвет
            newline.tag = "Line2";

            //рисуем линию
            for (int t = 0; t < NRoots2; t++)
            {
                function.SetPosition(t, new Vector3((float)PreobrX(t + 1), (float)PreobrY(Tops[i]), 0));
                i++;
            }
        }
    }

    void ChebyshevsMethod()
    {
        if (n != 8)
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
        Output.text += ("Error = " + rating.ToString() + "\n\n");

        if (Math.Abs(rating) > KMax)
            KMax = Math.Abs(rating);
        Tops.Add(Math.Abs(rating));
        NRoots1++;
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
        double h = (b - a) / (n-1);
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
        Output.text += ("Error = " + rating.ToString() + "\n\n");
        if (Math.Abs(rating) > KMax)
            KMax = Math.Abs(rating);
        Tops.Add(Math.Abs(rating));
        NRoots2++;
    }

    //вычисление координат области рисования графика
    void XYPLVN()
    {
        XP = transform.position.x + transform.localScale.x / 2;
        XL = transform.position.x - transform.localScale.x / 2;
        YN = transform.position.y - transform.localScale.y / 2;
        YV = transform.position.y + transform.localScale.y / 2;
    }


    double PreobrX(double x)//Преобразовать координаты x под СК
    {
        x = x * (XP - XL) / 8 + XL;

        return x;
    }

    double PreobrY(double y)//Преобразовать координаты y под СК
    {
        y = y * (YN - YV) / (-KMax) + YN;
        return y;
    }

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
    }

    void ReadText()
    {
        if (n != 8)
        {
            int i;
            double[][] mas = new double[9][];
            mas[0] = new double[2] { -0.577350, 0.577350 };
            mas[1] = new double[3] { -0.707107, 0, 0.707107 };
            mas[2] = new double[4] { -0.794654, -0.187592, 0.187592, 0.794654 };
            mas[3] = new double[5] { -0.832498, -0.374541, 0, 0.374541, 0.832498 };
            mas[4] = new double[6] { -0.866247, -0.422519, -0.266635, 0.266635, 0.422519, 0.866247 };
            mas[5] = new double[7] { -0.883862, -0.529657, -0.323912, 0, 0.323912, 0.529657, 0.883862 };
            mas[6] = new double[0] { };
            mas[7] = new double[9] { -0.911589, -0.601019, -0.528762, -0.167906, 0, 0.167906, 0.528762, 0.601019, 0.911589 };

            for (i = 0; i < n; i++)
            {
                t[i] = mas[n - 2][i];
            }
        }
    }
}
