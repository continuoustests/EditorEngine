using System;
using EditorEngine.Core.Editors;
using Rhino.Mocks;
using EditorEngine.Core.Messaging.Messages;
using NUnit.Framework;
using EditorEngine.Core.FileSystem;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Tests.Messaging;
using EditorEngine.Core.Arguments;

namespace EditorEngine.Core.Tests.Editors
{
	[TestFixture]
	public class EditorDispatcherTests
	{
		private EditorDispatcher _dispatcher;
		private IEditor _editor;
		private IFileWriter _fileWriter;
		private IPluginLoader _pluginFactory;
		private Fake_MessageDispatcher _messageDispatcher;
		
		[SetUp]
		public void Setup()
		{
			_messageDispatcher = new Fake_MessageDispatcher();
			_pluginFactory = MockRepository.GenerateMock<IPluginLoader>();
			_editor = MockRepository.GenerateMock<IEditor>();
			_fileWriter = MockRepository.GenerateMock<IFileWriter>();
			_dispatcher = new EditorDispatcher(_pluginFactory, _messageDispatcher, _fileWriter);
			_dispatcher.SetEditor(_editor);
		}
		
		[Test]
		public void Should_initialize_editor_if_plugin_loads()
		{
			_editor.Stub(x => x.IsAlive).Return(true);
			_pluginFactory.Stub(x => x.Load("gedit")).Return(_editor);
			var message = new CommandMessage(Guid.NewGuid(), "correlationID=meh|gedit");
			_dispatcher.Consume(
				new EditorLoadMessage(
					message, "gedit"));
			_editor.AssertWasCalled(x => x.Initialize(null, new string[] {}), y => y.IgnoreArguments());
			_messageDispatcher.Published<EditorLoadedMessage>();
		}
		
		[Test]
		public void Should_not_initialize_editor_if_plugin_does_not_load()
		{
			_editor.Stub(x => x.IsAlive).Return(true);
			var message = new CommandMessage(Guid.NewGuid(), "correlationID=meh|gedit");
			_dispatcher.Consume(
				new EditorLoadMessage(
					message, "gedit"));
			_editor.AssertWasNotCalled(x => x.Initialize(null, new string[] {}), y => y.IgnoreArguments());
		}
		
		[Test]
		public void Should_go_to_file()
		{
			_editor.Stub(x => x.IsAlive).Return(true);
			_dispatcher.Consume(new EditorGoToMessage("MyFile", 10, 10, null));
			_editor.AssertWasCalled(method => method.GoTo(null), x => x.IgnoreArguments());
		}
		
		[Test]
		public void Should_set_focus()
		{
			_editor.Stub(x => x.IsAlive).Return(true);
			_dispatcher.Consume(new EditorSetFocusMessage());
			_editor.AssertWasCalled(method => method.SetFocus());
		}
		
		[Test]
		public void Should_send_insertion_to_editor_if_it_can_handle_it()
		{
			_editor.Stub(x => x.IsAlive).Return(true);
			_editor.Stub(x => x.CanInsertFor("tofile.txt")).Return(true);
			var msg = new EditorInsertMessage("inectfile.txt", new GoTo() { File = "tofile.txt" }, null);
			_dispatcher.Consume(msg);
			_editor.AssertWasCalled(method => method.Insert(msg));
		}
		
		[Test]
		public void Should_send_insertion_to_file_writer_if_editor_cannot_handle_it()
		{
			_editor.Stub(x => x.IsAlive).Return(true);
			_editor.Stub(x => x.CanInsertFor("tofile.txt")).Return(false);
			var msg = new EditorInsertMessage("inectfile.txt", new GoTo() { File = "tofile.txt" }, null);
			_dispatcher.Consume(msg);
			_fileWriter.AssertWasCalled(method => method.Insert(msg));
		}

		[Test]
		public void When_process_id_is_not_running_a_shutdown_message_should_be_published()
		{
			_editor.Stub(x => x.IsAlive).Return(false);
			_pluginFactory.Stub(x => x.Load("gedit")).Return(_editor);
			var message = new CommandMessage(Guid.NewGuid(), "correlationID=meh|gedit");
			_dispatcher.Consume(
				new EditorLoadMessage(
					message, "gedit"));
			Wait.ForFiveSecond().OrUntil(() => { return _messageDispatcher.GetPublishedMessage<ShutdownMessage>() != null; });
			_messageDispatcher.Published<ShutdownMessage>();
		}
	}
}

