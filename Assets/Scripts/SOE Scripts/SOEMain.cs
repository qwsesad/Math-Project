using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SOEMain : MonoBehaviour
{
    public ErrorS er;
    public SOESolve soesol;
    public Dropdown MethodForSolving;
    public Dropdown MethodForComparison;
    public void Solve()
    {
        int method = MethodForSolving.value;
        if (method == 0) //проверяем: выбран ли метод
        {
            er.SetUp("Choose method for solving!");
            return;
        }
        soesol.method = method;
        soesol.Go();
    }
}
