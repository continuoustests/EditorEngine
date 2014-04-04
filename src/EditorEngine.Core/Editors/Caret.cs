using System;
using EditorEngine.Core.CommandBuilding;

namespace EditorEngine.Core.Editors
{
    public class Caret
    {
        public string File { get; private set; }
        public Position Position { get; private set; }
        public string Content { get; private set; }


        public Caret(string file, Position position, string content)
        {
            File = file;
            Position = position;
            Content = content;
        }
    }
}