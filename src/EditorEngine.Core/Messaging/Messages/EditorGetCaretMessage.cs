using System;

namespace EditorEngine.Core.Messaging.Messages
{
    class EditorGetCaretMessage : Message
    {
        public CommandMessage Message { get; private set; }

        public EditorGetCaretMessage(CommandMessage message)
        {
            Message = message;
        }
    }
}