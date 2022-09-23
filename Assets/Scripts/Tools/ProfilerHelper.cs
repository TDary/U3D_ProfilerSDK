using OProcessedData;
using System;
using OProfilerData;
using UnityEditorInternal;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.Profiling;
#endif
#if UNITY_2018_3_OR_NEWER
using UnityEngine.Profiling;
using UnityEditorInternal.Profiling;
using System.Text;
using UnityEditor;
using System.Globalization;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
#endif
public class ProfilerHelper
{
    /// <summary>
    /// 提取单帧基本信息。
    /// profilerProperty是由指定帧提取出来的profiler数据。
    /// actualFrame是目标数据在所有profiler data文件中所在的帧数,用于标识数据位置。
    /// </summary>
    /// <param name="profilerProperty"></param>
    /// <param name="actualFrame"></param>
    /// <returns></returns>
    public static BaseData ExtractBaseData(ProfilerProperty profilerProperty, int actualFrame)
    {
        BaseData baseData = new BaseData();
        baseData.frame = actualFrame.ToString();
        baseData.CPUTotalTime = profilerProperty.frameTime;
        baseData.GPUTotalTime = profilerProperty.frameGpuTime;
        baseData.frameFPS = profilerProperty.frameFPS;
        if (String.IsNullOrEmpty(baseData.CPUTotalTime))
        {
            if (baseData.frameFPS == "0.0")
            {
                baseData.CPUTotalTime = "0.0";
            }
            else
            {
                baseData.CPUTotalTime = (1000f / float.Parse(baseData.frameFPS)).ToString();
            }
        }

        return baseData;
    }

