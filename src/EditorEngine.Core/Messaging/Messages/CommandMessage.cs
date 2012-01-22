using System;
using System.Text;
using System.Collections.Generic;
namespace EditorEngine.Core.Messaging.Messages
{
	class CommandMessage : Message
	{
        private char _separator;
        private string _word;

		public Guid ClientID { get; private set; }
		public string Command { get; set; }
		public List<string> Arguments = new List<string>();

		public CommandMessage(Guid clientID, string message)
		{
			ClientID = clientID;
            _separator = ' ';
            _word = "";
            for (int i = 0; i < message.Length; i++)
                processCharacter(message[i]);
            addWord();
        }

        private void processCharacter(char argument)
        {
            if (isArgumentSeparator(argument))
                _separator = argument;

            if (itTerminatesArgument(argument))
            {
                addWord();
                _word = "";
                return;
            }
            _word += argument.ToString();
        }

        private void addWord()
        {
            if (_word.Length == 0)
				return;
			if (Command == null)
            	Command = _word;
			else
				Arguments.Add(_word);
        }

        private bool itTerminatesArgument(char argument)
        {
            return argumentIsTerminatedWithSpace(argument) ||
                   argumentIsTerminatedWithQuote(argument);
        }

        private bool isArgumentSeparator(char argument)
        {
            return (_word.Length == 0 && argument == ' ') ||
                   (_word.Length == 0 && argument == '"');
        }

        private bool argumentIsTerminatedWithSpace(char arguments)
        {
            return (arguments == ' ' && _separator == ' ');
        }
		
		private bool argumentIsTerminatedWithQuote(char arguments)
        {
            return (arguments == '"' && _separator == '"');
        }
	}
}

