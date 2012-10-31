using UnityEngine;
using System.Collections;

namespace Embodiment
{

	public class StringMessage : Message
	{
		private string content;
		
		public StringMessage(string from, string to) : base(from, to, Message.MessageType.STRING)
		{

		}
		
		public StringMessage(string from, string to, string message) : base(from, to, Message.MessageType.STRING)
		{
			this.content = message;
		}
		
		public string MessageContent
		{
			get{ return this.content; }
			set{ this.content = value; }
		}
		
		public override string getPlainTextRepresentation()
		{
			 return this.content;
		}
		
		public override void loadPlainTextRepresentation(string message)
		{
			this.content = message;
		}
	}

}
