using System.Collections.Generic;
using UnityEngine;

internal class SnapSDK
{
    private static SnapSDK _ProfilerAnalyzeSnap = null;
    string SnapPath;
    string ID;
    string ServerUrl;
    string SnapNativeJsonPath;
    string SnapManageJsonPath;
    string Index;

    static SnapSDK()
    {
        _ProfilerAnalyzeSnap = new SnapSDK();
    }

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
        SnapNativeJsonPath = comd.ContainsKey("-snapnativeJsonPath") ? comd["-snapnativeJsonPath"] : "";
        SnapManageJsonPath = comd.ContainsKey("-snapmanageJsonPath") ? comd["-snapmanageJsonPath"] : "";
        Index = comd.ContainsKey("-Index") ? comd["-Index"] : "";

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
        string Response = SHttpSender.SendGet(httprequest);
        Debug.Log("解析完成上报 ID：" + ID + " 上报：" + httprequest + "  Response" + Response);
    }
}