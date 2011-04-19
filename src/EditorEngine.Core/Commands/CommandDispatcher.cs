using System;
using System.Collections.Generic;
using System.Linq;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Messaging;
namespace EditorEngine.Core.Commands
{
	class CommandDispatcher : IConsumerOf<CommandMessage>
	{
		private List<ICommandHandler> _handlers = new List<ICommandHandler>();
		
		public CommandDispatcher(ICommandHandler[] handlers)
		{
			_handlers.AddRange(handlers);
		}
		
		public void Consume(CommandMessage message)
		{
			lock (_handlers)
			{
				var handler = _handlers.FirstOrDefault(x => x.ID.Equals(message.Command));
				if (handler == null)
					return;
				handler.Execute(message.Arguments);
			}
		}
	}
}

