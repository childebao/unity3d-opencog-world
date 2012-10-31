using UnityEngine;
using System.Collections;

namespace Embodiment
{

	public class RawMessage : Message
	{
		private string rawText;
		
		public RawMessage(string from, string to) : base(from, to, Message.MessageType.RAW)
		{

		}
		
		public RawMessage(string from, string to, string message) : base(from, to, Message.MessageType.RAW)
		{
			this.rawText = message;
		}
		
		public string RawText
		{
			get{ return this.rawText; }
			set{ this.rawText = value; }
		}
		
		public override string getPlainTextRepresentation()
		{
			 return this.rawText;
		}
		
		public override void loadPlainTextRepresentation(string message)
		{
			this.rawText = message;
		}
	}

}