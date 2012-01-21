using System;
using System.IO;
using EditorEngine.Core.Arguments;

namespace EditorEngine.Core.Messaging.Messages
{
	public class EditorInsertMessage : Message
	{
		public string Text { get; private set; }
		public GoTo Destination { get; private set; }

		public EditorInsertMessage(string text, GoTo destination)
		{
			Text = text;
			Destination = destination;
		}

		public static EditorInsertMessage Parse(string[] argument)
		{
			if (argument.Length != 2)
				return null;
			if (!File.Exists(argument[0]))
				return null;
			var goTo = new PositionArgumentParser().Parse(argument[1]);
			if (goTo == null)
				return null;
			if (!File.Exists(goTo.File))
				return null;
			var content = File.ReadAllText(argument[0]);
			if (content.EndsWith(Environment.NewLine))
				content = content.Substring(0, content.Length - Environment.NewLine.Length);
			return new EditorInsertMessage(
					content,
					goTo);
		}
	}
}
