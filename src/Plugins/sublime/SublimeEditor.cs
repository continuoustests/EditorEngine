using System;
using System.Collections.Generic;
using EditorEngine.Core.Editors;
using EditorEngine.Core.Endpoints;
using EditorEngine.Core.Messaging.Messages;

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace sublime
{
	public class SublimeEditor : IEditor
	{
		public ICommandEndpoint Publisher { private get; set; }
		
		// Is the editor still running?
		public bool IsAlive { 
			get {
				try {
					return request("ping") == "pong";
				} catch {
					return false;
				}
			}
		}
		
		public void Initialize(Location location) {
			if (location != null)
				GoTo(location);
		}
		
		public void SetFocus() {
		}
		
		public void GoTo(Location location) {
			var msg = string.Format(
				"goto \"{0}\" {1} {2}",
				location.File,
				location.Line,
				location.Column);
			send(msg);
		}

		public void BeginBatchUpdate() {
		}
		
		public void EndBatchUpdate() {
		}

		public bool CanInsertFor(string file) {
			Console.WriteLine(file);
			return true;
		}
		
		public void Insert(EditorInsertMessage msg) {
			var message = string.Format(
				"insert \"{0}\" \"{1}\" {2} {3} {4}",
				msg.Text,
				msg.Destination.File,
				msg.Destination.Line,
				msg.Destination.Column,
				msg.MoveOffset);
			send(message);
		}

		public bool CanRemoveFor(string file) {
			return true;
		}
		
		public void Remove(EditorRemoveMessage msg) {
			var message = string.Format(
				"remove \"{0}\" {1} {2} {3} {4}",
				msg.Start.File,
				msg.Start.Line,
				msg.Start.Column,
				msg.End.Line,
				msg.End.Column);
			Console.WriteLine(message);
			send(message);
		}

		public KeyValuePair<string,string>[] GetDirtyFiles(string file) {
			throw new NotImplementedException();
		}

		private void send(string msg) {
			var server = connect();
	        sendMessage(server, msg + "\n");
	        close(server);
		}

		private string request(string msg) {
	        var server = connect();
	        sendMessage(server, msg + "\n");
	        var respons = readMessage(server);
	        close(server);
	        return respons;
		}

		private void sendMessage(Socket server, string msg) {
			server.Send(Encoding.UTF8.GetBytes(msg));
		}

		private string readMessage(Socket server) {
			var data = new byte[5120000]; // 5Mb
	        var receivedDataLength = server.Receive(data);
	        return Encoding.UTF8
	        	.GetString(data, 0, receivedDataLength)
	        	.Replace("\n", "");
		}

		private Socket connect() {
			var ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9998);
		    var server = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
		    server.Connect(ip);
		    return server;
		}

		private void close(Socket server) {
			server.Shutdown(SocketShutdown.Both);
			server.Close();
		}
	}
}
