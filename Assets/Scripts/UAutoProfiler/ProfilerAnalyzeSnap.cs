using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.MemoryProfiler;
using UnityEngine;

class ProfilerAnalyzeSnap
{
    private static ProfilerAnalyzeSnap _ProfilerAnalyzeSnap = null;
    string SnapPath;
    string ID;
    string ServerUrl;
    string SnapNativeJsonPath;
    string SnapManageJsonPath;
    string Index;

    static ProfilerAnalyzeSnap()
    {
        _ProfilerAnalyzeSnap = new ProfilerAnalyzeSnap();
    }

    public static ProfilerAnalyzeSnap Instance()
    {
        return _ProfilerAnalyzeSnap;
    }

    public void Init(Dictionary<string, string> command)
    {
        Debug.Log(" ProfilerAnalyzeSnap Init");
        SnapPath = command.ContainsKey("-snapPath") ? command["-snapPath"] : "";
        ID = command.ContainsKey("-ID") ? command["-ID"] : "0";
        ServerUrl = command.ContainsKey("-ServerUrl") ? command["-ServerUrl"] : "";
        SnapNativeJsonPath = command.ContainsKey("-snapnativeJsonPath") ? command["-snapnativeJsonPath"] : "";
        SnapManageJsonPath = command.ContainsKey("-snapmanageJsonPath") ? command["-snapmanageJsonPath"] : "";
        Index = command.ContainsKey("-Index") ? command["-Index"] : "";

        if (SnapPath == "")
        {
            Debug.LogWarning("Snap文件为空");
            return;
        }

        Debug.Log("SnapPath:" + SnapPath);
        Debug.Log("SnapManageJsonPath:" + SnapManageJsonPath);
        Debug.Log("SnapNativeJsonPath:" + SnapNativeJsonPath);
        Debug.Log("当前实时解析进程ID" + Index);
        Debug.Log("ID:" + ID);

        //var p = UnityEditor.Profiling.Memory.Experimental.PackedMemorySnapshot.Load(SnapPath);
        //SnapshotUtil.ConvertMemorySnapshotIntoJson(new PackedMemorySnapshot(p), frame, UUID);

        Debug.Log("解析完成 ID：" + ID);
        string httprequest = ServerUrl + "snapreport?id=" + ID + "&result=1" + "&index=" + Index;
        string Response = MHttpSender.SendGet(httprequest);
        Debug.Log("解析完成上报 ID：" + ID + " 上报：" + httprequest + "  Response" + Response);

    }
}
