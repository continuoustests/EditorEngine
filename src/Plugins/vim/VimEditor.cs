using System;
using System.Linq;
using EditorEngine.Core.Editors;
using System.Diagnostics;
using System.IO;
using EditorEngine.Core.Endpoints.Tcp;
using System.Threading;
using EditorEngine.Core.Endpoints;
using EditorEngine.Core.Messaging.Messages;
using System.Collections.Generic;
using System.Reflection;
namespace vim
{
	public class Buffer
	{
		public int ID { get; private set; }
		public string Fullpath { get; private set; }
		public bool Closed { get; private set; }
		
		public  Buffer(int id, string path)
		{
			ID = id;
			Fullpath = path;
			Closed = false;
		}
		
		public void Close()
		{
			Closed = true;
		}
		
		public void ReOpen()
		{
			Closed = false;
		}
	}
	
	class VIMLocation
	{
		public Buffer Buffer { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
		public int Offset { get; private set; }
		
		public VIMLocation(Buffer buffer, int line, int column, int offset)
		{
			Buffer = buffer;
			Line = line;
			Column = column;
			Offset = offset;
		}
	}
	
	public class VimEditor : IEditor
	{
		private Process _process = null;
		private ITcpServer _server = null;
		private List<Buffer> _buffers = new List<Buffer>();
		private int _correlationCounter = 1;
		private List<ReplyResult> _replys = new List<ReplyResult>();
		private bool _debug = false;
		private string _executable = null;
		private string _parameters = null;
		
		public ITcpServer Server
		{ 
			set
			{ 
				_server = value;
				_server.IncomingMessage += Handle_serverIncomingMessage;
				_server.ClientConnected += Handle_serverClientConnected;
			}
		}
		
		public List<Buffer> Buffers { get { return _buffers; } }
		
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
			if (_debug)
				Console.WriteLine("Server started and running on port {0}", _server.Port);
			_executable = getExecutable();
			_parameters = getParameters();
			if (_process != null)
				_process.Kill();
			_process = new Process();
			_process.StartInfo = new ProcessStartInfo(_executable, _parameters);
			_process.StartInfo.CreateNoWindow = true;
			_process.StartInfo.UseShellExecute = true;
			_process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			_process.Start();
			Thread.Sleep(500);
			GoTo(location);
		}
		
		private string getExecutable()
		{
			try
			{
				var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				return File.ReadAllText(Path.Combine(path, "vim.executable"))
					.Replace(Environment.NewLine, "");
			}
			catch
			{
				return "gvim";
			}
		}

		private string getParameters()
		{
			try
			{
				var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				return File.ReadAllText(Path.Combine(path, "vim.parameters"))
					.Replace("{0}", _server.Port.ToString())
					.Replace(Environment.NewLine, "");
			}
			catch
			{
				return string.Format("-nb:127.0.0.1:{0}:mypass -c \"map <F8> <F21>\"", _server.Port);
			}
		}

		void Handle_serverClientConnected (object sender, EventArgs e)
		{
		}

		void Handle_serverIncomingMessage(object sender, MessageArgs e)
		{
			if (_debug)
				Console.WriteLine("Recieving: " +  e.Message);
			if (handleReply(e.Message))
				return;
			if (_debug)
				Console.WriteLine("Recieving: " +  e.Message);
			if (getCommand(e.Message).StartsWith("keyAtPos=0 \"j\""))
				Publisher.Run("keypress ctrl+shift+j");
			else if (getCommand(e.Message).StartsWith("keyAtPos=0 \""))
				Publisher.Run("keypress " + getCommand(e.Message).Substring(12, getCommand(e.Message).IndexOf("\"", 12) - 12));
			else if (e.Message.StartsWith("0:fileOpened=0") && e.Message.Substring(e.Message.IndexOf("\""), e.Message.LastIndexOf("\"") - e.Message.IndexOf("\"")).Replace("\"", "").Trim() == "")
				Publisher.Run("keypress nobuffers");
			else if (e.Message.StartsWith("0:fileOpened=0"))
				applyBufferID(e.Message.Substring(e.Message.IndexOf("\""), e.Message.LastIndexOf("\"") - e.Message.IndexOf("\"")).Replace("\"", "").Trim(), true);
			else if (getCommand(e.Message).StartsWith("killed"))
				removeBuffer(getBuffer(e.Message));
		}

		public void SetFocus()
		{
			send("0:raise!0");
		}

		public void GoTo(Location location)
		{
			if (location == null)
				return;
			var id = getBufferID(location.File);
			if (id == -1)
				id = editFile(location.File);
			goTo(id, location);
		}

		private int editFile(string file)
		{
			var id = applyBufferID(file, false);
			send("{0}:editFile!0 \"{1}\"", id, file.Replace("\\", "\\\\"));
			return id;
		}

