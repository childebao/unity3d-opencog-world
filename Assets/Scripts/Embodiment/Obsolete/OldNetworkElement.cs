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

public class OldNetworkElement : MonoBehaviour
{
	// Instance that contains system settings
	private Config config = Config.getInstance();
	
	private Logger log = new Logger();
	
    private string id;
	private IPAddress ipAddress;
	private int portNumber;
	
	/// <summary>
	/// Router settings
	/// </summary>
	private string routerID;
	public string routerIpString;
	private IPAddress routerIP;
	private int routerPort;
	
	
	private Socket socket;
	
	/// <summary>
	/// Unread messages counter and lock.
	/// </summary>
	private System.Object unreadMessagesLock;
	private int numberOfUnreadMessages;
	
	/// <summary>
	/// Message management fields
	/// </summary>
	private int unreadMessageCheckInterval;
	private int unreadMessageRetrievalLimit;
	public bool noAckMessages;
	
	/// <summary>
	/// A hash set that contains all unavailable element ids in 
	/// a given time.
	/// </summary>
	private HashSet<string> unavailableElements = new HashSet<string>();
	
	/// <summary>
	/// Queue used to store received messages from router. Uses a concurrent
	/// implementation of the queue interface.
	/// </summary>
	private Queue<Message> messageQueue = new Queue<Message>();
	private System.Object messageQueueLock;
	
	private bool requestMessageFlag;
	private Thread requestMessageThread;
	
	/// <summary>
	/// Holds the server socket thread
	/// </summary>
	private NetworkElementServerSocket listenerServerSocket;
	private Thread listenerThread;
	
	private System.Object connectionLock;
	
	/// <summary>
	/// Start the listener thread that will receive messages.
	/// </summary>
	private void startListener()
	{
		this.listenerServerSocket = new NetworkElementServerSocket(this);
		listenerThread = new Thread(new ThreadStart(this.listenerServerSocket.run));
		listenerThread.Start();
	}
	
	/// <summary>
	/// Stop the listener thread when this NE is destroyed.
	/// </summary>
	public void stopListener()
	{
		this.listenerServerSocket.stopListener();
		listenerThread.Join(500);
	}
	
	/// <summary>
	/// Send a LOGIN command to router. The router response to such command 
	/// an 'OK' if the element was correctly inserted into the router table.
	/// </summary>
	/// <returns>
	/// The result of hand shaking.
	/// </returns>
	protected bool handshakeWithRouter()
	{
		string command = "LOGIN " + this.id + " " + this.ipAddress.ToString() + " " + this.portNumber + "\n";
		// "\n" is important here due to the different encoding mechanisms between Windows and Linux.
		log.Debugging("handshakeWithRouter: Handshaking...");
		
		bool result = sendCommandToRouter(command);
		
		if(result) {
			log.Debugging("handshakeWithRouter: Done");
		} else {
			log.Debugging("handshakeWithRouter: Failed");
		}
		
		return result;
	}
	
	/// <summary>
	/// Send a command to router. Command differs from messages since they are 
	/// not intended to be stored in router to be forwarded to other NetworkElements.
	/// </summary>
	/// <param name="command">
	/// The command to be sent.
	/// </param>
	/// <returns>
	/// True, if the command was correctly sent.
	/// False, otherwise.
	/// </returns>
	private bool sendCommandToRouter(string command)
	{
		bool result = false;
		
		string response = this.sendMessageToRouter(command);
		if(response.Equals(OK_MESSAGE))
		{
			result = true;	
		}
		else
		{
			log.Error("sendCommandToRouter: Answer = [" + response +
			               "], expected 'OK'.");
			this.socket = null;
			this.markAsUnavailable(this.routerID);
			result = false;
		}
		
		return result;
	}
	
	/// <summary>
	/// Send a REQUEST_UNREAD_MESSAGES command to router. 
	/// The router response to such command is up to <limit> messages to this NE.
	/// </summary>
	/// <param name="limit">
	/// Maximal number of messages to be retrieved. 
	/// The -1 value means all messages.
	/// </param>
	/// <returns>
	/// True, if sent request.
	/// False, means there is no unread messages.
	/// </returns>
	private bool requestUnreadMessages(int limit)
	{
		if(!haveUnreadMessage()) return false;
		
		string command = "REQUEST_UNREAD_MESSAGES " + this.id + " " + limit + "\n";
		this.sendCommandToRouter(command);
		return true;
	}
	
