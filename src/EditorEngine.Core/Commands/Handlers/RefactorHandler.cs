using System;
using System.IO;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;

namespace EditorEngine.Core.Commands.Handlers
{
	public class RefactorHandler : ICommandHandler
	{
		private IMessageDispatcher _dispatcher;

		public string ID { get { return "refactor"; } }

		public RefactorHandler(IMessageDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		public void Execute(CommandMessage message)
		{
			var argument = message.Arguments;
			if (argument.Count != 1)
				return;
			if (!File.Exists(argument[0]))
				return;
			_dispatcher.Publish(
				new EditorRefactorMessage(
					File.ReadAllLines(argument[0])));
		}
	}
}
