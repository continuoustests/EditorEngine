using System;

namespace EditorEngine.Core.Messaging.Messages
{
	public class EditorCommandMessage : Message
	{
		public string[] Arguments { get; private set; }

		public EditorCommandMessage(string[] args) {
			Arguments = args;
		}
	}
}