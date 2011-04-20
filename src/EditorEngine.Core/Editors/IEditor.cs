using System;
using System.Diagnostics;
namespace EditorEngine.Core.Editors
{
	public interface IEditor
	{
		bool IsAlive { get; }
		
		void Initialize(Location location);
		
		void SetFocus();
		void GoTo(Location location);
	}
}

