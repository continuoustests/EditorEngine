using System;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
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
		
		public void Execute (string arguments)
		{
			var location = getLocation(arguments);
			if (location == null)
				return;
			_dispatcher.Publish(new EditorGoToMessage(location.File, location.Line, location.Column));
		}
		
		private goTo getLocation(string arguments)
		{
			var chunks = arguments.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
			if (chunks.Length == 0 || chunks.Length > 3)
			{
				_dispatcher.Publish(new UsageErrorMessage(string.Format("Invalid number of arguments. Usage: {0}", getUsage())));
				return null;
			}
			
			// Get file
			var filename = chunks[0].Trim();
			if (!File.Exists(filename))
			{
				_dispatcher.Publish(new UsageErrorMessage(string.Format("Invalid file {0} specified. Useage: {1}", filename, getUsage())));
				return null;
			}
			var location = new goTo() { File = filename };
			if (chunks.Length == 1)
				return location;
			
			// Get line number
			int line;
			if (!int.TryParse(chunks[1], out line))
				return location;
			location.Line = line;
			if (chunks.Length == 2)
				return location;
			
			// Get column
			int column;
			if (!int.TryParse(chunks[2], out column))
				return location;
			location.Column = column;
			return location;
		}
		
		private string getUsage()
		{
			return "goto /Some/Path/To/FileName.cs|{line number}|{column number}";
		}
		
		class goTo
		{
			public string File { get; set; }
			public int Line { get; set; }
			public int Column { get; set; }
		}
	}
}

