using System;
using EditorEngine.Core.Messaging;

namespace EditorEngine.Core.Messaging.Messages
{
    public class EditorGetWindowsMessage : Message
    {
        public CommandMessage Message { get; private set; }

        public EditorGetWindowsMessage(CommandMessage message)
        {
            Message = message;
        }
    }
}
