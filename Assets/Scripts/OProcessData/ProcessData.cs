using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using OProfilerData;
using Tools;
using UnityEngine.Profiling;

namespace OProcessedData
{
    internal class TotalProfilerData
    {
        public BaseData baseData;
        public AreaData cpuData;
        public AreaData gpuData;
        public AreaData renderingData;
        public AreaData memoryData;
        public AreaData physicsData;
        public FunctionDatas functionData;
        public TimeLineDatas timeLineData;

        public TotalProfilerData()
        {
            this.baseData = new BaseData();
            this.cpuData = new AreaData();
            this.gpuData = new AreaData();
            this.renderingData = new AreaData();
            this.memoryData = new AreaData();
            this.physicsData = new AreaData();
            this.functionData = new FunctionDatas();
            this.timeLineData = new TimeLineDatas();
        }
    }
    public class AreaData
    {

        public ODataDict ProfilerDict;
        public ProfilerArea profilerArea;
        public AreaData()
        {
            this.ProfilerDict = null;
        }

        public AreaData(string AreaData, ProfilerArea profilerArea)
        {

            this.profilerArea = profilerArea;

            try
            {
                AreaDataToDict(AreaData, profilerArea);
            }
            catch
            {
                ProfilerDict = null;
            }
        }

        public void AreaDataToDict(string dataStr, ProfilerArea profilerArea)
        {

            if (ProfilerDict == null)
            {
                ProfilerDict = new ODataDict();
            }
            else
            {
                ProfilerDict.Clear();
            }

            string[] cacheData = dataStr.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < cacheData.Length; i++)
            {

                if (profilerArea.Equals(ProfilerArea.Rendering))
                {
                    RenderingDataToDict(cacheData[i]);
                }
                else if (profilerArea.Equals(ProfilerArea.Memory))
                {
                    MemoryDataToDict(cacheData[i]);
                }
                else
                {
                    if (ProfilerDict.DataDict == null)
                    {
                        ProfilerDict.DataDict = new Dictionary<string, string>();
                    }

                    string[] kv = cacheData[i].Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

                    if (!ProfilerDict.DataDict.ContainsKey(kv[0]))
                    {
                        if (kv.Length == 2)
                        {
                            ProfilerDict.DataDict.Add(kv[0], kv[1]);
                        }
                        else
                        {
                            ProfilerDict.DataDict.Add(kv[0], "0");
                        }
                    }
                }
            }
        }

        private void RenderingDataToDict(string dataStr)
        {

            if (dataStr.Contains("(") && dataStr.Contains(")") && dataStr.Contains("\t"))
            {
                if (ProfilerDict.IndexDict == null)
                {
                    ProfilerDict.IndexDict = new Dictionary<string, ODataDict>();
                }

                ODataDict cacheKDataDict = new ODataDict();

                cacheKDataDict.DataDict = new Dictionary<string, string>();

                string[] ChildData = dataStr.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);

                ChildData[0] = ChildData[0].Trim();
                ChildData[0] = ChildData[0].Trim("()".ToCharArray());

                for (int i = 1; i < ChildData.Length; i++)
                {
                    string[] kv = ChildData[i].Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

                    kv[1] = kv[1].Trim();

                    if (!cacheKDataDict.DataDict.ContainsKey(kv[0]))
                    {
                        if (kv.Length == 2)
                        {
                            cacheKDataDict.DataDict.Add(kv[0], kv[1]);
                        }
                        else
                        {
                            cacheKDataDict.DataDict.Add(kv[0], "0");
                        }
                    }

                }
                if (!ProfilerDict.IndexDict.ContainsKey(ChildData[0]))
                {
                    ProfilerDict.IndexDict.Add(ChildData[0], cacheKDataDict);
                }
            }
            else
            {
                if (ProfilerDict.DataDict == null)
                {
                    ProfilerDict.DataDict = new Dictionary<string, string>();
                }

                string[] cacheChildData = dataStr.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);

