namespace Embodiment
{
	public class TickMessage : Message
	{
		public TickMessage(string from, string to) : base(from, to, Message.MessageType.TICK)
		{
		}

		public override string getPlainTextRepresentation()
		{
			return "TICK_MESSAGE";
		}

		public override void loadPlainTextRepresentation(string message)
		{
		}
	}
}


