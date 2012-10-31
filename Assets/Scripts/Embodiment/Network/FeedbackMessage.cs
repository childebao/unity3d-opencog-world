using UnityEngine;
using System.Collections;

namespace Embodiment
{

	public class FeedbackMessage : Message
	{
		private string messageContent;
		
		public FeedbackMessage(string from, string to) : base(from, to, Message.MessageType.FEEDBACK)
		{

		}
		
		public FeedbackMessage(string from, string to, string message) : base(from, to, Message.MessageType.FEEDBACK)
		{
			this.messageContent = message;
		}
		
		public string MessageContent
		{
			get{ return this.messageContent; }
			set{ this.messageContent = value; }
		}
		
		public override string getPlainTextRepresentation()
		{
			 return this.messageContent;
		}
		
		public override void loadPlainTextRepresentation(string message)
		{
			this.messageContent = message;
		}
	}

}

