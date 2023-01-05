using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public Button sendBtn;
    public InputField inputField;
    public Text chatText;

    private Queue<string> m_msgQueue;

    private void Awake()
    {
        m_msgQueue = new Queue<string>();
    }

    void Start()
    {
        // 注册消息回调
        ClientNet.instance.RegistRecvMsgCb((msg) =>
        {
            // 把消息缓存到队列中，注意不要在这里直接操作UI对象
            m_msgQueue.Enqueue(msg);
        });

        // 连接服务端
        ClientNet.instance.Connect("127.0.0.1", 8888, (ok) =>
         {
             Debug.Log("连接服务器, ok: " + ok);
         });

        sendBtn.onClick.AddListener(SendMsg);

    }

    /// <summary>
    /// 发送消息
    /// </summary>
    private void SendMsg()
    {
        if (ClientNet.instance.IsConnected())
        {
            // 把字符串转成字节流
            byte[] data = System.Text.Encoding.UTF8.GetBytes(inputField.text + "\n");
            // 发送给服务端
            ClientNet.instance.SendData(data);
            // 清空输入框文本
            inputField.text = "";
        }
        else
        {
            Debug.LogError("你还没连接服务器");
        }
    }

    private void Update()
    {
        if (m_msgQueue.Count > 0)
        {
            // 从消息队列中取消息，并更新到聊天文本中
            chatText.text += m_msgQueue.Dequeue() + "\n";
        }

        // 按回车键，发送消息
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendMsg();
        }
    }

    private void OnDestroy() {
        ClientNet.instance.CloseSocket();
    }
}
