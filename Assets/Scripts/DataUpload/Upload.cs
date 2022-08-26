using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Kingsoft.Test.BBF.Xml;
using KProfilerData;
using KStatisticData;
using System.Text;
using System.Net;
using System.Diagnostics;

public class UploadData
{
    [Kingsoft.Test.BBF.Xml.KXmlAttribute]
    public string Uuid = String.Empty;
    [Kingsoft.Test.BBF.Xml.KXmlAttribute]
    public string Game_id = String.Empty;

    [Kingsoft.Test.BBF.Xml.KXmlNodeList(HasRootNode = false, ChildNodeName = "RightPushDatas")]
    public List<RightPushData> RightPushDatas = new List<RightPushData>();
    
    [Kingsoft.Test.BBF.Xml.KXmlIgnore]
    private Dictionary<string, RightPushData> rightPush = null;

    public static void INIPythonStartInfo(ProcessStartInfo pythonStartInfo) {
        pythonStartInfo.FileName = INIReader.ReadInivalue("Config", "PythonPath");
        int enable_SilentMode = int.Parse(INIReader.ReadInivalue("Config", "SilentUpload"));

        if (pythonStartInfo.FileName.Equals(string.Empty)) {
            //没有找到Python路径
            return;
        }

        if (enable_SilentMode == 1) {
            pythonStartInfo.UseShellExecute = false;
            pythonStartInfo.CreateNoWindow = true;
        }
    }

    public void RightPush(string key, string val, int frame)
    {
        if (rightPush == null)
            rightPush = new Dictionary<string, RightPushData>();

        if (!rightPush.ContainsKey(key))
        {
            var data = new RightPushData()
            {
                mainData = key,
                FrameDatas = new List<RightPushFrameData>()
            };
            RightPushDatas.Add(data);
            rightPush[key] = data;
        }

        rightPush[key].FrameDatas.Add(new RightPushFrameData() { Frame = frame, Info = val });
    }

    public List<StringBuilder> GetReport()
    {
        List<StringBuilder> sbList = new List<StringBuilder>();
        string separator = "@@";
        /*string iniPath = Path.Combine(Application.dataPath, "Config.ini");
        if (INIReader.ExistINIFile(iniPath)) {
            separator = INIReader.ReadInivalue("Config", "separator", iniPath);
        }*/

        foreach (var rightPushData in RightPushDatas)
        {
            foreach (var frameData in rightPushData.FrameDatas)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(rightPushData.mainData.Replace("\n", ""));
                sb.Append("\t");
                sb.Append(frameData.Frame);
                sb.Append(separator);
                sb.Append(frameData.Info.Replace("\n", ""));
                sbList.Add(sb);
            }
        }
        return sbList;
    }

    public StringBuilder GetReportTimeLine() {
        StringBuilder sb = new StringBuilder();
        string separator = "@@";

        foreach (var rightPushData in RightPushDatas) {
            foreach (var frameData in rightPushData.FrameDatas) {
                sb.Append(frameData.Frame);
                sb.Append(separator);
                sb.AppendLine(frameData.Info);
            }
        }
        return sb;
    }
}

[KXmlElement(NodeName = ("AddToSetUploadData"))]
public class AddToSetDatas {
    public string Uuid = String.Empty;
    public string Game_id = String.Empty;

    public List<string> addToSetDatas = new List<string>();

    public void AddToSet(string key) {
        if (addToSetDatas.Contains(key))
            return;

        addToSetDatas.Add(key);
    }

    public StringBuilder GetReport() {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < addToSetDatas.Count; i++) {
            sb.AppendLine(addToSetDatas[i].Replace("\n", ""));
        }

        return sb;
    }
}

public class RightPushData
{
    public string mainData = String.Empty;

    [Kingsoft.Test.BBF.Xml.KXmlNodeList(HasRootNode = false, ChildNodeName = "Item")]
    public List<RightPushFrameData> FrameDatas { get; set; }
}

public class RightPushFrameData
{
    public int Frame = 0;
    public String Info = String.Empty;
}

public class Outputter
{
    private static void OutputRawData(RawAreaDataInfo info) {
        using (var sw = new StreamWriter(info.filePath, true))
        {
            sw.Write(info.sb_rawDatas);
        }
    }

    private static void OutputFunctionData(FunctionDataInfo info) {
        List<StringBuilder> sbList = info.functionUploadData.GetReport();
        using (var sw = new StreamWriter(info.filePath, true))
        {
            foreach (var sb in sbList)
            {
                sw.WriteLine(sb);
            }
        }
    }


    private static void OutputTimeLineData(TimeLineDataInfo info) {
        using (var sw = new StreamWriter(info.filePath, true)) {
            sw.Write(info.sb_timeLineDatas);
        }
    }

