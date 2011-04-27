using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
namespace EditorEngine.Core.Commands.Handlers
{
	public class SetFocusHandler : ICommandHandler
	{
		private IMessageDispatcher _dispatcher;
		
		public string ID { get { return "setfocus"; } }
		
		public SetFocusHandler(IMessageDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}
				
		public void Execute(string arguments)
		{
			_dispatcher.Publish(new EditorSetFocusMessage());
		}
	}
}

