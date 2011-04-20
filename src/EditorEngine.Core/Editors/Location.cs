using System;
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
	}
}

