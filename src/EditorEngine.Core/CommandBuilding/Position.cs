using System;

namespace EditorEngine.Core.CommandBuilding
{
	public class Position
	{
		public int Line { get; set; }
		public int Column { get; set; }

		public Position(int line, int column)
		{
			Line = line;
			Column = column;
		}

		public static Position Parse(string argument)
		{
			var chunks = argument.Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
			if (chunks.Length != 2)
				return null;
			int line;
			if (!int.TryParse(chunks[0], out line))
				return null;
			int column;
			if (!int.TryParse(chunks[1], out column))
				return null;
			return new Position(line, column);
		}
	}
}
