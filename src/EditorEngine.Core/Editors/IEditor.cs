using System;
using System.Collections.Generic;
using System.Diagnostics;
using EditorEngine.Core.Endpoints;
using EditorEngine.Core.Messaging.Messages;
namespace EditorEngine.Core.Editors
{
	public interface IEditor
	{
		ICommandEndpoint Publisher { set; }
		bool IsAlive { get; }
		
		void Initialize(Location location);
		
		void SetFocus();
		void GoTo(Location location);

		void BeginBatchUpdate();
		void EndBatchUpdate();

		bool CanInsertFor(string file);
		void Insert(EditorInsertMessage msg);

		bool CanRemoveFor(string file);
		void Remove(EditorRemoveMessage msg);

		KeyValuePair<string,string>[] GetDirtyFiles();
	}
}

