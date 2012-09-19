using System;
using System.Collections.Generic;
using EditorEngine.Core.Editors;
using EditorEngine.Core.Endpoints;
using EditorEngine.Core.Messaging.Messages;

namespace sublime
{
	public class SublimeEditor : IEditor
	{
		private Client _client = new Client("\n");
		public ICommandEndpoint Publisher { private get; set; }
		
		// Is the editor still running?
		public bool IsAlive { 
			get {
				return _client.Request("ping", "pong") == "pong";
			}
		}
		
		public void Initialize(Location location) {
			_client.Connect(9999, (m) => Console.WriteLine(m));
		}
		
		public void SetFocus() {
		}
		
		public void GoTo(Location location) {
		}

		public void BeginBatchUpdate() {
		}
		
		public void EndBatchUpdate() {
		}

		public bool CanInsertFor(string file) {
			return false;
		}
		
		public void Insert(EditorInsertMessage msg) {
		}

		public bool CanRemoveFor(string file) {
			return false;
		}
		
		public void Remove(EditorRemoveMessage msg) {
		}

		public KeyValuePair<string,string>[] GetDirtyFiles(string file) {
			throw new NotImplementedException();
		}
	}
}
