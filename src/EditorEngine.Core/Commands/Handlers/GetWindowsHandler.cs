using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Endpoints;

namespace EditorEngine.Core.Commands.Handlers
{
    public class GetWindowsHandler :
        ICommandHandler,
        IConsumerOf<EditorWindowListMessage>
    {
        private IMessageDispatcher _dispatcher;
        private ICommandEndpoint _endpoint;

        public string ID { get { return "get-windows"; } }

        public GetWindowsHandler(IMessageDispatcher dispatcher, ICommandEndpoint endpoint)
        {
            _dispatcher = dispatcher;
            _endpoint = endpoint;
        }

        public void Execute(CommandMessage message)
        {
            _dispatcher.Publish(
                new EditorGetWindowsMessage(
                    message));
        }

        public void Consume(EditorWindowListMessage message)
        {
            _endpoint.Run(
                message.Message.ClientID,
                message.GetCommand());
        }
    }
}
