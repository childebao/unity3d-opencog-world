using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Represents a First-In, First-Out thread-safe collection of objects
/// </summary>
/// <typeparam name="T"></typeparam>
public class TQueue<T> : ICollection
{
    #region Variables

    /// <summary>
    /// The private q which holds the actual data
    /// </summary>
    private readonly Queue<T> m_Queue;

    /// <summary>
    /// Lock for the Q
    /// </summary>
    private readonly ReaderWriterLockSlim LockQ = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

    /// <summary>
    /// Used only for the SyncRoot properties
    /// </summary>
    private readonly object objSyncRoot = new object();
	
	public String Name;
	

    // Variables

    #endregion

    #region Init

    /// <summary>
    /// Initializes the Queue
    /// </summary>
    public TQueue()
    {
        m_Queue = new Queue<T>();
    }
	
	public TQueue(string queueName)
    {
		this.Name = queueName;
        m_Queue = new Queue<T>();
    }

    /// <summary>
    /// Initializes the Queue
    /// </summary>
    /// <param name="capacity">the initial number of elements the queue can contain</param>
    public TQueue(int capacity)
    {
        m_Queue = new Queue<T>(capacity);
    }

    /// <summary>
    /// Initializes the Queue
    /// </summary>
    /// <param name="collection">the collection whose members are copied to the Queue</param>
    public TQueue(IEnumerable<T> collection)
    {
        m_Queue = new Queue<T>(collection);
    }

    // Init

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// Returns an enumerator that enumerates through the collection
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        Queue<T> localQ;

        // init enumerator
		Console.print ("EnterReadLock TQueue 76");
        LockQ.EnterReadLock();
        try
        {
            // create a copy of m_TList
            localQ = new Queue<T>(m_Queue);
        }
        finally
        {
            LockQ.ExitReadLock();
        }

