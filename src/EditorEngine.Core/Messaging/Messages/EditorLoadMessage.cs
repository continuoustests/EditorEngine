using System;

namespace EditorEngine.Core.Messaging.Messages
{
	public class EditorLoadMessage : Message
	{
		public CommandMessage Message { get; private set; }
		public string Editor { get; private set; }
		
		public EditorLoadMessage(CommandMessage message, string editor)
		{
			Message = message;
			Editor = editor;
		}
	}
	
	public class EditorLoadedMessage : Message
	{
		public CommandMessage Message { get; private set; }
		public string Editor { get; private set; }
		
		public EditorLoadedMessage(CommandMessage message, string editor)
		{
			Message = message;
			Editor = editor;
		}
	}
}

