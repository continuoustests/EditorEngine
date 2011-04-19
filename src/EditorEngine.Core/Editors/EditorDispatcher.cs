using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
using System.Diagnostics;

namespace EditorEngine.Core.Editors
{
	class EditorDispatcher :
		IConsumerOf<EditorGoToMessage>,
		IConsumerOf<EditorLoadMessage>
	{
		private object _padlock = new object();
		private IEditor _editor = null;
		
		private void editor(Action invocation)
		{
			editor(invocation, false);
		}
		
		private void editor(Action invocation, bool giveFocus)
		{
			lock (_padlock)
			{
				if (_editor == null) return;
				invocation.Invoke();
				_editor.SetFocus();
			}
		}
		
		public void SetEditor(IEditor editor)
		{
			_editor = editor;
		}
		
		public void Consume(EditorLoadMessage message)
		{
			_editor = new Test_geditEditor();
		}
		
		public void Consume(EditorGoToMessage message)
		{
			editor(() => _editor.GoTo(message.File, message.Line, message.Column));
		}
	}
	
	class Test_geditEditor : IEditor
	{
		public void SetFocus()
		{
		}
		
		public void GoTo(string file, int line, int column)
		{
			invoke("{0} +{1}", file, line);
		}
		
		private void invoke(string arguments, params object[] args)
		{
			var proc = new Process();
			proc.StartInfo = new ProcessStartInfo("gedit", string.Format(arguments, args));
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.Start();
			proc.WaitForExit();
		}
	}
}

