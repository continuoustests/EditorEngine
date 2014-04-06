using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Arguments;

namespace EditorEngine.Core.Commands.Handlers
{
    class RequestUserSelectionHandler : ICommandHandler
    {
        private IMessageDispatcher _dispatcher;
        
        public string ID { get { return "user-select"; } }
        
        public RequestUserSelectionHandler(IMessageDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }
        
        public void Execute(CommandMessage message)
        {
            Logger.Write("Hanlind use rrequest message with " + message.Arguments.Count.ToString() + " arguments");
            var arguments = message.Arguments;
            if (arguments.Count != 2)
                return;
            _dispatcher.Publish(new EditorRequestUserSelection(arguments[0], arguments[1]));
        }
    }
}