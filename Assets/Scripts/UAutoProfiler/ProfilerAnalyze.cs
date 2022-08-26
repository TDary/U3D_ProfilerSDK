using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProfilerAnalyze : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    static Dictionary<string, string> GetInputCommand(string[] args)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        string key = string.Empty;
        foreach (var item in args)
        {
            if (item.StartsWith("-"))
            {
                key = item.Trim();
            }
            else if (!string.IsNullOrEmpty(item) && !string.IsNullOrEmpty(key) && !result.ContainsKey(key))
            {
                result.Add(key, item.Trim());
            }
        }
        return result;
    }

    public static void AnalyzeSnap()
    {
        Dictionary<string, string> command = GetInputCommand(System.Environment.GetCommandLineArgs());

        try
        {
            ProfilerAnalyzeSnap.Instance().Init(command);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            string Index = command.ContainsKey("-Index") ? command["-Index"] : "";
            string ID = command.ContainsKey("-ID") ? command["-ID"] : "0";
            string ServerUrl = command.ContainsKey("-ServerUrl") ? command["-ServerUrl"] : "";
            string httprequest = ServerUrl + "snapstartreport?id=" + ID + "&result=0" + "&index=" + Index;
            string Response = MHttpSender.SendGet(httprequest);

            Debug.Log("解析异常 上报：" + httprequest + "  Response:" + Response);
        }
    }

    public static void Analyze()
    {
        Dictionary<string, string> command = GetInputCommand(System.Environment.GetCommandLineArgs());

        try
        {
            ProfilerAnalyzeSDK.Instance().Init(command);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            string Index = command.ContainsKey("-Index") ? command["-Index"] : "";
            string ID = command.ContainsKey("-ID") ? command["-ID"] : "0";
            string ServerUrl = command.ContainsKey("-ServerUrl") ? command["-ServerUrl"] : "";
            string httprequest = ServerUrl + "taskreport?id=" + ID + "&result=0" + "&index=" + Index;
            string Response = MHttpSender.SendGet(httprequest);

            Debug.Log("解析异常 上报：" + httprequest + "  Response:" + Response);
        }

    }

}
