using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace EditorEngine.Core.Messaging.Messages
{
	public class EditorDirtyFilesListMessage : Message
	{
		public CommandMessage Message { get; private set; }
		public KeyValuePair<string,string>[] DirtyFiles { get; private set; }

		public EditorDirtyFilesListMessage(CommandMessage message, KeyValuePair<string,string>[] files)
		{
			Message = message;
			DirtyFiles = files;
		}

		public string GetCommand()
		{
			var sb = new StringBuilder();
			sb.Append(Message.CorrelationID);
			DirtyFiles.ToList()
				.ForEach(x =>
					{
						var tempFile = Path.GetTempFileName();
						File.WriteAllText(tempFile, x.Value);
						sb.AppendLine(
							string.Format("{0}|{1}", x.Key, tempFile));
					});
			return sb.ToString();
		}
	}
}