		private void goTo(int buffer, Location location)
		{
			send("{0}:setDot!0 {1}/{2}", buffer, location.Line, location.Column);
		}
	
		public void BeginBatchUpdate()
		{
			send("0:startAtomic!0");
		}

		public void EndBatchUpdate()
		{
			send("0:endAtomic!0");
		}

		public bool CanInsertFor(string file)
		{
			return true;
		}

		public void Insert(EditorInsertMessage message)
		{
			GoTo(new Location(
				message.Destination.File,
				message.Destination.Line,
				message.Destination.Column));
			var location = getLocation();
			if (location == null)
				return;
			var newline = "\\n";
			if (Environment.OSVersion.Platform != PlatformID.Unix &&
				Environment.OSVersion.Platform != PlatformID.MacOSX)
				newline = "\\r\\n";
			var content = runFunction("{0}:getText", location.Buffer.ID);
			if (content == null)
				return;
			var lines = content.Split(new[] { newline }, StringSplitOptions.None);
			if (lines.Length < location.Line)
			{
				if (_debug)
					Console.WriteLine("Asked for line {0} but document only contained {1} lines",
						location.Line,
						lines.Length);
				return;
			}
			var line = lines[location.Line - 1];
			if (line.Length < location.Column)
			{
				if (_debug)
					Console.WriteLine("Asked for column {0} but line was only {1} chars long",
						location.Column,
						line);
				return;
			}
			var lineModified =
				line.Substring(0, location.Column) +
				message.Text.Replace(Environment.NewLine, newline) +
				line.Substring(location.Column, line.Length - location.Column);
			var length = line.Length;
			var lastLine = location.Line != lines.Length - 1;
			if (lastLine)
				length += newline.Length;
			send("{0}:remove/0 {1} {2}",
				location.Buffer.ID,
				location.Offset - location.Column,
				length);
			send("{0}:insert/0 {1} \"{2}\"",
				location.Buffer.ID,
				location.Offset - location.Column,
				lineModified);
		}	

		public bool CanRemoveFor(string file)
		{
			return true;
		}

		public void Remove(EditorRemoveMessage message)
		{
			if (message.Start.Line > message.End.Line)
				return;
			GoTo(new Location(
				message.Start.File,
				message.Start.Line,
				message.Start.Column));
			var location = getLocation();
			if (message.Start.Line == message.End.Line)
			{
				if (message.Start.Column >= message.End.Column)
					return;
				send("{0}:remove/0 {1} {2}",
					location.Buffer.ID,
					location.Offset,
					message.End.Column - message.Start.Column);
				return;
			}

			var content = runFunction("{0}:getText", location.Buffer.ID);
			if (content == null)
				return;
			var newline = "\\n";
			if (Environment.OSVersion.Platform != PlatformID.Unix &&
				Environment.OSVersion.Platform != PlatformID.MacOSX)
				newline = "\\r\\n";
			var lines = content.Split(new[] { newline }, StringSplitOptions.None);
			for (int line = message.End.Line; line >= message.Start.Line; line--)
			{
				var column = 0;
				if (line == message.Start.Line)
					column = message.Start.Column;
				var length = lines[line - 1].Length - column + newline.Length;
				if (line == message.End.Line)
					length = message.End.Column;
				GoTo(new Location(
					message.Start.File,
					line,
					column));
				location = getLocation();
				if (location == null)
					return;
				send("{0}:remove/0 {1} {2}",
					location.Buffer.ID,
					location.Offset,
					length);
			}
		}

		public KeyValuePair<string,string>[] GetDirtyFiles()
		{
			var modifiedCount = getModified();
			if (modifiedCount == "0")
				return new KeyValuePair<string,string>[] {};
			return _buffers
				.Where(x => !x.Closed && getModified(x.ID) == "1")
				.Select(x => new KeyValuePair<string,string>(x.Fullpath, getText(x.ID)))
				.ToArray();
		}

		private string getModified()
		{
			return getModified(0);
		}

		private string getModified(int bufferID)
		{
			var reply = runFunction("{0}:getModified", bufferID);
			if (reply == null)
				return "";
			return reply
				.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1];
		}

		private string getText()
		{
			return getText(0);
		}

		private string getText(int bufferID)
		{
			var reply = runFunction("{0}:getText", bufferID);
			if (reply == null)
				return "";
			var start = reply.IndexOf(" ");
			if (start == -1)
				return "";
			start += 1;
			if (start == reply.Length)
				return "";
			var newline = "\\n";
			if (Environment.OSVersion.Platform != PlatformID.Unix &&
				Environment.OSVersion.Platform != PlatformID.MacOSX)
				newline = "\\r\\n";
			return reply
				.Substring(start, reply.Length - start)
				.Replace(newline, Environment.NewLine);
		}