        // get the enumerator
        foreach (T item in localQ)
        {
            yield return item;
        }
    }

    #endregion

    #region IEnumerable Members

    /// <summary>
    /// Returns an enumerator that enumerates through the collection
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        Queue<T> localQ;

        // init enumerator
		Console.print ("EnterReadLock TQueue 107");
        LockQ.EnterReadLock();
        try
        {
            // create a copy of m_TList
            localQ = new Queue<T>(m_Queue);
        }
        finally
        {
            LockQ.ExitReadLock();
        }

        // get the enumerator
        foreach (T item in localQ)
        {
            yield return item;
        }
    }

    #endregion

    #region ICollection Members

    #region CopyTo

    /// <summary>
    /// Copies the Queue's elements to an existing array
    /// </summary>
    /// <param name="array">the one-dimensional array to copy into</param>
    /// <param name="index">the zero-based index at which copying begins</param>
    public void CopyTo(Array array, int index)
    {
		Console.print ("EnterReadLock TQueue 139");
        LockQ.EnterReadLock();
        try
        {
            // copy
            m_Queue.ToArray().CopyTo(array, index);
        }

        finally
        {
            LockQ.ExitReadLock();
        }
    }

    /// <summary>
    /// Copies the Queue's elements to an existing array
    /// </summary>
    /// <param name="array">the one-dimensional array to copy into</param>
    /// <param name="index">the zero-based index at which copying begins</param>
    public void CopyTo(T[] array, int index)
    {
		Console.print ("EnterReadLock TQueue 160");
        LockQ.EnterReadLock();
        try
        {
            // copy
            m_Queue.CopyTo(array, index);
        }

        finally
        {
            LockQ.ExitReadLock();
        }
    }

    // CopyTo

    #endregion

    #region Count

    /// <summary>
    /// Returns the number of items in the Queue
    /// </summary>
    public int Count
    {
        get
        {
			//Console.print ("EnterReadLock TQueue 187, current thread: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            //LockQ.EnterReadLock();
			Boolean bLockAcquired = false;
			
			try {
				LockQ.EnterUpgradeableReadLock();
				bLockAcquired = true;
			} catch (Exception ex) {
				throw new Exception("Lock not acquired: " + ex.ToString());
			}
			
			if (bLockAcquired)
			{
				try
	            {
	                return m_Queue.Count;
	            }
	
	            finally
	            {
	                LockQ.ExitUpgradeableReadLock();
	            }
			}
			else
			{
				Console.print ("Failed to acquire lock in TQueue.Count()");	
				
				throw new Exception("Lock not acquired in TQueue.Count()");
				
				return int.MaxValue;
			}
            
        }
    }

    // Count

    #endregion

    #region IsSynchronized

    public bool IsSynchronized
    {
        get { return true; }
    }

    // IsSynchronized

    #endregion

    #region SyncRoot

    public object SyncRoot
    {
        get { return objSyncRoot; }
    }

    // SyncRoot

    #endregion

    #endregion

    #region Enqueue

    /// <summary>
    /// Adds an item to the queue
    /// </summary>
    /// <param name="item">the item to add to the queue</param>
    public void Enqueue(T item)
    {
		Boolean bLockAcquired = false;
		
		//Console.print ("In Enqueue, before EnterWriteLock");
		//Console.print ("Enqueing item: " + item.ToString() + ", current thread: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
		
		try 
		{
			LockQ.EnterWriteLock();
			bLockAcquired = true;
		} 
		catch(LockRecursionException lre)
		{
			Console.print("Catch on EnterWriteLock: Type: " + lre.GetType().Name + ", message: " + lre.Message + ", Source: " + lre.Source);
			
			if (LockQ.IsReadLockHeld)
			{
				Console.print ("Read lock is held");	
				
				Console.print ("Current read count: " + LockQ.CurrentReadCount);
				
				Console.print ("Trying to upgrade read lock");
				
				if (LockQ.IsUpgradeableReadLockHeld)
				{
					Console.print ("IsUpgradeableReadLockHeld = true");					
				}
				else
				{
					Console.print ("IsUpgradeableReadLockHeld = false");	
				}
				
				LockQ.EnterWriteLock ();
				bLockAcquired = true;
				
				Console.print ("Upgrade read lock complete.");
			}
			else
			{
				Console.print ("No read lock is held, trying again");
				
				Console.print ("Current read count: " + LockQ.CurrentReadCount);
				
				try 
				{
					LockQ.EnterWriteLock();
					bLockAcquired = true;
				} 
				catch(LockRecursionException lre2)
				{
					Console.print("Catch on EnterWriteLock attempt 2: Type: " + lre.GetType().Name + ", message: " + lre.Message + ", Source: " + lre.Source);
					
					throw lre2;
				}
			}
		}
        
		//Console.print ("In Enqueue, after EnterWriteLock");
		
		if (bLockAcquired) 
		{
			try
	        {
				//Console.print ("Enqueueing something with hascode: " + item.GetHashCode() + "!");
				m_Queue.Enqueue(item);
	        }
			catch(LockRecursionException lre)
			{
				 Console.print("Catch on m_Queue.Enqueue: Type: " + lre.GetType().Name + ", message: " + lre.Message);
			}
			
	        finally
	        {
	            LockQ.ExitWriteLock();
	        }
		}
		
        
    }

    // Enqueue

    #endregion

    #region Dequeue

    /// <summary>
    /// Removes and returns the item in the beginning of the queue
    /// </summary>
    public T Dequeue()
    {
        LockQ.EnterWriteLock();
        try
        {
            return m_Queue.Dequeue();
        }

        finally
        {
            LockQ.ExitWriteLock();
        }
    }

    // Dequeue

    #endregion

    #region EnqueueAll

    /// <summary>
    /// Enqueues the list of items
    /// </summary>
    /// <param name="ItemsToQueue">list of items to enqueue</param>
    public void EnqueueAll(IEnumerable<T> ItemsToQueue)
    {
        LockQ.EnterWriteLock();
        try
        {
            // loop through and add each item
            foreach (T item in ItemsToQueue)
            {
                m_Queue.Enqueue(item);
            }
        }
		

        finally
        {
            LockQ.ExitWriteLock();
        }
    }

    /// <summary>
    /// Enqueues the list of items
    /// </summary>
    /// <param name="ItemsToQueue">list of items to enqueue</param>
    public void EnqueueAll(TList<T> ItemsToQueue)
    {
        LockQ.EnterWriteLock();
        try
        {
            // loop through and add each item
            foreach (T item in ItemsToQueue)
            {
                m_Queue.Enqueue(item);
            }
        }

        finally
        {
            LockQ.ExitWriteLock();
        }
    }

    // EnqueueAll

    #endregion

    #region DequeueAll

    /// <summary>
    /// Dequeues all the items and returns them as a thread safe list
    /// </summary>
    public TList<T> DequeueAll()
    {
        LockQ.EnterWriteLock();
        try
        {
            // create return object
            TList<T> returnList = new TList<T>();

            // dequeue until everything is out
            while (m_Queue.Count > 0)
            {
                returnList.Add(m_Queue.Dequeue());
            }

            // return the list
            return returnList;
        }

        finally
        {
            LockQ.ExitWriteLock();
        }
    }

    // DequeueAll

    #endregion

    #region Clear

    /// <summary>
    /// Removes all items from the queue
    /// </summary>
    public void Clear()
    {
        LockQ.EnterWriteLock();
        try
        {
            m_Queue.Clear();
        }

        finally
        {
            LockQ.ExitWriteLock();
        }
    }

    // Clear

    #endregion

    #region Contains

    /// <summary>
    /// Determines whether the item exists in the Queue
    /// </summary>
    /// <param name="item">the item to find in the queue</param>
    public bool Contains(T item)
    {
		Console.print ("EnterReadLock TQueue 474");
        LockQ.EnterReadLock();
        try
        {
            return m_Queue.Contains(item);
        }

        finally
        {
            LockQ.ExitReadLock();
        }
    }

    // Contains

    #endregion

    #region Peek

    /// <summary>
    /// Returns the item at the start of the Queue without removing it
    /// </summary>
    public T Peek()
    {
		Console.print ("EnterReadLock TQueue 498");
        LockQ.EnterReadLock();
        try
        {
            return m_Queue.Peek();
        }

        finally
        {
            LockQ.ExitReadLock();
        }
    }

    // Peek

    #endregion

    #region ToArray

    /// <summary>
    /// Copies the Queue to a new array
    /// </summary>
    public T[] ToArray()
    {
		Console.print ("EnterReadLock TQueue 522");
        LockQ.EnterReadLock();
        try
        {
            return m_Queue.ToArray();
        }

        finally
        {
            LockQ.ExitReadLock();
        }
    }

    // ToArray

    #endregion

    #region TrimExcess

    /// <summary>
    /// Sets the capacity of the Queue to the current number of items, if that number
    /// is less than 90 percent of the current capacity
    /// </summary>
    public void TrimExcess()
    {
        LockQ.EnterWriteLock();
        try
        {
            m_Queue.TrimExcess();
        }

        finally
        {
            LockQ.ExitWriteLock();
        }
    }

    // TrimExcess

    #endregion
}