using System;
namespace EditorEngine.Core.Editors
{
	public interface IEditor
	{
		void SetFocus();
		void GoTo(string file, int line, int column);
	}
}

