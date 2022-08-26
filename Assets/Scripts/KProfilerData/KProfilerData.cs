using UnityEngine;
using UnityEditorInternal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using Kingsoft.Test.BBF.Xml;
using System.Text;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.Profiling;
#endif
#if UNITY_2018_1_OR_NEWER
using UnityEditorInternal.Profiling;
#endif

namespace KProfilerData
{
    public class KRawDataFileList
    {
        public List<KRawDataFileInfo> fileInfoList;

        public KRawDataFileList()
        {
            this.fileInfoList = new List<KRawDataFileInfo>();
        }

        public void Add(KRawDataFileInfo fileInfo)
        {
            this.fileInfoList.Add(fileInfo);
        }

        public void AddRange(KRawDataFileList fileInfos)
        {
            this.fileInfoList.AddRange(fileInfos.fileInfoList);
        }

        public void Clear()
        {
            this.fileInfoList.Clear();
        }

        public int Count()
        {
            return this.fileInfoList.Count;
        }
    }

    public class KRawDataFileInfo
    {
        public int beginFrame;
        public int endFrame;
        public string fileNames;
    }

    public class KBaseDatas
    {
        public List<KBaseData> kBaseDataList;

        [KXmlIgnoreAttribute]
        public int size { get; set; }
        public KBaseDatas()
        {
            this.kBaseDataList = new List<KBaseData>();
        }

        public void Add(KBaseData data)
        {
            this.kBaseDataList.Add(data);
            this.size += data.size;
        }

        public void AddRange(KBaseDatas datas)
        {
            this.kBaseDataList.AddRange(datas.kBaseDataList);
            this.size += datas.size;
        }

        public void Clear()
        {
            this.kBaseDataList.Clear();
            this.size = 0;
        }

        public int Count()
        {
            return this.kBaseDataList.Count;
        }
    }

    public class KBaseData
    {
        public string frame { get; set; }

        public string frameFPS { get; set; }

        public string CPUTotalTime { get; set; }

        public string GPUTotalTime { get; set; }

        [KXmlIgnoreAttribute]
        private const int DEFAULT_VALUE = 0;

        [KXmlIgnoreAttribute]
        public int size { get; set; }

        public KBaseData()
        {
            this.frame = "-1";
        }

        public StringBuilder ToStringBuilder()
        {
            StringBuilder sb = new StringBuilder();
            string separator = "@@";
            /*string iniPath = Path.Combine(Application.dataPath, "Config.ini");
            if (INIReader.ExistINIFile(iniPath)) {
                separator = INIReader.ReadInivalue("Config", "separator", iniPath);
            }*/
            sb.Append("##base");
            sb.Append(separator);
            sb.Append(frame);//0
            sb.Append(separator);
            sb.Append(frameFPS);//1
            sb.Append(separator);
            sb.Append(CPUTotalTime);//2
            sb.Append(separator);
            sb.Append(GPUTotalTime);//3

            return sb;
        }
    }

    public class KAreaDatas
    {
        public List<KAreaData> kAreaDataList;

        [KXmlIgnoreAttribute]
        public int size { get; set; }

        public KAreaDatas()
        {
            this.kAreaDataList = new List<KAreaData>();
        }

        public void Add(KAreaData data)
        {
            this.kAreaDataList.Add(data);
            this.size += data.size;
        }

        public void AddRange(KAreaDatas datas)
        {
            this.kAreaDataList.AddRange(datas.kAreaDataList);
            this.size += datas.size;
        }

        public void Clear()
        {
            this.kAreaDataList.Clear();
            this.size = 0;
        }
    }

    public class KAreaData
    {
        public int frame { get; set; }

        public string str_Data { get; set; }

        [KXmlIgnoreAttribute]
        public int size { get; set; }

        public KAreaData()
        {
            this.frame = -1;
        }
    }

    public class KFunctionDatas
    {
        public List<KFunctionData> kFunctionStackData;

