using System;
using EditorEngine;
using EditorEngine.Core.Editors;
using System.Diagnostics;

namespace gedit
{
	public class Editor : IEditor
	{
		public Process _process { get; private set; }
		
		public bool IsAlive
		{
			get
			{
				if (_process == null)
					return true;
				return !_process.HasExited;
			}
		}
		
		public Editor()
		{
			_process = null;
		}
		
		public void Initialize(Location location)
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
		
		private void invoke(string arguments, params object[] args)
		{
			var proc = new Process();
			proc.StartInfo = new ProcessStartInfo("gedit", string.Format(arguments, args));
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.Start();
		}
	}
}

