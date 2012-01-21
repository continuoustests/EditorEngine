using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Arguments;

namespace EditorEngine.Core.Commands.Handlers
{
	public class InjectHandler : ICommandHandler
	{
		private IMessageDispatcher _dispatcher;

		public string ID { get { return "inject"; } }

		public InjectHandler(IMessageDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		public void Execute(string[] argument)
		{
			var message = EditorInjectMessage.Parse(argument);
			if (message == null)
				return;
			_dispatcher.Publish(message);
		}
	}
}
