using System;
namespace EditorEngine.Core.Endpoints.Tcp
{
	public class MessageArgs : EventArgs
	{
		public Guid ClientID { get; private set; }
		public string Message { get; private set; }
		
		public MessageArgs(Guid clientID, string message)
		{
			ClientID = clientID;
			Message = message;
		}
	}
	
	public interface ITcpServer
	{
		event EventHandler ClientConnected;
		event EventHandler<MessageArgs> IncomingMessage;
		
		int Port { get; }
		int ConnectedClients { get; }
		
		void Start();
		void Start(int port);
		void Send(string message);
		void Send(string message, Guid clientID);
	}
}

