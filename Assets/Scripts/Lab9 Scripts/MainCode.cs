using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MathNet;
using MathNet.Symbolics;

public class MainCode : MonoBehaviour
{
    public FunctionDraw funcdraw;
    public ComparisonDraw CompDraw;
    public ErrorS er;
    public GetIntervals intervals;
    public GetFunction GetFunc;
    public Dropdown MethodForSolving;
    public Dropdown MethodForComparison;
    Vector3 Canvpos;
    // Start is called before the first frame update
    void Start()
    {
        funcdraw.GetObject().SetActive(false);
    }

    void Update()
    {

    }

    public void Solve()
    {
        funcdraw.GetObject().SetActive(true);
        int method = MethodForSolving.value;

        if (!intervals.Get()) //��������� ���� ����������
            return;

        if (!GetFunc.Get()) //��������� ���� �������, ���� � eps
            return;

        if (method == 0) //���������: ������ �� �����
        {
            er.SetUp("Choose method for solving!");
            return;
        }


        // �������� �������� ��������
        {
            if (intervals.MinX>= intervals.MaxX)
            {
                er.SetUp("MaxX should be more than MinX!");
                return;
            }
            if (intervals.MinY >= intervals.MaxY)
            {
                er.SetUp("MaxY should be more than MinY!");
                return;
            }
            if (GetFunc.eps <= 0)
            {
                er.SetUp("eps should be more than 0");
                return;
            }
            if (GetFunc.step <= 0)
            {
                er.SetUp("step should be more than 0");
                return;
            }

            if (GetFunc.step <= GetFunc.eps)
            {
                er.SetUp("step should be more than eps");
                return;
            }
        }

        // ������� ���������, �������, ���, eps � �� ���������
        {
            funcdraw.MinX = intervals.MinX;
            funcdraw.MaxX = intervals.MaxX;
            funcdraw.MinY = intervals.MinY;
            funcdraw.MaxY = intervals.MaxY;
            funcdraw.func = GetFunc.func;
            funcdraw.step = GetFunc.step;
            funcdraw.eps = GetFunc.eps;
            funcdraw.Method = method;
            CompDraw.DestroyObjects();
            funcdraw.Go();
        }
    }

    public void Compare()
    {
        funcdraw.GetObject().SetActive(true);
        int method = MethodForSolving.value;
        int method2 = MethodForComparison.value;

        if (!intervals.Get()) //��������� ���� ����������
            return;

        if (!GetFunc.Get()) //��������� ���� �������, ���� � eps
            return;

        if (method == 0) //���������: ������ �� �����
        {
            er.SetUp("Choose method for solving!");
            return;
        }

        if (method2 == 0) //���������: ������ �� �����
        {
            er.SetUp("Choose method for comparison!");
            return;
        }

        if (method2 == method) //���������: ������ �� �����
        {
            er.SetUp("Choose another method for comparison!");
            return;
        }


        // �������� �������� ��������
        {
            if (intervals.MinX >= intervals.MaxX)
            {
                er.SetUp("MaxX should be more than MinX!");
                return;
            }
            if (GetFunc.eps <= 0)
            {
                er.SetUp("eps should be more than 0");
                return;
            }
            if (GetFunc.step <= 0)
            {
                er.SetUp("step should be more than 0");
                return;
            }
        }

        // ������� ���������, �������, ���, eps � �� ���������
        {
            CompDraw.MinX = intervals.MinX;
            CompDraw.MaxX = intervals.MaxX;
            CompDraw.MinY = intervals.MinY;
            CompDraw.MaxY = intervals.MaxY;
            CompDraw.func = GetFunc.func;
            CompDraw.step = GetFunc.step;
            CompDraw.eps = GetFunc.eps;
            CompDraw.Method = method;
            CompDraw.Method2 = method2;
            CompDraw.Go();
        }
    }

    public void Exit()
    {
        funcdraw.DestroyObjects();
        CompDraw.DestroyObjects();
    }
}
