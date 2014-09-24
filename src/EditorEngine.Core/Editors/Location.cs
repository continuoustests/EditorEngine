using System;
using EditorEngine.Core.CommandBuilding;

namespace EditorEngine.Core.Editors
{
	public class Location
	{
		public string File { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
		
		public Location(string file, int line, int column)
		{
			File = file;
			Line = line;
			Column = column;
		}
		
		public void Add(Position position)
		{
			Line += position.Line;
			if (position.Line > 0)
				Column = position.Column;
			else
				Column += position.Column;
		}
	}
}

