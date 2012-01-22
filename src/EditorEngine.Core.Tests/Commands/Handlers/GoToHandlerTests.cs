using System;
using NUnit.Framework;
using EditorEngine.Core.Commands.Handlers;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Tests.Messaging;
using System.Reflection;
namespace EditorEngine.Core.Tests.Commands.Handlers
{
	[TestFixture]
	public class GoToHandlerTests
	{
		private GoToHandler _handler;
		private Fake_MessageDispatcher _dispatcher;
		
		[SetUp]
		public void Setup()
		{
			_dispatcher = new Fake_MessageDispatcher();
			_handler = new GoToHandler(_dispatcher);
		}
		
		[Test]
		public void When_passing_invalid_arguments_it_should_publish_usage_error_message()
		{
			_handler.Execute(Guid.Empty, new[] {"not", "valid", "argument" });
			_dispatcher.Published<UsageErrorMessage>();
		}
		
		[Test]
		public void When_no_arguments_are_passed_it_should_publish_usage_error_message()
		{
			_handler.Execute(Guid.Empty, new[] {""});
			_dispatcher.Published<UsageErrorMessage>();
		}
		
		[Test]
		public void When_too_many_arguments_are_passed_it_should_publish_usage_error_message()
		{
			_handler.Execute(Guid.Empty, new[] {"1|2|3|4"});
			_dispatcher.Published<UsageErrorMessage>();
		}
		
		[Test]
		public void When_file_is_not_an_existing_file_it_should_publish_usage_error_message()
		{
			_handler.Execute(Guid.Empty, new[] {string.Format("{0}.NotExisting", Assembly.GetExecutingAssembly().Location)});
			_dispatcher.Published<UsageErrorMessage>();
		}
		
		[Test]
		public void When_passing_valid_arguments_it_should_publish_editor_goto_message()
		{
			_handler.Execute(Guid.Empty, new[] {string.Format("{0}|{1}|{2}", Assembly.GetExecutingAssembly().Location, 10, 15)});
			var messsage = _dispatcher.GetPublishedMessage<EditorGoToMessage>();
			Assert.That(messsage.File, Is.EqualTo(Assembly.GetExecutingAssembly().Location));
		}
		
		[Test]
		public void When_passing_a_line_number_it_should_parse_out_a_valid_line_number()
		{
			_handler.Execute(Guid.Empty, new[] {string.Format("{0}|12", Assembly.GetExecutingAssembly().Location)});
			var messsage = _dispatcher.GetPublishedMessage<EditorGoToMessage>();
			Assert.That(messsage.Line, Is.EqualTo(12));
		}
		
		[Test]
		public void When_passing_a_column_it_should_parse_out_a_valid_column()
		{
			_handler.Execute(Guid.Empty, new[] {string.Format("{0}|0|7", Assembly.GetExecutingAssembly().Location)});
			var messsage = _dispatcher.GetPublishedMessage<EditorGoToMessage>();
			Assert.That(messsage.Column, Is.EqualTo(7));
		}
	}
}