    /// <summary>
    /// 提取单帧区域信息。
    /// dataFrame是目标数据在当前profiler data中所在的帧数，用于提取目标数据, 不应小于0。
    /// area是目标数据所属区域，如ProfilerArea.CPU。
    /// </summary>
    /// <param name="dataFrame"></param>
    /// <param name="area"></param>
    /// <returns></returns>
    public static AreaData ExtractAreaData(int dataFrame, ProfilerArea area)
    {
        AreaData areaData = new AreaData(ProfilerDriver.GetOverviewText(area, dataFrame), area);
        return areaData;
    }

#if UNITY_2020_2_OR_NEWER
    //unity2020.2开始采用新的数据获取方式
    public static AreaData ExtractAreaData(int dataFrame, ProfilerArea area, RawFrameDataView rawFrameDataView)
    {
        string str = string.Empty;
        if (rawFrameDataView != null && rawFrameDataView.valid)
        {
            switch (area)
            {
                case ProfilerArea.Rendering:
                    long counterValue = GetContextValue(rawFrameDataView, "Batches Count");
                    if (counterValue != -1L)
                    {
                        StringBuilder stringBuilder = new StringBuilder(1024);
                        stringBuilder.Append(string.Format("SetPass Calls: {0}   \tDraw Calls: {1} \t\tBatches: {2} \tTriangles: {3} \tVertices: {4}", GetContextValueAsNum(rawFrameDataView, "SetPass Calls Count"), GetContextValueAsNum(rawFrameDataView, "Draw Calls Count"), counterValue, GetContextValueAsNum(rawFrameDataView, "Triangles Count"), GetContextValueAsNum(rawFrameDataView, "Vertices Count")));
                        stringBuilder.Append(string.Format("\n(Dynamic Batching)\tBatched Draw Calls: {0} \tBatches: {1} \tTriangles: {2} \tVertices: {3} \tTime: {4:0.00}ms", GetContextValueAsNum(rawFrameDataView, "Dynamic Batched Draw Calls Count"), GetContextValueAsNum(rawFrameDataView, "Dynamic Batches Count"), GetContextValueAsNum(rawFrameDataView, "Dynamic Batched Triangles Count"), GetContextValueAsNum(rawFrameDataView, "Dynamic Batched Vertices Count"), GetContextValue(rawFrameDataView, "Dynamic Batching Time") * 1E-06));
                        stringBuilder.Append("\n(Static Batching)\t\tBatched Draw Calls: " + GetContextValueAsNum(rawFrameDataView, "Static Batched Draw Calls Count") + " \tBatches: " + GetContextValueAsNum(rawFrameDataView, "Static Batches Count") + " \tTriangles: " + GetContextValueAsNum(rawFrameDataView, "Static Batched Triangles Count") + " \tVertices: " + GetContextValueAsNum(rawFrameDataView, "Static Batched Vertices Count"));
                        stringBuilder.Append("\n(Instancing)\t\tBatched Draw Calls: " + GetContextValueAsNum(rawFrameDataView, "Instanced Batched Draw Calls Count") + " \tBatches: " + GetContextValueAsNum(rawFrameDataView, "Instanced Batches Count") + " \tTriangles: " + GetContextValueAsNum(rawFrameDataView, "Instanced Batched Triangles Count") + " \tVertices: " + GetContextValueAsNum(rawFrameDataView, "Instanced Batched Vertices Count"));
                        stringBuilder.Append(string.Format("\nUsed Textures: {0} / {1}", GetContextValue(rawFrameDataView, "Used Textures Count"), GetContextValueAsBytes(rawFrameDataView, "Used Textures Bytes")));
                        stringBuilder.Append(string.Format("\nRender Textures: {0} / {1}", GetContextValue(rawFrameDataView, "Render Textures Count"), GetContextValueAsBytes(rawFrameDataView, "Render Textures Bytes")));
                        stringBuilder.Append(string.Format("\nRender Textures Changes: {0}", GetContextValue(rawFrameDataView, "Render Textures Changes Count")));
                        stringBuilder.Append(string.Format("\nUsed Buffers: {0} / {1}", GetContextValue(rawFrameDataView, "Used Buffers Count"), GetContextValueAsBytes(rawFrameDataView, "Used Buffers Bytes")));
                        stringBuilder.Append(string.Format("\nVertex Buffer Upload In Frame: {0} / {1}", GetContextValue(rawFrameDataView, "Vertex Buffer Upload In Frame Count"), GetContextValueAsBytes(rawFrameDataView, "Vertex Buffer Upload In Frame Bytes")));
                        stringBuilder.Append(string.Format("\nIndex Buffer Upload In Frame: {0} / {1}", GetContextValue(rawFrameDataView, "Index Buffer Upload In Frame Count"), GetContextValueAsBytes(rawFrameDataView, "Index Buffer Upload In Frame Bytes")));
                        stringBuilder.Append(string.Format("\nShadow Casters: {0}\n", GetContextValue(rawFrameDataView, "Shadow Casters Count")));
                        str = stringBuilder.ToString();
                    }
                    else
                    {
                        str = ProfilerDriver.GetOverviewText(area, dataFrame);
                    }
                    break;
                case ProfilerArea.Memory:
                    long counterValue1 = GetContextValue(rawFrameDataView, "Total Used Memory");
                    if (counterValue1 != -1L)
                    {
                        StringBuilder stringBuilder = new StringBuilder(1024);
                        stringBuilder.Append("Total Used Memory: " + EditorUtility.FormatBytes(counterValue1) + "   ");
                        stringBuilder.Append("GC: " + GetContextValueAsBytes(rawFrameDataView, "GC Used Memory") + "   ");
                        stringBuilder.Append("Gfx: " + GetContextValueAsBytes(rawFrameDataView, "Gfx Used Memory") + "   ");
                        stringBuilder.Append("Audio: " + GetContextValueAsBytes(rawFrameDataView, "Audio Used Memory") + "   ");
                        stringBuilder.Append("Video: " + GetContextValueAsBytes(rawFrameDataView, "Video Used Memory") + "   ");
                        stringBuilder.Append("Profiler: " + GetContextValueAsBytes(rawFrameDataView, "Profiler Used Memory") + "   ");
                        stringBuilder.Append("\nTotal Reserved Memory: " + GetContextValueAsBytes(rawFrameDataView, "Total Reserved Memory") + "   ");
                        stringBuilder.Append("GC: " + GetContextValueAsBytes(rawFrameDataView, "GC Reserved Memory") + "   ");
                        stringBuilder.Append("Gfx: " + GetContextValueAsBytes(rawFrameDataView, "Gfx Reserved Memory") + "   ");
                        stringBuilder.Append("Audio: " + GetContextValueAsBytes(rawFrameDataView, "Audio Reserved Memory") + "   ");
                        stringBuilder.Append("Video: " + GetContextValueAsBytes(rawFrameDataView, "Video Reserved Memory") + "   ");
                        stringBuilder.Append("Profiler: " + GetContextValueAsBytes(rawFrameDataView, "Profiler Reserved Memory") + "   ");
                        stringBuilder.Append("\nSystem Used Memory: " + GetContextValueAsBytes(rawFrameDataView, "System Used Memory") + "   ");
                        stringBuilder.Append(string.Format("\n\nTextures: {0} / {1}   ", GetContextValue(rawFrameDataView, "Texture Count"), (object)GetContextValueAsBytes(rawFrameDataView, "Texture Memory")));
                        stringBuilder.Append(string.Format("\nMeshes: {0} / {1}   ", GetContextValue(rawFrameDataView, "Mesh Count"), (object)GetContextValueAsBytes(rawFrameDataView, "Mesh Memory")));
                        stringBuilder.Append(string.Format("\nMaterials: {0} / {1}   ", GetContextValue(rawFrameDataView, "Material Count"), (object)GetContextValueAsBytes(rawFrameDataView, "Material Memory")));
                        stringBuilder.Append(string.Format("\nAnimationClips: {0} / {1}   ", GetContextValue(rawFrameDataView, "AnimationClip Count"), (object)GetContextValueAsBytes(rawFrameDataView, "AnimationClip Memory")));
                        stringBuilder.Append(string.Format("\nAsset Count: {0}   ", GetContextValue(rawFrameDataView, "Asset Count")));
                        stringBuilder.Append(string.Format("\nGame Object Count: {0}   ", GetContextValue(rawFrameDataView, "Game Object Count")));
                        stringBuilder.Append(string.Format("\nScene Object Count: {0}   ", GetContextValue(rawFrameDataView, "Scene Object Count")));
                        stringBuilder.Append(string.Format("\nObject Count: {0}   ", GetContextValue(rawFrameDataView, "Object Count")));
                        stringBuilder.Append(string.Format("\n\nGC Allocation In Frame: {0} / {1}   ", GetContextValue(rawFrameDataView, "GC Allocation In Frame Count"), (object)GetContextValueAsBytes(rawFrameDataView, "GC Allocated In Frame")));
                        long counterValue2 = GetContextValue(rawFrameDataView, "GARLIC heap used");
                        if (counterValue2 != -1L)
                        {
                            long counterValue3 = GetContextValue(rawFrameDataView, "GARLIC heap available");
                            stringBuilder.Append("\n\nGARLIC heap used: " + EditorUtility.FormatBytes(counterValue2) + "/" + EditorUtility.FormatBytes(counterValue3 + counterValue2) + "   ");
                            stringBuilder.Append("(" + EditorUtility.FormatBytes(counterValue3) + " available)   ");
                            stringBuilder.Append("peak used: " + GetContextValueAsBytes(rawFrameDataView, "GARLIC heap peak used") + "   ");
                            stringBuilder.Append(string.Format("num allocs: {0}\n", GetContextValue(rawFrameDataView, "GARLIC heap allocs")));
                            stringBuilder.Append("ONION heap used: " + GetContextValueAsBytes(rawFrameDataView, "ONION heap used") + "   ");
                            stringBuilder.Append("peak used: " + GetContextValueAsBytes(rawFrameDataView, "ONION heap peak used") + "   ");
                            stringBuilder.Append(string.Format("num allocs: {0}", GetContextValue(rawFrameDataView, "ONION heap allocs")));
                        }
                        str = stringBuilder.ToString();
                    }
                    else
                    {
                        str = ProfilerDriver.GetOverviewText(area, dataFrame);
                    }
                    break;
                default:
                    str = ProfilerDriver.GetOverviewText(area, dataFrame);
                    break;
            }
        }
        else
        {
            str = ProfilerDriver.GetOverviewText(area, dataFrame);
        }
        //Debug.Log(str);
        AreaData areaData = new AreaData(str, area);
        return areaData;
    }
    public static string GetContextValueAsNum(RawFrameDataView rawFrameDataView, string valueName)
    {
        int markerId = rawFrameDataView.GetMarkerId(valueName);
        return markerId == -1 ? "N/A" : FormatNumber(rawFrameDataView.GetCounterValueAsLong(markerId));
    }
    public static long GetContextValue(FrameDataView frameData, string name)
    {
        int markerId = frameData.GetMarkerId(name);
        return markerId == -1 ? -1L : frameData.GetCounterValueAsLong(markerId);
    }
    public static string FormatNumber(long num)
    {
        if (num < 1000L)
            return num.ToString((IFormatProvider)CultureInfo.InvariantCulture.NumberFormat);
        return num < 1000000L ? ((double)num * 0.001).ToString("f1", (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat) + "k" : ((double)num * 1E-06).ToString("f1", (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat) + "M";
    }
    public static string GetContextValueAsBytes(FrameDataView frameData, string name)
    {
        int markerId = frameData.GetMarkerId(name);
        return markerId == -1 ? "N/A" : EditorUtility.FormatBytes(frameData.GetCounterValueAsLong(markerId));
    }
#endif

    /// <summary>
    /// 提取单帧函数堆栈信息。
    /// profilerProperty是由指定帧提取出来的profiler数据。
    /// actualFrame是目标数据在所有profiler data文件中所在的帧数,用于标识数据位置。
    /// </summary>
    /// <param name="profilerProperty"></param>
    /// <param name="actualFrame"></param>
    /// <returns></returns>
    public static void ExtractFunctionData(ProfilerProperty profilerProperty, int actualFrame, ref Dictionary<string, string> funcnameReplace, ref List<FunctionData> functionDatas, ref List<string> funcPathstack)
    {
        while (profilerProperty.Next(true))
        {
            try
            {
                FunctionData functionData = new FunctionData();
                functionData.FStackData(actualFrame, profilerProperty, ref funcPathstack, ref funcnameReplace);
                functionDatas.Add(functionData);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
                //提取该帧数据失败
            }
        }
    }

    /// <summary>
    /// 提取单帧线程TimeLine信息。
    /// profilerFrameDataIterator是由指定帧提取出来的线程TimeLine数据。
    /// actualFrame是目标数据在所有profiler data文件中所在的帧数,用于标识数据位置。
    /// dataFrame是目标数据在当前profiler data文件中所在的帧数,用于提取数据。
    /// </summary>
    /// <param name="profilerFrameDataIterator"></param>
    /// <param name="actualFrame"></param>
    /// <param name="dataFrame"></param>
    /// <returns></returns>
    public static void ExtractTimeLineData(ProfilerFrameDataIterator profilerFrameDataIterator, int actualFrame, int threadIndex, ref Dictionary<string, string> funcnameReplace, ref List<TimeLineData> timeLineDatas, ref List<string> funcPathstack)
    {
        while (profilerFrameDataIterator.Next(true))
        {
            try
            {
                TimeLineData timeLineData = new TimeLineData();
                timeLineData.TimeData(actualFrame, profilerFrameDataIterator, threadIndex, ref funcnameReplace, ref funcPathstack);
                timeLineDatas.Add(timeLineData);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
                //提取该帧数据失败
                //return new List<KTimeLineData>();
            }
        }
        //return timeLineDatas;
    }

    public static void GetProfilerFrameData(int frame, int threadindex, ref ProfilerFrameDataIterator profilerFrameDataIterator)
    {
        //暂时为抓取渲染线程数据
        profilerFrameDataIterator.SetRoot(frame, threadindex);

        //由于SetRoot()方法性能消耗过大，调用频率过高会导致Unity崩溃，故使用Thread.Sleep以降低每秒执行频率
        Thread.Sleep(1);
    }

    public static void GetProfilerProperty(int frame, ref ProfilerProperty profilerProperty)
    {
#if UNITY_2019_1_OR_NEWER
        profilerProperty.SetRoot(frame, HierarchyFrameDataView.columnSelfTime, 0);
#else
        profilerProperty.SetRoot(frame, ProfilerColumn.SelfTime, ProfilerViewType.Hierarchy);
#endif
        //由于SetRoot()方法性能消耗过大，调用频率过高会导致Unity崩溃，故使用Thread.Sleep以降低每秒执行频率
        Thread.Sleep(1);
        profilerProperty.onlyShowGPUSamples = false;
    }
}
