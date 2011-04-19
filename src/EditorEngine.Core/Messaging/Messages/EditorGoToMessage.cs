using System;
namespace EditorEngine.Core.Messaging.Messages
{
	public class EditorGoToMessage : Message
	{
		public string File { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
		
		public EditorGoToMessage(string file, int line, int column)
		{
			File = file;
			Line = line;
			Column = column;
		}
	}
}

