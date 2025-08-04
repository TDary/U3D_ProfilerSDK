using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.MemoryProfiler.Editor;
using UnityEngine;

internal class SnapSDK
{
    private static SnapSDK _ProfilerAnalyzeSnap = null;
    string SnapPath;
    string ID;
    string ServerUrl;
    string SummaryJsonPath;
    string UnityObjectsJsonPath;
    string AllMemoryDatajsonPath;
    string FunReferenceDataPath;
    string Index;
    string UUid;

    static SnapSDK()
    {
        _ProfilerAnalyzeSnap = new SnapSDK();
    }

    private MemoryProfilerNoWindow mp_windows;

    public static SnapSDK Instance()
    {
        return _ProfilerAnalyzeSnap;
    }

    public void start(Dictionary<string, string> comd)
    {
        Debug.Log(" ProfilerAnalyzeSnap Init");
        SnapPath = comd.ContainsKey("-snapPath") ? comd["-snapPath"] : "";
        ID = comd.ContainsKey("-ID") ? comd["-ID"] : "0";
        ServerUrl = comd.ContainsKey("-ServerUrl") ? comd["-ServerUrl"] : "";
        SummaryJsonPath = comd.ContainsKey("-SummaryJson") ? comd["-SummaryJson"] : "";
        UnityObjectsJsonPath = comd.ContainsKey("-UnityObjectsJson") ? comd["-UnityObjectsJson"] : "";
        AllMemoryDatajsonPath = comd.ContainsKey("-AllMemoryDataJson") ? comd["-AllMemoryDataJson"] : "";
        FunReferenceDataPath = comd.ContainsKey("-FunReferenceData") ? comd["-FunReferenceData"] : "";
        Index = comd.ContainsKey("-Index") ? comd["-Index"] : "";
        UUid = comd.ContainsKey("-UUID") ? comd["-UUID"] : "";

        if (string.IsNullOrEmpty(Index))
        {
            Debug.LogWarning("Index为空");
            return;
        }
        else if (string.IsNullOrEmpty(UUid))
        {
            Debug.LogError("UUID为空----");
            return;
        }
        else if (string.IsNullOrEmpty(SummaryJsonPath))
        {
            Debug.LogError("SummaryJsonPath为空----");
            return;
        }
        else if (string.IsNullOrEmpty(UnityObjectsJsonPath))
        {
            Debug.LogError("UnityObjectsJsonPath为空----");
            return;
        }
        else if (string.IsNullOrEmpty(AllMemoryDatajsonPath))
        {
            Debug.LogError("AllMemoryDatajsonPath为空----");
            return;
        }
        else if (string.IsNullOrEmpty(SnapPath))
        {
            Debug.LogError("SnapFilePath为空----");
            return;
        }
        else if (string.IsNullOrEmpty(FunReferenceDataPath))
        {
            Debug.LogError("FunReferenceDataPath为空----");
            return;
        }
        else if (string.IsNullOrEmpty(ServerUrl))
        {
            Debug.LogError("ServerUrl为空----");
            return;
        }
        else
        {
            //
        }

        Debug.Log($"SnapPath:{SnapPath}");
        Debug.Log($"UUID：{UUid}");
        Debug.Log($"SummaryJsonPath：{SummaryJsonPath}");
        Debug.Log($"UnityObjectsJsonPath：{UnityObjectsJsonPath}");
        Debug.Log($"AllMemoryDatajsonPath：{AllMemoryDatajsonPath}");
        Debug.Log($"FunReferenceDataPath：{FunReferenceDataPath}");
        Debug.Log($"当前实时解析进程ID:{Index}");
        Debug.Log($"ID:{ID}");
        Debug.Log($"ServerUrl:{ServerUrl}");

        // 初始化MemoryProfiler模块
        mp_windows = new MemoryProfilerNoWindow();
        mp_windows.Init();

        if (IsFileInUsed(SnapPath))
        {
            Debug.LogError($"{SnapPath}该文件被占用中----");
            return;
        }
        else if (!IsFileHasData(SnapPath))
        {
            Debug.LogError($"{SnapPath}该文件大小异常----");
            return;
        }

        mp_windows.LoadedSnapshot(SnapPath);

        while (true)
        {
            if (mp_windows.IsLoadSuccess())
            {
                //Load完成，进行获取数据
                break;
            }
            Thread.Sleep(1);
        }
        GetParseData();

        Debug.Log("解析完成 ID：" + ID);
        string httprequest = ServerUrl + "snapreport?id=" + ID + "&result=1" + "&index=" + Index;
        string Response = SHttpSender.SendGet(httprequest);
        Debug.Log("解析完成上报 ID：" + ID + " 上报：" + httprequest + "  Response" + Response);
    }

    public void WriteResultFile(string filename, string data)
    {
        File.WriteAllText(filename, data);
    }

    /// 输出结果数据
    /// </summary>
    void GetParseData()
    {
        Debug.Log("输出结果数据----");
        try
        {
            var summaryJson = mp_windows.BuildSummaryData();
            var unityObjectsJson = mp_windows.BuildUnityObjectsData();
            var allMemoryJson = mp_windows.BuildAllMemoryData();
            WriteResultFile(SummaryJsonPath, summaryJson);
            WriteResultFile(UnityObjectsJsonPath, unityObjectsJson);
            WriteResultFile(AllMemoryDatajsonPath, allMemoryJson);
            mp_windows.BuildStackReferenceData(FunReferenceDataPath);
            Debug.Log("解析Memory完毕----");
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    /// <summary>
    /// 判断文件是否被占用中
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private bool IsFileInUsed(string filePath)
    {
        try
        {
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                Debug.Log($"File {filePath} is not in use");
                return false;
            }
        }
        catch (IOException)
        {
            Debug.LogError($"File {filePath} is in use");
            return true;
        }
    }

    /// <summary>
    /// 判断文件是否有数据，为0时证明没截到快照  进行跳过
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private bool IsFileHasData(string path)
    {
        FileInfo fileInfo = new FileInfo(path);

        if (fileInfo.Length == 0)
        {
            Debug.Log($"File {path} is empty");
            return false;
        }
        else
        {
            Debug.LogError($"File {path} is not empty");
            return true;
        }
    }
}