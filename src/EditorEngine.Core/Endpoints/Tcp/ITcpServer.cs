using System;
namespace EditorEngine.Core.Endpoints.Tcp
{
	class MessageArgs : EventArgs
	{
		public string Message { get; private set; }
		
		public MessageArgs(string message)
		{
			Message = message;
		}
	}
	
	interface ITcpServer
	{
		event EventHandler ClientConnected;
		event EventHandler<MessageArgs> IncomingMessage;
		
		int Port { get; }
		
		void Start();
		void Send(string message);
	}
}

