using System;

namespace EditorEngine.Core.Messaging.Messages
{
	public class EditorLoadMessage : Message
	{
		public string Editor { get; private set; }
		
		public EditorLoadMessage(string editor)
		{
			Editor = editor;
		}
	}
}

