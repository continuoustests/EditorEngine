using System;
using System.Linq;
using System.Text;
using EditorEngine.Core.Editors;
using System.Diagnostics;
using System.IO;
using EditorEngine.Core.Endpoints.Tcp;
using System.Threading;
using EditorEngine.Core.CommandBuilding;
using EditorEngine.Core.Endpoints;
using EditorEngine;
using EditorEngine.Core.Messaging.Messages;
using System.Collections.Generic;
using System.Reflection;
using EditorEngine.Core.Arguments;
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
		private bool _isInitialized = false;
		private ITcpServer _server = null;
		private List<Buffer> _buffers = new List<Buffer>();
		private int _correlationCounter = 1;
		private List<ReplyResult> _replys = new List<ReplyResult>();
		private string _executable = null;
		private string _parameters = null;
		private int _functrionTimeout = 2000;
		private Queue<string> _modifications = new Queue<string>();
		private bool _isHeadless = false;
		private bool _vimSaidExitNow = false;
		
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

		public VimEditor TimeoutAfter(int milliseconds)
		{
			_functrionTimeout = milliseconds;
			return this;
		}
		
		public bool IsAlive
		{
			get
			{
				if (!_isHeadless) {
					if (!_isInitialized || _process == null)
						return true;
					return !_process.HasExited;
				}

				if (_vimSaidExitNow)
					return false;
				if (!_isInitialized && _server.ConnectedClients > 0) {
					_isInitialized = true;
				}
				if (!_isInitialized || _server.ConnectedClients > 0) {
					return true;
				}
				return false;
			}
		}
		
		public void Initialize(Location location, string[] args)
		{
			var port = getPort(args);
			_isHeadless = getIsHeadless(args);
			_server = null;
			_server = new TcpServer(Environment.NewLine);
			_server.IncomingMessage += Handle_serverIncomingMessage;
			_server.ClientConnected += Handle_serverClientConnected;
			_server.Start(port);
			Logger.Write("Server started and running on port {0}", _server.Port);
			if (!_isHeadless) {
				_executable = getExecutable(args);
				_parameters = getParameters();
				if (_process != null)
					_process.Kill();
				_process = new Process();
				_process.StartInfo = new ProcessStartInfo(_executable, _parameters);
				_process.StartInfo.CreateNoWindow = true;
				_process.StartInfo.UseShellExecute = true;
				_process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				_process.Start();
				listenForModifications();
				Thread.Sleep(500);
				_isInitialized = true;
				GoTo(location);
			}
		}

		private int getPort(string[] arguments)
		{
			try
			{
				var args = ArgumentParser.Parse(arguments);
				var port = args
					.Where(x => x.Key == "--editor.vim.port")
					.Select(x => x.Value)
					.FirstOrDefault();
				if (port != null)
					return int.Parse(port);
				return 0;
			}
			catch
			{
				return 0;
			}
		}

		private bool getIsHeadless(string[] arguments)
		{
			try
			{
				var args = ArgumentParser.Parse(arguments);
				var isHeadless = args
					.Where(x => x.Key == "--editor.vim.headless")
					.Select(x => x.Value)
					.FirstOrDefault();
				if (isHeadless != null)
					return isHeadless.ToLower() == "true";
				return false;
			}
			catch
			{
				return false;
			}
		}

		private void listenForModifications()
		{
			var timer = new System.Timers.Timer(10000);
			timer.Elapsed += (sender, e) => handleModifications();
			timer.Interval = 100;
			timer.Enabled = true;
		}
		
		private string getExecutable(string[] arguments)
		{
			try
			{
				var args = ArgumentParser.Parse(arguments);
				var executable = args
					.Where(x => x.Key == "--editor.vim.executable")
					.Select(x => x.Value)
					.FirstOrDefault();
				if (executable != null)
					return executable;
				
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
			Logger.Write("Recieving: " +  e.Message);
			if (handleReply(e.Message))
				return;
			if (e.Message == "0:disconnect=0")
				_vimSaidExitNow = true;
			else if (getCommand(e.Message).StartsWith("keyAtPos=0 \"snippet-complete\""))
				ThreadPool.QueueUserWorkItem(completeSnippet);
			else if (getCommand(e.Message).StartsWith("keyAtPos=0 \""))
				Publisher.Run(getCommand(e.Message)
					.Substring(12, getCommand(e.Message).LastIndexOf("\"") - 12));
			else if (e.Message.StartsWith("0:fileOpened=0") &&
					 e.Message.Substring(
					 	e.Message.IndexOf("\""), e.Message.LastIndexOf("\"") - e.Message.IndexOf("\""))
					 	.Replace("\"", "").Trim() == "")
				Publisher.Run("gototype");
			else if (e.Message.StartsWith("0:fileOpened=0"))
				applyBufferID(
					e.Message.Substring(
						e.Message.IndexOf("\""),
						e.Message.LastIndexOf("\"") - e.Message.IndexOf("\"")).Replace("\"", "")
						.Trim(), true);
			else if (getCommand(e.Message).StartsWith("killed"))
				removeBuffer(getBuffer(e.Message));
			else if (getCommand(e.Message).StartsWith("insert="))
				_modifications.Enqueue(e.Message);
			else if (getCommand(e.Message).StartsWith("remove="))
				_modifications.Enqueue(e.Message);
		}

		public void SetFocus()
		{
			send("0:raise!0");
			// This is so dirty. Look away, I feel so ashamed....
			if (!_isHeadless && Environment.OSVersion.Platform == PlatformID.Unix) {
				Process.Start("oi", "process set-to-foreground process \"" + _process.Id.ToString() + "\"");
			}
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
			send("{0}:setDot!0 {1}/{2}", buffer, location.Line, location.Column - 1);
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
			var origin = getLocation();
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
			var content = getText(location.Buffer.ID);
			if (content == null)
				return;
			var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			if (lines.Length < location.Line)
			{
				Logger.Write("Asked for line {0} but document only contained {1} lines",
					location.Line,
					lines.Length);
				return;
			}
			var line = lines[location.Line - 1];
			if (line.Length < location.Column)
			{
				Logger.Write("Asked for column {0} but line was only {1} chars long",
					location.Column,
					line);
				return;
			}

			Logger.Write("Line is: " + line);
			var removeLength = line.Length - message.Destination.Column + 1;
			if (removeLength > line.Length)
				removeLength = line.Length;
			var insertColumn = location.Column; // + ((message.Destination.Column - 1) - location.Column);
			var textModified =
				message.Text
					.Replace("\\", "\\\\")
					.Replace(Environment.NewLine, newline) +
				line.Substring(insertColumn, removeLength);
			//send("{0}:remove/0 {1} {2}",
			//	location.Buffer.ID,
			//	location.Offset - location.Column,
			//	length - 1);
			//send("{0}:insert/0 {1} \"{2}\"",
			//	location.Buffer.ID,
			//	location.Offset - location.Column,
			//	lineModified);
			send("{0}:remove/0 {1} {2}",
				location.Buffer.ID,
				location.Offset,
				removeLength);
			send("{0}:insert/0 {1} \"{2}\"",
				location.Buffer.ID,
				location.Offset,
				textModified);
			if (message.MoveOffset != null)
			{
				var offsetLocation = new Location(
					message.Destination.File,
					message.Destination.Line,
					message.Destination.Column);
				offsetLocation.Add(message.MoveOffset);
				GoTo(offsetLocation);
			}
			else
			{
				if (origin == null)
					return;
				var originAdjusted = origin.Line;
				if (origin.Line > message.Destination.Line) {
					originAdjusted = 
						origin.Line + 
						message.Text.Split(new[] {Environment.NewLine}, StringSplitOptions.None).Length;
				}
				GoTo(new Location(
					origin.Buffer.Fullpath,
					origin.Line,
					origin.Column));
			}
		}	

		public bool CanRemoveFor(string file)
		{
			return true;
		}

		public void Remove(EditorRemoveMessage message)
		{
			Logger.Write("Removing chunk for " + message.Start.File);
			if (message.Start.Line > message.End.Line)
				return;
			GoTo(new Location(
				message.Start.File,
				message.Start.Line,
				message.Start.Column));
			var location = getLocation();
			if (location == null)
				return;
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

			var content = getText(location.Buffer.ID);
			if (content == null)
				return;
			var newline = "\\n";
			if (Environment.OSVersion.Platform != PlatformID.Unix &&
				Environment.OSVersion.Platform != PlatformID.MacOSX)
				newline = "\\r\\n";
			var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			for (int line = message.End.Line; line >= message.Start.Line; line--)
			{
				var column = 1;
				if (line == message.Start.Line)
					column = message.Start.Column;
				var length = lines[line - 1].Length - column + newline.Length;
				if (line == message.End.Line)
					length = message.End.Column - 1;
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

		public KeyValuePair<string,string>[] GetDirtyFiles(string file)
		{
			var modifiedCount = getModified();
			if (modifiedCount == "0")
				return new KeyValuePair<string,string>[] {};
			return _buffers
				.Where(x => !x.Closed && getModified(x.ID) == "1" && (file == null || file.Equals(x.Fullpath)))
				.Select(x => new KeyValuePair<string,string>(x.Fullpath, getText(x.ID)))
				.ToArray();
		}

		public void RunCommand(string[] args)
        {
        }

        public Caret GetCaret()
        {
        	Logger.Write("Collection information for GetCaret");
        	var location = getLocation();
        	if (location == null)
        		return new Caret("", new Position(0, 0), "");
        	var content = getText(location.Buffer.ID);
			if (content == null)
				return new Caret("", new Position(0, 0), "");
			var buffer = _buffers
				.FirstOrDefault(x => x.ID == location.Buffer.ID);
			if (buffer == null)
				return new Caret("", new Position(0, 0), "");
			return new Caret(buffer.Fullpath, new Position(location.Line, location.Column+1), content);
        }

        public void RequestUserSelection(string identifier, string[] items)
        {
            var itemList = "";
            foreach (var item in items) {
                if (itemList != "")
                    itemList += ",";
                itemList += item;
            }
        	Publisher.Run("user-select unsupported \"" + identifier + "\" \"" + itemList + "\"");	
        }

        public void RequestUserInput(string identifier, string defaultValue)
        {
            Publisher.Run("user-input unsupported \"" + identifier + "\" \"" + defaultValue + "\"");   
        }
        
		private void handleModifications()
		{
			var modifications = new List<string>();
			while (_modifications.Count != 0)
			{
				var message = _modifications.Dequeue();
				var bufferID = getBuffer(message);
				var buffer = _buffers.FirstOrDefault(x => x.ID.Equals(bufferID));
				if (buffer == null)
					continue;
				if (!modifications.Contains(buffer.Fullpath))
					modifications.Add(buffer.Fullpath);
			}
			modifications
				.ForEach(x =>
					Publisher.Run(
						string.Format("editor buffer-changed \"{0}\"",
							x)));
		}

		private void completeSnippet(object status)
		{
			var location = getLocation();
			var content = getText(location.Buffer.ID);
			if (content == null)
				return;
			var newline = "\\n";
			if (Environment.OSVersion.Platform != PlatformID.Unix &&
				Environment.OSVersion.Platform != PlatformID.MacOSX)
				newline = "\\r\\n";
			var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			var line = lines[location.Line - 1];
			var insertColumn = location.Column + 1; // Add one since command mode jumps one back
			var word = Word.Extract(line, insertColumn); 
			Remove(
				new EditorRemoveMessage(
					new EditorEngine.Core.Arguments.GoTo()
						{
							File = location.Buffer.Fullpath,
							Line = location.Line,
							Column = word.Column
						},
					new EditorEngine.Core.Arguments.GoTo()
						{
							File = location.Buffer.Fullpath,
							Line = location.Line,
							Column = word.Column + word.Content.Length
						}));
			var whitespaces = getWhitespacePrefix(line);
			var snippetStartColumn = word.Column;
			var message = 
				string.Format("snippet-complete \"{0}\" \"{1}\" \"{2}|{3}|{4}\" \"{5}\"",
					Path.GetExtension(location.Buffer.Fullpath),
					word.Content,
					location.Buffer.Fullpath,
					location.Line,
					snippetStartColumn,
					whitespaces);
			Logger.Write(message);
			Publisher.Run(message);
		}

		private string getWhitespacePrefix(string line)
		{
			var sb = new StringBuilder();
			foreach (var chr in line)
			{
				if (chr == ' ')
					sb.Append("s");
				else if (chr == '\t')
					sb.Append("t");
				else
					break;
			}
			return sb.ToString();
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
			start += 2;
			if (start == reply.Length)
				return "";
			var newline = "\\n";
			if (Environment.OSVersion.Platform != PlatformID.Unix &&
				Environment.OSVersion.Platform != PlatformID.MacOSX)
				newline = "\\r\\n";
			return reply
				.Substring(start, reply.Length - start - 1)
				.Replace(newline, Environment.NewLine)
				.Replace("\\\"", "\"")
				.Replace("\\\\", "\\");
		}

		private VIMLocation getLocation()
		{
			return getLocation(-1);
		}

		private VIMLocation getLocation(int bufferID)
		{
			Logger.Write("Getting location");
			var message = runFunction("{0}:getCursor", bufferID);
			Logger.Write("Function returned " + message);
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
				Logger.Write(ex.ToString());
				return null;
			}
		}
		
		private void send(string message)
		{
			send(message, new object[] {});
		}
		
		private void send(string message, params object[] args)
		{
			Logger.Write("Sending " + string.Format(message, args));
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
			Logger.Write("Running function " + function);
			ReplyResult reply;
			lock (_replys)
			{
				_correlationCounter++;
				reply = new ReplyResult(_correlationCounter, (message) => { return int.Parse(message.Substring(0, message.IndexOf(" "))); });
				_replys.Add(reply);
			}
			send(function + "/" + reply.CorrelationID.ToString());
			var timeout = DateTime.Now.AddMilliseconds(_functrionTimeout);
			while (timeout > DateTime.Now && reply.Reply == null)
				Thread.Sleep(50);
			lock (_replys)
			{
				_replys.Remove(reply);
			}
			Logger.Write("Function ({0}) timed out", function);
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

	public class Word
	{
		public static Word Extract(string line, int position)
		{
			return new Word(line, position);
		}

		private string _line;
		private int _position;
		
		public string Content { get; private set; }
		public int Column { get; private set; }

		public Word(string line, int position)
		{
			_line = line;
			if (position != 0)
				_position = position - 1;
			var start = getStart();
			var end = getEnd(start);
			Content = _line.Substring(start, end - start);
			Column = start + 1;
            if (start == position)
                Column = start;
		}

		private int getStart()
		{
			var separators = new List<int>();
			separators.Add(_line.LastIndexOf(" ", _position));
			separators.Add(_line.LastIndexOf("\t", _position));
			if (separators.Max(x => x) == -1)
				return 0;
			return separators.Max(x => x) + 1;
		}

		private int getEnd(int after)
		{
			var separators = new List<int>();
			separators.Add(_line.IndexOf(" ", after));
			separators.Add(_line.IndexOf("\t", after));
			if (separators.Max(x => x) == -1)
				return _line.Length;
			return separators.Max(x => x);
		}
	}
}

