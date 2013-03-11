using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;

namespace EditorEngine.Core.Commands.Handlers
{
	public class EditorCommandHandler : ICommandHandler
	{
		private IMessageDispatcher _dispatcher;

		public string ID { get { return "command"; } }

		public EditorCommandHandler(IMessageDispatcher dispatcher) {
			_dispatcher = dispatcher;
		}

		public void Execute(CommandMessage message) {
			_dispatcher.Publish(
				new EditorCommandMessage(
					message.Arguments.ToArray()));
		}
	}
}