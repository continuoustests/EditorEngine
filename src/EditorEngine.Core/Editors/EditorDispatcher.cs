using System;
using System.Linq;
using System.Collections.Generic;
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
		IConsumerOf<EditorLoadMessage>,
		IConsumerOf<EditorSetFocusMessage>,
		IConsumerOf<EditorInjectMessage>,
		IConsumerOf<EditorRemoveMessage>,
		IConsumerOf<EditorReplaceMessage>,
		IConsumerOf<EditorRefactorMessage>
	{
		private object _padlock = new object();
		private IEditor _editor = null;
		private IFileWriter _fileWriter;
		private IPluginLoader _pluginFactory;
		private IMessageDispatcher _dispatcher;
		
		public EditorDispatcher(IPluginLoader pluginFactory, IMessageDispatcher dispatcher, IFileWriter fileWriter)
		{
			_pluginFactory = pluginFactory;
			_dispatcher = dispatcher;
			_fileWriter = fileWriter;
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
			}
		}
		
		private T queryEditor<T>(Func<T> invocation)
		{
			lock (_padlock)
			{
				if (_editor == null) return default(T);
				return (T) invocation.Invoke();
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
			editor(() => _editor.SetFocus());
		}
		
		public void Consume(EditorSetFocusMessage message)
		{
			editor(() => _editor.SetFocus());
		}

		public void Consume(EditorInjectMessage message)
		{
			if (queryEditor<bool>(() => { return _editor.CanInjectFor(message.Destination.File); }))
				editor(() => _editor.Inject(message));
			else
				_fileWriter.Inject(message);
		}
		
		public void Consume(EditorRemoveMessage message)
		{
			if (queryEditor<bool>(() => { return _editor.CanRemoveFor(message.Start.File); }))
				editor(() => _editor.Remove(message));
			else
				_fileWriter.Remove(message);
		}
		
		public void Consume(EditorRefactorMessage message)
		{
			var messages = new List<Message>();
			foreach (var msg in message.Lines)
			{
				Message parsedMessage = null;
				var cmd = new CommandMessage(msg);
				if (cmd.Command == "inject")
					parsedMessage = EditorInjectMessage.Parse(cmd.Arguments.ToArray());
				else if (cmd.Command == "remove")
					parsedMessage = EditorRemoveMessage.Parse(cmd.Arguments.ToArray());
				else if (cmd.Command == "replace")
					parsedMessage = EditorReplaceMessage.Parse(cmd.Arguments.ToArray());
				if (parsedMessage == null)
					return;
				messages.Add(parsedMessage);
			}
			foreach (var msg in messages)
			{
				if (msg.GetType() == typeof(EditorInjectMessage))
					Consume((EditorInjectMessage)msg);
				else if (msg.GetType() == typeof(EditorRemoveMessage))
					Consume((EditorRemoveMessage)msg);
				else if (msg.GetType() == typeof(EditorReplaceMessage))
					Consume((EditorReplaceMessage)msg);
			}
		}
		
		public void Consume(EditorReplaceMessage message)
		{
			var remove = new EditorRemoveMessage(
						message.Start,
						message.End);
			var inject = new EditorInjectMessage(
						message.Text,
						message.End);
			if (queryEditor<bool>(() => { return _editor.CanRemoveFor(message.Start.File); }) &&
				queryEditor<bool>(() => { return _editor.CanInjectFor(message.Start.File); }))
			{
				editor(() => _editor.Remove(remove));
				editor(() => _editor.Inject(inject));
			}
			else
			{
				_fileWriter.Remove(remove);
				_fileWriter.Inject(inject);
			}
		}
	}
}

