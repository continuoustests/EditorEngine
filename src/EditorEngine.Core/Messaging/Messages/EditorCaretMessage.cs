using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using EditorEngine.Core.Editors;
using EditorEngine.Core.CommandBuilding;

namespace EditorEngine.Core.Messaging.Messages
{
    public class EditorCaretMessage : Message
    {
        public CommandMessage Message { get; private set; }
        public string File { get; private set; }
        public Position Position { get; private set; }
        public string Content { get; private set; }

        public EditorCaretMessage(CommandMessage message, Caret caret)
        {
            Message = message;
            File = caret.File;
            Position = caret.Position;
            Content = caret.Content;
        }

        public string GetCommand()
        {
            var sb = new StringBuilder();
            sb.Append(Message.CorrelationID);
            sb.AppendLine(string.Format("{0}|{1}|{2}", File, Position.Line, Position.Column));
            sb.AppendLine(Content);
            return sb.ToString();
        }
    }
}