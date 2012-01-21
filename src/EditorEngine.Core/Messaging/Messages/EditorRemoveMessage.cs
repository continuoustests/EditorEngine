using System;
using System.IO;
using EditorEngine.Core.Arguments;

namespace EditorEngine.Core.Messaging.Messages
{
	public class EditorRemoveMessage : Message
	{
		public GoTo Start { get; private set; }
		public GoTo End { get; private set; }

		public EditorRemoveMessage(GoTo start, GoTo end)
		{
			Start = start;
			End = end;
		}

		public static EditorRemoveMessage Parse(string[] argument)
		{
			if (argument.Length != 2)
				return null;
			
			var goTo = new PositionArgumentParser().Parse(argument[0]);
			if (goTo == null)
				return null;
			if (!File.Exists(goTo.File))
				return null;
			
			int line, column;
			var chunks = argument[1].Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
			if (chunks.Length != 2)
				return null;
			if (!int.TryParse(chunks[0], out line))
				return null;
			if (!int.TryParse(chunks[1], out column))
				return null;
			return new EditorRemoveMessage(
					goTo,
					new GoTo()
						{
							Line = line,
							Column = column
						});
		}
	}
}
