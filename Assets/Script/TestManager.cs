using UnityEngine;
using System.Collections;
using System.Threading;

public class TestManager : MonoBehaviour
{
    ThreadManager _threadManager;

    // Use this for initialization
    void Start()
    {
        _threadManager = GetComponent<ThreadManager>();

        StartCoroutine(Request());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator Request()
    {
        for(int i = 0; i < 100; i++)
        {
            //int workThreadCount = 0;
            //int completionThreadCount = 0;
            //ThreadPool.GetAvailableThreads(out workThreadCount, out completionThreadCount);
            //Debug.Log($"workThreadCount = {workThreadCount}, completionThreadCount = {completionThreadCount}");

            var capture = i;
            _threadManager.PushTask((obj) => {

                var result = obj as int?;
                //Debug.Log($"result = {result}");

                var gameObject = new GameObject($"{capture}-1");

            }, i);

            _threadManager.PushTask((obj) => {

                var result = obj as int?;
                //Debug.Log($"result = {result}");

                var gameObject = new GameObject($"{capture}-2");

            }, i);

            _threadManager.PushTask((obj) => {

                var result = obj as int?;
                //Debug.Log($"result = {result}");

                var gameObject = new GameObject($"{capture}-3");

            }, i);

            yield return new WaitForSeconds(0.1f);
        }
    }
}
