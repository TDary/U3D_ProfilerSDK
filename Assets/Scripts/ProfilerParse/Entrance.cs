using Analyze;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            string ID = comd.ContainsKey("-ID") ? comd["-ID"] : "0";
            Debug.Log("解析异常 上报----");
        }

    }

    public static void EntranceGetMemory()
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
}
