using Analyze;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

internal class Entrance : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void EntranceParseBegin()
    {
        Dictionary<string, string> comd = GetInputCommand(System.Environment.GetCommandLineArgs());

        try
        {
            AnalyzeSDK.instance().start(comd);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            string ServerUrl = comd.ContainsKey("-ServerUrl") ? comd["-ServerUrl"] : "";
            string UUID = comd.ContainsKey("-UUID") ? comd["-UUID"] : "";
            string RawPath = comd.ContainsKey("-rawPath") ? comd["-rawPath"] : "";
            string AnalyzeType = comd.ContainsKey("-analyzeType") ? comd["-analyzeType"] : "";
            var data = RawPath.Split('/');
            StringBuilder stringdata = new StringBuilder();
            stringdata.Append("http://");
            stringdata.Append(ServerUrl);
            stringdata.Append("/stop?uuid=");
            stringdata.Append(UUID);
            stringdata.Append("&rawfile=");
            stringdata.Append(data[data.Length - 1]);
            stringdata.Append("&anaType=");
            stringdata.Append(AnalyzeType);
            stringdata.Append("&state=");
            stringdata.Append("failed");
            string Response = SHttpSender.SendGet(stringdata.ToString());
            Debug.Log("解析异常上报----Response" + Response);
            //Todo:写一个失败的文件，不使用http消息作为基准了
        }
    }

    public static void EntranceGetSimpleMemory()
    {
        Dictionary<string, string> comd = GetInputCommand(System.Environment.GetCommandLineArgs());
        string get_ip = string.Empty;
        string filePath = string.Empty;
        get_ip = comd.ContainsKey("-IP") ? comd["-IP"] : "";
        filePath = comd.ContainsKey("-FilePath") ? comd["-FilePath"] : "";
        try
        {
            if (!string.IsNullOrEmpty(get_ip))
            {
                GetMemByTools.instance().ExtractMemoryDetailed(get_ip,filePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            string ID = comd.ContainsKey("-ID") ? comd["-ID"] : "0";
            Debug.Log("异常 上报----");
        }
    }

    public static void EntranceParseSnapBegin()
    {
        Dictionary<string, string> comd = GetInputCommand(System.Environment.GetCommandLineArgs());
        try
        {
            SnapSDK.Instance().start(comd);
        }
        catch (Exception ex){
            //todo:写失败的异常文件
            Debug.LogError(ex.ToString());
            Debug.Log("异常 上报----");
        }
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
}