        [KXmlIgnoreAttribute]
        public int size { get; set; }
        public KFunctionDatas()
        {
            this.kFunctionStackData = new List<KFunctionData>();
        }

        public void Add(KFunctionData data)
        {
            this.kFunctionStackData.Add(data);
            this.size += data.size;
        }

        public void AddRange(KFunctionDatas datas)
        {
            this.kFunctionStackData.AddRange(datas.kFunctionStackData);
            this.size += datas.size;
        }

        public void Clear()
        {
            this.kFunctionStackData.Clear();
            this.size = 0;
        }

        public int Count()
        {
            return this.kFunctionStackData.Count;
        }
    }

    public class KTimeLineDatas
    {
        public List<KTimeLineData> kTimeLineData;

        [KXmlIgnoreAttribute]
        public int size { get; set; }
        public KTimeLineDatas()
        {
            this.kTimeLineData = new List<KTimeLineData>();
        }

        public void Add(KTimeLineData data)
        {
            this.kTimeLineData.Add(data);
            this.size += data.size;
        }

        public void AddRange(KTimeLineDatas datas)
        {
            this.kTimeLineData.AddRange(datas.kTimeLineData);
            this.size += datas.size;
        }

        public void Clear()
        {
            this.kTimeLineData.Clear();
            this.size = 0;
        }

        public int Count()
        {
            return this.kTimeLineData.Count;
        }
    }

    public class KFunctionData
    {
        public int frame;

        public string FunctionPath;
        public string FunctionName;
        public double TotalCPUPercent;

        public double SelfCPUPercent;
        public double Calls;
        public double GCMemory_KB;

        public double TotalTime_ms;
        public double SelfTime_ms;
        public double DrawCalls;
        public double TotalGPUTime_ms;
        public double SelfGPUTime_ms;
        public double TotalGPUPercent;
        public double SelfGPUPercent;
        public double WarningCount;
        public string ObjectName;

        [KXmlIgnoreAttribute]
        public int size { get; set; }
        public KFunctionData()
        {

        }

