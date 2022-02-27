using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MathExpParser;

public class SampleUIScript : MonoBehaviour
{

    [SerializeField]
    private Text answerText;

    [SerializeField]
    private InputField inputField;

    [SerializeField]
    private Button SubmitButton;

    private MathParser mathParser;

    private void Start()
    {
        mathParser = new MathParser();

        SubmitButton.onClick.AddListener(Calculate);
    }

    private void Calculate() {

        string rawMathExp = inputField.text;

        float answer = mathParser.Calculate(rawMathExp);

        answerText.text = answer.ToString();
    }

}