	/**
	 * Mark an element as unavailable to reach.
	 * 
	 * @param id the unavailable element id
	 */
	protected void markAsUnavailable(string id)
	{
		this.unavailableElements.Add(id);
		
		if(this.routerID.Equals(id))
		{
			lock(unreadMessagesLock)
			{
				this.numberOfUnreadMessages = 0;	
			}
		}
	}
	
	/**
	 * Remove an element from unavailable list.
	 * 
	 * @param id the available element id
	 */
	protected void markAsAvailable(string id)
	{
		if(!isElementAvailable(id))
		{
			this.unavailableElements.Remove(id);
			log.Info("markAsAvailableElement: removing [" + id +
			          "] from unavailable list.");
		}
	}
	
	protected void markAsInitialized()
	{
	}
	
	protected bool isInitialized()
	{
		return true;
	}
	
	/// <summary>
	/// To check if certain element is available now.
	/// </summary>
	/// <param name="id">
	/// The id of the element to be checked.
	/// </param>
	/// <returns>
	/// True if the element is available, otherwise false.
	/// </returns>
	public bool isElementAvailable(string id)
	{
		return !this.unavailableElements.Contains(id);
	}
	
	public static readonly string FAILED_MESSAGE = "FAILED";
	public static readonly string OK_MESSAGE = "OK";
	

	/// <summary>
	/// Get and set methods of some private variables.
	/// </summary>
	public int PortNumber
	{
		get{ return this.portNumber; }
		set{ this.portNumber = value; }
	}
	
	public string IpAddress
	{
		get{ return this.ipAddress.ToString(); }
		set{ this.ipAddress = IPAddress.Parse(value); }
	}
	
	public Logger getLogger()
	{
		return this.log;
	}
	
	public int getMessageQueueCount()
	{
		int count;
		lock(messageQueueLock)
		{
			count = this.messageQueue.Count;
		}
		return count;
	}
	
	public Queue<Message> getCurrentMessageQueue()
	{
		Queue<Message> messagesToProcess;
		lock(this.messageQueueLock)
		{
			messagesToProcess = new Queue<Message>(this.messageQueue);
			this.messageQueue.Clear();
		}
		return messagesToProcess;
	}
	
	/// <summary>
	/// Open a connection to Router.
	/// </summary>
	/// <returns>
	/// True if a connection was already established.
	/// False if errors occurred.
	/// </returns>
	public bool connectToRouter()
	{
		// Connection already exists.
		if(this.socket != null)	return true;
		
		this.socket = null;
		try
		{
			this.socket = new Socket(AddressFamily.InterNetwork,
			                         SocketType.Stream,
			                         ProtocolType.Tcp);
			IPEndPoint ipe = new IPEndPoint(this.routerIP, this.routerPort);
			
			// Establish a connection.
			this.socket.Connect(ipe);	
			//this.socket.BeginConnect(ipe, new AsyncCallback(), this.socket);
		}
		catch(Exception e)
		{
            if (this.socket != null) {
                this.socket.Close();
                this.socket = null;
            }
			
			log.Error("connectToRouter: Error constructing a socket. [" + e.Message + "]");	
			return false;
		}
		return true;
	}
	
	/// <summary>
	/// This is a virtual method to be overrided by subclass.
	/// It is to send a message to some peer that contains in message itself.
	/// </summary>
	/// <param name="message">
	/// A message to send.
	/// </param>
	/// <returns>
	/// True if message was sent successfully.
	/// False if errors occured.
	/// </returns>
	public virtual bool sendMessage(Message message)
	{
		string commandPayload = message.getPlainTextRepresentation();
		
		if( commandPayload.Length == 0 )
		{
			log.Error("sendMessage: Invalid empty command given.");
			return false;
		}
		
		string[] lineArr = commandPayload.Split('\n');
		int numberOfLines = lineArr.Length;
		
		StringBuilder command = new StringBuilder("NEW_MESSAGE ");
		command.Append(message.From);
		command.Append(" ");
		command.Append(message.To);
		command.Append(" ");
		command.Append((int) message.Type);
		command.Append(" ");
		command.Append(numberOfLines);
		command.Append("\n");
		command.Append(commandPayload);
		command.Append("\n");
		
		string response = this.sendMessageToRouter(command.ToString());
		
		if( response.Equals(OK_MESSAGE) )
		{
			log.Fine("sendMessage: Answer = 'OK'.");
		}
		else
		{
			log.Error("sendMessage: Answer = '" + response + "'.");
			this.socket = null;
			this.markAsUnavailable(this.routerID);
			return false;
		}
		
		return true;
	}
	