    public static void ThreadOutputAreaData(object areaDataInfo) {
        RawAreaDataInfo area_Info = (RawAreaDataInfo)areaDataInfo;

        area_Info.logger.Log(string.Format("Start output area datas, file: {0}", area_Info.filePath));
        try {
            OutputRawData(area_Info);
        }
        catch (Exception ex){
            //输出文件失败
            area_Info.logger.LogError(string.Format("Fail to output area data."));
            area_Info.logger.LogError(string.Format("ex: {0}", ex.ToString()));
            return;
        }
        area_Info.logger.Log(string.Format("Finish output datas, file: {0}", area_Info.filePath));

        int enable_upload = int.Parse(INIReader.ReadInivalue("Config", "enable_upload"));

        if (enable_upload == 1) {
            string uploadPath = INIReader.ReadInivalue("Config", "UploadPath");
            string game_id = INIReader.ReadInivalue("Config", "game_id");
            string uuid = INIReader.ReadInivalue("Config", "uuid");

            ProcessStartInfo psi = new ProcessStartInfo();
            UploadData.INIPythonStartInfo(psi);
            psi.Arguments = string.Format("{0} --filename={1} --game-id={2} --uuid={3}", Path.Combine(uploadPath, "base_rpush.py"), area_Info.filePath, game_id, uuid);
            Process.Start(psi).WaitForExit();
        }
    }

    public static void ThreadOutputFunctionData(object functionDataInfo)
    {
        FunctionDataInfo func_Info = (FunctionDataInfo)functionDataInfo;
        if (func_Info.functionUploadData == null) {
            func_Info.logger.LogError("FunctionUploadData is null");
            return;
        }
        if (string.IsNullOrEmpty(func_Info.filePath)) {
            func_Info.logger.LogError("FilePath is null");
            return;
        }

        func_Info.logger.Log(string.Format("Start output function datas, file: {0}", func_Info.filePath));
        try
        {
            OutputFunctionData(func_Info);
        }
        catch (Exception ex)
        {
            //输出文件失败
            func_Info.logger.LogError(string.Format("Fail to output function data."));
            func_Info.logger.LogError(string.Format("ex: {0}", ex.ToString()));
            return;
        }
        func_Info.logger.Log(string.Format("Finish output function data, file: {0}", func_Info.filePath));

        int enable_upload = int.Parse(INIReader.ReadInivalue("Config", "enable_upload"));

        if (enable_upload == 1) {
            string uploadPath = INIReader.ReadInivalue("Config", "UploadPath");
            string game_id = INIReader.ReadInivalue("Config", "game_id");
            string uuid = INIReader.ReadInivalue("Config", "uuid");

            ProcessStartInfo psi = new ProcessStartInfo();
            UploadData.INIPythonStartInfo(psi);
            psi.Arguments = string.Format("{0} --filename={1} --game-id={2} --uuid={3}", Path.Combine(uploadPath, "function_rpush.py"), func_Info.filePath, game_id, uuid);
            Process.Start(psi).WaitForExit();
        }
    }

    public static void ThreadOutputTimeLineData(object timeLineDataInfo) {
        TimeLineDataInfo timeLine_Info = (TimeLineDataInfo)timeLineDataInfo;

        try {
            OutputTimeLineData(timeLine_Info);
        }
        catch (Exception ex) {
            //输出文件失败
            timeLine_Info.logger.LogError(string.Format("Fail to output TimeLine data."));
            timeLine_Info.logger.LogError(string.Format("ex: {0}", ex.ToString()));
            return;
        }
        timeLine_Info.logger.Log(string.Format("Finish output datas, file: {0}", timeLine_Info.filePath));
    }

    public static void UploadTimeLineData(object timeLineDataInfo) {
        TimeLineDataInfo timeLine_Info = (TimeLineDataInfo)timeLineDataInfo;

        int enable_upload = int.Parse(INIReader.ReadInivalue("Config", "enable_upload"));

        if (enable_upload == 1) {
            string uploadPath = INIReader.ReadInivalue("Config", "UploadPath");
            string game_id = INIReader.ReadInivalue("Config", "game_id");
            string uuid = INIReader.ReadInivalue("Config", "uuid");

            ProcessStartInfo psi = new ProcessStartInfo();
            UploadData.INIPythonStartInfo(psi);
            psi.Arguments = string.Format("{0} --filename={1} --game-id={2} --uuid={3}", Path.Combine(uploadPath, "threads_timeline.py"), timeLine_Info.filePath, game_id, uuid);
            Process.Start(psi).WaitForExit();
        }
    }
}

public class RawAreaDataInfo {
    public string filePath = "";
    public StringBuilder sb_rawDatas = null;
    public Logger logger = null;
}

public class FunctionDataInfo {
    public string filePath = "";
    public UploadData functionUploadData = null;
    public Logger logger = null;
}

public class TimeLineDataInfo {
    public string filePath = "";
    public StringBuilder sb_timeLineDatas = null;
    public Logger logger = null;
}
