using System;

namespace EditorEngine.Core.Messaging.Messages
{
	public class EditorGetDirtyFilesMessage : Message
	{
		public CommandMessage Message { get; private set; }

		public EditorGetDirtyFilesMessage(CommandMessage message)
		{
			Message = message;
		}
	}
}
