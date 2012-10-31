using UnityEngine;
using System.Collections;

namespace Embodiment
{
	/// <summary>
	/// A prototye of message, 
	/// subclasses that extend from it can obtain instances via factory method.
	/// </summary>
	public abstract class Message
	{		
		/// <summary>
		/// ID of source NetworkElement
		/// </summary>
		protected string from;
		
		/// <summary>
		/// ID of target NetworkElement
		/// </summary>
		protected string to;
		
		/// <summary>
		/// Message type
		/// </summary>
		protected MessageType type;
		
		/// <summary>
		/// Message types definition.
		/// </summary>
		/// 
		public enum MessageType : int {
			NONE = 0,
			STRING = 1,
			LEARN = 2,
			REWARD = 3,
			SCHEMA = 4,
			LS_CMD = 5,
			ROUTER = 6,
			CANDIDATE_SCHEMA = 7,
			TICK = 8,
			FEEDBACK = 9,
			TRY = 10,
			STOP_LEARNING = 11,
			/// <summary>
			/// A custom message type for test purpose, to be removed.
			/// </summary>
			RAW = 12
		}
		
		public Message (string from, string to, MessageType type)
		{
			this.from = from;
			this.to = to;
			this.type = type;
		}
		
		public string From
		{
			get{ return this.from; }
			set{ this.from = value; }
		}
		
		public string To
		{
			get{ return this.to; }
			set{ this.to = value; }
		}
		
		public MessageType Type
		{
			get{ return this.type; }
			set{ this.type = value; }
		}
		
		public static Message factory(string from, string to, MessageType type, string message)
		{
			switch(type)
			{
			case MessageType.STRING:
				return new StringMessage(from, to, message);
				
			case MessageType.FEEDBACK:
				return new FeedbackMessage(from, to, message);
				
			case MessageType.RAW:
				return new RawMessage(from, to, message);
				
			default:
				return null;
			}
		}
	
		public abstract string getPlainTextRepresentation();
		
		public abstract void loadPlainTextRepresentation(string strMessage);
	}
	
}