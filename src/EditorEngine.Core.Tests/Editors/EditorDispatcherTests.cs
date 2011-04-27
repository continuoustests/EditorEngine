using System;
using EditorEngine.Core.Editors;
using Rhino.Mocks;
using EditorEngine.Core.Messaging.Messages;
using NUnit.Framework;
using EditorEngine.Core.FileSystem;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Tests.Messaging;

namespace EditorEngine.Core.Tests.Editors
{
	[TestFixture]
	public class EditorDispatcherTests
	{
		private EditorDispatcher _dispatcher;
		private IEditor _editor;
		private IPluginLoader _pluginFactory;
		private Fake_MessageDispatcher _messageDispatcher;
		
		[SetUp]
		public void Setup()
		{
			_messageDispatcher = new Fake_MessageDispatcher();
			_pluginFactory = MockRepository.GenerateMock<IPluginLoader>();
			_editor = MockRepository.GenerateMock<IEditor>();
			_dispatcher = new EditorDispatcher(_pluginFactory, _messageDispatcher);
			_dispatcher.SetEditor(_editor);
		}
		
		[Test]
		public void Should_initialize_editor_if_plugin_loads()
		{
			_editor.Stub(x => x.IsAlive).Return(true);
			_pluginFactory.Stub(x => x.Load("gedit")).Return(_editor);
			_dispatcher.Consume(new EditorLoadMessage("gedit"));
			_editor.AssertWasCalled(x => x.Initialize(null), y => y.IgnoreArguments());
		}
		
		[Test]
		public void Should_not_initialize_editor_if_plugin_does_not_load()
		{
			_editor.Stub(x => x.IsAlive).Return(true);
			_dispatcher.Consume(new EditorLoadMessage("gedit"));
			_editor.AssertWasNotCalled(x => x.Initialize(null), y => y.IgnoreArguments());
		}
		
		[Test]
		public void Should_go_to_file()
		{
			_editor.Stub(x => x.IsAlive).Return(true);
			_dispatcher.Consume(new EditorGoToMessage("MyFile", 10, 10));
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
		public void When_process_id_is_not_a_running_shutdown_message_should_be_published()
		{
			_editor.Stub(x => x.IsAlive).Return(false);
			_pluginFactory.Stub(x => x.Load("gedit")).Return(_editor);
			_dispatcher.Consume(new EditorLoadMessage("gedit"));
			Wait.ForTwoSecond().OrUntil(() => { return _messageDispatcher.GetPublishedMessage<ShutdownMessage>() != null; });
			_messageDispatcher.Published<ShutdownMessage>();
		}
	}
}

