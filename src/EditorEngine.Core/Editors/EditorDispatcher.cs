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
		IConsumerOf<EditorInsertMessage>,
		IConsumerOf<EditorRemoveMessage>,
		IConsumerOf<EditorReplaceMessage>,
		IConsumerOf<EditorRefactorMessage>,
		IConsumerOf<EditorGetDirtyFilesMessage>,
		IConsumerOf<EditorCommandMessage>,
		IConsumerOf<EditorGetCaretMessage>
	{
		private object _padlock = new object();
		private IEditor _editor = null;
		private IFileWriter _fileWriter;
		private IPluginLoader _pluginFactory;
		private IMessageDispatcher _dispatcher;
		
		public string CurrentEditor {
			get { 
				if (_editor == null)
					return "";
				return _editor.GetType().ToString();
			}
		}

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
			if (_editor == null)
			{
				_editor = _pluginFactory.Load(message.Editor);
				if (_editor == null) {
					_dispatcher.Publish(new EditorLoadedMessage(message.Message, ""));
					return;
				}
				var args = new string[] {};
				if (message.Message.Arguments != null)
					args = 	message.Message.Arguments.ToArray();
				_editor.Initialize(null, args);
				_dispatcher.Publish(new EditorLoadedMessage(message.Message, message.Editor));
			}
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

		public void Consume(EditorInsertMessage message)
		{
			if (queryEditor<bool>(() => { return _editor.CanInsertFor(message.Destination.File); }))
				editor(() => _editor.Insert(message));
			else
				_fileWriter.Insert(message);
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
				var cmd = new CommandMessage(Guid.Empty, msg);
				if (cmd.Command == "insert")
					parsedMessage = EditorInsertMessage.Parse(cmd.Arguments.ToArray());
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
				if (msg.GetType() == typeof(EditorInsertMessage))
					Consume((EditorInsertMessage)msg);
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
			var insert = new EditorInsertMessage(
						message.Text,
						message.Start,
						null);
			if (queryEditor<bool>(() => { return _editor.CanRemoveFor(message.Start.File); }) &&
				queryEditor<bool>(() => { return _editor.CanInsertFor(message.Start.File); }))
			{
				editor(() => _editor.Remove(remove));
				editor(() => _editor.Insert(insert));
			}
			else
			{
				_fileWriter.Remove(remove);
				_fileWriter.Insert(insert);
			}
		}

		public void Consume(EditorGetDirtyFilesMessage message)
		{
			string file = null;
			if (message.Message.Arguments.Count > 0)
				file = message.Message.Arguments[0];
			_dispatcher.Publish(
				new EditorDirtyFilesListMessage(
					message.Message,
					_editor.GetDirtyFiles(file)));
		}

		public void Consume(EditorCommandMessage message)
		{
			_editor.RunCommand(message.Arguments);
		}

		public void Consume(EditorGetCaretMessage message)
		{
			_dispatcher.Publish(
				new EditorCaretMessage(message.Message, _editor.GetCaret()));
			Logger.Write("Published EditorGetCaretMessage");
		}
	}
}

