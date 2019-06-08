using System;
using System.Text;
using System.Threading;

namespace LibUDP {
	public delegate void DataReceived(byte[] buffer, int size, string ip, int port);

	public sealed class UDPSocket : IDisposable {
		private bool alive;
		private Thread thread;
		private DataReceived dataReceived, dataReceivedOriginal;
		private System.Windows.Forms.Control target;
		private System.Net.Sockets.Socket socket;

		public UDPSocket() : this(null, 0) {
		}

		public UDPSocket(DataReceived dataReceived) : this(dataReceived, 0) {
		}

		public UDPSocket(DataReceived dataReceived, int port) {
			socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
			socket.ReceiveBufferSize = 32 * 1024;
			socket.ReceiveTimeout = 0; //no timeout
			socket.SendBufferSize = 32 * 1024;
			socket.SendTimeout = 0; //no timeout

			if (port > 0) {
				socket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, port));
			}

			alive = true;

			if (dataReceived != null) {
				thread = new Thread(ThreadProc);
				thread.IsBackground = true;
				thread.Name = "UDP Server Thread";
				if (dataReceived.Target is System.Windows.Forms.Control) {
					target = dataReceived.Target as System.Windows.Forms.Control;
					dataReceivedOriginal = dataReceived;
					this.dataReceived = DataReceivedProxy;
				} else {
					this.dataReceived = dataReceived;
				}
				thread.Start();
			}
		}

		private void DataReceivedProxy(byte[] buffer, int size, string ip, int port) {
			target.BeginInvoke(dataReceivedOriginal, buffer, size, ip, port);
		}

		private void ThreadProc() {
			byte[] buffer = new byte[32 * 1024];
			while (alive) {
				try {
					System.Net.EndPoint ep = new System.Net.IPEndPoint(0, 0);
					int size = socket.ReceiveFrom(buffer, ref ep);
					System.Net.IPEndPoint ipep = ep as System.Net.IPEndPoint;
					dataReceived(buffer, size, ipep.Address.ToString(), ipep.Port);
				} catch {
				}
			}
		}

		public void Send(byte[] buffer, string ip, int port) {
			socket.SendTo(buffer, new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port));
		}

		public void Send(byte[] buffer, int offset, int size, string ip, int port) {
			socket.SendTo(buffer, offset, size, System.Net.Sockets.SocketFlags.None, new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port));
		}

		public int WaitToReceive(byte[] buffer) {
			return socket.Receive(buffer);
		}

		public int WaitToReceive(byte[] buffer, out string ip, out int port) {
			System.Net.EndPoint ep = new System.Net.IPEndPoint(0, 0);
			int size = socket.ReceiveFrom(buffer, ref ep);
			System.Net.IPEndPoint ipep = ep as System.Net.IPEndPoint;
			ip = ipep.Address.ToString();
			port = ipep.Port;
			return size;
		}

		public void Close() {
			alive = false;
			if (socket != null) {
				socket.Close();
				socket.Dispose();
				socket = null;
			}
			if (thread != null) {
				thread.Join();
				thread = null;
			}
		}

		public void Dispose() {
			Close();
		}
	}
}