                for (int j = 0; j < cacheChildData.Length; j++)
                {

                    //cacheChildData[j] = cacheChildData[j].Replace("\t", "");
                    if (!cacheChildData[j].Contains(": "))
                    {
                        continue;
                    }

                    string[] kv = cacheChildData[j].Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

                    kv[1] = kv[1].Trim();

                    if (!ProfilerDict.DataDict.ContainsKey(kv[0]))
                    {
                        if (kv.Length == 2)
                        {
                            ProfilerDict.DataDict.Add(kv[0], kv[1]);
                        }
                        else
                        {
                            ProfilerDict.DataDict.Add(kv[0], "0");
                        }
                    }
                }
            }
        }

        private void MemoryDataToDict(string dataStr)
        {

            if (dataStr.Contains("(WP8) ") || dataStr.Contains("Used ") || dataStr.Contains("Reserved "))
            {

                if (ProfilerDict.IndexDict == null)
                {
                    ProfilerDict.IndexDict = new Dictionary<string, ODataDict>();
                }

                ODataDict cacheKDataDict = new ODataDict();

                cacheKDataDict.DataDict = new Dictionary<string, string>();

                string parentLabel = "";

                if (dataStr.Contains("(WP8) "))
                {
                    parentLabel = "(WP8) ";
                }
                else if (dataStr.Contains("Used "))
                {
                    parentLabel = "Used ";
                }
                else if (dataStr.Contains("Reserved "))
                {
                    parentLabel = "Reserved ";
                }

                string[] ChildData = dataStr.Split(new string[] { "   " }, StringSplitOptions.RemoveEmptyEntries);
#if UNITY_2020_2_OR_NEWER
                string rememname = "";
                string rememvalue = "";
#endif
                for (int i = 0; i < ChildData.Length; i++)
                {
                    ChildData[i] = ChildData[i].Replace(parentLabel, "");

                    string[] kv = ChildData[i].Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

                    if (!cacheKDataDict.DataDict.ContainsKey(kv[0]))
                    {
                        if (kv.Length == 2)
                        {
#if UNITY_2020_2_OR_NEWER
                            rememname = kv[0];
                            rememvalue = kv[1];
#endif
                            cacheKDataDict.DataDict.Add(kv[0], kv[1]);
                        }
                        else
                        {
                            cacheKDataDict.DataDict.Add(kv[0], "0");
                        }
                    }
                }
                parentLabel = parentLabel.Trim();
                parentLabel = parentLabel.Trim("()".ToCharArray());

                if (!ProfilerDict.IndexDict.ContainsKey(parentLabel))
                {
                    ProfilerDict.IndexDict.Add(parentLabel, cacheKDataDict);
                }
#if UNITY_2020_2_OR_NEWER
                if (!ProfilerDict.IndexDict[parentLabel].DataDict.ContainsKey(rememname))
                {
                    ProfilerDict.IndexDict[parentLabel].DataDict.Add(rememname, rememvalue);
                }
#endif
            }
            else
            {
                if (ProfilerDict.DataDict == null)
                {
                    ProfilerDict.DataDict = new Dictionary<string, string>();
                }

                string[] cacheData = dataStr.Split(new string[] { "   " }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < cacheData.Length; i++)
                {
                    string[] kv = cacheData[i].Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

                    if (!ProfilerDict.DataDict.ContainsKey(kv[0]))
                    {
                        if (kv.Length == 2)
                        {
                            ProfilerDict.DataDict.Add(kv[0], kv[1]);
                        }
                        else
                        {
                            ProfilerDict.DataDict.Add(kv[0], "0");
                        }
                    }
                }
            }
        }

        public string ToStringBuilder()
        {
            StringBuilder sb = new StringBuilder();
            string separator = "@@";
            ODataDict defaultDict = new ODataDict();

            //遍历字典
            switch (profilerArea)
            {
                case ProfilerArea.CPU:
                    {
                        #region CPU data to StringBuilder
                        sb.Append("##cpu");
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Rendering", "0")));//0
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Scripts", "0")));//1
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Physics", "0")));//2
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("GarbageCollector", "0")));//3
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("VSync", "0")));//4
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Gi", "0")));//5
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Others", "0")));//6
                        sb.Append(separator);
                        //Unity2018后加入字段Animation
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Animation", "0")));//7
                        //Unity2017.4后加入字段UI
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("UI", "0")));//8
                        #endregion
                        break;
                    }
                case ProfilerArea.GPU:
                    {
                        #region GPU data to StringBuilder
                        sb.Append("##gpu");
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Opaque", "0")));//0
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Transparent", "0")));//1
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Shadows/Depth", "0")));//2
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Deferred PrePass", "0")));//3
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Deferred Lighting", "0")));//4
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("PostProcess", "0")));//5
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Other", "0")));//6
                        #endregion
                        break;
                    }
                case ProfilerArea.Rendering:
                    {
                        #region Rendering data to StringBuilder
                        string[] complexData = new string[2];

                        sb.Append("##rendering");
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("SetPass Calls", "0")));//0
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Draw Calls", "0")));//1
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Total Batches", "0")));//2
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Tris", "0")));//3
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Verts", "0")));//4
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.IndexDict.GetValue("Dynamic Batching", defaultDict).DataDict.GetValue("Batched Draw Calls", "0")));//5
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.IndexDict.GetValue("Dynamic Batching", defaultDict).DataDict.GetValue("Batches", "0")));//6
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.IndexDict.GetValue("Dynamic Batching", defaultDict).DataDict.GetValue("Tris", "0")));//7
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.IndexDict.GetValue("Dynamic Batching", defaultDict).DataDict.GetValue("Verts", "0")));//8
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.IndexDict.GetValue("Static Batching", defaultDict).DataDict.GetValue("Batched Draw Calls", "0")));//9
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.IndexDict.GetValue("Static Batching", defaultDict).DataDict.GetValue("Batches", "0")));//10
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.IndexDict.GetValue("Static Batching", defaultDict).DataDict.GetValue("Tris", "0")));//11
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.IndexDict.GetValue("Static Batching", defaultDict).DataDict.GetValue("Verts", "0")));//12
                        sb.Append(separator);

