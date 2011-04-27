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
			_server = new ServerWrapper(null);
			_client = new Client();
			_server.Start();
			_client.Connect(_server.Port);
			Wait.ForTwoSecond().OrUntil(() => { return _server.ClientConnected; });
		}
		
		[Test]
		public void Should_send_message()
		{
			_server.Send("Some message");
			Wait.ForTwoSecond().OrUntil(() => { return _client.RecievedMessage != null; });
			Assert.That(_client.RecievedMessage, Is.EqualTo("Some message"));
		}
		
		[Test]
		public void Should_recieve_message()
		{
			_client.Send("A message");
			Wait.ForTwoSecond().OrUntil(() => { return _server.RecievedMessage != null; });
			Assert.That(_server.RecievedMessage, Is.EqualTo("A message"));
		}
	}
	
	[TestFixture]
	public class TcpServerMessageTerminationTests
	{
		private ServerWrapper _server;
		private Client _client;
		
		[SetUp]
		public void Setup()
		{
			_server = new ServerWrapper("end");
			_client = new Client("end");
			_server.Start();
			_client.Connect(_server.Port);
			Wait.ForTwoSecond().OrUntil(() => { return _server.ClientConnected; });
		}
		
		[Test]
		public void Should_send_message()
		{
			_server.Send("Some message");
			Wait.ForTwoSecond().OrUntil(() => { return _client.RecievedMessage != null; });
			Assert.That(_client.RecievedMessage, Is.EqualTo("Some message"));
		}
		
		[Test]
		public void Should_recieve_message()
		{
			_client.Send("A message");
			Wait.ForTwoSecond().OrUntil(() => { return _server.RecievedMessage != null; });
			Assert.That(_server.RecievedMessage, Is.EqualTo("A message"));
		}
	}
	
	class ServerWrapper
	{
		private TcpServer _server;
		
		public int Port { get { return _server.Port; } }
		
		public bool ClientConnected { get; private set; }
		public string RecievedMessage { get; private set; }
		
		public ServerWrapper(string terminateString)
		{
			RecievedMessage = null;
			if (terminateString == null)
				_server = new TcpServer();
			else
				_server = new TcpServer(terminateString);
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

