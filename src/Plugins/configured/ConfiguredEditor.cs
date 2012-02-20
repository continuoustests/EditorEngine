using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EditorEngine.Core.Editors;
using EditorEngine.Core.Endpoints;
using EditorEngine.Core.Messaging.Messages;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Threading;

namespace configured
{
    public class ConfiguredEditor : IEditor
    {
		private string _editorName;
        private DateTime _isAliveTimeout = DateTime.Now;
        private bool _isInitialized = false;
        private AliveCommand _aliveCommand = null;
        private Command _launchCommand = null;
        private Command _goToCommand = null;
        private Command _setFocusCommand = null;

        public ICommandEndpoint Publisher { private get; set; }

        public bool IsAlive
        {
            get
            {
                if (_isInitialized == false)
                    return true;
                if (_aliveCommand == null)
                    return true;
                if (DateTime.Now < _isAliveTimeout)
                    return true;
                _isAliveTimeout = DateTime.Now.AddSeconds(10);
                var result = queryCommand(
                    _aliveCommand.Executable,
                    _aliveCommand.Parameter,
                    false);
                return result.Contains(_aliveCommand.CheckText);
            }
        }

		public ConfiguredEditor()
		{
			_editorName = Path.GetFileNameWithoutExtension(typeof(ConfiguredEditor).Assembly.Location);
		}

        public void Initialize(Location location)
        {
            tryOpenConfiguration();
            if (_launchCommand == null)
                return;
            runCommand(
                _launchCommand.Executable,
                _launchCommand.Parameter,
                true);
            Thread.Sleep(500);
            if (location != null)
                GoTo(location);
            _isInitialized = true;
        }

        public void SetFocus()
        {
            if (_setFocusCommand == null)
                return;
            runCommand(
                _setFocusCommand.Executable,
                _setFocusCommand.Parameter,
                false);
        }

        public void GoTo(Location location)
        {
            if (_goToCommand == null)
                return;
            runCommand(
                _goToCommand.Executable,
                _goToCommand.Parameter
                    .Replace("{0}", location.File)
                    .Replace("{1}", location.Line.ToString())
                    .Replace("{2}", location.Column.ToString()),
                false);
            SetFocus();
        }

		public void BeginBatchUpdate() {}
		public void EndBatchUpdate() {}

		public bool CanInsertFor(string file)
		{
			return false;
		}

		public void Insert(EditorInsertMessage message)
		{
		}

		public bool CanRemoveFor(string file)
		{
			return false;
		}

		public void Remove(EditorRemoveMessage msg)
		{
		}
		
		public KeyValuePair<string,string>[] GetDirtyFiles()
		{
			return new KeyValuePair<string,string>[] {};
		}

        private void tryOpenConfiguration()
        {
            try
            {
                openConfiguration();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to open {0} editor", _editorName);
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private void openConfiguration()
        {
            var file =
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    _editorName + ".editor");
            var document = new XmlDocument();
            document.Load(file);
            var node = document.SelectSingleNode("editor/launch");
            if (node != null)
            {
                _launchCommand = new Command(
                    node.SelectSingleNode("executable").InnerText,
                    node.SelectSingleNode("parameters").InnerText);
            }
            node = document.SelectSingleNode("editor/alive");
            if (node != null)
            {
                _aliveCommand = new AliveCommand(
                    node.SelectSingleNode("executable").InnerText,
                    node.SelectSingleNode("parameters").InnerText,
                    node.SelectSingleNode("text_to_look_for").InnerText);
            }
            node = document.SelectSingleNode("editor/goto");
            if (node != null)
            {
                _goToCommand = new Command(
                    node.SelectSingleNode("executable").InnerText,
                    node.SelectSingleNode("parameters").InnerText);
            }
            node = document.SelectSingleNode("editor/setfocus");
            if (node != null)
            {
                _setFocusCommand = new Command(
                    node.SelectSingleNode("executable").InnerText,
                    node.SelectSingleNode("parameters").InnerText);
            }
        }

        private Process runCommand(string cmd, string parameters, bool visible)
        {
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

        private string queryCommand(string cmd, string parameters, bool visible)
        {
			if (cmd == "")
				return "";
            var process = new Process();
            process.StartInfo = new ProcessStartInfo(cmd, parameters);
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = visible;
            process.StartInfo.UseShellExecute = visible;
            process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            if (visible)
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            else
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }
    }

    class Command
    {
        public string Executable { get; private set; }
        public string Parameter { get; private set; }

        public Command(string exe, string cmd)
        {
            Executable = exe;
            Parameter = cmd;
        }
    }

    class AliveCommand : Command
    {
        public string CheckText { get; private set; }

        public AliveCommand(string exe, string cmd, string checkText)
            : base(exe, cmd)
        {
            CheckText = checkText;
        }
    }
}