#if UNITY_4
                        //unity4，对比unity5分隔符由‘-’改成‘/’
                        complexData = KProfilerDict.DataDict.GetValue("Used Textures", "0 / 0").Split(new string[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
#else
                        complexData = ProfilerDict.DataDict.GetValue("Used Textures", "0 - 0").Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
#endif
                        sb.Append(WipeOffUnit(complexData[0]));//13
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(complexData[1]));//14
                        sb.Append(separator);

#if UNITY_4
                        //unity4，对比unity5分隔符由‘-’改成‘/’
                        complexData = KProfilerDict.DataDict.GetValue("RenderTextures", "0 / 0").Split(new string[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
#else
                        complexData = ProfilerDict.DataDict.GetValue("RenderTextures", "0 - 0").Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
#endif
                        sb.Append(WipeOffUnit(complexData[0]));//15
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(complexData[1]));//16
                        sb.Append(separator);

                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("RenderTexture Switches", "0")));//17
                        sb.Append(separator);

#if UNITY_4
                        //unity4，对比unity5分隔符由‘-’改成‘/’
                        complexData = KProfilerDict.DataDict.GetValue("Screen", "0 / 0").Split(new string[] { " / " }, StringSplitOptions.RemoveEmptyEntries);	
#else
                        complexData = ProfilerDict.DataDict.GetValue("Screen", "0 - 0").Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
