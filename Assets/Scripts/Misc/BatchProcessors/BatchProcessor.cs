using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface IBatchProcessor<T>
{
    void Process(List<T> itemsToProcess, Action<T> action, bool waitUntilAllThreadsFinish);
    void Process(int numberOfThreads, List<T> itemsToProcess, Action<T> action, bool waitUntilAllThreadsFinish);
}

/// <summary>
/// Given a list of items to process (like generating terrain for chunks), process them on multiple
/// threads.
/// </summary>
/// <typeparam name="T"></typeparam>
public class BatchProcessor<T> : IBatchProcessor<T> where T : class
{
    //private const int NumberOfThreads = 2;
    //private List<T>[] m_ItemLists;
	
    // Process a list of items on the ideal number of threads
    public void Process(List<T> itemsToProcess, Action<T> action, bool waitUntilAllThreadsFinish)
    {
		int numberOfThreads = Environment.ProcessorCount - 1;
        Process(numberOfThreads, itemsToProcess, action, waitUntilAllThreadsFinish);
    }

    // Process a list of items using the given number of threads.
    public void Process(int numberOfThreads, List<T> itemsToProcess, Action<T> action, bool waitUntilAllThreadsFinish)
    {
		List<Thread> newThreads = new List<Thread>();
        if (numberOfThreads < 1)
        {
            numberOfThreads = 1;
        }
        int totalToProcess = itemsToProcess.Count;
        if (totalToProcess == 0)
        {
            return;
        }

        if (numberOfThreads > totalToProcess)
        {
            numberOfThreads = totalToProcess;
        }

        int itemsPerThread = totalToProcess / numberOfThreads;
        int numberForThreadToProcess = itemsPerThread;
        for (int threadNumber = 0; threadNumber < numberOfThreads; threadNumber++)
        {
            if (threadNumber == numberOfThreads - 1)
            {
                numberForThreadToProcess = (totalToProcess - (threadNumber * itemsPerThread));
            }

            int number = threadNumber;
            int process = numberForThreadToProcess;
            Thread newThread =
                new Thread(() => ActionAgainstMultiple(itemsToProcess, number * itemsPerThread, process, action));
            newThreads.Add(newThread);
            newThread.Start();
        }
		
		//Debug.Log("In Batch Process, Before Join");		
		
        if (waitUntilAllThreadsFinish)
        {
            foreach (Thread newThread in newThreads)
            {
                newThread.Join();
            }
        }
		
		//Debug.Log("In Batch Process, After Join");	
    }


    private static void ActionAgainstMultiple(IList<T> items, int startIndex, int length, Action<T> action)
    {
        for (int index = startIndex; index < startIndex + length; index++)
        {
            T item = items[index];
            if (item != null)
            {
                action(item);
            }

            Thread.Sleep(1);
        }
    }
}