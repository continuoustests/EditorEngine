using System;
using System.ServiceModel;
using EditorEngine.Core.Endpoints.Tcp;
using EditorEngine.Core.Messaging;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Logging;
using System.Text;
using System.IO;
using System.Diagnostics;
namespace EditorEngine.Core.Endpoints
{
	class CommandEndpoint : ICommandEndpoint, IService
	{
		private TcpServer _server;
		private IMessageDispatcher _dispatcher;
		private string _instanceFile;
		
		public CommandEndpoint(IMessageDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
			_server = new TcpServer();
			_server.IncomingMessage += Handle_serverIncomingMessage;
		}

		void Handle_serverIncomingMessage (object sender, MessageArgs e)
		{
			Logger.Write("Handling: " + e.Message);
			var command = new CommandMessage(e.ClientID, e.Message.Trim());
			_dispatcher.Publish(command);
			if (!e.Message.Trim().StartsWith("correlationID="))
				_server.Send(e.Message); // Pass on to all consuming clients
		}
		
		public void Run(string cmd)
		{
			Logger.Write("Publishing: " + cmd);
			_server.Send(cmd);
		}
		
		public void Run(Guid clientID, string cmd)
		{
			Logger.Write("Publishing: " + cmd);
			_server.Send(cmd, clientID);
		}
		
		public void Start(string key)
		{
			_server.Start();
			writeInstanceInfo(key);
		}
		
		public void Stop()
		{
			_server.Send("shutdown");
			if (File.Exists(_instanceFile))
				File.Delete(_instanceFile);
		}
		
		private void writeInstanceInfo(string key)
		{
			var path = Path.Combine(Path.GetTempPath(), "EditorEngine");
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			_instanceFile = Path.Combine(path, string.Format("{0}.pid", Process.GetCurrentProcess().Id));
			var sb = new StringBuilder();
			sb.AppendLine(key);
			sb.AppendLine(_server.Port.ToString());
			File.WriteAllText(_instanceFile, sb.ToString());
		}
	}
}

