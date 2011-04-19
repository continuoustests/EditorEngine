using System;
using NUnit.Framework;
using EditorEngine.Core.Commands;
using EditorEngine.Core.Messaging.Messages;
namespace EditorEngine.Core.Tests.Commands
{
	[TestFixture]
	public class CommandDispatcherTests
	{
		private CommandDispatcher _dispatcher;
		private Fake_CommandHandler _command;
		
		[SetUp]
		public void Setup()
		{
			_command = new Fake_CommandHandler();
			_dispatcher = new CommandDispatcher(new ICommandHandler[] { _command });
		}
		
		[Test]
		public void Should_run_command_handler_when_consuming_command_message()
		{
			var message = new CommandMessage("MyCommand some arguments");
			_dispatcher.Consume(message);
			Assert.That(_command.PassedArguments, Is.EqualTo(message.Arguments));
		}
	}
	
	class Fake_CommandHandler : ICommandHandler
	{
		public string PassedArguments { get; private set; }
		
		public string ID { get { return "MyCommand"; } }
		
		public void Execute(string arguments)
		{
			PassedArguments = arguments;
		}
	}
}

