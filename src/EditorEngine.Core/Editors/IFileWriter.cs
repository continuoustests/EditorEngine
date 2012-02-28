using System;
using System.IO;
using System.Text;
using EditorEngine.Core.Messaging.Messages;

namespace EditorEngine.Core.Editors
{
	public interface IFileWriter
	{
		void Insert(EditorInsertMessage message);
		void Remove(EditorRemoveMessage msg);
	}

	public class FileWriter : IFileWriter
	{
		public void Insert(EditorInsertMessage message)
		{
			var tempFile = Path.GetTempFileName();
			using (var reader = new StreamReader(message.Destination.File))
			{
				using (var writer = new StreamWriter(tempFile))
				{
					long lineNr = 0;
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						lineNr++;
						if (lineNr != 1)
							writer.Write(Environment.NewLine);
						if (lineNr == message.Destination.Line)
						{
							var prefix = "";
							for (int i = 0; i < message.Destination.Column; i++)
								prefix += line[i];
							writer.Write(prefix);
							writer.Write(message.Text);
							writer.Write(line.Substring(prefix.Length, line.Length - prefix.Length));
						}
						else
							writer.Write(line);
					}
				}
			}
            File.Delete(message.Destination.File);
            File.Move(tempFile, message.Destination.File);
		}

		public void Remove(EditorRemoveMessage message)
		{
			var tempFile = Path.GetTempFileName();
			using (var reader = new StreamReader(message.Start.File))
			{
				using (var writer = new StreamWriter(tempFile))
				{
					long lineNr = 0;
					string line;
					var singleLineDelete = message.Start.Line == message.End.Line;
					while ((line = reader.ReadLine()) != null)
					{
						lineNr++;
						if (lineNr != 1 &&
							(singleLineDelete ||
							 lineNr < message.Start.Line ||
							 lineNr > message.End.Line))
							writer.Write(Environment.NewLine);
						if (lineNr >= message.Start.Line &&
							lineNr <= message.End.Line)
						{
							int column = 0;
							foreach (char chr in line)
							{
								column++;
								var delete = true;
								if (lineNr == message.Start.Line && lineNr == message.End.Line)
									delete = column > message.Start.Column && column <= message.End.Column;
								else if (lineNr == message.Start.Line)
									delete = column > message.Start.Column;
								else if (lineNr == message.End.Line)
									delete = column <= message.End.Column;
								if (!delete)
									writer.Write(chr);
							}
						}
						else
							writer.Write(line);
					}
				}
			}
            File.Delete(message.Start.File);
            File.Move(tempFile, message.Start.File);
		}
	}
}
