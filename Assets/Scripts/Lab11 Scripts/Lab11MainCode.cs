using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MathNet;
using MathNet.Symbolics;
using System;

public class Lab11MainCode : MonoBehaviour
{
    public Lab11Solve solvef;
    //public ComparisonDraw CompDraw;
    public ErrorS er;
    public Lab11GetIntervals intervals;
    public Lab11GetFunction GetFunc;
    public Dropdown MethodForSolving;
    public Dropdown MethodForComparison;
    public Lab11Compare CompDraw;
    Vector3 Canvpos;


    public void Solve()
    {
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
            if (GetFunc.stepnumber <= 0)
            {
                er.SetUp("Wrong step");
                return;
            }
        }

        // ������� ���������, �������, ���, eps � �� ���������
        {
            solvef.a = intervals.a;
            solvef.b = intervals.b;
            solvef.n = GetFunc.n;
            solvef.func = GetFunc.func;
            solvef.stepnumber = GetFunc.stepnumber;
            solvef.Method = method;
            CompDraw.DestroyObjects();
            solvef.Go();
        }
    }

    public void Compare()
    {
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
            if (GetFunc.stepnumber <= 0)
            {
                er.SetUp("Wrong step");
                return;
            }
        }

        // ������� ���������, �������, ���, eps � �� ���������
        {
            CompDraw.a = intervals.a;
            CompDraw.b = intervals.b;
            CompDraw.func = GetFunc.func;
            CompDraw.stepnumber = GetFunc.stepnumber;
            CompDraw.Method = method;
            CompDraw.Method2 = method2;
            CompDraw.Go();
        }
    }

    public void Exit()
    {
        CompDraw.DestroyObjects();
    }
}