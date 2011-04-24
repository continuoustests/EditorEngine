using System;
using EditorEngine.Core.Editors;
using System.Diagnostics;
using System.IO;
using EditorEngine.Core.Endpoints.Tcp;
using System.Threading;
using EditorEngine.Core.Endpoints;
namespace vim
{
	public class VimEditor : IEditor
	{
		private Process _process = null;
		private TcpServer _server = null;
		
		public ICommandEndpoint Publisher { private get; set; }
		
		public bool IsAlive
		{
			get
			{
				if (_process == null)
					return true;
				return !_process.HasExited;
			}
		}
		
		public void Initialize(Location location)
		{
			_server = null;
			_server = new TcpServer(Environment.NewLine);
			_server.IncomingMessage += Handle_serverIncomingMessage;
			_server.ClientConnected += Handle_serverClientConnected;
			_server.Start();
			Console.WriteLine("Server started and running on port {0}", _server.Port);
			var argument = string.Format("-nb:127.0.0.1:{0}:mypass -c \"map <F8> <F21>\"", _server.Port);
			if (_process != null)
				_process.Kill();
			_process = new Process();
			_process.StartInfo = new ProcessStartInfo("gvim", argument);
			_process.StartInfo.CreateNoWindow = true;
			_process.StartInfo.UseShellExecute = true;
			_process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			_process.Start();
		}

		void Handle_serverClientConnected (object sender, EventArgs e)
		{
		}

		void Handle_serverIncomingMessage(object sender, MessageArgs e)
		{
			if (e.Message.StartsWith("1:keyAtPos=0 \"j\""))
				Publisher.Run("keypress ctrl+shift+j");
			Console.WriteLine(e.Message);
		}

		public void SetFocus()
		{
			send("0:raise!0");
		}

		public void GoTo(Location location)
		{
			send("1:editFile!0 \"{0}\"", location.File);
			send("1:setDot {0}/{1}!0", location.Line, location.Column);
		}
		
		private void send(string message)
		{
			send(message, new object[] {});
		}
		
		private void send(string message, params object[] args)
		{
			Console.WriteLine("Sending " + string.Format(message, args));
			_server.Send(string.Format(message, args));
		}
	}
}

