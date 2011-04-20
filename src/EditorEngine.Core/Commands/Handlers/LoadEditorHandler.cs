using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
namespace EditorEngine.Core.Commands.Handlers
{
	/// <summary>
	/// Useage: editor {name of editor}
	/// </summary>
	public class LoadEditorHandler : ICommandHandler
	{
		private IMessageDispatcher _dispatcher;
		
		public string ID { get { return "editor"; } }
		
		public LoadEditorHandler(IMessageDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}
		
		public void Execute(string arguments)
		{
			var argumentCount = arguments.Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length;
			if (argumentCount != 1)
			{
				_dispatcher.Publish(new UsageErrorMessage(string.Format("Invalid number of arguments. {0}", getUsage())));
				return;
			}
			_dispatcher.Publish(new EditorLoadMessage(arguments.Trim()));
		}
		
		private string getUsage()
		{
			return "Usage: editor {name of editor}";
		}
	}
}

