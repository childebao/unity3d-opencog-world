using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

using Embodiment;

public class ServerListener
{
    Logger log = Logger.getInstance();

	private bool stopFlag;
	
	private TcpListener listener;
	
	private NetworkElement ne;
	
	public ServerListener (NetworkElement ne)
	{
		this.ne = ne;
		stopFlag = false;
	}

	/// <summary>
    /// To fit the Unity3D architecture, we use coroutine to simulate a thread.
	/// </summary>
	/// <returns></returns>
	public IEnumerator work()
	{
		try
		{
			listener = new TcpListener(IPAddress.Parse(ne.IpAddress), ne.PortNumber);
			listener.Start();
		}
		catch (SocketException se)
		{
            log.Error(se.ToString());
			yield break;
		}
		
		while (!stopFlag)
		{
			if (!listener.Pending())
			{
				// If listener is pending, sleep for a while to relax the CPU.
				yield return new WaitForSeconds(0.05f);
			}
			else
			{
				try
				{
					Socket workSocket = listener.AcceptSocket();
					new MessageHandler(ne, workSocket).start();
				}
				catch (SocketException se)
				{
                    log.Error(se.ToString());
				}
			}
		}
		
	}
	
	public void stop()
	{
		stopFlag = true;
		try
		{
			listener.Stop();
			listener = null;
		}
		catch (SocketException se)
		{
            log.Error(se.ToString());
		}
	}
}

