using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Endpoints;

namespace EditorEngine.Core.Commands.Handlers
{
    public class GetCaretHandler : ICommandHandler, IConsumerOf<EditorCaretMessage>
    {
        private IMessageDispatcher _dispatcher;
        private ICommandEndpoint _endpoint;

        public string ID { get { return "get-caret"; } }

        public GetCaretHandler(IMessageDispatcher dispatcher, ICommandEndpoint endpoint)
        {
            _dispatcher = dispatcher;
            _endpoint = endpoint;
        }

        public void Execute(CommandMessage message)
        {
            Logger.Write("Dispatching EditorGetCaretMessage");
            _dispatcher.Publish(
                new EditorGetCaretMessage(message));
        }

        public void Consume(EditorCaretMessage message)
        {
            _endpoint.Run(
                message.Message.ClientID,
                message.GetCommand());
        }
    }
}