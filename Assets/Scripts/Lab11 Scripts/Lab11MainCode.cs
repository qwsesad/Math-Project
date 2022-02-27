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

        if (!intervals.Get()) //проверяем ввод интервалов
            return;

        if (!GetFunc.Get()) //проверяем ввод функции, шага и eps
            return;

        if (method == 0) //проверяем: выбран ли метод
        {
            er.SetUp("Choose method for solving!");
            return;
        }


        // проверка введённых значений
        {
            if (GetFunc.stepnumber <= 0)
            {
                er.SetUp("Wrong step");
                return;
            }
        }

        // Передаём интервалы, функцию, шаг, eps и всё выполняем
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

        if (!intervals.Get()) //проверяем ввод интервалов
            return;

        if (!GetFunc.Get()) //проверяем ввод функции, шага и eps
            return;

        if (method == 0) //проверяем: выбран ли метод
        {
            er.SetUp("Choose method for solving!");
            return;
        }

        if (method2 == 0) //проверяем: выбран ли метод
        {
            er.SetUp("Choose method for comparison!");
            return;
        }

        if (method2 == method) //проверяем: выбран ли метод
        {
            er.SetUp("Choose another method for comparison!");
            return;
        }

        // проверка введённых значений
        {
            if (GetFunc.stepnumber <= 0)
            {
                er.SetUp("Wrong step");
                return;
            }
        }

        // Передаём интервалы, функцию, шаг, eps и всё выполняем
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