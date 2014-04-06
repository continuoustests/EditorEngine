using System;

namespace EditorEngine.Core.Messaging.Messages
{
    public class EditorRequestUserSelection : Message
    {
        public string Identifier { get; private set; }
        public string[] Items { get; private set; }

        public EditorRequestUserSelection(string identifier, string itemlist) {
            Identifier = identifier;
            Items = itemlist.Split(new[] {','});
        }
    }
}