using System;
using System.Text;
namespace EditorEngine.Core.Messaging.Messages
{
	class CommandMessage : Message
	{
		public string Command { get; set; }
		public string Arguments { get; set; }
		
		public CommandMessage(string message)
		{
			var chunks = message.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
			if (chunks.Length < 1)
				throw new Exception("Invalid command " + message);
			Command = chunks[0].Trim();
			Arguments = concatArgumentChunks(chunks);
		}
		
		private string concatArgumentChunks(string[] chunks)
		{
			var sb = new StringBuilder();
			for (int i = 1; i < chunks.Length; i++)
				sb.Append(chunks[i]);
			return sb.ToString().Trim();
		}
	}
}