	public string sendMessageToRouter(string message)
	{
		lock(this.connectionLock)
		{
			bool connected = this.connectToRouter();
			
			if(connected) {
				try
				{
					Stream s = new NetworkStream(this.socket);
					StreamReader sr = new StreamReader(s);
					StreamWriter sw = new StreamWriter(s);
					
					sw.Write(message);
					sw.Flush();
					
					//byte[] byteArr = Encoding.UTF8.GetBytes(message);
					//this.socket.Send(byteArr);
					
					log.Fine("sendMessageToRouter: message sent");
					sr.Close();
					sw.Close();
					s.Close();
					
					if (this.noAckMessages) {
						return OK_MESSAGE;
					} else {
						return sr.ReadLine();
					}
				}
				catch( Exception e )
				{
					log.Error( "sendMessageToRouter: " + e.Message );
					return FAILED_MESSAGE;
				}
			}
		} // end lock
		return FAILED_MESSAGE;
	}
	
	/// <summary>
	/// Return true if there are new unread messages waiting on the router.
	/// It is a local chack, no communication is actually performed.
	/// This method just returns a local boolean state variable which is set
	/// assynchronously by the router when a new message to this NE arrives.
	/// </summary>
	/// <returns>
	/// True if there are new unread messages waiting on the router.
	/// False otherwise.
	/// </returns>
	public bool haveUnreadMessage()
	{
		return (this.numberOfUnreadMessages > 0);
	}
	
	/// <summary>
	/// Convenience method to be called when the NE is being destroyed 
	/// and should notify router to erase its ip/port information.
	/// </summary>
	public void logoutFromRouter()
	{
		string command = "LOGOUT " + this.id + "\n";
		this.sendMessageToRouter(command);
	}
	
	/**
	 * Notify about new messages in router.
	 * 
	 * @param messages message number
	 */
	public void newMessageInRouter(int messages)
	{
		log.Debugging("newMessageInRouter: Notified about new messages in Router.");
		lock(this.unreadMessagesLock)
		{
			this.numberOfUnreadMessages += messages;
			log.Debugging("newMessageInRouter: Unread messages [" + this.numberOfUnreadMessages + "]");
		}
	}
	
	/**
	 * Append new messages to current message list.
	 * 
	 * @param messages new message list
	 */
	public void newReceivedMessages(List<Message> messages)
	{
		lock(this.messageQueueLock)
		{
			foreach(Message msg in messages)
			{
				this.messageQueue.Enqueue(msg);
			}
		}
		lock(this.unreadMessagesLock)
		{
			this.numberOfUnreadMessages -= messages.Count;
		}
	}
	
	public void newReceivedMessage(Message message)
	{
		lock(this.messageQueueLock)
		{
			this.messageQueue.Enqueue(message);	
		}
		
		lock(this.unreadMessagesLock)
		{
			this.numberOfUnreadMessages--;
		}
	}
	
	/**
	 * Abstact interface for subclass to override.
	 */
	public virtual bool processNextMessage(Message message)
	{
		return false;
	}
	
	public void availableElement(string id)
	{
		bool needHandshake = (this.routerID.Equals(id) && !isElementAvailable(id));
		this.markAsAvailable(id);
		if(needHandshake)
		{
			this.handshakeWithRouter();
		}
	}
	
	public void unavailableElement(string id)
	{
		this.markAsUnavailable(id);
		
		if(this.routerID.Equals(id))
		{
			this.socket = null;
			this.handshakeWithRouter();
		}
	}
	
