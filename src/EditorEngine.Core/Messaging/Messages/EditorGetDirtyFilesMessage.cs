using System;

namespace EditorEngine.Core.Messaging.Messages
{
	public class EditorGetDirtyFilesMessage : Message
	{
		public Guid ClientID { get; private set; }

		public EditorGetDirtyFilesMessage(Guid clientID)
		{
			ClientID = clientID;
		}
	}
}
