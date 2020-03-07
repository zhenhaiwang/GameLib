using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System;

namespace GameLib
{
    public sealed class ConnectThread : BaseThread
    {
        private TcpClient m_TcpClient;
        private string m_HostName;
        private int m_Port;
        private Action<bool> m_Callback;

        public ConnectThread(TcpClient tcpClient, string hostName, int port, Action<bool> callback)
        {
            m_TcpClient = tcpClient;
            m_HostName = hostName;
            m_Port = port;
            m_Callback = callback;
        }

        public void ClearCallback()
        {
            m_Callback = null;
        }

        protected override void Main()
        {
            Log.Debug("Connect thread begin");

            try
            {
                m_TcpClient.Connect(m_HostName, m_Port);
                m_Callback(true);
            }
            catch (ObjectDisposedException ex)  // closed by timeout.
            {
                Log.Debug("Connect ObjectDisposedException: " + ex.Message);
            }
            catch (Exception ex)
            {
                Log.Debug("Connect exception: " + ex.Message);
                m_Callback(false);
            }

            Log.Debug("Connect thread end");
        }
    }

    public class ReceiverThread : BaseThread
    {
        private const uint MaxPacketSize = 1024 * 512;
        private const short PackageHeaderSize = 2;

        private byte[] m_RecvBuffer;
        private int m_RecvBufferOffset;

        private Action m_OnReceiveFail;
        private Action<byte[]> m_OnReceivePackage;

        private NetworkStream m_NetStream;

        public ReceiverThread(NetworkStream stream, Action<byte[]> onReceivePackage, Action onReceiveFail) : base()
        {
            m_NetStream = stream;
            m_RecvBuffer = new byte[2 * MaxPacketSize];
            m_RecvBufferOffset = 0;

            m_OnReceivePackage = onReceivePackage;
            m_OnReceiveFail = onReceiveFail;
        }

        protected override void Main()
        {
            Log.Debug("Receiver thread begin");

            while (!CheckTerminated())
            {
                ReadFromStream();
                ScanPackets();
            }

            Log.Debug("Receiver thread end");
        }

        protected void ReadFromStream()
        {
            if (m_NetStream.CanRead)
            {
                try
                {
                    int length = m_NetStream.Read(m_RecvBuffer, m_RecvBufferOffset, m_RecvBuffer.Length - m_RecvBufferOffset);

                    m_RecvBufferOffset += length;

                    if (length == 0)
                    {
                        Log.Debug("Receiver thread read length = 0");
                        m_OnReceiveFail.Call();
                        SetTerminated();
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug("Receiver thread read exception: " + ex);
                    m_OnReceiveFail.Call();
                    SetTerminated();
                }
            }
            else
            {
                Thread.Sleep(16);
            }
        }

        protected void ScanPackets()
        {
            while (m_RecvBufferOffset > PackageHeaderSize && !CheckTerminated())
            {
                ushort pkgSize = 0;

                if (BitConverter.IsLittleEndian)
                {
                    pkgSize = BitConverter.ToUInt16(new byte[] { m_RecvBuffer[1], m_RecvBuffer[0] }, 0);
                }
                else
                {
                    pkgSize = BitConverter.ToUInt16(new byte[] { m_RecvBuffer[0], m_RecvBuffer[1] }, 0);
                }

                if (pkgSize <= m_RecvBufferOffset)
                {
                    byte[] _buffer = new byte[pkgSize - PackageHeaderSize];
                    Array.Copy(m_RecvBuffer, PackageHeaderSize, _buffer, 0, _buffer.Length);

                    m_OnReceivePackage.Call(_buffer);

                    if (m_RecvBufferOffset > pkgSize)
                    {
                        for (int i = pkgSize, j = 0; i < m_RecvBufferOffset; i++, j++)
                        {
                            m_RecvBuffer[j] = m_RecvBuffer[i];
                        }

                        m_RecvBufferOffset -= pkgSize;
                    }
                    else
                    {
                        m_RecvBufferOffset = 0;
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

    public sealed class SenderThread : BaseThread
    {
        private const int MaxPacketSize = 1024 * 512;
        private const short PackageHeaderSize = 2;
        private List<byte[]> m_PacketsToSend;

        private NetworkStream m_NetStream;

        public SenderThread(NetworkStream stream, List<byte[]> packetsToSend) : base()
        {
            m_NetStream = stream;
            m_PacketsToSend = packetsToSend;
        }

        protected override void Main()
        {
            Log.Debug("Sender thread begin");

            while (!CheckTerminated())
            {
                bool sleep = false;
                byte[] buffer = null;

                lock (m_PacketsToSend)
                {
                    if (m_PacketsToSend.Count > 0)
                    {
                        buffer = m_PacketsToSend[0];
                    }
                    else
                    {
                        sleep = true;
                    }
                }

                if (buffer != null)
                {
                    byte[] len = BitConverter.GetBytes((ushort)(buffer.Length + PackageHeaderSize));

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(len);
                    }

                    byte[] _bufferWithLen = new byte[len.Length + buffer.Length];
                    len.CopyTo(_bufferWithLen, 0);
                    buffer.CopyTo(_bufferWithLen, len.Length);

                    try
                    {
                        m_NetStream.Write(_bufferWithLen, 0, (int)_bufferWithLen.Length);
                        m_NetStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        Log.DebugFormat("Sender thread write exception: {0}, {1}, {2}", ex.Message, ex.StackTrace, ex.InnerException.Message);
                    }

                    lock (m_PacketsToSend)
                    {
                        if (m_PacketsToSend.Count > 0)
                        {
                            m_PacketsToSend.RemoveAt(0);
                        }
                    }
                }

                if (sleep)
                {
                    Thread.Sleep(15);
                }
            }

            Log.Debug("Sender thread end");
        }
    }
}
