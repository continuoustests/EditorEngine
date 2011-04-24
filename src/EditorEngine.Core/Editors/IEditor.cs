using System;
using System.Diagnostics;
using EditorEngine.Core.Endpoints;
namespace EditorEngine.Core.Editors
{
	public interface IEditor
	{
		ICommandEndpoint Publisher { set; }
		bool IsAlive { get; }
		
		void Initialize(Location location);
		
		void SetFocus();
		void GoTo(Location location);
	}
}

