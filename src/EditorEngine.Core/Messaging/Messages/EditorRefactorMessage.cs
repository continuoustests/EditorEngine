using System;

namespace EditorEngine.Core.Messaging.Messages
{
	public class EditorRefactorMessage : Message
	{
		public string[] Lines { get; private set; }

		public EditorRefactorMessage(string[] lines)
		{
			Lines = lines;
		}
	}
}