        public static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        public void FunctionStackData(int frame, ProfilerProperty profilerProperty, ref List<string> funcPathstack, ref Dictionary<string, string> funcnameReplace)
        {
            this.frame = frame;

            this.FunctionPath = profilerProperty.propertyPath.Replace("\n", "");
#if UNITY_2019_1_OR_NEWER
            this.FunctionName = profilerProperty.GetColumn(HierarchyFrameDataView.columnName).Replace("\n", "");
            this.TotalCPUPercent = ToDouble(profilerProperty.GetColumn(HierarchyFrameDataView.columnTotalPercent));
            this.SelfCPUPercent = ToDouble(profilerProperty.GetColumn(HierarchyFrameDataView.columnSelfPercent));
            this.Calls = ToDouble(profilerProperty.GetColumn(HierarchyFrameDataView.columnCalls));
            this.GCMemory_KB = ToMemory(profilerProperty.GetColumn(HierarchyFrameDataView.columnGcMemory));
            this.TotalTime_ms = ToDouble(profilerProperty.GetColumn(HierarchyFrameDataView.columnTotalTime));
            this.SelfTime_ms = ToDouble(profilerProperty.GetColumn(HierarchyFrameDataView.columnSelfTime));
            this.DrawCalls = ToDouble(profilerProperty.GetColumn(7));
            this.TotalGPUTime_ms = ToDouble(profilerProperty.GetColumn(8));
            this.SelfGPUTime_ms = ToDouble(profilerProperty.GetColumn(9));
            this.TotalGPUPercent = ToDouble(profilerProperty.GetColumn(10));
            this.SelfGPUPercent = ToDouble(profilerProperty.GetColumn(11));
            this.WarningCount = ToDouble(profilerProperty.GetColumn(HierarchyFrameDataView.columnWarningCount));
            this.ObjectName = profilerProperty.GetColumn(HierarchyFrameDataView.columnObjectName);
#else
            this.FunctionName = profilerProperty.GetColumn(ProfilerColumn.FunctionName).Replace("\n", "");
            this.TotalCPUPercent = ToDouble(profilerProperty.GetColumn(ProfilerColumn.TotalPercent));
            this.SelfCPUPercent = ToDouble(profilerProperty.GetColumn(ProfilerColumn.SelfPercent));
            this.Calls = ToDouble(profilerProperty.GetColumn(ProfilerColumn.Calls));
            this.GCMemory_KB = ToMemory(profilerProperty.GetColumn(ProfilerColumn.GCMemory));
            this.TotalTime_ms = ToDouble(profilerProperty.GetColumn(ProfilerColumn.TotalTime));
            this.SelfTime_ms = ToDouble(profilerProperty.GetColumn(ProfilerColumn.SelfTime));
            this.DrawCalls = ToDouble(profilerProperty.GetColumn(ProfilerColumn.DrawCalls));
            this.TotalGPUTime_ms = ToDouble(profilerProperty.GetColumn(ProfilerColumn.TotalGPUTime));
            this.SelfGPUTime_ms = ToDouble(profilerProperty.GetColumn(ProfilerColumn.SelfGPUTime));
            this.TotalGPUPercent = ToDouble(profilerProperty.GetColumn(ProfilerColumn.TotalGPUPercent));
            this.SelfGPUPercent = ToDouble(profilerProperty.GetColumn(ProfilerColumn.SelfGPUPercent));
            this.WarningCount = ToDouble(profilerProperty.GetColumn(ProfilerColumn.WarningCount));
            this.ObjectName = profilerProperty.GetColumn(ProfilerColumn.ObjectName);
#endif

            if (this.FunctionName.IndexOf("/") != -1)
            {
                if (!funcnameReplace.ContainsKey(this.FunctionName))
                {
                    string replaceName = this.FunctionName.Replace("/", "&");
                    funcnameReplace.Add(this.FunctionName, replaceName);
                    this.FunctionName = replaceName;
                }
                else
                {
                    this.FunctionName = funcnameReplace[this.FunctionName];
                }
            }

            foreach (var repitem in funcnameReplace)
            {
                this.FunctionPath = this.FunctionPath.Replace(repitem.Key, repitem.Value);
            }

            sw.Start();
            //由于函数名为空时会导致该函数以及所有子函数的path为空，现手动进行path拼接
            int depth = profilerProperty.depth;
            if (this.FunctionPath == "")
            {
                this.FunctionPath = string.Format("{0}/{1}", funcPathstack[depth - 2], this.FunctionName);
            }
            if (depth > funcPathstack.Count)
            {
                funcPathstack.Add(this.FunctionPath);
            }
            else
            {
                funcPathstack[depth - 1] = this.FunctionPath;
            }
            sw.Stop();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string separator = "@@";

            sb.Append(TotalCPUPercent);
            sb.Append(separator);
            sb.Append(SelfCPUPercent);
            sb.Append(separator);
            sb.Append(Calls);
            sb.Append(separator);
            sb.Append(GCMemory_KB);
            sb.Append(separator);
            sb.Append(TotalTime_ms);
            sb.Append(separator);
            sb.Append(SelfTime_ms);
            sb.Append(separator);
            sb.Append(DrawCalls);
            sb.Append(separator);
            sb.Append(TotalGPUTime_ms);
            sb.Append(separator);
            sb.Append(SelfGPUTime_ms);
            sb.Append(separator);
            sb.Append(TotalGPUPercent);
            sb.Append(separator);
            sb.Append(SelfGPUPercent);
            sb.Append(separator);
            sb.Append(WarningCount);
            sb.Append(separator);
            sb.Append(ObjectName);

            return sb.ToString();
        }

