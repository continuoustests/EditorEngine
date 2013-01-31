using System;
using EditorEngine.Core.Endpoints;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
namespace EditorEngine.Core.Commands.Handlers
{
	/// <summary>
	/// Useage: editor {name of editor}
	/// </summary>
	public class LoadEditorHandler : ICommandHandler, IConsumerOf<EditorLoadedMessage>
	{
		private ICommandEndpoint _endpoint;
		private IMessageDispatcher _dispatcher;
		
		public string ID { get { return "editor"; } }
		
		public LoadEditorHandler(IMessageDispatcher dispatcher, ICommandEndpoint endpoint)
		{
			_dispatcher = dispatcher;
			_endpoint = endpoint;
		}
		
		public void Execute(CommandMessage message)
		{
			var arguments = message.Arguments;
			if (arguments.Count == 0)
			{
				_dispatcher.Publish(
					new UsageErrorMessage(string.Format("Invalid number of arguments. {0}", getUsage())));
				return;
			}
			_dispatcher.Publish(new EditorLoadMessage(message, arguments[0].Trim()));
		}

		public void Consume(EditorLoadedMessage message)
		{
			_endpoint.Run(
				message.Message.ClientID,
				message.Message.CorrelationID + message.Editor);
			if (message.Editor == "")
				_dispatcher.Publish(new ShutdownMessage());
		}
		
		private string getUsage()
		{
			return "Usage: editor {name of editor}";
		}
	}
}

