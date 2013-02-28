using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using EditorEngine.Core.Editors;
using EditorEngine.Core.Endpoints;
using EditorEngine.Core.Messaging.Messages;
using EditorEngine.Core.Arguments;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace sublime
{
	public class SublimeEditor : IEditor
	{
		private Command _launchCommand = null;
		private int _port = 9998;
		private bool _initialized = false;
		public ICommandEndpoint Publisher { private get; set; }
		
		// Is the editor still running?
		public bool IsAlive { 
			get {
				try {
					if (!_initialized)
						return true;
					return request("ping") == "pong";
				} catch {
					Thread.Sleep(100);
					try {
						return request("ping") == "pong";
					} catch {
						return false;
					}
				}
			}
		}
		
		public void Initialize(Location location, string[] args) {
			openConfiguration();
			appendArguments(args);
			if (_launchCommand == null)
                return;
            var proc = runCommand(
                _launchCommand.Executable,
                _launchCommand.Parameter,
                true);
            var file = writeInvite(proc);
            System.Threading.ThreadPool
            	.QueueUserWorkItem(
            		(m) => {
            			var invite = m.ToString();
            			while (true) {
            				try {
            					var content = File.ReadAllText(invite);
            					if (content.Length > 0) {
            						_port = int.Parse(content);
            						break;
            					}
        					} catch {
        					}
        					System.Threading.Thread.Sleep(100);
            			}
            			_initialized = true;
            			System.Threading.Thread.Sleep(100);
            		}, file);
		}
		
		public void SetFocus() {
			if (_launchCommand == null)
                return;
            runCommand(
                _launchCommand.Executable,
                "",
                true);
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
			var modified = request("get-dirty-buffers")
					.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
			if (file != null) {
				if (!modified.Contains(file))
                    return new KeyValuePair<string,string>[] {};
                modified = new[] { file };
			}
			var buffers = new List<KeyValuePair<string, string>>();
			foreach (var fileName in modified) {
				var content = 
                    request("get-buffer-content \"" + fileName + "\"")
                        .Replace("||newline||", Environment.NewLine);
				buffers.Add(new KeyValuePair<string, string>(fileName, content));
			}
			return buffers.ToArray();
		}

		private string writeInvite(Process proc) {
			var existing = Process
				.GetProcesses()
				.FirstOrDefault(x => 
					x.ProcessName.Contains(_launchCommand.Executable) &&
					File.Exists(Path.Combine(Path.GetTempPath(), "sublime_invite." + x.Id.ToString())));
			if (existing != null)
				return Path.Combine(Path.GetTempPath(), "sublime_invite." + existing.Id.ToString());
			
			var file = Path.Combine(Path.GetTempPath(), "sublime_invite." + proc.Id.ToString());
			if (File.Exists(file))
				File.Delete(file);
			File.WriteAllText(file, "");
			return file;
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
			var ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
		    var server = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
		    server.Connect(ip);
		    return server;
		}

		private void close(Socket server) {
			server.Shutdown(SocketShutdown.Both);
			server.Close();
		}

        private void openConfiguration() {
            var file =
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "sublime.editor");
            var document = new XmlDocument();
            document.Load(file);
            var node = document.SelectSingleNode("editor/launch");
            if (node != null)
            {
                _launchCommand = new Command(
                    node.SelectSingleNode("executable").InnerText,
                    node.SelectSingleNode("parameters").InnerText);
            }
            node = document.SelectSingleNode("editor/server_port");
            if (node != null)
                _port = int.Parse(node.InnerText);
        }

		private void appendArguments(string[] arguments) {
			var args = ArgumentParser.Parse(arguments);
			var sublimeProject = args
				.Where(p => p.Key == "--editor.sublime.project")
	            .Select(p => p.Value)
	            .FirstOrDefault();
			if (sublimeProject != null) {
				if (Environment.OSVersion.Platform != PlatformID.MacOSX && Environment.OSVersion.Platform != PlatformID.Unix)
					sublimeProject = sublimeProject.Replace("/", "\\");
				if (!isRooted(sublimeProject))
						sublimeProject = Path.Combine(Environment.CurrentDirectory, sublimeProject);
				if (File.Exists(sublimeProject)) {
					_launchCommand.Parameter += " --add \"" + sublimeProject + "\"";
				}
			}
			if (isSublimeRunning())
				_launchCommand.Parameter += " -n";
		}

		private bool isSublimeRunning() {
			return Process
				.GetProcesses()
				.Any(x => x.ProcessName.Contains(_launchCommand.Executable));
		}

		private bool isRooted(string path) {
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
				return (File.Exists(path) && path.StartsWith("/")) || path.StartsWith("~");
			return Path.IsPathRooted(path);
		}

        private Process runCommand(string cmd, string parameters, bool visible) {
			if (cmd == "")
				return null; 
            var process = new Process();
            process.StartInfo = new ProcessStartInfo(cmd, parameters);
            process.StartInfo.CreateNoWindow = visible;
            process.StartInfo.UseShellExecute = visible;
            if (visible)
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            else
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            return process;
        }
	}

	class Command
    {
        public string Executable { get; private set; }
        public string Parameter { get; set; }

        public Command(string exe, string cmd)
        {
            Executable = exe;
            Parameter = cmd;
        }
    }
}
