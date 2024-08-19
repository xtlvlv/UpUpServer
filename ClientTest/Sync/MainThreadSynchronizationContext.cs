using System;
using System.Collections.Concurrent;
using System.Threading;

public class MainThreadSynchronizationContext : SynchronizationContext
{
    private static MainThreadSynchronizationContext instance;
    public static MainThreadSynchronizationContext Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MainThreadSynchronizationContext();
            }

            return instance;
        }
    }

    private int mMainThreadId = Thread.CurrentThread.ManagedThreadId;
    private readonly ConcurrentQueue<Action> mConcurrentQueue = new ConcurrentQueue<Action>();
    private Action mTempAction;

    public void Update()
    {
        int count = mConcurrentQueue.Count;
        for (int i = 0; i < count; ++i)
        {
            if (mConcurrentQueue.TryDequeue(out mTempAction))
            {
                mTempAction();
            }
        }
    }

    public override void Post(SendOrPostCallback sendOrPostCallback, object state = null)
    {
        if (Thread.CurrentThread.ManagedThreadId == mMainThreadId)
        {
            sendOrPostCallback(state);
            return;
        }

        mConcurrentQueue.Enqueue(() => { sendOrPostCallback(state); });
    }
}