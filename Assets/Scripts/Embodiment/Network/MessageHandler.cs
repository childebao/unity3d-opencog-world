using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Embodiment
{
	public class MessageHandler
	{
        Logger log = Logger.getInstance();

		private NetworkElement ne;
			
		/// <summary>
		/// states 
		/// </summary>
		static readonly int DOING_NOTHING = 0;
		static readonly int READING_MESSAGES = 1;
		
		/// <summary>
		/// TCP socket where the connection is being handled. 
		/// </summary>
		private Socket socket;
		
		private Thread thread;
		
		/// <summary>
		/// Message handling fields 
		/// </summary>
		private Message.MessageType currentMessageType;
		private string currentMessageTo;
		private string currentMessageFrom;
		private StringBuilder currentMessage;
		private List<Message> messageBuffer;
		
		private bool useMessageBuffer = false;
		private int maxMessagesInBuffer = 100;
		
		private int lineCount;
		private int state;
		
		public MessageHandler (NetworkElement ne, Socket socket)
		{
			this.ne = ne;
			this.socket = socket;
			this.lineCount = 0;
			this.state = DOING_NOTHING;
			
			this.currentMessageTo = null;
			this.currentMessageFrom = null;
			this.currentMessage = new StringBuilder();
			this.messageBuffer = new List<Message>();
			
			this.thread = new Thread(this.run);
		}
		
		public void start()
		{
			this.thread.Start();	
		}
		
		public void run()
		{
            log.Info("MessageHandler: Start handling socket connection.");
			StreamReader reader = null;
			StreamWriter writer = null;
			
			try
			{
				Stream s = new NetworkStream(this.socket);
				reader = new StreamReader(s);
				writer = new StreamWriter(s);
			}
			catch( IOException ioe )
			{
				this.socket.Close();
                log.Error("MessageHandler: An I/O error occured. [" + 
					               ioe.Message + "].");	
			}
			
			bool endInput = false;
			
			while( !endInput )
			{
				try
				{
					// TODO Make some tests to judge the read time.
					string line = reader.ReadLine();
					
					if(line != null)
					{
						string answer = this.parse(line);
					}
					else
					{
						endInput = true;
					}	
				}
				catch( IOException ioe )
				{
                    log.Error("MessageHandler: An I/O error occured. [" + 
						               ioe.Message + "].");	
					endInput = true;
				}
			} // while
			
			try
			{
				reader.Close();
				writer.Close();
				this.socket.Close();
			}
			catch( IOException ioe ) 
			{
                log.Error("MessageHandler: An I/O error occured. [" + 
					               ioe.Message + "].");	
			}	
		}
		
		/// <summary>
		/// Parse a text line from message received. 
		/// </summary>
		/// <param name="inputLine">
		/// The raw data that received by server socket.
		/// </param>
		/// <returns>
		/// An 'OK' string if the line was successfully parsed,
		/// a 'FAILED' string if something went wrong,
		/// null if there is still more to parse.
		/// </returns>
		public string parse(string inputLine)
		{
			string answer = null;
			
			char selector = inputLine[0];
			string contents = inputLine.Substring(1);
			
			if(selector == 'c')
			{
				string[] tokenArr = contents.Split(' ');
				IEnumerator token = tokenArr.GetEnumerator();
				token.MoveNext();
				string command = token.Current.ToString();
				
				if(command.Equals("NOTIFY_NEW_MESSAGE"))
				{
					if(token.MoveNext()) // Has more elements
					{	
						// Get new message number.
						int numberOfMessages = int.Parse(token.Current.ToString());
	
						this.ne.notifyNewMessages(numberOfMessages);
						answer = NetworkElement.OK_MESSAGE;

                        log.Debugging("onLine: Notified about [" + 
						          numberOfMessages + "] messages in Router.");
					}
					else
					{
						answer = NetworkElement.FAILED_MESSAGE;	
					}
				}
				else if(command.Equals("UNAVAILABLE_ELEMENT"))
				{
					if(token.MoveNext()) // Has more elements
					{	
						// Get unavalable element id.
						string id = token.Current.ToString();

                        log.Debugging("onLine: Unavailable element message received for [" + 
						          id + "].");
						this.ne.markAsUnavailable(id);
						answer = NetworkElement.OK_MESSAGE;
					}
					else
					{
						answer = NetworkElement.FAILED_MESSAGE;	
					}
				}
				else if(command.Equals("AVAILABLE_ELEMENT"))
				{
					if(token.MoveNext()) // Has more elements
					{	
						string id = token.Current.ToString();

                        log.Debugging("onLine: Available element message received for [" + 
						          id + "].");
						this.ne.markAsAvailable(id);
						answer = NetworkElement.OK_MESSAGE;
					}
					else
					{
						answer = NetworkElement.FAILED_MESSAGE;	
					}
				}
				else if(command.Equals("START_MESSAGE")) // Parse a common message
				{
					if(this.state == READING_MESSAGES)
					{
						// A previous message was already read.
						log.Debugging("onLine: From [" + this.currentMessageFrom +
						          "] to [" + this.currentMessageTo +
						          "] Type [" + this.currentMessageType + "]");
					
						Message message = Message.factory(this.currentMessageFrom,
						                                  this.currentMessageTo,
						                                  this.currentMessageType,
						                                  this.currentMessage.ToString());
						if( message == null )
						{
							log.Error("Could not factory message from the following string: " +
							               this.currentMessage.ToString());	
						}
						if(this.useMessageBuffer)
						{
							this.messageBuffer.Add(message);
							if(messageBuffer.Count > this.maxMessagesInBuffer)
							{
								this.ne.pullMessage(this.messageBuffer);	
								this.messageBuffer.Clear();
							}
						}
						else
						{
							this.ne.pullMessage(message);	
						}
							
						this.lineCount = 0;
						this.currentMessageTo = "";
						this.currentMessageFrom = "";
						this.currentMessageType = Message.MessageType.NONE;
						this.currentMessage.Remove(0, this.currentMessage.Length);
					}
					else
					{
						if(this.state == DOING_NOTHING)
						{
							// Enter reading state from idle state.
							this.state = READING_MESSAGES;	
						}
						else
						{
							log.Error("onLine: Unexepcted command [" +
							               command + "]. Discarding line [" +
							               inputLine + "]");	
						}
					}
					
					if( token.MoveNext() )
					{
						this.currentMessageFrom = token.Current.ToString();
						
						if( token.MoveNext() )
						{
							this.currentMessageTo = token.Current.ToString();
							if( token.MoveNext() )
							{
								this.currentMessageType = (Message.MessageType) int.Parse(token.Current.ToString());
							}
							else
							{
								answer = NetworkElement.FAILED_MESSAGE;
							}
						}
						else
						{
							answer = NetworkElement.FAILED_MESSAGE;
						}	
					}
					else
					{
						answer = NetworkElement.FAILED_MESSAGE;
					}
					this.lineCount = 0;
				}
				else if(command.Equals("NO_MORE_MESSAGES"))
				{
					if(this.state == READING_MESSAGES)
					{
						log.Info("onLine: From [" + this.currentMessageFrom +
						          "] to [" + this.currentMessageTo +
						          "] Type [" + this.currentMessageType + "].");	
						
						Message message = Message.factory(this.currentMessageFrom,
						                                  this.currentMessageTo,
						                                  this.currentMessageType,
						                                  this.currentMessage.ToString());
						
						if(message == null)
						{
							log.Error("Could not factory message from the following string: [" +
							               this.currentMessage.ToString() + "]");
						}
						if(this.useMessageBuffer)
						{
							this.messageBuffer.Add(message);
							this.ne.pullMessage(messageBuffer);
							this.messageBuffer.Clear();
						}
						else
						{
							this.ne.pullMessage(message);	
						}
						
						// reset variables to default values
						this.lineCount = 0;
						this.currentMessageTo = "";
						this.currentMessageFrom = "";
						this.currentMessageType = Message.MessageType.NONE;
						this.currentMessage.Remove(0, this.currentMessage.Length);
						this.state = DOING_NOTHING; // quit reading state
						answer = NetworkElement.OK_MESSAGE;
					}
					else
					{
						log.Error("onLine: Unexpected command [" +
						               command + "]. Discarding line [" +
						               inputLine + "]");
						answer = NetworkElement.FAILED_MESSAGE;
					}
				}
				else
				{
					log.Error("onLine: Unexpected command [" +
					               command + "]. Discarding line [" +
					               inputLine + "]");
					answer = NetworkElement.FAILED_MESSAGE;
				} // end processing command.
			} // end processing selector 'c'
			else if(selector == 'd')
			{
				if(this.state == READING_MESSAGES)
				{
					if(this.lineCount > 0)
					{
						this.currentMessage.Append("\n");	
					}
					this.currentMessage.Append(contents);
					this.lineCount++;
					
				}
				else
				{
					log.Error("onLine: Unexpected dataline. Discarding line [" +
					               inputLine + "]");
					answer = NetworkElement.FAILED_MESSAGE;
				}
			} // end processing selector 'd'
			else
			{
				log.Error("onLine: Invalid selector [" + selector
				               + "]. Discarding line [" + inputLine + "].");
				answer = NetworkElement.FAILED_MESSAGE;
			} // end processing selector
			
			return answer;
		}
	}
}

