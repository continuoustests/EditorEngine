using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using EditorEngine.Core.Endpoints.Tcp;
using EditorEngine.Core.Endpoints;
using System.Collections.Generic;
using EditorEngine.Core.Editors;
using EditorEngine.Core.Arguments;
using EditorEngine.Core.Messaging.Messages;
namespace vim.Tests
{
	[TestFixture]
	public class VimEditorTests
	{
		private VimEditor _editor;
		private Fake_TcpServer _server;
		private Fake_CommandEndpoint _endpoint;
		
		[SetUp]
		public void Setup()
		{
			_server = new Fake_TcpServer();
			_endpoint = new Fake_CommandEndpoint();
			_editor = new VimEditor();
			_editor.Publisher = _endpoint;
			_editor.Server = _server;
		}
		
		[Test]
		public void When_recieving_mm_highlight_keypress_is_should_pass_on_to_publisher()
		{
			_server.Publish("12:keyAtPos=0 \"j\"");
			_endpoint.Ran("keypress ctrl+shift+j");
		}
		
		[Test]
		public void When_sending_go_to_it_should_pass_of_goto_commands()
		{
			_editor.GoTo(new Location("file", 10, 15));
			_server.Publish("0:fileOpened=0 \"file\"");
			_server.Sent("1:editFile!0 \"file\"");
			_server.Sent("1:setDot!0 10/15");
			Assert.That(_editor.Buffers.Count, Is.EqualTo(1));
			Assert.That(_editor.Buffers.Exists(x => x.ID.Equals(1) && x.Fullpath.Equals("file")), Is.True);
		}
		
		[Test]
		public void When_vim_opens_a_file_we_should_assign_next_free_buffer_to_it()
		{
			_server.Publish("0:fileOpened=0 \"file2\" T F");
			Assert.That(_editor.Buffers.Exists(x => x.ID.Equals(1) && x.Fullpath.Equals("file2")), Is.True);
		}
		
		[Test]
		public void When_vim_closes_a_file_we_should_remove_it_from_existing_buffers()
		{
			_server.Publish("0:fileOpened=0 \"file1\" T F");
			_server.Publish("0:fileOpened=0 \"file2\" T F");
			_server.Publish("2:killed=0");
			Assert.That(_editor.Buffers.Count, Is.EqualTo(2));
			Assert.That(_editor.Buffers.Exists(x => x.ID.Equals(2) && x.Fullpath.Equals("file2") && x.Closed.Equals(true)), Is.True);
		}
		
		[Test]
		public void When_opening_a_document_we_should_apply_next_free_buffer()
		{
			_server.Publish("0:fileOpened=0 \"file1\" T F");
			_server.Publish("0:fileOpened=0 \"file2\" T F");
			_server.Publish("0:fileOpened=0 \"file3\" T F");
			_server.Publish("2:killed=0");
			_server.Publish("0:fileOpened=0 \"file4\" T F");
			Assert.That(_editor.Buffers.Exists(x => x.ID.Equals(4) && x.Fullpath.Equals("file4")), Is.True);
		}
		
		[Test]
		public void When_re_opening_a_document_we_should_apply_old_buffer()
		{
			_server.Publish("0:fileOpened=0 \"file1\" T F");
			_server.Publish("0:fileOpened=0 \"file2\" T F");
			_server.Publish("0:fileOpened=0 \"file3\" T F");
			_server.Publish("2:killed=0");
			Assert.That(
				_editor.Buffers.Exists(x => x.ID.Equals(2) &&
									   x.Fullpath.Equals("file2") &&
									   x.Closed.Equals(true)),
				Is.True);
			_server.Publish("0:fileOpened=0 \"file4\" T F");
			_server.Publish("0:fileOpened=0 \"file2\" T F");
			Assert.That(
				_editor.Buffers.Exists(x => x.ID.Equals(2) && 
									   x.Fullpath.Equals("file2") &&
									   x.Closed.Equals(false)),
				Is.True);
		}

