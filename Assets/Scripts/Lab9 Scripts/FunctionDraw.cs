using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using MathNet.Symbolics;
using Expr = MathNet.Symbolics.SymbolicExpression;
using Quaternion = UnityEngine.Quaternion;

public class FunctionDraw : MonoBehaviour
{
    public int N; //количество точек для графика
    public int NInt; //количество интервалов
    public int Method; //метод решения
    int NRoots; //количество корней
    double YN; //нижний y области рисования
    double YV; //верхний y области рисования
    double XP; //правый x области рисования
    double XL; //левый x области рисования
    public bool MouseDown = false; //проверка нажатия левой кнопки мыши в области рисования
    bool inside = false; //проверка наведения курсора на область рисования
    bool shift = false; //проверка нажатия на shift

    public double MinX; //левое значение интервала по x
    public double MaxX; //правое значение интервала по x
    public double MinY; //нижнее значение интервала по y
    public double MaxY; //верхнее значение интервала по y
    public double step; //шаг по x 
    public double eps; //точность
    public List<double> roots; //лист корней
    List<List<double>> intervals; //лист интервалов
    public string func; //строка функции
    Expr f; //функция
    Expr FirstDerivative; //первая производная
    Expr SecondDerivative; //вторая производная
    Vector3 MouseStart; //координаты места, где кнопка мыши была нажата
    Dictionary<string, FloatingPoint> symbols; //основной словарь
    Dictionary<string, FloatingPoint> symbolsd1; //словарь для первой производной
    Dictionary<string, FloatingPoint> symbolsd2; //словарь для второй производной

    List<Vector3> Tops; //лист точек для графика
    LineRenderer function; //линия графика

    public GameObject x; //prefab осей
    public GameObject XDivision; //prefab делений по x
    public GameObject YDivision; //prefab делений по y
    public GameObject Circle; //prefab точки
    public GameObject LinePrefab; //prefab линии
    public GameObject Land; //область рисования
    public ErrorS er; //обработчик ошибок
    public Camera _camera; //камера
    public Text Output; //область вывода

    //запуск решения
    public void Go()
    {
        DestroyObjects(); //очищаем форму и память для случаев, когда решение не первое
        SetUp(); //делаем первые объявления
        CoordinateSystem(); //рисуем систему координат

        //запускаем выбранный метод решения
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
        SetRoots(); //расставляем корни на графике
    }

    //первоначальные объявления
    void SetUp ()
    { 
        symbols = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //задание основного словаря
        symbolsd1 = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //задание словаря для первой производной
        symbolsd2 = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //задание словаря для второй производной
        roots = new List<double>(); //задание листа корней
        intervals = new List<List<double>>(); //задание листа интервалов

        f = Expr.Parse(func); //парсинг функции
        var x_ = Expr.Variable("x"); //задание переменной, по которой будут вычисляться производные
        FirstDerivative = f.Differentiate(x_); //функция первой производной
        SecondDerivative = FirstDerivative.Differentiate(x_); //функция второй производной

        NInt = 0; //обнуляем количество интервалов
        NRoots = 0; //обнуляем количество корней
        N = 0; //обнуляем количество точек для линии

        XYPLVN(); //координаты области рисования
        MinMaxY(); //поиск интервалов
        Points(); //рисование линии
    }