        private Double ToDouble(string dataStr)
        {
            if (string.IsNullOrEmpty(dataStr) || dataStr.Equals("N/A") || dataStr.Contains("-"))
            {
                return 0;
            }

            dataStr = Regex.Replace(dataStr, "[a-zA-Z]", "");

            dataStr = dataStr.Replace(" ", "");

            dataStr = dataStr.Replace("%", "");

            return double.Parse(dataStr);
        }

        private Double ToMemory(string dataStr)
        {

            double memory = 0;

            if (dataStr.Contains("GB"))
            {
                memory = ToDouble(dataStr) * 1024 * 1024;
            }
            else if (dataStr.Contains("MB"))
            {
                memory = ToDouble(dataStr) * 1024;
            }
            else if (dataStr.Contains("KB"))
            {
                memory = ToDouble(dataStr);
            }
            else if (dataStr.Contains("B"))
            {
                memory = ToDouble(dataStr) / 1024;
            }
            return memory;
        }
    }

    public class KTimeLineData
    {
        public int frame;

        public string functionPath;
        public string functionName;
        public string threadName;
        public string groupName;
        public float funcStartTime_ms;
        public float duration_ms;
        public int depth;
        public int threadID;
        public int GroupID;

        [KXmlIgnoreAttribute]
        public int size { get; set; }
        public KTimeLineData()
        {

        }

        public static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        public void TimeLineData(int frame, ProfilerFrameDataIterator profilerFrameDataIterator, int threadID, ref Dictionary<string, string> funcnameReplace, ref List<string> funcPathstack)
        {
            this.frame = frame;
            this.functionPath = profilerFrameDataIterator.path;
            this.functionName = profilerFrameDataIterator.name;
            this.threadName = profilerFrameDataIterator.GetThreadName();
#if (UNITY_5_3 || UNITY_5_3_OR_NEWER)
            this.groupName = profilerFrameDataIterator.GetGroupName();
#else
			this.groupName = profilerFrameDataIterator.group.ToString();
#endif
            this.funcStartTime_ms = profilerFrameDataIterator.startTimeMS;
            this.duration_ms = profilerFrameDataIterator.durationMS;
            this.depth = profilerFrameDataIterator.depth;
            this.threadID = threadID;
            this.GroupID = profilerFrameDataIterator.group;

            if (this.functionName.IndexOf("/") != -1)
            {
                if (!funcnameReplace.ContainsKey(this.functionName))
                {
                    string replaceName = this.functionName.Replace("/", "&");
                    funcnameReplace.Add(this.functionName, replaceName);
                    this.functionName = replaceName;
                }
                else
                {
                    this.functionName = funcnameReplace[this.functionName];
                }
            }

            foreach (var repitem in funcnameReplace)
            {
                this.functionPath = this.functionPath.Replace(repitem.Key, repitem.Value);
            }

            sw.Start();
            //由于函数名为空时会导致该函数以及所有子函数的path为空，现手动进行path拼接
            int depth = profilerFrameDataIterator.depth;
            if (this.functionPath == "")
            {
                this.functionPath = string.Format("{0}/{1}", funcPathstack[depth - 2], this.functionName);
            }
            if (depth > funcPathstack.Count)
            {
                funcPathstack.Add(this.functionPath);
            }
            else
            {
                funcPathstack[depth - 1] = this.functionPath;
            }
            sw.Stop();

        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string separator = "@@";

            sb.Append(frame);
            sb.Append(separator);
            sb.Append(functionName);
            sb.Append(separator);
            sb.Append(threadName);
            sb.Append(separator);
            sb.Append(groupName);
            sb.Append(separator);
            sb.Append(funcStartTime_ms);
            sb.Append(separator);
            sb.Append(duration_ms);
            sb.Append(separator);
            sb.Append(depth);
            sb.Append(separator);
            sb.Append(threadID);
            sb.Append(separator);
            sb.Append(GroupID);

            return sb.Replace("\n", "").ToString();
        }
    }
}
