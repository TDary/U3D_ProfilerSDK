using System;
using System.IO;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class Logger
{
    private string logPath = "";
    private Thread logListener = null;
    private bool isWork = false;
    private Queue<string> logQueue = new Queue<string>();
    public Logger(string logPath)
    {
        this.logPath = logPath;
        this.isWork = true;
        logListener = new Thread(Listener);
        logListener.Start();
    }
    private void Listener()
    {
        while (isWork)
        {
            while (logQueue.Count > 0)
            {
                string content = "";
                lock (this)
                {
                    content = logQueue.Dequeue();
                }
                if (string.IsNullOrEmpty(logPath))
                {
                    Debug.Log("log路径为空！");
                    isWork = false;
                    return;
                }
                using (StreamWriter sw = new StreamWriter(logPath, true))
                {
                    sw.WriteLine(content);
                }
            }
            Thread.Sleep(1000);
        }
    }
    public void Log(string content)
    {
        if (string.IsNullOrEmpty(logPath))
        {
            Debug.Log("log路径为空！");
            return;
        }
        string logContent = string.Format("[{0}][Info]{1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), content);
        lock (this)
        {
            logQueue.Enqueue(logContent);
        }
    }
    public void LogError(string content)
    {
        if (string.IsNullOrEmpty(logPath))
        {
            Debug.Log("log路径为空！");
            return;
        }
        string logContent = string.Format("[{0}][Error]{1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), content);
        lock (this)
        {
            logQueue.Enqueue(logContent);
        }
    }
    public void Close()
    {
        isWork = false;
        logListener.Join();
        while (logQueue.Count > 0)
        {
            string content = "";
            lock (this)
            {
                content = logQueue.Dequeue();
            }
            if (string.IsNullOrEmpty(logPath))
            {
                Debug.Log("log路径为空！");
                isWork = false;
                break;
            }
            using (StreamWriter sw = new StreamWriter(logPath, true))
            {
                sw.WriteLine(content);
            }
        }
        logPath = "";
    }
}
