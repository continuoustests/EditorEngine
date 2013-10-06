using System;
using System.IO;
using EditorEngine.Core.Arguments;

namespace EditorEngine.Core.Messaging.Messages
{
	public class EditorReplaceMessage : Message
	{
		public string Text { get; private set; }
		public GoTo Start { get; private set; }
		public GoTo End { get; private set; }
		
		public EditorReplaceMessage(string content, GoTo start, GoTo end)
		{
			Text = content;
			Start = start;
			End = end;
		}

		public static EditorReplaceMessage Parse(string[] argument)
		{
			if (argument.Length != 3)
				return null;

			var content = argument[0];
			if (File.Exists(argument[0])) {
				content = File.ReadAllText(argument[0]);
				if (content.EndsWith(Environment.NewLine))
					content = content.Substring(0, content.Length - Environment.NewLine.Length);
			}
			
			var goTo = new PositionArgumentParser().Parse(argument[1]);
			if (goTo == null)
				return null;
			if (!File.Exists(goTo.File))
				return null;
			
			int line, column;
			var chunks = argument[2].Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
			if (chunks.Length != 2)
				return null;
			if (!int.TryParse(chunks[0], out line))
				return null;
			if (!int.TryParse(chunks[1], out column))
				return null;
			return new EditorReplaceMessage(
					content,
					goTo,
					new GoTo()
						{
							File = goTo.File,
							Line = line,
							Column = column
						});
		}
	}
}
