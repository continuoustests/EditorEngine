using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using EditorEngine.Core.Endpoints;
using EditorEngine.Core.CommandBuilding;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Editors;

namespace test
{
	public class TestEditor : IEditor
	{
		private bool _isAlive = false;

		// Injection method for the tcp dispatcher to be injected
		public ICommandEndpoint Publisher { get; set; }
		
		// Is the editor still running?
		public bool IsAlive { get { return _isAlive; } }
		
		// Launch the editor and if not null go to location
		public void Initialize(Location location, string[] args) {
			_isAlive = true;
			Publisher.Run("test-editor started");
		}
		
		// Set the editor window as active window
		public void SetFocus() {
		}
		
		// Have the editor go to / open a file at the specified location
		public void GoTo(Location location) {
			Publisher.Run(
				string.Format(
					"test-editor goto \"{0}|{1}|{2}\"",
					location.File,
					location.Line,
					location.Column));
		}

		// A batch of commands is about to be sent to the editor
		public void BeginBatchUpdate() {
			Publisher.Run("test-editor batch-update-started");
		}
		
		// Batch of commands completed
		public void EndBatchUpdate() {
			Publisher.Run("test-editor batch-update-ended");
		}

		// Does this editor support text injection
		public bool CanInsertFor(string file) {
			return false;
		}
		
		// Insert text into a file
		public void Insert(EditorInsertMessage msg) {
			Publisher.Run(
				string.Format(
					"test-editor insert {0} \"{1}|{2}|{3}\" \"{4}|{5}\"",
					toMD5(msg.Text),
					msg.Destination.File,
					msg.Destination.Line,
					msg.Destination.Column,
					msg.MoveOffset.Line,
					msg.MoveOffset.Column));
		}

		// Does this editor support removing text
		public bool CanRemoveFor(string file) {
			return false;
		}
		
		public void Remove(EditorRemoveMessage msg) {
		// Remove text from file
			Publisher.Run(
				string.Format(
					"test-editor remove \"{0}|{1}|{2}\" \"{3}|{4}\"",
					msg.Start.File,
					msg.Start.Line,
					msg.Start.Column,
					msg.End.Line,
					msg.End.Column));
		}

		// Return list of open unsaved files
		public KeyValuePair<string,string>[] GetDirtyFiles(string file) {
			return new KeyValuePair<string,string>[] {};
		}

        public Caret GetCaret() {
        	return new Caret("", new Position(0, 0), "");
        }

        public void RequestUserSelection(string identifier, string[] items, string defaultValue) {
        }

        public void RequestUserSelectionAtCaret(string identifier, string[] items)
        {
        }

        public void RequestUserInput(string identifier, string defaultValue) {
        }

		private string toMD5(string input)
		{
		    var md5 = System.Security.Cryptography.MD5.Create();
		    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
		    byte[] hash = md5.ComputeHash(inputBytes);
		 
		    StringBuilder sb = new StringBuilder();
		    for (int i = 0; i < hash.Length; i++) {
		        sb.Append(hash[i].ToString("X2"));
		    }
		    return sb.ToString();
		}
		public void RunCommand(string[] args)
        {
        	Publisher.Run("test-editor command");
        	if (args.Length > 0)
        		Publisher.Run("test-editor " + args[0]);
        	if (args.Length == 1 && args[0] == "kill")
        		_isAlive = false;
        }
	}
}