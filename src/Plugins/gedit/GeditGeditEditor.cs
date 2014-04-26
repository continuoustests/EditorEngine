using System;
using System.Collections.Generic;
using EditorEngine;
using EditorEngine.Core.Editors;
using System.Diagnostics;
using EditorEngine.Core.Endpoints;
using EditorEngine.Core.CommandBuilding;
using EditorEngine.Core.Messaging.Messages;

namespace gedit
{
	public class GeditEditor : IEditor
	{
		private Process _process = null;
		
		public ICommandEndpoint Publisher { private get; set; }
		public bool IsAlive
		{
			get
			{
				if (_process == null)
					return true;
				return !_process.HasExited;
			}
		}
		
		public void Initialize(Location location, string[] args)
		{
			var argument = "";
			if (location != null)
				argument = string.Format("{0} +{1}", location.File, location.Line);
			if (_process != null)
				_process.Kill();
			_process = new Process();
			_process.StartInfo = new ProcessStartInfo("gedit", argument);
			_process.StartInfo.CreateNoWindow = true;
			_process.StartInfo.UseShellExecute = true;
			_process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			_process.Start();
		}
		
		public void SetFocus()
		{
		}
		
		public void GoTo(Location location)
		{
			invoke("{0} +{1}", location.File, location.Line);
		}

		public void BeginBatchUpdate() {}
		public void EndBatchUpdate() {}
		
		public bool CanInsertFor(string file)
		{
			return false;
		}

		public void Insert(EditorInsertMessage message)
		{
		}
		
		public bool CanRemoveFor(string file)
		{
			return false;
		}

		public void Remove(EditorRemoveMessage message)
		{
		}

		public KeyValuePair<string,string>[] GetDirtyFiles(string file)
		{
			return new KeyValuePair<string,string>[] {};
		}
		
		public Caret GetCaret() {
        	return new Caret("", new Position(0, 0), "");
        }

        public void RequestUserSelection(string identifier, string[] items)
        {
        }

        public void RequestUserInput(string identifier, string defaultValue)
        {
            Publisher.Run("user-select unsupported \"" + identifier + "\" \"" + defaultValue + "\"");   
        }

		private void invoke(string arguments, params object[] args)
		{
			var proc = new Process();
			proc.StartInfo = new ProcessStartInfo("gedit", string.Format(arguments, args));
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.Start();
		}
		
		public void RunCommand(string[] args)
        {
        }
	}
}

