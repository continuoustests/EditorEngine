using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using System.Linq;
namespace EditorEngine.Core.Tests.Endpoints.Tcp
{
	class Client : IDisposable
	{
        private TcpClient _client;
        private NetworkStream _stream;
        readonly byte[] _buffer = new byte[1000000];
        private int _currentPort;
        private readonly MemoryStream _readBuffer = new MemoryStream();
        private Queue queue = new Queue();
		private bool IsSending = false;
		private string _messageTermination = null;
		
		public string RecievedMessage { get; private set; }

		public Client()
		{
		}
		
		public Client(string terminateString)
		{
			_messageTermination = terminateString;
		}
		
        public void Connect(int port)
        {
            Connect(port, 0);
        }

        private void Connect(int port, int retryCount)
        {
            if (retryCount >= 5)
                return;
			try {
	            _client = new TcpClient();
	            _client.Connect("127.0.0.1", port);
	            _currentPort = port;
	            _stream = _client.GetStream();
	            _stream.BeginRead(_buffer, 0, _buffer.Length, ReadCompleted, _stream);
			} 
			catch 
			{
                Reconnect(retryCount);
			}
        }

        public void Disconnect()
        {
            while (IsSending)
                Thread.Sleep(10);
            if (_stream != null) {
                try {_stream.Close(); }
                catch (Exception ex) { Logger.Write(ex); }
            }
            if (_client != null) {
                try { _client.Close(); }
                catch (Exception ex) { Logger.Write(ex); }
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
			
        private void Reconnect(int retryCount)
        {
            retryCount++;
            _readBuffer.SetLength(0);
			Disconnect();
			Connect(_currentPort, retryCount);
		}

        private void ReadCompleted(IAsyncResult result)
        {
            var stream = (NetworkStream)result.AsyncState;
            try
            {
                var x = stream.EndRead(result);
                if(x == 0) Reconnect(0);
                for (var i = 0; i < x;i++)
                {
                    if (isEndOfMessage(i))
                    {
                        var data = _readBuffer.ToArray();
						string actual;
                        if (_messageTermination == null)
							actual = Encoding.UTF8.GetString(data, 0, data.Length);
						else
						    actual = Encoding.UTF8.GetString(data, 0, data.Length - (_messageTermination.Length - 1));
                        RecievedMessage = actual;
                        _readBuffer.SetLength(0);
                    }
                    else
                    {
                        _readBuffer.WriteByte(_buffer[i]);
                    }
                }
                stream.BeginRead(_buffer, 0, _buffer.Length, ReadCompleted, stream);
            }
            catch (Exception ex)
            {
                WriteError(ex);
                Reconnect(0);
            }
        }

		private bool isEndOfMessage(int index)
		{
			if (_messageTermination == null)
				return _buffer[index].Equals(0);
			if (_messageTermination.Length > (index + 1))
				return false;
			for (int i = index; i > (index - _messageTermination.Length); i--)
			{
				if (!Encoding.UTF8.GetString(new byte[] { _buffer[i]}).Equals(_messageTermination.Substring(_messageTermination.Length - (index - i) - 1, 1)))
					return false;
			}
			return true;
		}

        public void Send(string o)
        {
            if (IsSending)
                throw new Exception("Cannot call send while doing SendAndWait, make up your mind");
            lock (queue)
            {
                queue.Enqueue(o);
                if(!IsSending) {
					SendFromQueue();                      
                }
            }
        }

        public void SendAndWait(string o)
        {
            Send(o);
            IsSending = true;
            var timeout = DateTime.Now;
            while (IsSending && DateTime.Now.Subtract(timeout).TotalMilliseconds < 8000)
                Thread.Sleep(10);
        }

        private void WriteCompleted(IAsyncResult result)
        {
            var client = (NetworkStream)result.AsyncState;
            try
            {
                client.EndWrite(result);
                lock(queue)
                {
		    		IsSending = false;
                    if (queue.Count > 0)
                        SendFromQueue();
                }
                
            }
            catch (Exception ex)
            {
                WriteError(ex);
				Reconnect(0);
            }
        }

        private void SendFromQueue()
        {
            string message = null;
            lock (queue)
            {
                if (!IsSending && queue.Count > 0)
                    message = queue.Dequeue().ToString();
            }
            if (message != null)
            {
                try
                {
					byte[] toSend;
					if (_messageTermination == null)
						toSend = Encoding.UTF8.GetBytes(message).Concat(new byte[] { 0x0 }).ToArray();
					else
						toSend = Encoding.UTF8.GetBytes(message + _messageTermination).ToArray();
                    _stream.BeginWrite(toSend, 0, toSend.Length, WriteCompleted, _stream);
                }
                catch
                {
                }
            }
        }

        private void WriteError(Exception ex)
        {
        }
	}
}