		private VIMLocation getLocation()
		{
			return getLocation(0);
		}

		private VIMLocation getLocation(int bufferID)
		{
			if (_debug)
				Console.WriteLine("Getting location");
			var message = runFunction("{0}:getCursor", bufferID);
			if (_debug)
				Console.WriteLine("Function returned " + message);
			if (message == null)
				return null;
			 var chunks = message.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
			try
			{
				var buffer = _buffers.Where(x => x.ID.Equals(int.Parse(chunks[1]))).FirstOrDefault();
				if (buffer == null)
					return null;
				return new VIMLocation(buffer, int.Parse(chunks[2]), int.Parse(chunks[3]), int.Parse(chunks[4]));
			}
			catch (Exception ex)
			{
				if (_debug)
					Console.WriteLine(ex.ToString());
				return null;
			}
		}
		
		private void send(string message)
		{
			send(message, new object[] {});
		}
		
		private void send(string message, params object[] args)
		{
			if (_debug)
				Console.WriteLine("Sending " + string.Format(message, args));
			_server.Send(string.Format(message, args));
		}
			
		private string getCommand(string command)
		{
			return command.Substring(command.IndexOf(':') + 1, command.Length - (command.IndexOf(':') + 1));
		}
		
		private int getBuffer(string command)
		{
			int id;
			if (int.TryParse(command.Substring(0, command.IndexOf(':')), out id))
				return id;
			return -1;
		}
		
		private void removeBuffer(int id)
		{
			var buffer = _buffers.FirstOrDefault(x => x.ID.Equals(id));
			if (buffer == null)
				return;
			buffer.Close();
		}

		private int getBufferID(string path)
		{
			if (path == "")
				return -1;
			var buffer = _buffers.FirstOrDefault(x => x.Fullpath.Equals(path) && !x.Closed);
			if (buffer == null)
				return -1;
			return buffer.ID;
		}
		
		private int applyBufferID(string path, bool setBufferNumber)
		{
			if (path == "")
				return -1;
			bool exists = false;
			int id = 1;
			if (_buffers.Count > 0)
			{
				if (_buffers.Exists(x => x.Fullpath.Equals(path)))
				{
					var buffer = _buffers.First(x => x.Fullpath.Equals(path));
					setBufferNumber = setBufferNumber || buffer.Closed == true;
					id = buffer.ID;
					buffer.ReOpen();
					exists = true;
				}
				else
					id = getFirstAvailableBuffer();
			}
			if (setBufferNumber)
				send("{0}:setBufferNumber!0 \"{1}\"", id, path);
			if (!exists)
				_buffers.Add(new Buffer(id, path));
			return id;
		}
		
		private int getFirstAvailableBuffer()
		{
			/*var buffers = _buffers.OrderBy(x => x.ID).ToList();
			for (int i = 0; i < buffers.Count; i++)
				if (buffers[i].ID != i+1)
					return i + 1;*/
			return _buffers.Max(x => x.ID) + 1;
		}
		
		private string runFunction(string function)
		{
			return runFunction(function, new object[] {});
		}

		private string runFunction(string function, params object[] args)
		{
			function = string.Format(function, args);
			ReplyResult reply;
			lock (_replys)
			{
				_correlationCounter++;
				reply = new ReplyResult(_correlationCounter, (message) => { return int.Parse(message.Substring(0, message.IndexOf(" "))); });
				_replys.Add(reply);
			}
			send(function + "/" + reply.CorrelationID.ToString());
			var timeout = DateTime.Now.AddSeconds(10);
			while (timeout > DateTime.Now && reply.Reply == null)
				Thread.Sleep(50);
			lock (_replys)
			{
				_replys.Remove(reply);
			}
			return reply.Reply;
		}
		
		private bool handleReply(string message)
		{
			lock (_replys)
			{
				if (_replys.Count == 0)
					return false;
				var reply = _replys.Where(x => x.AnsweredBy(message)).FirstOrDefault();
				if (reply == null)
					return false;
				reply.SetReply(message);
				return true;
			}
		}
	}
	
	class ReplyResult
	{
		private Func<string,int> _replyParser;
		
		public int CorrelationID { get; private set; }
		public string Reply { get; private set; }
		
		public ReplyResult(int correlationID, Func<string,int> correlationFromReply)
		{
			CorrelationID = correlationID;
			Reply = null;
			_replyParser = correlationFromReply;
		}
		
		public bool AnsweredBy(string message)
		{
			try
			{
				return _replyParser.Invoke(message).Equals(CorrelationID);
			}
			catch
			{
				return false;
			}
		}
		
		public void SetReply(string reply)
		{
			Reply = reply;
		}
	}
}

