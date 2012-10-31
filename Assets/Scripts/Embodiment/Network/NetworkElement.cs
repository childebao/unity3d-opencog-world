using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Embodiment;

public class NetworkElement : MonoBehaviour
{
	public readonly int CONNECTION_TIMEOUT = 10;
    protected static readonly string WHITESPACE = " ";
    protected static readonly string NEWLINE = "\n";
    public static readonly string FAILED_MESSAGE = "FAILED";
    public static readonly string OK_MESSAGE = "OK";
	
	// Logger
	Logger log = Logger.getInstance();
	
	// System configs
	protected Config config = Config.getInstance();
	
	// Settings of this network element instance.
	private string myID;
	private IPAddress myIP;
	private int myPort;
	
	// Settings of router.
	private string routerID;
	private IPAddress routerIP;
	private int routerPort;
	
	// Used to show router ip in Unity3D editor so that it can be edited 
	// conveniently.
	public string routerIpString;
	
	// Server listener to make this network element acting as a server.
	private ServerListener listener;
	
	// Unread messages
	private System.Object unreadMessagesLock = new System.Object();
	private int unreadMessagesNum;
	
	// Queue used to store received messages from router. Uses a concurrent
	// implementation of the queue interface.
	private Queue<Message> messageQueue = new Queue<Message>();
	
	// A hashset to record the unavailable end points.
	private HashSet<string> unavailableElements = new HashSet<string>();
	
	// Client socket to talk to the router.
	private Socket clientSocket;

    // Flag to check if the connection between this network element and router
    // has been established.
    protected bool established = false;
	
	protected IEnumerator connect()
	{
		Socket asyncSocket = new Socket(AddressFamily.InterNetwork,
		                                SocketType.Stream,
		                                ProtocolType.Tcp);
		IPEndPoint ipe = new IPEndPoint(routerIP, routerPort);
		
		log.Debugging("Start connecting to router.");
		
        // Start the async connection request.
		IAsyncResult ar = 
			asyncSocket.BeginConnect(ipe, new AsyncCallback(_connectCallback),
			                         asyncSocket);
		
		yield return new WaitForSeconds(0.1f);
		
		int retryTimes = CONNECTION_TIMEOUT;
		while (!ar.IsCompleted)
		{
			retryTimes--;
			if (retryTimes == 0)
			{
				log.Warn("Connection timed out.");
				yield break;
			}
			yield return new WaitForSeconds(0.1f);
		}
	}
	
	/// <summary>
	/// Async callback function to be invoked once the connection is established. 
	/// </summary>
	/// <param name="ar">
	/// Async result <see cref="IAsyncResult"/>
	/// </param>
	private void _connectCallback(IAsyncResult ar)
	{
		try 
		{
			// Retrieve the socket from the state object.
			this.clientSocket = (Socket) ar.AsyncState;
			// Complete the connection.
			this.clientSocket.EndConnect(ar);

            established = true;

			log.Debugging("Socket connected to router.");
			
			_loginRouter();
		}
		catch (Exception e)
		{
			log.Warn(e.ToString());
		}
	}
	
	private void _loginRouter()
	{
		string command = "LOGIN " + this.myID + WHITESPACE + 
						this.myIP.ToString() + WHITESPACE + this.myPort + 
                        NEWLINE;
		_send(command);
	
	}
	
    /// <summary>
    /// Disconnect the network element from the router.
    /// </summary>
	private void disconnect()
	{
		_logoutRouter();
		if (clientSocket != null)
		{
			clientSocket.Shutdown(SocketShutdown.Both);
			clientSocket.Close();
			clientSocket = null;
		}
	}
	
    /// <summary>
    /// Logout this network element from router by sending a "logout" command.
    /// </summary>
	private void _logoutRouter()
	{
		string command = "LOGOUT " + this.myID + NEWLINE;
		_send(command);
	}
	
    /// <summary>
    /// Send the raw text data to router by socket.
    /// </summary>
    /// <param name="text">raw text to be sent</param>
    /// <returns>Send result</returns>
	private bool _send(string text)
	{
        if (clientSocket == null) return false;

		lock (clientSocket)
		{
            if (!clientSocket.Connected)
            {
                established = false;
                clientSocket = null;
                return false;
            }
			
			try 
			{
				Stream s = new NetworkStream(clientSocket);
				StreamReader sr = new StreamReader(s);
				StreamWriter sw = new StreamWriter(s);

				sw.Write(text);
				sw.Flush();
				
				//byte[] byteArr = Encoding.UTF8.GetBytes(message);
				//this.socket.Send(byteArr);
				sr.Close();
				sw.Close();
				s.Close();
			}
			catch (Exception e)
			{
				log.Error(e.ToString());
				return false;
			}
		}
		return true;
	}
	