	public void requestMessagesDeamon()
	{
		do
		{
			this.requestUnreadMessages(this.unreadMessageRetrievalLimit);
			try
			{
				Thread.Sleep(this.unreadMessageCheckInterval);
			}
			catch (ThreadInterruptedException e)
			{
				log.Error( "requestMessagesDeamon: " + e.Message );	
			}
		}
		while(this.requestMessageFlag);
	}
	
	public void stopRequestMessages()
	{
		this.requestMessageFlag = false;
		this.requestMessageThread.Join(500);
	}
	
	/// <summary>
	/// Convenience method that makes NetworkElements act as an usual server.
	/// This method will be called once per frame by MonoBehavior instance.
	/// </summary>
	public void runOncePerFrame()
	{
		bool mustExit = false;
		
		if(this.messageQueue.Count > 0)
		{
			//long startTime = DateTime.Now.Ticks;
			Queue<Message> messagesToProcess;
			lock(this.messageQueueLock)
			{
				messagesToProcess = new Queue<Message>(this.messageQueue);
				this.messageQueue.Clear();
			}
			foreach(Message msg in messagesToProcess)
			{
				if(msg == null) log.Error("Null message to process.");
				log.Info("serverLoop: Handle message from [" + msg.From +
				          "]. Content: " + msg.getPlainTextRepresentation());
				mustExit = processNextMessage(msg);
				if(mustExit) break;
			}
		}
		if(mustExit) return;		
	}

	/**
	 * A loop make this NE try connecting to router until success.
	 */
    private void tryConnectingToRouterUntilSuccess()
    {
        while (true)
        {
            if (this.handshakeWithRouter())
            {
                this.markAsAvailable(this.routerID);
                break;
            }
            try
            {
                Thread.Sleep(200000);
            }
            catch (ThreadInterruptedException e)
            {
                log.Error("connectToRouter: Error putting NetworkElement main thread to sleep. " +
                               e.Message);
                continue;
            }
        }
    }

    /// <summary>
    /// Initialize the network element.
    /// </summary>
    /// <param name="inputId">
    /// id of this NE
    /// </param>
    /// <param name="inputPort">
    /// the port that this NE will be listening at.
    /// </param>
    protected void initialize(string inputId, int inputPort)
    {
        try
        {
            this.id = inputId;
            this.ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
            this.portNumber = inputPort;

            this.routerID = config.get("ROUTER_ID", "ROUTER");
			if(this.routerIpString == null || this.routerIpString == "") {
            	this.routerIpString = config.get("ROUTER_IP", "192.168.1.48"); // set it to your router ip!
			}
            this.routerIP = IPAddress.Parse(this.routerIpString);
            this.routerPort = config.getInt("ROUTER_PORT", 16312);

            this.numberOfUnreadMessages = 0;
            this.noAckMessages = true;

            this.unreadMessageCheckInterval = 100;
            this.unreadMessageRetrievalLimit = 1;

            log.Info("NetworkElement: Router address[" + this.routerIP.ToString() + "]");
        }
        catch (Exception e)
        {
            log.Error("NetworkElement::initialize: " +
                "Exception caught as [" + e.Message + "].");
        }

        // Create some locks for different purposes.
        this.connectionLock = new System.Object();
        this.unreadMessagesLock = new System.Object();
        this.messageQueueLock = new System.Object();

        this.startListener();
		
        if (this.noAckMessages)
        {
            this.requestMessageFlag = true;
            requestMessageThread = new Thread(new ThreadStart(requestMessagesDeamon));
            requestMessageThread.Start();
        }
    }

    /// <summary>
    /// This method should be called by instance when destructing.
    /// It will logout the instance from the router and stop message listening.
    /// </summary>
    protected void customFinalize()
    {
		log.Info("NetworkElement Component finalizes.");
		
		// Logout from the router.
		this.logoutFromRouter();
		try
		{
			if( this.socket != null && this.socket.Connected )
			{
				this.socket.Close();	
			}
		}
		catch( Exception e )
		{
			log.Error("Finalize: Could not close client socket. " +
			               e.Message);	
		}
		
		if( this.noAckMessages )
		{
			this.stopRequestMessages();
		}
		
		try
		{
			this.stopListener();
		}
		catch( Exception e )
		{
			log.Error("Finalize: Could not stop server socket listener. " +
			               e.Message);
		}
    }

}
