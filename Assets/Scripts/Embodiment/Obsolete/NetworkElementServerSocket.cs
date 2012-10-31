using UnityEngine;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Embodiment
{

	public class NetworkElementServerSocket
	{
		private bool stopListenerFlag;
		
		private TcpListener serverSocket;
	
		private OldNetworkElement ne;
		
		public NetworkElementServerSocket (OldNetworkElement ne)
		{
			this.ne = ne;
			this.stopListenerFlag = false;
		}
		
		public void run()
		{
			portListener();
		}
		
		public void stopListener()
		{
			this.stopListenerFlag = true;
			try
			{
				this.serverSocket.Stop();
				this.ne.getLogger().Info("stopListener: Closing server socket.");
			}
			catch ( SocketException se )
			{
				this.ne.getLogger().Error("stopListener: Exception caught [" + se.Message + "]");
			}
			catch ( Exception e )
			{
				this.ne.getLogger().Error("stopListener: An error occured. [" +
				               e.Message + "].");	
			}
		}
		
		
		/// <summary>
		/// Start a server socket to monitor the request from other peers. 
		/// </summary>
		private void portListener()
		{
			this.serverSocket = null;
			try
			{
				this.serverSocket = new TcpListener(IPAddress.Parse(this.ne.IpAddress), this.ne.PortNumber);

				this.serverSocket.Start();
			} catch ( Exception e )
			{
				this.ne.getLogger().Error("portListener: Cannot bind to port [" + this.ne.PortNumber +
				               "]. Error: " + e.Message);
				return;
			}
			
			try
			{
				while( !this.stopListenerFlag )
				{
					// Adding the condition so that there would not be 
					// WSACancelBlockingCall error when stopping the server socket.
					if( !this.serverSocket.Pending() )
					{
						Thread.Sleep(50);
						continue;
					}
					else
					{
						Socket socket = this.serverSocket.AcceptSocket();
						
						new NetworkElementConnectionHandler(this.ne, socket).start();
					}
				}
			}
			catch( SocketException se)
			{
				Debug.LogError(se.Message);	
			}
			finally
			{
				this.stopListener();
			}
		}
	}
	
}