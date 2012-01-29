using System;
using System.IO;
using EditorEngine.Core.CommandBuilding;

namespace EditorEngine.Core.Arguments
{
	public class GoTo
	{
		public string File { get; set; }
		public int Line { get; set; }
		public int Column { get; set; }

		public void Add(Position position)
		{
			Line += position.Line;
			if (position.Line > 0)
				Column = position.Column;
			else
				Column += position.Column;
		}
	}

	public class PositionArgumentParser
	{
		public GoTo Parse(string argument)
		{
			var chunks = argument.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
			if (chunks.Length == 0)
				return null;

			var goTo = new GoTo();
			if (!File.Exists(chunks[0]))
				return null;
			goTo.File = chunks[0];
			if (chunks.Length == 1)
				return goTo;

			int line;
			if (!int.TryParse(chunks[1], out line))
				return goTo;
			goTo.Line = line;
			if (chunks.Length == 2)
				return goTo;
			
			int column;
			if (!int.TryParse(chunks[2], out column))
				return goTo;
			goTo.Column = column;
			return goTo;
		}
	}
}
