using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ThreadTask
{
    public object param;
    public object result;
    public Action<object> completed;
}

public class ThreadManager : MonoBehaviour
{
    private ManualResetEvent resetEvent = new ManualResetEvent(false);

    //private Queue<ThreadTask> _readyQueue = new Queue<ThreadTask>();

    private ConcurrentQueue<ThreadTask> _readyQueue = new ConcurrentQueue<ThreadTask>();
    private ConcurrentQueue<ThreadTask> _completeQueue = new ConcurrentQueue<ThreadTask>();

    private CancellationTokenSource cts = new CancellationTokenSource();
    private List<Thread> _workerThread = new List<Thread>();

    private object listLock = new object();
    private HashSet<int> _taskSet = new HashSet<int>();

    static ThreadManager() {
        int workThreadCount = 0;
        int completionThreadCount = 0;
        ThreadPool.GetMaxThreads(out workThreadCount, out completionThreadCount);
        Debug.Log($"min Thread = {workThreadCount}, completion = {completionThreadCount}");

        // 1200, 200
        var result = ThreadPool.SetMinThreads(0, 0);
        result = ThreadPool.SetMaxThreads(1200, 200);
        Debug.Log($"min set = {result}");
    }

    private void Awake()
    {
        //var result = ThreadPool.SetMinThreads(8, 0);
        //Debug.Log($"min set = {result}");
    }

    // Start is called before the first frame update
    private void Start()
    {
       
        //ThreadPool.SetMaxThreads(4, 0);

        /*
        for(int i = 0; i < 16; i++)
        {
            _workerThread.Add(new Thread(_WorkerThreadFunc));
        }

        foreach(var thread in _workerThread)
        {
            thread.Start();
        }
        */

        StartCoroutine(_CheckThread());
    }

    // Update is called once per frame
    private void Update()
    {
        /*
        if(_readyQueue.Count > 0)
        {
            resetEvent.Set();
        }
        */


        while (_completeQueue.Count > 0)
        {
            ThreadTask threadTask = null;
            _completeQueue.TryDequeue(out threadTask);
            if (threadTask == null) {
                continue;
            }

            threadTask?.completed.Invoke(threadTask.result);
        }
    }

    private void OnDestroy()
    {
        cts.Cancel();

        foreach (var thread in _workerThread)
        {
            thread.Abort();
        }
    }

    public void PushTask(Action<object> completed, object param)
    {
        var threadTask = new ThreadTask()
        {
            param = param,
            completed = completed
        };

        //_readyQueue.Enqueue(threadTask);

        
        var task = Task.Run(() =>
        {
            UpdateThread(threadTask);
        });
        

        //ThreadPool.QueueUserWorkItem(UpdateThread, threadTask);
        //resetEvent.Set();
    }

    private void _WorkerThreadFunc()
    {
        try
        {
            while (true)
            {
                resetEvent.WaitOne();

                ThreadTask threadTask;
                _readyQueue.TryDequeue(out threadTask);
                if (threadTask == null)
                {
                    continue;
                }

                UpdateThread(threadTask);
            }
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }


    private void UpdateThread(object state)
    {
        var threadTask = state as ThreadTask;
        if (threadTask == null) {
            return;
        }

        lock(listLock)
        {
            _taskSet.Add(Thread.CurrentThread.ManagedThreadId);
        }

        // 어떤 처리
        _ProcessThreadTask(threadTask);

        //
        _completeQueue.Enqueue(threadTask);
    }

    private void _ProcessThreadTask(ThreadTask task)
    {
        var value = task.param as int?;
        if (value == null) {
            return;
        }

        value *= 1000;

        var rand = new System.Random(0);
        var sleepTime = rand.Next(2000, 3000);

        if (cts.IsCancellationRequested) {
            return;
        }

        //Thread.Sleep(sleepTime);

        long result = 0;
        for(long i = 1; i <= 50000000; i++)
        {
            result += i;

            if (cts.IsCancellationRequested)
            {
                return;
            }
        }

        Debug.Log($"ThreadId = {Thread.CurrentThread.ManagedThreadId}, result = {result}");

        task.result = result;
    }

    IEnumerator _CheckThread()
    {
        while(true)
        {
            int workThreadCount = 0;
            int completionThreadCount = 0;
            ThreadPool.GetAvailableThreads(out workThreadCount, out completionThreadCount);

            Debug.Log($"workThreadCount = {workThreadCount}, completionThreadCount = {completionThreadCount}, thread count = {_taskSet.Count}");

            yield return new WaitForSeconds(0.1f);
        }
    }

}