		[Test]
		public void When_asked_to_insert_for_a_file_it_will_accept_the_challenge()
		{
			var newline = "\\n";
			if (Environment.OSVersion.Platform != PlatformID.Unix && 
				Environment.OSVersion.Platform != PlatformID.MacOSX)
				newline = "\\r\\n";
			_server.WhenPublishsing("0:getCursor/2", "2 1 2 3 7");
			_server.WhenPublishsing("1:getText/3", "3 the" + newline + "content");
			_server.Publish("0:fileOpened=0 \"file_to_write_to\" T F");
			_editor.Insert(
				new EditorInsertMessage(
					"text to insert",
					new GoTo()
						{
							File = "file_to_write_to",
							Line = 2,
							Column = 3
						}));
			_server.Sent("1:setDot!0 2/3");
			_server.Sent("0:getCursor/2");
			_server.Sent("1:remove/0 4 " + (7 + newline.Length).ToString());
			_server.Sent("1:insert/0 4 \"context to inserttent\"");
		}
		
		[Test]
		public void When_asked_to_remove_for_a_file_it_will_accept_the_challenge()
		{
			var newline = "\\n";
			if (Environment.OSVersion.Platform != PlatformID.Unix && 
				Environment.OSVersion.Platform != PlatformID.MacOSX)
				newline = "\\r\\n";
			_server.WhenPublishsing("0:getCursor/2", "2 1 2 3 7");
			_server.WhenPublishsing("1:getText/3", "3 the" + newline + "content" + newline + "and some more");
			_server.WhenPublishsing("0:getCursor/4", "4 1 3 7 15");
			_server.WhenPublishsing("0:getCursor/5", "5 1 4 7 25");
			_server.Publish("0:fileOpened=0 \"file_to_write_to\" T F");
			_editor.Remove(
				new EditorRemoveMessage(
					new GoTo()
						{
							File = "file_to_write_to",
							Line = 2,
							Column = 3
						},
					new GoTo()
						{
							Line = 3,
							Column = 7
						}));
			_server.Sent("1:setDot!0 2/3");
			_server.Sent("0:getCursor/2");
			_server.Sent("1:remove/0 15 7");
			_server.Sent("1:remove/0 25 6");
		}
	}
	
	class Fake_TcpServer : ITcpServer
	{
		private List<string> _messages = new List<string>();
		private List<KeyValuePair<string,string>> _queries = new List<KeyValuePair<string,string>>();
		
		public int MessageCount { get { return _messages.Count; } }

		public string Messages {
			get {
				var text = "";
				_messages.ForEach(x => text += x + Environment.NewLine);
				return text;
			}
		}
		
		public int Port { get { return 5566; } }
		
		public event EventHandler ClientConnected;

		public event EventHandler<MessageArgs> IncomingMessage;

		public void Start()
		{
		}

		public void WhenPublishsing(string command, string replyWith)
		{
			_queries.Add(new KeyValuePair<string,string>(command, replyWith));
		}

		public void Send(string message)
		{
			_messages.Add(message);
			var query = _queries.FirstOrDefault(x => x.Key.Equals(message));
			if (!query.Equals(default(KeyValuePair<string,string>)))
				Publish(query.Value);
		}
		
		public void Publish(string message)
		{
			if (IncomingMessage != null)
				IncomingMessage(this, new MessageArgs(message));
		}
		
		public void Sent(string message)
		{
			Assert.That(_messages.Exists(x => x.Equals(message)), Is.True);
		}
	}
	
	class Fake_CommandEndpoint : ICommandEndpoint
	{
		private List<string> _cmds = new List<string>();
		
		public void Run (string cmd)
		{
			_cmds.Add(cmd);
		}
		
		public void Ran(string cmd)
		{
			Assert.That(_cmds.Exists(x => x.Equals(cmd)), Is.True);
		}
	}
}

