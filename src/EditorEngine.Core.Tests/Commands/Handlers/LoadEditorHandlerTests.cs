using System;
using NUnit.Framework;
using EditorEngine.Core.Commands.Handlers;
using EditorEngine.Core.Tests.Messaging;
using EditorEngine.Core.Messaging.Messages;
namespace EditorEngine.Core.Tests.Commands.Handlers
{
	[TestFixture]
	public class LoadEditorHandlerTests
	{
		private LoadEditorHandler _handler;
		private Fake_MessageDispatcher _dispatcher;
		
		[SetUp]
		public void Setup()
		{
			_dispatcher = new Fake_MessageDispatcher();
			_handler = new LoadEditorHandler(_dispatcher);
		}
		
		[Test]
		public void When_not_supplying_an_argument_it_should_publish_usage_message()
		{
			_handler.Execute(Guid.Empty, new string[] {});
			_dispatcher.Published<UsageErrorMessage>();
		}
		
		[Test]
		public void When_supplying_to_many_arguments_it_should_publish_usage_message()
		{
			_handler.Execute(Guid.Empty, new[] {"1", "2"});
			_dispatcher.Published<UsageErrorMessage>();
		}
		
		[Test]
		public void When_supplying_an_editor_it_should_publish_an_editor_load_message()
		{
			_handler.Execute(Guid.Empty, new[] {"vi"});
			var message = _dispatcher.GetPublishedMessage<EditorLoadMessage>();
			Assert.That(message.Editor, Is.EqualTo("vi"));
		}
	}
}

