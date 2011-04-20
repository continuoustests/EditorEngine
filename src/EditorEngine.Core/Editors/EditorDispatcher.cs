using System;
using System.Linq;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using EditorEngine.Core.FileSystem;
using System.Threading;

namespace EditorEngine.Core.Editors
{
	class EditorDispatcher :
		IConsumerOf<EditorGoToMessage>,
		IConsumerOf<EditorLoadMessage>
	{
		private object _padlock = new object();
		private IEditor _editor = null;
		private IPluginLoader _pluginFactory;
		private IMessageDispatcher _dispatcher;
		
		public EditorDispatcher(IPluginLoader pluginFactory, IMessageDispatcher dispatcher)
		{
			_pluginFactory = pluginFactory;
			_dispatcher = dispatcher;
			ThreadPool.QueueUserWorkItem(shutdownTimer);
		}
		
		private void shutdownTimer(object state)
		{
			while (true)
			{
				Thread.Sleep(500);
				if (_editor == null)
					continue;
				if (!_editor.IsAlive)
				{
					_dispatcher.Publish(new ShutdownMessage());
					break;
				}
			}
		}
		
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
			_editor = _pluginFactory.Load(message.Editor);
			if (_editor != null)
				_editor.Initialize(null);
		}
		
		public void Consume(EditorGoToMessage message)
		{
			editor(() => _editor.GoTo(new Location(message.File, message.Line, message.Column)));
		}
	}
}