    //рисование координатной системы
    void CoordinateSystem()
    {
        GameObject YLine; //ось y
        GameObject XLine; //ось x
        Quaternion rotation; //вращение для оси y
        Vector3 scalex = new Vector3(transform.localScale.x, 3, 0); //scale для x
        Vector3 scaley = new Vector3(transform.localScale.y, 3, 0); //scale для y

        double X0 = PreobrX((MaxX + MinX)/ 2); //ноль для задания оси y
        double Y0 = PreobrY((MaxY + MinY) / 2); //ноль для задания оси x
        double XJust0 = PreobrX(0); //ноль по x
        double YJust0 = PreobrY(0); //ноль по y
        double XDivisionsStep = (MaxX - MinX) / 5; //шаг для делений по x
        double YDivisionsStep = (MaxY - MinY) / 5; //шаг для делений по y

        //YLine
        if (MinX * MaxX <= 0) //Если ось y видно
        {
            rotation = new Quaternion(x.transform.rotation.x, x.transform.rotation.y, 1, x.transform.rotation.w);  //как повернуть ось x на 90 градусов, чтобы получить ось y
            YLine = Instantiate(x, new Vector3((float) XJust0, (float) Y0, 0), x.transform.rotation); //создаём ось y
            YLine.transform.localScale = scaley; //увеличиваем ось y
            YLine.transform.rotation = rotation; //поварачиваем ось y
            YLine.tag = "Line"; //присваеваем tag для очистки

            //рисуем деления
            double i = MinY;
            while (i <= MaxY)
            {
                if (i != 0) //если y не равно 0
                {
                    GameObject Division; //новое деление
                    Division = Instantiate(YDivision, new Vector3((float) XJust0, (float) PreobrY(i), 0), YDivision.transform.rotation); //создаём деление
                    Division.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (((double)((int)(i * 100))) / 100).ToString(); //делаем подпись
                    Division.tag = "Division"; //присваеваем tag для очистки
                    i += YDivisionsStep; //идём к следующему делению
                }
                else //если y равно 0
                    i += YDivisionsStep; //деление рисовать не надо, идём дальше
            }
        }

        //Xline
        if (MinY * MaxY <= 0) //Если ось x видно
        {
            XLine = Instantiate(x, new Vector3((float) X0, (float) YJust0, 0), x.transform.rotation); //создаём ось x
            XLine.transform.localScale = scalex; //увеличиваем ось x
            XLine.tag = "Line"; //присваеваем tag для очистки

            //рисуем деления
            double i = MinX;
            while (i <= MaxX)
            {
                if (i != 0) //если x не равно 0
                {
                    GameObject Division; //новое деление
                    Division = Instantiate(XDivision, new Vector3((float) PreobrX(i), (float) YJust0, 0), XDivision.transform.rotation); //создаём деление
                    Division.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (((double)((int)(i*100)))/100).ToString(); //делаем подпись
                    Division.tag = "Division"; //присваеваем tag для очистки
                    i += XDivisionsStep; //идём к следующему делению
                }
                else //если x равно 0
                    i += XDivisionsStep; //деление рисовать не надо, идём дальше
            }
        }
    }