	protected bool sendMessage(Message message)
	{
		string payload = message.getPlainTextRepresentation();
		
		if (payload.Length == 0)
		{
			log.Error("Invalid empty command given.");
			return false;
		}
		
		string[] lineArr = payload.Split('\n');
		int numberOfLines = lineArr.Length;
		
		StringBuilder command = new StringBuilder("NEW_MESSAGE ");
        command.Append(message.From + WHITESPACE);
		command.Append(message.To + WHITESPACE);
        command.Append((int)message.Type + WHITESPACE);
		command.Append(numberOfLines + NEWLINE);

		command.Append(payload + NEWLINE);
		
		bool result = _send(command.ToString());
		
		if(result)
		{
			log.Fine("Successful.");
		}
		else
		{
			log.Error("Failed to send messsage.");
			return false;
		}
		
		return true;
	}
	
    /// <summary>
    /// Check if there are unread messages not yet pull from router.
    /// </summary>
    /// <returns>
    /// Unread message status.
    /// </returns>
	protected bool haveUnreadMessages()
	{
		return (unreadMessagesNum > 0);
	}
	
	protected bool isElementAvailable(string id)
	{
		return !unavailableElements.Contains(id);
	}
	
	public void markAsUnavailable(string id)
	{
		if (isElementAvailable(id))
		{
			unavailableElements.Add(id); 	
		}
		
		if (routerID.Equals(id))
		{
			// Oops, router is unavailable!
			// Reset the unread message number.
			lock (unreadMessagesLock)
			{
				unreadMessagesNum = 0;
			}
		}
	}
	
    /// <summary>
    /// Mark a network element as available.
    /// </summary>
    /// <param name="id">Network element id</param>
	public void markAsAvailable(string id)
	{
		if (!isElementAvailable(id))
		{
			unavailableElements.Remove(id);
		}
	}
	
    /// <summary>
    /// Notify a number of new messages from router.
    /// </summary>
    /// <param name="newMessagesNum">number of new arriving messages</param>
	public void notifyNewMessages(int newMessagesNum)
	{
		log.Debugging("Notified about new messages in Router.");
		lock(this.unreadMessagesLock)
		{
			unreadMessagesNum += newMessagesNum;
			log.Debugging("Unread messages [" + this.unreadMessagesNum + "]");
		}
	}
	
    /// <summary>
    /// Pull unread messages from router. 
    /// </summary>
    /// <param name="messages">A list of unread messages</param>
	public void pullMessage(List<Message> messages)
	{
		lock(this.messageQueue)
		{
			foreach(Message msg in messages)
			{
				this.messageQueue.Enqueue(msg);
			}
		}
		lock(this.unreadMessagesLock)
		{
			unreadMessagesNum -= messages.Count;
		}
	}
	
    /// <summary>
    /// Pull a message from router.
    /// </summary>
    /// <param name="message">An unread message</param>
	public void pullMessage(Message message)
	{
		lock(this.messageQueue)
		{
			this.messageQueue.Enqueue(message);	
		}
		
		lock(this.unreadMessagesLock)
		{
			this.unreadMessagesNum--;
		}
	}
	
    /// <summary>
    /// Abstract method to be implemented by subclasses.
    /// </summary>
    /// <param name="message">Message to be processed</param>
    /// <returns>True if the message is an "exit" command.</returns>
	public virtual bool processNextMessage(Message message)
	{
		return false;
	}
	
	/// <summary>
	/// Request unread messages from router.
	/// Should be invoked in some Update() function to make it check messages
	/// in certain interval.
	/// </summary>
	protected IEnumerator requestMessage(int limit)
	{
		while (true)
		{
			if (haveUnreadMessages())
			{	
				string command = "REQUEST_UNREAD_MESSAGES " + myID + 
                                 WHITESPACE + limit + NEWLINE;
				_send(command);
			}
			yield return new WaitForSeconds(0.1f);
		}
	}
	
	/// <summary>
	/// Convenience method that makes NetworkElements act as an usual server.
	/// This method will be called once per frame by MonoBehavior instance.
	/// </summary>
	protected void pulse()
	{
		if(messageQueue.Count > 0)
		{
			//long startTime = DateTime.Now.Ticks;
			Queue<Message> messagesToProcess;
			lock(messageQueue)
			{
				messagesToProcess = new Queue<Message>(this.messageQueue);
				messageQueue.Clear();
			}
			foreach(Message msg in messagesToProcess)
			{
				if(msg == null) log.Error("Null message to process.");
				log.Fine("Handle message from [" + msg.From +
				          "]. Content: " + msg.getPlainTextRepresentation());
				bool mustExit = processNextMessage(msg);
				if(mustExit) break;
			}
		}
	}
	
	protected void initialize(string id)
	{
		myID = id;
		myPort = PortManager.allocatePort();
        myIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
		routerIP = IPAddress.Parse(this.routerIpString);
		routerPort = config.getInt("ROUTER_PORT", 16312);
		
		listener = new ServerListener(this);
		
		StartCoroutine(connect());
		StartCoroutine(listener.work());
		StartCoroutine(requestMessage(1));
	}
	
	protected void finalize()
	{
		StopCoroutine("listener.work");
		StopCoroutine("requestMessage");
		listener.stop();
		disconnect();
		PortManager.releasePort(myPort);
	}

    // Getters and setters	
    public int PortNumber
    {
        get { return this.myPort; }
        set { this.myPort = value; }
    }

    public string IpAddress
    {
        get { return this.myIP.ToString(); }
        set { this.myIP = IPAddress.Parse(value); }
    }
}

