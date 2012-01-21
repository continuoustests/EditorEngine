using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;

namespace EditorEngine.Core.Commands.Handlers
{
	public class ReplaceHandler : ICommandHandler
	{
		private IMessageDispatcher _dispatcher;

		public string ID { get { return "replace"; } }

		public ReplaceHandler(IMessageDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		public void Execute(string[] argument)
		{
			var message = EditorReplaceMessage.Parse(argument);
			if (message == null)
				return;
			_dispatcher.Publish(message);
		}
	}
}
