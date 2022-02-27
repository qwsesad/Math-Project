using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lab11GetFunction : MonoBehaviour
{
    public string func;
    public int n;
    public int stepnumber;
    public InputField function;
    public InputField N;
    public InputField StepNumber;
    public ErrorS er;
    public bool Get()
    {
        if (function.text != "")
            func = function.text;
        else
        {
            er.SetUp("Enter function!");
            return false;
        }
        if (N.text != "")
            n = int.Parse(N.text);
        else
        {
            er.SetUp("Enter n!");
            return false;
        }
        if (StepNumber.text != "")
            stepnumber = int.Parse(StepNumber.text);
        else
        {
            er.SetUp("Enter step!");
            return false;
        }
        return true;
    }

}