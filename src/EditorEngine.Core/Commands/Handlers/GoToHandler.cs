using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Arguments;
using System.IO;
namespace EditorEngine.Core.Commands.Handlers
{
	/// <summary>
	/// Useage: goto /Some/Path/To/FileName.cs|{line number}|{column number}
	/// </summary>
	public class GoToHandler : ICommandHandler
	{
		private IMessageDispatcher _dispatcher;
		
		public string ID { get { return "goto"; } }
		
		public GoToHandler(IMessageDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}
		
		public void Execute(CommandMessage message)
		{
			var arguments = message.Arguments;
			if (arguments.Count != 1)
			{
				invalidArgs();
				return;
			}
			var location = getLocation(arguments[0]);
			if (location == null)
				return;
			Console.WriteLine("Dispatching goto");
			_dispatcher.Publish(new EditorGoToMessage(location.File, location.Line, location.Column));
		}
		
		private GoTo getLocation(string arguments)
		{
			var goTo = new PositionArgumentParser().Parse(arguments);
			if (goTo == null)
			{
				invalidArgs();
				return null;
			}
			
			return goTo;
		}

		private void invalidArgs()
		{
			_dispatcher.Publish(
					new UsageErrorMessage(
						string.Format("Invalid number of arguments. Usage: {0}", getUsage())));
		}
		
		private string getUsage()
		{
			return "goto /Some/Path/To/FileName.cs|{line number}|{column number}";
		}
	}
}

