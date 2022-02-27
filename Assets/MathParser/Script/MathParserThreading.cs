using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_WEBGL
using System.Threading;
#endif

using MathExpParser.Utility;

namespace MathExpParser
{
    public class MathParserThreading : MonoBehaviour
    {
        #region Parameter

        //Singleton
        private static MathParserThreading s_Instance;
        public static MathParserThreading Instance
        {
            get
            {
                if (s_Instance != null)
                {
                    return s_Instance;
                }

                s_Instance = FindObjectOfType<MathParserThreading>();
                if (s_Instance != null)
                {
                    return s_Instance;
                }

                GameObject emptyObject = new GameObject();
                s_Instance = emptyObject.AddComponent<MathParserThreading>();

                return s_Instance;
            }
        }

        private MathParser _mathParser;
        public MathParser mathParser {
            get {
                if (_mathParser == null) {
                    _mathParser = new MathParser();
                }
                return _mathParser;
            }
        }

        private FastQueue<TaskResult> results = new FastQueue<TaskResult>();
        private FastQueue<System.Action> enqueueTask = new FastQueue<System.Action>();

        #endregion

        public void CalculateAsyn(string math_pression, System.Action<ParseResult> p_callback, Dictionary<string, float> customLookUpTable = null)
        {
#if !UNITY_WEBGL
            enqueueTask.Enqueue(delegate { Calculate(math_pression, p_callback, customLookUpTable); });
#else
            Calculate(math_pression, p_callback, customLookUpTable);
#endif
        }

        private void Calculate(string math_pression, System.Action<ParseResult> p_callback, Dictionary<string, float> customLookUpTable = null)
        {

            float answer = mathParser.Calculate(math_pression, customLookUpTable);

            ParseResult parsedResult = new ParseResult(answer);

            results.Enqueue(new TaskResult(parsedResult, p_callback));
        }

        void WhileLoopThreadFunc() {
#if !UNITY_WEBGL

            Thread t = new Thread(new ThreadStart(() => {

                int queueLength = enqueueTask.Count;
                try
                {
                    for (int i = 0; i < queueLength; i++)
                    {
                        var task = enqueueTask.Dequeue();
                        task();
                    }
                }
                catch { 
                    
                }
            }));

            t.Start();
#endif
        }

        void Update()
        {
            WhileLoopThreadFunc();

            if (results.Count > 0)
            {
                int itemsInQueue = results.Count;
                lock (results)
                {
                    for (int i = 0; i < itemsInQueue; i++)
                    {
                        TaskResult result = results.Dequeue();

                        if (result.callback != null)
                            result.callback(result.parseResult);
                    }
                }
            }
        }

        #region Data Structure
        private struct TaskResult
        {
            public System.Action<ParseResult> callback;
            public ParseResult parseResult;
            public TaskResult(ParseResult p_parseResult, System.Action<ParseResult> p_callback)
            {
                parseResult = p_parseResult;
                callback = p_callback;
            }
        }

        public struct ParseResult
        {
            public float answer;

            public ParseResult(float answer)
            {
                this.answer = answer;
            }
        }
        #endregion

    }
}