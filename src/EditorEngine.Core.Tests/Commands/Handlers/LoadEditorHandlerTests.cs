using System;
using Rhino.Mocks;
using NUnit.Framework;
using EditorEngine.Core.Endpoints;
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
		private ICommandEndpoint _endpoint;
		
		[SetUp]
		public void Setup()
		{
			_dispatcher = new Fake_MessageDispatcher();
			_endpoint = MockRepository.GenerateMock<ICommandEndpoint>();
			_handler = new LoadEditorHandler(_dispatcher, _endpoint);
		}
		
		[Test]
		public void When_not_supplying_an_argument_it_should_publish_usage_message()
		{
			_handler.Execute(msg(""));
			_dispatcher.Published<UsageErrorMessage>();
		}
		
		[Test]
		public void When_supplying_to_many_arguments_it_should_publish_usage_message()
		{
			_handler.Execute(msg("1 2"));
			_dispatcher.Published<UsageErrorMessage>();
		}
		
		[Test]
		public void When_supplying_an_editor_it_should_publish_an_editor_load_message()
		{
			_handler.Execute(msg("vi"));
			var message = _dispatcher.GetPublishedMessage<EditorLoadMessage>();
			Assert.That(message.Editor, Is.EqualTo("vi"));
		}

		[Test]
		public void When_consuming_editor_loaded_message_it_will_notify_clients()
		{
			_handler.Consume(
				new EditorLoadedMessage(
					new CommandMessage(Guid.Empty, "correlationID=done|editor vi"), "vi"));
			_endpoint.AssertWasCalled(
				x => x.Run(
					Guid.Empty,
					"correlationID=done|vi"));
		}
		
		private CommandMessage msg(string args)
		{
			return new CommandMessage(Guid.Empty, "editor " + args);
		}
	}
}

