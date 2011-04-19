using System;
using NUnit.Framework;
using System.Threading;
using EditorEngine.Core.Tests.Messaging;
using EditorEngine.Core.Endpoints.Tcp;
namespace EditorEngine.Core.Tests.Endpoints.Tcp
{
	[TestFixture]
	public class TcpServerTests
	{
		private ServerWrapper _server;
		private Client _client;
		
		[SetUp]
		public void Setup()
		{
			_server = new ServerWrapper();
			_client = new Client();
			_server.Start();
			_client.Connect(_server.Port);
			Wait.ForOneSecond().OrUntil(() => { return _server.ClientConnected; });
		}
		
		[Test]
		public void Should_send_message()
		{
			_server.Send("Some message");
			Wait.ForOneSecond().OrUntil(() => { return _client.RecievedMessage != null; });
			Assert.That(_client.RecievedMessage, Is.EqualTo("Some message"));
		}
		
		[Test]
		public void Should_recieve_message()
		{
			_client.Send("A message");
			Wait.ForOneSecond().OrUntil(() => { return _server.RecievedMessage != null; });
			Assert.That(_server.RecievedMessage, Is.EqualTo("A message"));
		}
	}
	
	class ServerWrapper
	{
		private TcpServer _server;
		
		public int Port { get { return _server.Port; } }
		
		public bool ClientConnected { get; private set; }
		public string RecievedMessage { get; private set; }
		
		public ServerWrapper()
		{
			RecievedMessage = null;
			_server = new TcpServer();
			_server.ClientConnected += Handle_serverClientConnected;
			_server.IncomingMessage += Handle_serverIncomingMessage;
		}

		void Handle_serverIncomingMessage (object sender, MessageArgs e)
		{
			RecievedMessage = e.Message;
		}

		void Handle_serverClientConnected (object sender, EventArgs e)
		{
			ClientConnected = true;
		}
		
		public void Start()
		{
			_server.Start();
		}
		
		public void Send(string message)
		{
			_server.Send(message);
		}
	}
}

