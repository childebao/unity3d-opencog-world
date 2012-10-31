using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Port manager can manage the allocation and recycling of the port numbers 
/// that will be binded to a listener socket. Each time it will allocate a 
/// usable port number to a port request, and release the port when the socket
/// is destroyed.
/// </summary>
public class PortManager
{
	private static readonly int MIN_PORT_NUMBER = 12315;
	
	private static HashSet<int> usedPorts = new HashSet<int>();
	
	public static int allocatePort()
	{
		int port = MIN_PORT_NUMBER;
		while (usedPorts.Contains(port) && port < 65535)
		{
			port++;
		}
		
		if (port >= 65535) // No ports are available
		{
			return -1;
		}
		
		usedPorts.Add(port);
		return port;
	}
	
	public static void releasePort(int port)
	{
		if (usedPorts.Contains(port))
		{
			usedPorts.Remove(port);
		}
	}
}

