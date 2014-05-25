using System;

namespace EditorEngine.Core.Messaging.Messages
{
    class EditorRequestUserSelectionAtCaret : Message
    {
        public string Identifier { get; private set; }
        public string[] Items { get; private set; }

        public EditorRequestUserSelectionAtCaret(string identifier, string itemlist) {
            Identifier = identifier;
            Items = itemlist.Split(new[] {','});
        }
    }
}
