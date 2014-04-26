using System;

namespace EditorEngine.Core.Messaging.Messages
{
    public class EditorRequestUserInput : Message
    {
        public string Identifier { get; private set; }
        public string DefaultValue { get; private set; }

        public EditorRequestUserInput(string identifier, string defaultvalue) {
            Identifier = identifier;
            DefaultValue = defaultvalue;
        }
    }
}