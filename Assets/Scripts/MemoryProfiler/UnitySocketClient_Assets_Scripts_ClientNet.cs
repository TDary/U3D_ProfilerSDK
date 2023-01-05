using System;
using UnityEngine;

using System.Net.Sockets;

public class ClientNet : MonoBehaviour
{
    private void Awake()
    {
        m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_readOffset = 0;
        m_recvOffset = 0;
        // 16KB
        m_recvBuf = new byte[0x4000];
    }

    private void Update()
    {
        if (null == m_socket) return;
        if (m_connectState == ConnectState.Ing && m_connectAsync.IsCompleted)
        {
            // 连接服务器失败
            if (!m_socket.Connected)
            {
                m_connectState = ConnectState.None;
                if (null != m_connectCb)
                    m_connectCb(false);
            }
        }

        if (m_connectState == ConnectState.Ok)
        {
            TryRecvMsg();
        }
    }

    private void TryRecvMsg()
    {
        // 开始接收消息
        m_socket.BeginReceive(m_recvBuf, m_recvOffset, m_recvBuf.Length - m_recvOffset, SocketFlags.None, (result) =>
        {
            // 如果有消息，会进入这个回调

            // 这个len是读取到的长度，它不一定是一个完整的消息的长度，我们下面需要解析头部两个字节作为真实的消息长度
            var len = m_socket.EndReceive(result);

            if (len > 0)
            {
                m_recvOffset += len;
                m_readOffset = 0;

                if (m_recvOffset - m_readOffset >= 2)
                {
                    // 头两个字节是真实消息长度，注意字节顺序是大端
                    int msgLen = m_recvBuf[m_readOffset + 1] | (m_recvBuf[m_readOffset] << 8);

                    if (m_recvOffset >= (m_readOffset + 2 + msgLen))
                    {
                        // 解析消息
                        string msg = System.Text.Encoding.UTF8.GetString(m_recvBuf, m_readOffset + 2, msgLen);
                        Debug.Log("Recv msgLen: " + msgLen + ", msg: " + msg);
                        if (null != m_recvMsgCb)
                            m_recvMsgCb(msg);

                        m_readOffset += 2 + msgLen;
                    }
                }

                // buf移位
                if (m_readOffset > 0)
                {
                    for (int i = m_readOffset; i < m_recvOffset; ++i)
                    {
                        m_recvBuf[i - m_readOffset] = m_recvBuf[i];
                    }
                    m_recvOffset -= m_readOffset;
                }
            }
        }, this);
    }

    /// <summary>
    /// 连接服务端
    /// </summary>
    /// <param name="host">IP地址</param>
    /// <param name="port">端口</param>
    /// <param name="cb">回调</param>
    public void Connect(string host, int port, Action<bool> cb)
    {
        m_connectCb = cb;
        m_connectState = ConnectState.Ing;
        m_socket.SendTimeout = 100;
        m_connectAsync = m_socket.BeginConnect(host, port, (IAsyncResult result) =>
        {
            // 连接成功会进入这里，连接失败不会进入这里
            var socket = result.AsyncState as Socket;
            socket.EndConnect(result);
            m_connectState = ConnectState.Ok;
            m_networkStream = new NetworkStream(m_socket);
            Debug.Log("Connect Ok");
            if (null != m_connectCb) m_connectCb(true);
        }, m_socket);

        Debug.Log("BeginConnect, Host: " + host + ", Port: " + port);
    }

    /// <summary>
    /// 注册消息接收回调函数
    /// </summary>
    /// <param name="cb">回调函数</param>
    public void RegistRecvMsgCb(Action<string> cb)
    {
        m_recvMsgCb = cb;
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="bytes">消息的字节流</param>
    public void SendData(byte[] bytes)
    {
        m_networkStream.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// 关闭Sockete
    /// </summary>
    public void CloseSocket()
    {
        m_socket.Shutdown(SocketShutdown.Both);
        m_socket.Close();
    }

    /// <summary>
    /// 判断Socket是否连接状态
    /// </summary>
    /// <returns></returns>
    public bool IsConnected()
    {
        return m_socket.Connected;
    }

    private enum ConnectState
    {
        None,
        Ing,
        Ok,
    }

    private Action<bool> m_connectCb;
    private Action<string> m_recvMsgCb;
    private ConnectState m_connectState = ConnectState.None;
    private IAsyncResult m_connectAsync;

    private byte[] m_recvBuf;
    private int m_readOffset;
    private int m_recvOffset;
    private Socket m_socket;
    private NetworkStream m_networkStream;

    private static ClientNet s_instance;
    public static ClientNet instance
    {
        get
        {
            if (null == s_instance)
            {
                var go = new GameObject("ClientNet");
                s_instance = go.AddComponent<ClientNet>();
            }

            return s_instance;
        }
    }
}
