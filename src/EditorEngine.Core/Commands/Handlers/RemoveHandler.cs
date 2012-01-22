using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;

namespace EditorEngine.Core.Commands.Handlers
{
	public class RemoveHandler : ICommandHandler
	{
		private IMessageDispatcher _dispatcher;

		public string ID { get { return "remove"; } }

		public RemoveHandler(IMessageDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		public void Execute(Guid clientID, string[] argument)
		{
			var message = EditorRemoveMessage.Parse(argument);
			if (message == null)
				return;
			_dispatcher.Publish(message);
		}
	}
}
