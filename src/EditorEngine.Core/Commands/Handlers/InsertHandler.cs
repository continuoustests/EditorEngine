using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Arguments;

namespace EditorEngine.Core.Commands.Handlers
{
	public class InsertHandler : ICommandHandler
	{
		private IMessageDispatcher _dispatcher;

		public string ID { get { return "insert"; } }

		public InsertHandler(IMessageDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		public void Execute(CommandMessage msg)
		{
			var argument = msg.Arguments.ToArray();
			var message = EditorInsertMessage.Parse(argument);
			if (message == null)
				return;
			_dispatcher.Publish(message);
		}
	}
}
