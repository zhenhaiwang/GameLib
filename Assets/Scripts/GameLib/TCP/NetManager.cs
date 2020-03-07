using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace GameLib
{
    /// <summary>
    /// 1.iOS 切换到后台后，主线程和收发线程都会被立刻停掉，在后台期间的网络包会被压到系统缓存。当再次唤醒时会被全部推到Socket，所以会连续收到很多数据包
    /// 2.Android 切换到后台后，主线程UI线程的Update不会被调用，但是收发线程不受影响
    /// </summary>
	public sealed class NetManager : MonoSingleton<NetManager>
    {
        public enum NETEVENT
        {
            Connecting,
            Success,
            Close,
            RetryFail,
            NotReachable,
        };

        public enum NETSTATE
        {
            Init = 0,
            Connecting,
            Connected,
            ConnectFailed,
            DisConnected
        }

        public const int TcpClientReceiveBufferSize = 1024 * 1024;
        public const int TcpClientReceiveTimeout = 30000;
        public const int TcpClientSendBufferSize = 1024 * 1024;
        public const int TcpClientSendTimeout = 30000;
        public const float TcpConnectTimeout = 6.0f;
        public const float HeartBeatTime = 8.0f;
        public const float HelloResponseTimeout = 4.0f;

        public const string NET_EVENT = "Net.Event";

        public event Action<NETEVENT> netConnectionCallback;

        private NETSTATE m_NetState = NETSTATE.Init;
        public NETSTATE netState
        {
            get { return m_NetState; }
            set { m_NetState = value; }
        }

        private List<byte[]> m_BufferToSend;
        private List<byte[]> m_BufferFromServer;

        private float m_HeartBeatTimeCount;
        private bool m_HelloHasResponse;

        private long m_SendHelloTimerId;
        private long m_ConnectTimerId;

        private long m_RetryTimerId;
        private int m_RetryTimes = 3;

        private float m_LockDispatcherSecond;

        private bool m_Stopped = true;

        private string m_HostName = "xxx.yyy.zzz.com";
        private int m_Port = 8080;

        private ConnectThread m_Connecter;
        private ReceiverThread m_Receiver;
        private SenderThread m_Sender;

        private TcpClient m_TcpClient;
        private NetTaskExecutor m_TaskExecutor;

        private void Awake()
        {
            m_BufferToSend = new List<byte[]>();
            m_BufferFromServer = new List<byte[]>();
            m_TaskExecutor = new NetTaskExecutor();
        }

        private void Update()
        {
            if (!m_Stopped)
            {
                m_TaskExecutor.Update();
                CheckReConnect();
                CheckLock();
                FireEvent();
                SendHello();
            }
        }

        public void LockEvent(float second)
        {
            Log.Debug("Lock event: " + second.ToString());

            m_LockDispatcherSecond += second;
        }

        public void UnLockEvent()
        {
            Log.Debug("UnLock event: " + m_LockDispatcherSecond.ToString());

            m_LockDispatcherSecond = 0.0f;
        }

        private bool IsLockEvent()
        {
            return m_LockDispatcherSecond > 0.0f;
        }

        private void CheckLock()
        {
            if (m_LockDispatcherSecond > 0.0f)
            {
                m_LockDispatcherSecond -= Time.deltaTime;

                if (m_LockDispatcherSecond < 0.0f)
                {
                    UnLockEvent();
                }
            }
        }

        public bool IsConnected()
        {
            return m_TcpClient != null && netState == NETSTATE.Connected;
        }

        public static bool IsNotReachable()
        {
            return Application.internetReachability == NetworkReachability.NotReachable;
        }

        public void SendPacket(byte[] buffer)
        {
            if (netState == NETSTATE.Connected)
            {
                if (buffer != null)
                {
                    lock (m_BufferToSend)
                    {
                        m_BufferToSend.Add(buffer);
                    }
                }
                else
                {
                    Log.Debug("Send packet buffer is null");
                }
            }
            else
            {
                Log.Debug("Send packet ignore, net state = " + netState.ToString());
            }
        }

        private void EnqueueResponsePacket(byte[] buffer)
        {
            if (buffer == null) return;

            lock (m_BufferFromServer)
            {
                m_BufferFromServer.Add(buffer);
            }
        }

        public void ConnectServer()
        {
            if (netState == NETSTATE.Init || netState == NETSTATE.DisConnected)
            {
                m_Stopped = false;
                ConnectServer(m_HostName, m_Port);
            }
            else
            {
                Log.Debug("Connect server error, net state = " + netState.ToString());
            }
        }

        private void ConnectServer(string host, int port)
        {
            Log.Debug("Connect server: host = " + host + ", port = " + port.ToString());

            m_Port = port;

            CloseConnect();
            EnableRetry(true);
            StartConnecting();
        }

        private void StartConnecting()
        {
            if (netState == NETSTATE.ConnectFailed || netState == NETSTATE.DisConnected)
            {
                netState = NETSTATE.Connecting;
                netConnectionCallback.Call(NETEVENT.Connecting);

                BeginConnectTimer();
                StartConnectThread();
            }
            else
            {
                Log.Debug("Start connecting ignore, net state = " + netState.ToString());
            }
        }

        public void StopConnectServer()
        {
            Log.Debug("Stop connect server");

            m_Stopped = true;

            CloseConnect();
            EnableRetry(false);

            RemoveConnectTimer();
            RemoveRetryTimer();
            TimeoutManager.instance.ClearTimeout(m_SendHelloTimerId);
            m_TaskExecutor.Clear();
        }

        private void ConnectTimeout()
        {
            if (m_TcpClient != null && !m_TcpClient.Connected)
            {
                Log.Debug("Begin connect timeout and close socket");

                CloseTcpClient();

                if (m_Connecter != null)
                {
                    m_Connecter.ClearCallback();
                    m_Connecter = null;
                }

                ConnectFail();
            }
        }

        private void BeginConnectTimer()
        {
            m_ConnectTimerId = TimeoutManager.instance.CreateTimeout(TcpConnectTimeout, delegate (long id, object param)
            {
                ConnectTimeout();
            });
        }

        private void RemoveConnectTimer()
        {
            TimeoutManager.instance.ClearTimeout(m_ConnectTimerId);
        }

        private void BeginRetryTimer()
        {
            m_RetryTimerId = TimeoutManager.instance.CreateTimeout(0.3f, delegate (long id, object param)
            {
                Log.Debug("Net retry times = " + m_RetryTimes.ToString());

                StartConnecting();
            });
        }

        private void RemoveRetryTimer()
        {
            TimeoutManager.instance.ClearTimeout(m_RetryTimerId);
        }

        private void SendHello()
        {
            m_HeartBeatTimeCount += Time.deltaTime;

            if (m_HeartBeatTimeCount > HeartBeatTime && IsConnected())
            {
                byte[] hello = new byte[1];

                SendPacket(hello);

                m_HeartBeatTimeCount = 0.0f;
                m_HelloHasResponse = false;

                m_SendHelloTimerId = TimeoutManager.instance.CreateTimeout(HelloResponseTimeout, delegate (long id, object param)
                {
                    if (!m_HelloHasResponse)
                    {
                        Log.Debug("Hello packet response timeout");

                        NotifyDisConnect();
                    }
                });
            }

            if (m_HelloHasResponse && m_SendHelloTimerId > 0)
            {
                TimeoutManager.instance.ClearTimeout(m_SendHelloTimerId);
                m_SendHelloTimerId = -1;
            }
        }

        private void FireEvent()
        {
            lock (m_BufferFromServer)
            {
                while (m_BufferFromServer.Count() > 0 && !IsLockEvent())
                {
                    byte[] buffer = m_BufferFromServer[0];
                    EventDispatcher.Execute(NET_EVENT, buffer);
                    m_BufferFromServer.RemoveAt(0);
                }
            }
        }

        private void CheckReConnect()
        {
            if (netState == NETSTATE.DisConnected && !IsNotReachable() && m_RetryTimes > 0)
            {
                Log.Debug("Check re-connect connecting");

                StartConnecting();
            }
        }

        private void StartReceiveThread()
        {
            if (m_TcpClient == null)
            {
                return;
            }

            m_Receiver = new ReceiverThread(m_TcpClient.GetStream(), EnqueueResponsePacket, delegate ()
            {
                Log.Debug("Receiver thread failed");

                m_TaskExecutor.Add(NotifyDisConnect);
            });

            m_Receiver.Run();
        }

        private void StartConnectThread()
        {
            if (m_TcpClient == null)
            {
                m_TcpClient = new TcpClient();
                m_TcpClient.NoDelay = true;
                m_TcpClient.ReceiveBufferSize = TcpClientReceiveBufferSize;
                m_TcpClient.ReceiveTimeout = TcpClientReceiveTimeout;
                m_TcpClient.SendBufferSize = TcpClientSendBufferSize;
                m_TcpClient.SendTimeout = TcpClientSendTimeout;
            }

            m_Connecter = new ConnectThread(m_TcpClient, m_HostName, m_Port, delegate (bool ok)
            {
                Log.Debug("Connecter thread callback: " + ok.ToString());

                if (ok)
                {
                    m_TaskExecutor.Add(ConnectOK);
                }
                else
                {
                    m_TaskExecutor.Add(ConnectFail);
                }

                m_Connecter.ClearCallback();
                m_Connecter = null;
            });

            m_Connecter.Run();
        }

        private void StartSendThread()
        {
            if (m_TcpClient == null)
            {
                return;
            }

            m_Sender = new SenderThread(m_TcpClient.GetStream(), m_BufferToSend);
            m_Sender.Run();
        }

        private void EnableRetry(bool isEnable)
        {
            m_RetryTimes = isEnable ? 3 : 0;

            Log.Debug("Enable retry: " + isEnable.ToString());
        }

        private void NotifyApplicationPause(bool isPause)
        {
            if (isPause)
            {
                EnableRetry(false);
            }
            else if (!m_Stopped)
            {
                EnableRetry(true);
            }
        }

        private void NotifyDisConnect()
        {
            Log.Debug("Notify dis-connect");

            if (netState == NETSTATE.Connected)
            {
                CloseConnect();
            }
            else
            {
                Log.Debug("Notify dis-connect ignore, net state = " + netState.ToString());
            }
        }

        private void CloseTcpClient()
        {
            if (m_TcpClient == null)
            {
                Log.Debug("Tcp client has closed already");

                return;
            }

            if (m_TcpClient.Connected)
            {
                try
                {
                    m_TcpClient.GetStream().Close();
                }
                catch (Exception ex)
                {
                    Log.Debug("Close tcp client stream Exception: " + ex);
                }
            }

            try
            {
                m_TcpClient.Close();
            }
            catch (System.Exception ex)
            {
                Log.Debug("Close tcp client Exception: " + ex);
            }

            m_TcpClient = null;
        }

        private void CloseConnect()
        {
            netState = NETSTATE.DisConnected;

            Log.Debug("Close connect");

            lock (m_BufferToSend)
            {
                m_BufferToSend.Clear();
            }

            m_HeartBeatTimeCount = 0;
            m_HelloHasResponse = false;
            TimeoutManager.instance.ClearTimeout(m_SendHelloTimerId);
            SetTerminated();

            if (m_TcpClient == null)
            {
                return;
            }

            CloseTcpClient();

            netConnectionCallback.Call(NETEVENT.Close);
        }

        private void SetTerminated()
        {
            if (m_Sender != null)
                m_Sender.SetTerminated();

            if (m_Receiver != null)
                m_Receiver.SetTerminated();
        }

        private void ConnectOK()
        {
            Log.Debug("Connect ok");

            if (netState == NETSTATE.Connecting)
            {
                netState = NETSTATE.Connected;

                RemoveConnectTimer();
                StartSendThread();
                StartReceiveThread();

                EnableRetry(true);

                netConnectionCallback.Call(NETEVENT.Success);
            }
            else
            {
                Log.Debug("Connect ok ignore, net state = " + netState.ToString());
            }
        }

        private void ConnectFail()
        {
            Log.Debug("Connect fail");

            if (netState == NETSTATE.Connecting)
            {
                netState = NETSTATE.ConnectFailed;

                RemoveConnectTimer();

                if (--m_RetryTimes > 0)
                {
                    CloseTcpClient();
                    BeginRetryTimer();
                }
                else
                {
                    CloseConnect();
                    netConnectionCallback.Call(NETEVENT.RetryFail);
                }
            }
            else
            {
                Log.Debug("Connnect fail ignore, net state = " + netState.ToString());
            }
        }

        private void OnApplicationQuit()
        {
            StopConnectServer();
        }

        private void OnApplicationPause(bool isPause)
        {
            Log.Debug("Net connection OnApplicationPause = " + isPause.ToString());

            NotifyApplicationPause(isPause);
        }
    }
}
