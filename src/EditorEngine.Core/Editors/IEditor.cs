using System;
using System.Collections.Generic;
using System.Diagnostics;
using EditorEngine.Core.Endpoints;
using EditorEngine.Core.Messaging.Messages;
namespace EditorEngine.Core.Editors
{
	public interface IEditor
	{
		// Injection method for the tcp dispatcher to be injected
		ICommandEndpoint Publisher { set; }
		
		// Is the editor still running?
		bool IsAlive { get; }
		
		// Launch the editor and if not null go to location
		void Initialize(Location location, string[] args);
		
		// Set the editor window as active window
		void SetFocus();
		
		// Have the editor go to / open a file at the specified location
		void GoTo(Location location);

		// A batch of commands is about to be sent to the editor
		void BeginBatchUpdate();
		
		// Batch of commands completed
		void EndBatchUpdate();

		// Does this editor support text injection
		bool CanInsertFor(string file);
		
		// Insert text into a file
		void Insert(EditorInsertMessage msg);

		// Does this editor support removing text
		bool CanRemoveFor(string file);
		
		// Remove text from file
		void Remove(EditorRemoveMessage msg);

		// Return list of open unsaved files
		KeyValuePair<string,string>[] GetDirtyFiles(string file);
	}
}

