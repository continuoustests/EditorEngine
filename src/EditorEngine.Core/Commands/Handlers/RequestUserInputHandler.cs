using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Arguments;

namespace EditorEngine.Core.Commands.Handlers
{
    class RequestUserInputHandler : ICommandHandler
    {
        private IMessageDispatcher _dispatcher;
        
        public string ID { get { return "user-input"; } }
        
        public RequestUserInputHandler(IMessageDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }
        
        public void Execute(CommandMessage message)
        {
            Logger.Write("Hanlind use request message with " + message.Arguments.Count.ToString() + " arguments");
            var arguments = message.Arguments;
            var defaultvalue = "";
            if (arguments.Count > 1)
                defaultvalue = arguments[1];
            _dispatcher.Publish(new EditorRequestUserInput(arguments[0], defaultvalue));
        }
    }
}