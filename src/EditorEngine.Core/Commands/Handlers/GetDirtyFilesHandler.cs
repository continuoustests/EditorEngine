using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Endpoints;

namespace EditorEngine.Core.Commands.Handlers
{
	public class GetDirtyFilesHandler :
		ICommandHandler,
		IConsumerOf<EditorDirtyFilesListMessage>
	{
		private IMessageDispatcher _dispatcher;
		private ICommandEndpoint _endpoint;

		public string ID { get { return "get-dirty-files"; } }

		public GetDirtyFilesHandler(IMessageDispatcher dispatcher, ICommandEndpoint endpoint)
		{
			_dispatcher = dispatcher;
			_endpoint = endpoint;
		}

		public void Execute(CommandMessage message)
		{
			_dispatcher.Publish(
				new EditorGetDirtyFilesMessage(
					message));
		}

		public void Consume(EditorDirtyFilesListMessage message)
		{
			_endpoint.Run(
				message.Message.ClientID,
				message.GetCommand());
		}
	}
}
