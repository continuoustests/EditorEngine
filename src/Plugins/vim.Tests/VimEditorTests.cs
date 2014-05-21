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
			_editor = new VimEditor().TimeoutAfter(0);
			_editor.Publisher = _endpoint;
			_editor.Server = _server;
		}
		
		[Test]
		public void When_recieving_mm_highlight_keypress_is_should_pass_on_to_publisher()
		{
			_server.Publish("12:keyAtPos=0 \"j\"");
			_endpoint.Ran("j");
		}
		
		[Test]
		public void When_sending_go_to_it_should_pass_of_goto_commands()
		{
			_editor.GoTo(new Location("file", 10, 15));
			_server.Publish("0:fileOpened=0 \"file\"");
			_server.Sent("1:editFile!0 \"file\"");
			_server.Sent("1:setDot!0 10/14");
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
						},
					null));
			_server.Sent("1:setDot!0 2/2");
			_server.Sent("0:getCursor/2");
			_server.Sent("1:remove/0 4 " + (6 + newline.Length).ToString());
			_server.Sent("1:insert/0 4 \"cotext to insertntent\"");
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
			_server.Sent("1:setDot!0 2/2");
			_server.Sent("0:getCursor/2");
			_server.Sent("1:remove/0 15 7");
            if (isWindows())
                _server.Sent("1:remove/0 25 8");
            else
			    _server.Sent("1:remove/0 25 6");
		}

        private bool isWindows()
        {
            return Environment.OSVersion.Platform != PlatformID.Unix &&
                   Environment.OSVersion.Platform != PlatformID.MacOSX;
        }

		[Test]
		public void When_getting_dirty_files_an_no_files_are_dirty_return_empty_list()
		{
			_server.WhenPublishsing("0:getModified/1", "1 0");
			Assert.That(_editor.GetDirtyFiles(null).Length, Is.EqualTo(0));
		}

		[Test]
		public void When_getting_dirty_files_and_files_are_dirty_it_will_reply_with_them()
		{
			_server.Publish("0:fileOpened=0 \"file1\" T F");
			_server.Publish("0:fileOpened=0 \"file2\" T F");
			_server.Publish("0:fileOpened=0 \"file3\" T F");
			_server.WhenPublishsing("0:getModified/2", "2 2");
			_server.WhenPublishsing("1:getModified/3", "3 1");
			_server.WhenPublishsing("2:getModified/5", "5 0");
			_server.WhenPublishsing("3:getModified/6", "6 1");
			_server.WhenPublishsing("1:getText/4", "4 This");
			_server.WhenPublishsing("3:getText/7", "7 the answer");
			var files = _editor.GetDirtyFiles(null);
			//throw new Exception(_server.Messages);
			Assert.That(files.Length, Is.EqualTo(2));
			Assert.That(files[0].Key, Is.EqualTo("file1"));
			Assert.That(files[0].Value, Is.EqualTo("This"));
			Assert.That(files[1].Key, Is.EqualTo("file3"));
			Assert.That(files[1].Value, Is.EqualTo("the answer"));
		}
		
		[Test]
		public void When_completing_snippet_it_will_pass_on_message()
		{
			var newline = "\\n";
			if (Environment.OSVersion.Platform != PlatformID.Unix && 
				Environment.OSVersion.Platform != PlatformID.MacOSX)
				newline = "\\r\\n";
			_server.WhenPublishsing("0:getCursor/2", "2 1 2 3 7");
			_server.WhenPublishsing("1:getText/3", "3 the" + newline + "content");
			_server.Publish("0:fileOpened=0 \"file_to_write_to\" T F");
			_server.Publish("0:keyAtPos=0 \"snippet-complete\"");
			Thread.Sleep(800);
			_server.Sent("0:getCursor/2");
			_endpoint.Ran("snippet-complete \"\" \"content\" \"file_to_write_to|2|1\" \"\"");
		}
	}

	[TestFixture]
	public class WordTests
	{
		[Test]
		public void When_given_a_single_word_sentence_it_will_return_the_word() {
			assertThat("word", 2, "word", 1);
		}
		
		[Test]
		public void When_given_a_single_word_sentence_with_cursor_at_start_it_will_return_the_word() {
			assertThat("\tword", 1, "word", 1);
		}
		
		[Test]
		public void When_given_a_sentence_with_multiple_words_it_will_return_the_selected_word() {
			assertThat("some word in between", 8, "word", 6);
		}

		[Test]
		public void When_starts_with_word__returns_first_word() {
			assertThat("some word in between", 3, "some", 1);
		}

		[Test]
		public void When_starts_with_word_and_is_positioned_first_returns_first_word() {
			assertThat("some word in between", 1, "some", 1);
		}
		
		[Test]
		public void When_ends_with_word_returns_last_word() {
			assertThat("some word in between", 21, "between", 14);
		}

		private void assertThat(string content, int position, string resultword, int column)
		{
			var word = Word.Extract(content, position);
			Assert.That(word.Content, Is.EqualTo(resultword));
			Assert.That(word.Column, Is.EqualTo(column));
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

		public int ConnectedClients { get { return 0; } }
		
		public event EventHandler ClientConnected;

		public event EventHandler<MessageArgs> IncomingMessage;

		public void Start()
		{
		}


		public void Start(int port)
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
		
		public void Send(string message, Guid client)
		{
			Send(message);
		}

		public void Publish(string message)
		{
			if (IncomingMessage != null)
				IncomingMessage(this, new MessageArgs(Guid.Empty, message));
		}
		
		public void Sent(string message)
		{
			Assert.That(_messages.Exists(x => x.Equals(message)), Is.True);
		}
	}
	
	class Fake_CommandEndpoint : ICommandEndpoint
	{
		private List<string> _cmds = new List<string>();
		
		public string Messages {
			get {
				var text = "";
				_cmds.ForEach(x => text += x + Environment.NewLine);
				return text;
			}
		}

		public void Run (string cmd)
		{
			_cmds.Add(cmd);
		}
		
		public void Run(Guid clientID, string cmd)
		{
			_cmds.Add(cmd);
		}
		public void Ran(string cmd)
		{
			Assert.That(_cmds.Exists(x => x.Equals(cmd)), Is.True);
		}
	}
}

