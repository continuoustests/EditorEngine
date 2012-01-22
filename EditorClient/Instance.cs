using System;
using System.IO;
namespace EditorClient
{
	public class Instance
	{
		public string File { get; private set; }
		public int ProcessID { get; private set; }
		public string Key { get; private set; }
		public int Port { get; private set; }
		
		public Instance(string file, int processID, string key, int port)
		{
			File = file;
			ProcessID = processID;
			Key = key;
			Port = port;
		}
		
		public static Instance Get(string file, string[] lines)
		{
			if (lines.Length != 2)
				return null;
			int processID;
			if (!int.TryParse(Path.GetFileNameWithoutExtension(file), out processID))
				return null;
			int port;
			if (!int.TryParse(lines[1], out port))
				return null;
			return new Instance(file, processID, lines[0], port);
		}
		
		public void Send(string message)
		{
			var client = new Client();
			client.Connect(Port);
			if (!client.IsConnected)
				return;
			client.SendAndWait(message);
			client.Disconnect();
		}

		public void Request(string message)
		{
			var client = new Client();
			client.Connect(Port);
			if (!client.IsConnected)
				return;
			client.SendAndWait(message);

			var then = DateTime.Now.AddSeconds(20);
			while (then > DateTime.Now)
			{
				if (client.RecievedMessage != null)
					break;
			}
			
			Console.WriteLine(client.RecievedMessage);
			client.Disconnect();
		}
	}
}