    //поиск интервалов
    void MinMaxY()
    {
        double Min = 0; //минимальное значение y на интервале
        double Max = 0; //максимальное значение y на интервале
        double prom = 0; //предыдущее значение функции
        double next = 0; //следующее значение функции
        double x1 = MinX; //следующее значение аргумента
        double x0 = MinX; //предыдущее значение аргумента
        bool check = false; //проверка необходимых условий

        //цикл первого значения (проверка разрыва в начале интервала)
        while (!check && x1<=MaxX) 
        {
            try //если функции не будет вычисляться, то осуществится переход в catch
            {
                symbols.Remove("x");
                symbols.Add("x", x1);
                Min = Max = next = f.Evaluate(symbols).RealValue; //вычисление значение функции и задание первых значений для минимума и максимума

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

        // основной цикл поиска интервалов
        while (x1 <= MaxX)
        {
            try //если функции не будет вычисляться, то осуществится переход в catch
            {
                symbols.Remove("x");
                symbols.Add("x", x1);
                next = f.Evaluate(symbols).RealValue; //вычисление значение функции

                //поиск минимальных и максимальных значений
                if (next > Max)
                    Max = next;
                else if (next < Min)
                    Min = next;

                try //если производные не будут вычисляться, то значения не войдут в интервалы
                {
                    //проверка необходимых условий интервала
                    if ((prom * next) <= 0 && Method == 1) //проверка условия разных знаков и дальнейшие проверки, если испольхуется метод ньютона 
                    {
                        double d1_1;
                        double d1_2;

                        symbolsd1.Remove("x");
                        symbolsd1.Add("x", x0);
                        d1_1 = FirstDerivative.Evaluate(symbols).RealValue; //первая производная на левом значении аргумента

                        symbolsd1.Remove("x");
                        symbolsd1.Add("x", x1);
                        d1_2 = FirstDerivative.Evaluate(symbols).RealValue; //первая производная на правом значении аргумента

                        //если первая производныя и на левом значении аргумента, и на правом не равна нулю
                        if (d1_1 * d1_2 != 0) 
                        {
                            double d2_1;
                            double d2_2;

                            symbolsd2.Remove("x");
                            symbolsd2.Add("x", x0);
                            d2_1 = SecondDerivative.Evaluate(symbols).RealValue; //вторая производная на левом значении аргумента

                            symbolsd1.Remove("x");
                            symbolsd1.Add("x", x1);
                            d2_2 = SecondDerivative.Evaluate(symbols).RealValue; //вторая производная на правом значении аргумента

                            //если вторая производныя и на левом значении аргумента, и на правом не равна нулю
                            if (d2_1 * d2_2 != 0)
                            {
                                //добавляем интервал
                                intervals.Add(new List<double>());
                                intervals[NInt].Add(x0);
                                intervals[NInt].Add(x1);
                                NInt++;
                            }
                        }
                    }
                    else if ((prom * next) <= 0 && Method != 1) //если разные знаки и используется не метод ньютона
                    {
                        //добавляем интервал
                        intervals.Add(new List<double>());
                        intervals[NInt].Add(x0);
                        intervals[NInt].Add(x1);
                        NInt++;
                    }
                }
                catch 
                { 
                    //если где-то производная не вычислились для метода ньютона, то значения аргументов не входят в интервал
                }

                x0 = x1;
                x1 += step;
                prom = next;
            }
            catch //если в точке разрыв функции
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

        //запись интервалов для y, если они не были заданы изначально
        if (MinY == -0.0123455)
            MinY = Min;
        if (MaxY == 0.0123455)
            MaxY = Max;
    }

    //рисование графика и задание точек для линии
    void Points()
    {
        double prom = 0;
        double x0 = MinX;
        double next = 0;

        Tops = new List<Vector3>(); //задаём лист точек для линии

        while (x0 <= MaxX)
        {
            try //если функции не будет вычисляться, то осуществится переход в catch
            {
                symbols.Remove("x");
                symbols.Add("x", (x0+x0+step)/2);
                next = f.Evaluate(symbols).RealValue; //вычисляем значение функции в середине интервала
                symbols.Remove("x");
                symbols.Add("x", x0);
                prom = f.Evaluate(symbols).RealValue; //вычсисляем значение функции при новом x

                if (prom <= MaxY && prom >= MinY) //если значение функции в пределах интервала по y
                {
                    Tops.Add(new Vector3((float)PreobrX(x0), (float)PreobrY(prom), 0));
                    N++;
                }
                if (next < MinY || next > MaxY) //если вышли за пределы
                    Line(); //рисуем линию до разрыва
                x0 += step;
            }
            catch //если в точке разрыв функции
            {
                Line(); //рисуем линию до разрыва
                x0 += step;
            }
        }
        Line(); //рисуем линию
    }

    //рисование линии
    void Line()
    {
        if (N != 0)
        {
            GameObject newline = Instantiate(LinePrefab, Vector3.zero, Quaternion.identity); //создаём новую линию
            function = newline.GetComponent<LineRenderer>(); //задаём линии отрисовку
            function.positionCount = N; //количество точек для линии

            //рисуем линию
            int z = 0;
            foreach (Vector3 top in Tops)
            {
                function.SetPosition(z, top);
                z++;
            }
            N = 0; //обнуляем крличество точек для линии
            Tops = new List<Vector3>(); //обнуляем списко точек для линии
        }
    }

    //Метод Ньютона
    void NewtonsMethod()
    {
        int i;
        double S; //значение функции
        double A = 0; //предыдущее значение функции
        double S1; //значение первой производной
        double S2; //значение второй производной 
        double x0; //корень
        bool check; //проверка необходимых условий
        int k; //количество итераций
        double H; //f(x)/f'(x)
        double Min; //минимальное значение первой производной на интервале
        double Max; //максимальное значение второй производной на интервале

        //цикл по интервалам
        for (i = 0; i < NInt; i++)
        {
            k = 1;
            check = true;
            H = 0;
            x0 = 0;
            Max = 0;
            Min = 0;
            try //если функции или производные не будут вычисляться, то осуществится переход в catch
            {
                double S11; //первая производная на левом значении интервалов
                double S21; //вторая производная на левом значении интервалов
                double S12; //первая производная на правом значении интервалов
                double S22; //вторая производная на правом значении интервалов
                symbols.Remove("x");
                symbols.Add("x", intervals[i][0]);
                S = f.Evaluate(symbols).RealValue; //вычисление функции на левом значении интервала
                S2 = SecondDerivative.Evaluate(symbols).RealValue; //вычисление второй производной на левом значении интервала
                S1 = FirstDerivative.Evaluate(symbols).RealValue; //вычисление первой производной на левом значении интервала
                S11 = Math.Abs(S1);
                S21 = Math.Abs(S2);

                //выбор первоначального приближения
                if (S * S2 > 0) //если значение функции и второй производной больше нуля на левом значении интервала
                    x0 = intervals[i][0]; //левое значение 
                else //если значение функции и второй производной не больше нуля на левом значении интервала
                {
                    symbols.Remove("x");
                    symbols.Add("x", intervals[i][1]);
                    S = f.Evaluate(symbols).RealValue; //вычисление функции на правом значении интервала
                    S2 = SecondDerivative.Evaluate(symbols).RealValue; //вычисление второй производной на правом значении интервала

                    if (S * S2 > 0) //если значение функции и второй производной больше нуля на правом значении интервала 
                    {
                        x0 = intervals[i][1]; //певрое приближение корня
                    }
                    else //если значение функции и второй производной везде не больше нуля
                        check = false; //необходимое условие не выполняется
                }

                S1 = FirstDerivative.Evaluate(symbols).RealValue; //вычисление первой производной на первоначальном приближении
                if (S1 != 0) //если первая производная не равна нулю
                {
                    A = S = f.Evaluate(symbols).RealValue; //вычисление функции
                    H = S / S1;
                }
                else //если первая производная равна нулю
                    check = false; //необходимое условие не выполняется
                x0 -= H; //первая итерация

                symbols.Remove("x");
                symbols.Add("x", intervals[i][1]);
                S12 = Math.Abs(FirstDerivative.Evaluate(symbols).RealValue); //вычисление первой производной на правом значении интервала
                S22 = Math.Abs(SecondDerivative.Evaluate(symbols).RealValue); //вычисление второй производной на правом значении интервала

                //поиск первого минимума для первой производной
                if (S11 < S12)
                    Min = S11;
                else
                    Min = S12;

                //поиск первого максимума для второй производной
                if (S22 > S21)
                    Max = S22;
                else
                    Max = S21;

                //цикл итераций с проверкой необходимых условий и оценкой точности
                while (check && (Math.Abs(Max/(Min*2)*H*H) >= eps))
                {
                    k++;
                    symbols.Remove("x");
                    symbols.Add("x", x0);
                    S1 = FirstDerivative.Evaluate(symbols).RealValue; //вычисление первой производной
                    S2 = SecondDerivative.Evaluate(symbols).RealValue; //вычисление второй производной
                    if (S1 != 0) //если первая производная не равна нулю 
                    {
                        A = S;
                        S = f.Evaluate(symbols).RealValue; //вычисление функции
                        H = S / S1;
                    }
                    else //если первая производная равна нулю
                        check = false; //необходимое условие не выполняется
                    x0 -= H; //следующее приближение корня

                    //поиск минимума для первой производной
                    S11 = Math.Abs(S1);
                    if (S11 < Min)
                        Min = S11;

                    //поиск максимума для второй производной
                    S22 = Math.Abs(S2);
                    if (S22 > Max)
                        Max = S22;

                    if (S2 == 0) //если вторая производная равна нулю
                        check = false; //необходимое условие не выполняется

                    if (Math.Abs(A) < Math.Abs(S)) //если есть разрыв
                        check = false; //необходимое условие не выполняется

                    if (x0 > intervals[i][1] || x0 < intervals[i][0]) //если корень вышел из интервала
                        check = false; //необходимое условие не выполняется

                    if (k > 1000) //если число итераций больше 1000
                    {
                        er.SetUp("Sorry( K > 1000. We have to stop it"); //вывод ошибки
                        check = false; //прерывание цикла во избежании бесконечных или очень долгих вычислений
                    }    
                }
                
                //перед добавление корня в список проверяем были ли выполнены необходимые условия на интервале
                if (check) //если условия выполнены
                {
                    symbols.Remove("x");
                    symbols.Add("x", x0);
                    S = f.Evaluate(symbols).RealValue; //вычисление функции в корне

                    //вывод
                    Output.text += "Root: \n";
                    Output.text += "a = " + intervals[i][0].ToString() + "\n";
                    Output.text += "b = " + intervals[i][1].ToString() + "\n";
                    Output.text += "k = " + k.ToString() + "\n";
                    Output.text += "x = " + x0.ToString() + "\n";
                    Output.text += "y(x) = " + S.ToString() + "\n\n";

                    //добавление корня, если его ещё нет в списке
                    if (!roots.Contains(x0))
                    {
                        roots.Add(x0);
                        NRoots++;
                    }
                }
            }
            catch
            { 
                //если где-то не удалось вычислить функцию или производные, то интервал пропускается
            }

        }
    }

    void TheDichotomyMethod()
    {
        int i;
        double A; //левое значение функции
        double B; //правое значение функции
        double FC; //значение функции по середине
        double c; //аргумент по середине
        double x1; //левое значение интервала
        double x2; //правое значение интервала
        bool check; //проверка необходимых условий
        int k; //количество итераций

        //цикл по интервалам
        for (i = 0; i < NInt; i++)
        {
            k = 0;
            check = true;
            c = 0;
            FC = 0;
            try //если функции или производные не будут вычисляться, то осуществится переход в catch
            {
                x1 = intervals[i][0];
                x2 = intervals[i][1];
                symbols.Remove("x");
                symbols.Add("x", x1);
                A = f.Evaluate(symbols).RealValue; //вычисление функции на левом значении интервала
                symbols.Remove("x");
                symbols.Add("x", x2);
                B = f.Evaluate(symbols).RealValue; //вычисление функции на правом значении интервала

                //цикл итераций с проверкой необходимых условий и оценкой точности
                while (check && (Math.Abs(x2 - x1) >= eps))
                {
                    c = (x1 + x2) / 2; //вычисление значения аргумента по середине
                    symbols.Remove("x");
                    symbols.Add("x", c);
                    FC = f.Evaluate(symbols).RealValue; //вычисление значения функции по середине

                    if (FC * B >= 0 && Math.Abs(FC) <= Math.Abs(B)) //если одинаковые знаки значения функции по середине и правого значение функции, или одно из значений равно нулю, а также нет разрыва
                    {
                        B = FC; //правое значение функции становится равно значению функции по середине
                        x2 = c; //правое значение интервала становится равно аргументу по середине
                    }
                    else if (FC * A >= 0 && Math.Abs(FC) <= Math.Abs(A)) //если одинаковые знаки значения функции по середине и левого значение функции, или одно из значений равно нулю, а также нет разрыва
                    {
                        A = FC; //левое значение функции становится равно значению функции по середине
                        x1 = c; //левое значение интервала становится равно аргументу по середине
                    }
                    else //если появился разрыв или проблема с интервалами
                        check = false; //необходимое условие не выполняется

                    if (c > intervals[i][1] || c < intervals[i][0]) //если корень вышел из интервала
                        check = false; //необходимое условие не выполняется

                    k++;
                    if (k > 1000) //если число итераций больше 1000
                    {
                        er.SetUp("Sorry( K > 1000. We have to stop it"); //вывод ошибки
                        check = false; //прерывание цикла во избежании бесконечных или очень долгих вычислений
                    }
                }

                if (check)
                {
                    //вывод
                    Output.text += "Root: \n";
                    Output.text += "a = " + intervals[i][0].ToString() + "\n";
                    Output.text += "b = " + intervals[i][1].ToString() + "\n";
                    Output.text += "k = " + k.ToString() + "\n";
                    Output.text += "x = " + c.ToString() + "\n";
                    Output.text += "y(x) = " + FC.ToString() + "\n\n";

                    //добавление корня, если его ещё нет в списке
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

    //проставление корней на графике красными точками
    void SetRoots()
    {
        GameObject Root;
        float Y0 = (float) PreobrY(0);
        foreach (double root in roots)
        {
            Root = Instantiate(Circle, new Vector3((float)PreobrX(root), Y0, 3), Circle.transform.rotation);
        }
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
       x = (x-MinX)*(XP-XL)/(MaxX-MinX)+XL;

       return x;
    }

    double PreobrY(double y)//Преобразовать координаты y под СК
    {
        y = (y - MinY) * (YN - YV) / (MinY - MaxY) + YN;
        return y;
    }

    //очистка памяти и формы
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

    //проверка нажатия на левую кнопку мыши внутри области рисования
    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseDown = true;
            MouseStart = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _camera.transform.position.y));
        }
    }

    //проверка отпускания левой кнопки мыши внутри области рисования
    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0))
            MouseDown = false;
    }

    //отправить объект
    public GameObject GetObject()
    {
        return Land;
    }

    //проверка нахождения курсора в области рисования
    private void OnMouseEnter()
    {
        inside = true;
    }

    //проверка выхода курсора из области рисования
    private void OnMouseExit()
    {
        inside = false;
    }

    //проверка нажатия на shift и отпускания клавиши
    private void Shift()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            shift = true;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            shift = false;
    }

    //изменение области рисования
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
