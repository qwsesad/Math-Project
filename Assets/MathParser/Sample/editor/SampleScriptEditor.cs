using UnityEditor;
using UnityEngine;
namespace MathExpParser
{
    [CustomEditor(typeof(SampleScript))]
    public class SampleScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SampleScript myScript = (SampleScript)target;
            if (GUILayout.Button("Try out"))
            {
                myScript.Execute();
            }
        }
    }
}