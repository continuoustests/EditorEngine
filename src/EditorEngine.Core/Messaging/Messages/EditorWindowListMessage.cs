using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace EditorEngine.Core.Messaging.Messages
{
    public class EditorWindowListMessage : Message
    {
        public CommandMessage Message { get; private set; }
        public string[] Windows { get; private set; }

        public EditorWindowListMessage(CommandMessage message, string[] windows)
        {
            Message = message;
            Windows = windows;
        }

        public string GetCommand()
        {
            var sb = new StringBuilder();
            sb.Append(Message.CorrelationID);
            Windows.ToList()
                .ForEach(x => sb.AppendLine(x));
            return sb.ToString();
        }
    }
}
