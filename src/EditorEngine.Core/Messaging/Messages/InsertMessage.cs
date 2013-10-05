using System;
using System.IO;
using EditorEngine.Core.Arguments;
using EditorEngine.Core.CommandBuilding;

namespace EditorEngine.Core.Messaging.Messages
{
	public class EditorInsertMessage : Message
	{
		public string Text { get; private set; }
		public GoTo Destination { get; private set; }
		public Position MoveOffset { get; private set; }

		public EditorInsertMessage(string text, GoTo destination, Position offset)
		{
			Text = text;
			Destination = destination;
			MoveOffset = offset;
		}

		public static EditorInsertMessage Parse(string[] argument)
		{
			if (argument.Length < 2)
				return null;
			
			var goTo = new PositionArgumentParser().Parse(argument[1]);
			if (goTo == null)
				return null;
			if (!File.Exists(goTo.File))
				return null;
			var content = argument[0];
			if (File.Exists(argument[0])) {
				content = File.ReadAllText(argument[0]);
				if (content.EndsWith(Environment.NewLine))
					content = content.Substring(0, content.Length - Environment.NewLine.Length);
			}
			Position moveOffset = null;
			if (argument.Length > 2)
				moveOffset = Position.Parse(argument[2]);
			return new EditorInsertMessage(
					content,
					goTo,
					moveOffset);
		}
	}
}