#endif
                        sb.Append(complexData[0].Trim());//18
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(complexData[1]));//19
                        sb.Append(separator);

                        complexData = ProfilerDict.DataDict.GetValue("VBO Total", "0 - 0").Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                        sb.Append(WipeOffUnit(complexData[0]));//20
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(complexData[1]));//21
                        sb.Append(separator);

                        complexData = ProfilerDict.DataDict.GetValue("VB Uploads", "0 - 0").Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                        sb.Append(WipeOffUnit(complexData[0]));//22
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(complexData[1]));//23
                        sb.Append(separator);

                        complexData = ProfilerDict.DataDict.GetValue("IB Uploads", "0 - 0").Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                        sb.Append(WipeOffUnit(complexData[0]));//24
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(complexData[1]));//25
                        sb.Append(separator);

                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Shadow Casters", "0")));//26
                        sb.Append(separator);

                        string[] tempData = ProfilerDict.DataDict["VRAM usage"].Replace(")", "").Split(new string[] { "(of" }, StringSplitOptions.RemoveEmptyEntries);
                        complexData = tempData[0].Split(' ');
                        sb.Append(ToMemory_MB(string.Format("{0}{1}", complexData[0], complexData[1])));//27
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(string.Format("{0}{1}", complexData[3], complexData[4])));//28
                        sb.Append(separator);
                        if (tempData[1].ToLower().Contains("unknown"))
                        {
                            sb.Append("0");//29
                        }
                        else
                        {
                            sb.Append(ToMemory_MB(tempData[1]));//29
                        }
                        #endregion
                        break;
                    }
                case ProfilerArea.Memory:
                    {
                        #region Memory data to StringBuilder
                        string[] complexData = new string[2];

                        sb.Append("##memory");
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.IndexDict.GetValue("Used", defaultDict).DataDict.GetValue("Total", "0")));//0
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.IndexDict.GetValue("Used", defaultDict).DataDict.GetValue("Unity", "0")));//1
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.IndexDict.GetValue("Used", defaultDict).DataDict.GetValue("Mono", "0")));//2
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.IndexDict.GetValue("Used", defaultDict).DataDict.GetValue("GfxDriver", "0")));//3
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.IndexDict.GetValue("Used", defaultDict).DataDict.GetValue("FMOD", "0")));//4
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.IndexDict.GetValue("Used", defaultDict).DataDict.GetValue("Profiler", "0")));//5
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.IndexDict.GetValue("Reserved", defaultDict).DataDict.GetValue("Total", "0")));//6
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.IndexDict.GetValue("Reserved", defaultDict).DataDict.GetValue("Unity", "0")));//7
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.IndexDict.GetValue("Reserved", defaultDict).DataDict.GetValue("Mono", "0")));//8
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.IndexDict.GetValue("Reserved", defaultDict).DataDict.GetValue("GfxDriver", "0")));//9
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.IndexDict.GetValue("Reserved", defaultDict).DataDict.GetValue("FMOD", "0")));//10
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.IndexDict.GetValue("Reserved", defaultDict).DataDict.GetValue("Profiler", "0")));//11
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.DataDict.GetValue("Total System Memory Usage", "0")));//12
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.IndexDict.GetValue("WP8", defaultDict).DataDict.GetValue("Commited Limit", "0")));//13
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(ProfilerDict.IndexDict.GetValue("WP8", defaultDict).DataDict.GetValue("Commited Total", "0")));//14
                        sb.Append(separator);

                        complexData = ProfilerDict.DataDict.GetValue("Textures", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        sb.Append(WipeOffUnit(complexData[0]));//15
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(complexData[1]));//16
                        sb.Append(separator);

                        complexData = ProfilerDict.DataDict.GetValue("Meshes", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        sb.Append(WipeOffUnit(complexData[0]));//17
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(complexData[1]));//18
                        sb.Append(separator);

                        complexData = ProfilerDict.DataDict.GetValue("Materials", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        sb.Append(WipeOffUnit(complexData[0]));//19
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(complexData[1]));//20
                        sb.Append(separator);

                        complexData = ProfilerDict.DataDict.GetValue("AnimationClips", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        sb.Append(WipeOffUnit(complexData[0]));//21
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(complexData[1]));//22
                        sb.Append(separator);

                        complexData = ProfilerDict.DataDict.GetValue("AudioClips", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        sb.Append(WipeOffUnit(complexData[0]));//23
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(complexData[1]));//24
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Assets", "0")));//25
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("GameObjects in Scene", "0")));//26
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Total Objects in Scene", "0")));//27
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Total Object Count", "0")));//28
                        sb.Append(separator);
                        complexData = ProfilerDict.DataDict.GetValue("GC Allocations per Frame", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        sb.Append(WipeOffUnit(complexData[0]));//29
                        sb.Append(separator);
                        sb.Append(ToMemory_MB(complexData[1]));//30
                        #endregion
                        break;
                    }

                case ProfilerArea.Physics:
                    {
                        #region Physics data to StringBuilder
                        sb.Append("##physics");
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Active Rigidbodies", "0")));//0
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Sleeping Rigidbodies", "0")));//1
                        sb.Append(separator);
#if UNITY_5_5_OR_NEWER
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Contacts", "0")));//2 unity5.5开始由Number of Contacts 改名为 Contacts
#else
						sb.Append(WipeOffUnit(KProfilerDict.DataDict.GetValue("Number of Contacts", "0")));//2
#endif
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Static Colliders", "0")));//3
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Dynamic Colliders", "0")));//4

                        //Unity5.5开始加入以下字段
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Active Dynamic", "0")));//5
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Active Kinematic", "0")));//6
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Rigidbody", "0")));//7
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Trigger Overlaps", "0")));//8
                        sb.Append(separator);
                        sb.Append(WipeOffUnit(ProfilerDict.DataDict.GetValue("Active Constraints", "0")));//9
                        #endregion
                        break;
                    }
            }
            return sb.ToString();
        }

        public string WipeOffUnit(string dataStr)
        {
            if (string.IsNullOrEmpty(dataStr) || dataStr.Equals("N/A") || dataStr.Contains("-"))
            {
                return "0";
            }

            int unitWeight = 1;

            if (dataStr.EndsWith("g", true, null))
            {
                unitWeight = unitWeight * 1000 * 1000 * 1000;
            }
            else if (dataStr.EndsWith("m", true, null))
            {
                unitWeight = unitWeight * 1000 * 1000;
            }
            else if (dataStr.EndsWith("k", true, null))
            {
                unitWeight = unitWeight * 1000;
            }

            dataStr = Regex.Replace(dataStr, "[a-zA-Z]", "");
            dataStr = dataStr.Replace("%", "");
            dataStr = dataStr.Trim();

            if (dataStr.Contains("."))
            {
                dataStr = (double.Parse(dataStr) * unitWeight).ToString();
            }
            else
            {
                dataStr = (int.Parse(dataStr) * unitWeight).ToString();
            }

            return dataStr;
        }

        public string ToMemory_MB(string dataStr)
        {
            dataStr = dataStr.Trim();

            if (string.IsNullOrEmpty(dataStr) || dataStr.Equals("N/A") || dataStr.Contains("-"))
            {
                return "0";
            }

            double memory = 0;

            if (dataStr.Contains("GB"))
            {
                memory = Double.Parse(WipeOffUnit(dataStr)) * 1024;
            }
            else if (dataStr.Contains("MB"))
            {
                memory = Double.Parse(WipeOffUnit(dataStr));
            }
            else if (dataStr.Contains("KB"))
            {
                memory = Double.Parse(WipeOffUnit(dataStr)) / 1024;
            }
            else if (dataStr.Contains("B"))
            {
                memory = Double.Parse(WipeOffUnit(dataStr)) / 1024 / 1024;
            }
            return memory.ToString();
        }

        public string ToMemory_KB(string dataStr)
        {
            dataStr = dataStr.Trim();

            if (string.IsNullOrEmpty(dataStr) || dataStr.Equals("N/A") || dataStr.Contains("-"))
            {
                return "0";
            }

            double memory = 0;

            if (dataStr.Contains("GB"))
            {
                memory = Double.Parse(WipeOffUnit(dataStr)) * 1024 * 1024;
            }
            else if (dataStr.Contains("MB"))
            {
                memory = Double.Parse(WipeOffUnit(dataStr)) * 1024;
            }
            else if (dataStr.Contains("KB"))
            {
                memory = Double.Parse(WipeOffUnit(dataStr));
            }
            else if (dataStr.Contains("B"))
            {
                memory = Double.Parse(WipeOffUnit(dataStr)) / 1024;
            }
            return memory.ToString();
        }


        public string GetVRAMData()
        {
            StringBuilder sb = new StringBuilder();
            string[] tempData = ProfilerDict.DataDict["VRAM usage"].Replace(")", "").Split(new string[] { "(of" }, StringSplitOptions.RemoveEmptyEntries);
            string[] complexData = tempData[0].Split(' ');
            string separator = "@@";

            sb.Append(ToMemory_MB(string.Format("{0}{1}", complexData[0], complexData[1])));//87
            sb.Append(separator);
            sb.Append(ToMemory_MB(string.Format("{0}{1}", complexData[3], complexData[4])));//88
            sb.Append(separator);
            if (tempData[1].ToLower().Contains("unknown"))
            {
                sb.Append("0MB");//89 加上MB单位
            }
            else
            {
                sb.Append(ToMemory_MB(tempData[1]));//84
            }

            return sb.ToString();
        }

        public string GetVBOSize()
        {
            StringBuilder sb = new StringBuilder();
            string tempData = ProfilerDict.DataDict["VBO Total"].Replace("-", "and");
            string[] complexData = tempData.Split(' ');
            sb.Append(ToMemory_KB(string.Format("{0}{1}", complexData[2], complexData[3])));
            return sb.ToString();
        }
        public string GetVBOCount()
        {
            string tempData = ProfilerDict.DataDict["VBO Total"].Replace("-", "and");
            string[] complexData = tempData.Split(' ');
            string sb = complexData[0];
            return sb;
        }

        public class ODataDict
        {

            public Dictionary<string, string> DataDict;
            public Dictionary<string, ODataDict> IndexDict;

            public ODataDict()
            {
                DataDict = new Dictionary<string, string>();
                IndexDict = new Dictionary<string, ODataDict>();
            }
            public void Clear()
            {
                if (DataDict != null)
                {
                    DataDict.Clear();
                    DataDict = null;
                }
                if (IndexDict != null)
                {
                    IndexDict.Clear();
                    IndexDict = null;
                }
            }
        }
    }

}
