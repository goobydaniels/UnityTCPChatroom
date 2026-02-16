using System.Collections.Concurrent;
using System;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly ConcurrentQueue<Action> m_Queue = new ConcurrentQueue<Action>();

    // Singleton instance to allow easy access from other threads
    private static UnityMainThreadDispatcher m_Instance = null;

    private void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Process the queue on the main thread
        while (m_Queue.TryDequeue(out Action action))
        {
            action.Invoke();
        }
    }

    // Method to enqueue actions from background threads
    public static void Enqueue(Action action)
    {
        m_Queue.Enqueue(action);
    }
}
