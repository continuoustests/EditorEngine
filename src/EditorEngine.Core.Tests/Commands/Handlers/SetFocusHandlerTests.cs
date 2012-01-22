using System;
using EditorEngine.Core.Commands.Handlers;
using EditorEngine.Core.Tests.Messaging;
using NUnit.Framework;
using EditorEngine.Core.Messaging.Messages;

namespace EditorEngine.Core.Tests.Commands.Handlers
{
	[TestFixture]
	public class SetFocusHandlerTests
	{
		private SetFocusHandler _handler;
		private Fake_MessageDispatcher _dispatcher;
		
		[SetUp]
		public void Setup()
		{
			_dispatcher = new Fake_MessageDispatcher();
			_handler = new SetFocusHandler(_dispatcher);
		}
		
		[Test]
		public void When_passing_invalid_arguments_it_should_publish_usage_error_message()
		{
			_handler.Execute(new CommandMessage(Guid.Empty, ""));
			_dispatcher.Published<EditorSetFocusMessage>();
		}
	}
}
