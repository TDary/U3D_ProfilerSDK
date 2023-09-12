using System;
using System.Collections.Generic;
using UnityEditorInternal;
using OProfilerData;
using UnityEditor.Profiling;
using OProcessedData;
using static OProcessedData.AreaData;
using UnityEngine;
using Tools;
using UnityEngine.Profiling;

public class ParsingToCsv
{
    public static List<FunctionData> GetLoadingFunction(List<FunctionData> KFuncDataList, string FunctionName)
    {
        List<FunctionData> a = new List<FunctionData>();
        foreach (FunctionData data in KFuncDataList)
        {
            if (data.FunctionName.Contains("."))
            {
                int end = data.FunctionName.IndexOf(".");
                string name1 = data.FunctionName;
                string name2 = name1.Substring(0, end);
                if (FunctionName == name2)
                {
                    a.Add(data);
                }
            }
        }
        return a;
    }

    /// <summary>
    /// 适用于2020以下的Unity版本
    /// </summary>
    /// <param name="area"></param>
    /// <param name="dataFrame"></param>
    /// <returns></returns>
    private string GetAreaText(ProfilerArea area,int dataFrame)
    {
        return ProfilerDriver.GetOverviewText(area, dataFrame);
    }

    private long GetContextValue(FrameDataView frameData, string name)
    {
        int markerId = frameData.GetMarkerId(name);
        return markerId == -1 ? -1L : frameData.GetCounterValueAsLong(markerId);
    }

    public bool GetSimpleData(ProfilerProperty profilerProperty, ref int f, ref Dictionary<string,Simple> alldata,ref int totalcount)
    {
        BaseData baseData = BasicDataAL.AnalyzeData(profilerProperty, f);
        float framFps = float.Parse(profilerProperty.frameFPS);
        float cpuTotaltime = float.Parse(baseData.CPUTotalTime);
        #region CPU
        AreaData cpuData = CPUDataAL.AnalyzeData(f, f);
        float renTime = float.Parse(cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Rendering", "0")));
        float scrisTime = float.Parse(cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Scripts", "0")));
        float physiTime = float.Parse(cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Physics", "0")));
        float otherTime = float.Parse(cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Others", "0")));
        float gcTime = float.Parse(cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("GarbageCollector", "0")));
        float uiTime = float.Parse(cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("UI", "0")));
        float vncTime = float.Parse(cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("VSync", "0")));
        float giTime = float.Parse(cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Global Illumination", "0")));
        float aniTime = float.Parse(cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Animation", "0")));
        #endregion
#if UNITY_2020_1_OR_NEWER
        RawFrameDataView rawFrameDataView = ProfilerDriver.GetRawFrameDataView(f, 0);
        if (rawFrameDataView != null && rawFrameDataView.valid)
        {
            long totalusedMemory = GetContextValue(rawFrameDataView, "Total Used Memory") / 1024 /1024;   //MB
            if (totalusedMemory == 0)
            {
                Debug.LogWarning("数据异常，跳过该帧：" + f);
                return false;
            }
            #region RenderingArea
            long setpasscallCount = GetContextValue(rawFrameDataView, "SetPass Calls Count");
            long drawcallCount = GetContextValue(rawFrameDataView, "Draw Calls Count");
            long batchesCount = GetContextValue(rawFrameDataView, "Batches Count"); 
            long trianglesCount = GetContextValue(rawFrameDataView, "Triangles Count");
            long verteicesCount = GetContextValue(rawFrameDataView, "Vertices Count");
            long dynamicbatchDrawCallCount = GetContextValue(rawFrameDataView, "Dynamic Batched Draw Calls Count");
            long dynamicbatchCount = GetContextValue(rawFrameDataView, "Dynamic Batches Count");
            long dynamicbatchTrisCount = GetContextValue(rawFrameDataView, "Dynamic Batched Triangles Count");
            long dynamicbatchVertsCount = GetContextValue(rawFrameDataView, "Dynamic Batched Vertices Count");
            long dynamicbatchTime = GetContextValue(rawFrameDataView, "Dynamic Batching Time");
            long staticbatchDrawCallCount = GetContextValue(rawFrameDataView, "Static Batched Draw Calls Count");
            long staticbatchCount = GetContextValue(rawFrameDataView, "Static Batches Count");
            long staticbatchTrisCount = GetContextValue(rawFrameDataView, "Static Batched Triangles Count");
            long staticbatchVertsCount = GetContextValue(rawFrameDataView, "Static Batched Vertices Count");
            long instancebatchDrawCallCount = GetContextValue(rawFrameDataView, "Instanced Batched Draw Calls Count");
            long instancebatchCount = GetContextValue(rawFrameDataView, "Instanced Batches Count");
            long instancebatchTrisCount = GetContextValue(rawFrameDataView, "Instanced Batched Triangles Count");
            long instancebatchVertsCount = GetContextValue(rawFrameDataView, "Instanced Batched Vertices Count");
            long usedTextureCount = GetContextValue(rawFrameDataView, "Used Textures Count");
            long usedTextureMemory = GetContextValue(rawFrameDataView, "Used Textures Bytes") / 1024;  //KB
            long renderTextureCount = GetContextValue(rawFrameDataView, "Render Textures Count");
            long renderTextureMemory = GetContextValue(rawFrameDataView, "Render Textures Bytes") / 1024;//KB
            long usedBufferCount = GetContextValue(rawFrameDataView, "Used Buffers Count");
            long usedBufferMemory = GetContextValue(rawFrameDataView, "Used Buffers Bytes") / 1024;//KB
            long vBufferCount = GetContextValue(rawFrameDataView, "Vertex Buffer Upload In Frame Count");
            long vBufferMemory = GetContextValue(rawFrameDataView, "Vertex Buffer Upload In Frame Bytes") / 1024;//KB
            long inBufferCount = GetContextValue(rawFrameDataView, "Index Buffer Upload In Frame Count");
            long inBufferMemory = GetContextValue(rawFrameDataView, "Index Buffer Upload In Frame Bytes") / 1024;//KB
            long shadowCasterCount  = GetContextValue(rawFrameDataView, "Shadow Casters Count");
            #endregion
            #region MemoryArea
            long systemusedMemory = GetContextValue(rawFrameDataView, "System Used Memory") / 1024;
            long totalreservedMemory = GetContextValue(rawFrameDataView, "Total Reserved Memory") / 1024;
            long untrackedMemory = systemusedMemory - totalreservedMemory;
            long ManagedUsedHeap = GetContextValue(rawFrameDataView, "GC Used Memory") / 1024;   //转为了Managed Heap中
            long ManagedReservedHeap = GetContextValue(rawFrameDataView, "GC Reserved Memory") / 1024;   //转为了Managed Heap中
            long GraphicsUsedGD = GetContextValue(rawFrameDataView, "Gfx Used Memory") / 1024;
            long audioUsed = GetContextValue(rawFrameDataView, "Audio Reserved Memory") / 1024;
            long videoUsed = GetContextValue(rawFrameDataView, "Video Reserved Memory") / 1024;
            long profilerUsed = GetContextValue(rawFrameDataView, "Profiler Used Memory") / 1024;
            long profilerReserved = GetContextValue(rawFrameDataView, "Profiler Reserved Memory") / 1024;
            long otherReserved = systemusedMemory - untrackedMemory - ManagedReservedHeap - GraphicsUsedGD - audioUsed - videoUsed - profilerReserved;
            long otherUsed = totalusedMemory - ManagedUsedHeap - GraphicsUsedGD - audioUsed - videoUsed - profilerUsed;
            long textureCount = GetContextValue(rawFrameDataView, "Texture Count");
            long textureSize = GetContextValue(rawFrameDataView, "Texture Memory") / 1024;
            long meshCount = GetContextValue(rawFrameDataView, "Mesh Count");
            long meshSize = GetContextValue(rawFrameDataView, "Mesh Memory") / 1024;
            long matCount = GetContextValue(rawFrameDataView, "Material Count");
            long matSize = GetContextValue(rawFrameDataView, "Material Memory") / 1024;
            long animationCount = GetContextValue(rawFrameDataView, "AnimationClip Count");
            long animaionSize = GetContextValue(rawFrameDataView, "AnimationClip Memory") / 1024;
            long assetesCount = GetContextValue(rawFrameDataView, "Asset Count");
            long gameobjCount = GetContextValue(rawFrameDataView, "Game Object Count");
            long sceneobjCount = GetContextValue(rawFrameDataView, "Scene Object Count");
            long objCount = GetContextValue(rawFrameDataView, "Object Count");
            long gcframeCount = GetContextValue(rawFrameDataView, "GC Allocation In Frame Count");
            long gcframeSize = GetContextValue(rawFrameDataView, "GC Allocated In Frame") / 1024;
            #endregion
            #region Audio

            #endregion
            #region Video

            #endregion
            #region Physics
            long physicsUsedMemory = GetContextValue(rawFrameDataView, "Physics Used Memory");
            long dynamicBodies = GetContextValue(rawFrameDataView, "Dynamic Bodies");
            long articulationBodies = GetContextValue(rawFrameDataView, "Articulation Bodies");
            long activeDynamicBodies = GetContextValue(rawFrameDataView, "Active Dynamic Bodies");
            long activeKinematicBodies = GetContextValue(rawFrameDataView, "Active Kinematic Bodies");
            long activeConstraints = GetContextValue(rawFrameDataView, "Active Constraints");
            long staticColliders = GetContextValue(rawFrameDataView, "Static Colliders");
            long collidersSynced = GetContextValue(rawFrameDataView, "Colliders Synced");
            long rigidbodiesSynced = GetContextValue(rawFrameDataView, "Rigidbodies Synced");
            long physicsQueries = GetContextValue(rawFrameDataView, "Physics Queries");
            #endregion
            if (!alldata.ContainsKey("frame"))
            {
                Simple data = new Simple();
                data.field = "frame";
                data.values = new List<float>(totalcount);
                data.values.Add(f);
                alldata.Add("frame", data);
            }
            else
                alldata["frame"].values.Add(f);

            if (!alldata.ContainsKey("fps"))
            {
                Simple data = new Simple();
                data.field = "fps";
                data.values = new List<float>(totalcount);
                data.values.Add(framFps);
                alldata.Add("fps", data);
            }
            else
                alldata["fps"].values.Add(framFps);

            if (!alldata.ContainsKey("cpuTotaltime"))
            {
                Simple data = new Simple();
                data.field = "cpuTotaltime";
                data.values = new List<float>(totalcount);
                data.values.Add(cpuTotaltime);
                alldata.Add("cpuTotaltime", data);
            }
            else
                alldata["cpuTotaltime"].values.Add(cpuTotaltime);

            if (!alldata.ContainsKey("renTime"))
            {
                Simple data = new Simple();
                data.field = "renTime";
                data.values = new List<float>(totalcount);
                data.values.Add(renTime);
                alldata.Add("renTime", data);
            }
            else
                alldata["renTime"].values.Add(renTime);

            if (!alldata.ContainsKey("scrisTime"))
            {
                Simple data = new Simple();
                data.field = "scrisTime";
                data.values = new List<float>(totalcount);
                data.values.Add(scrisTime);
                alldata.Add("scrisTime", data);
            }
            else
                alldata["scrisTime"].values.Add(scrisTime);

            if (!alldata.ContainsKey("physiTime"))
            {
                Simple data = new Simple();
                data.field = "physiTime";
                data.values = new List<float>(totalcount);
                data.values.Add(physiTime);
                alldata.Add("physiTime", data);
            }
            else
                alldata["physiTime"].values.Add(physiTime);

            if (!alldata.ContainsKey("otherTime"))
            {
                Simple data = new Simple();
                data.field = "otherTime";
                data.values = new List<float>(totalcount);
                data.values.Add(otherTime);
                alldata.Add("otherTime", data);
            }
            else
                alldata["otherTime"].values.Add(otherTime);

            if (!alldata.ContainsKey("gcTime"))
            {
                Simple data = new Simple();
                data.field = "gcTime";
                data.values = new List<float>(totalcount);
                data.values.Add(gcTime);
                alldata.Add("gcTime", data);
            }
            else
                alldata["gcTime"].values.Add(gcTime);

            if (!alldata.ContainsKey("uiTime"))
            {
                Simple data = new Simple();
                data.field = "uiTime";
                data.values = new List<float>(totalcount);
                data.values.Add(uiTime);
                alldata.Add("uiTime", data);
            }
            else
                alldata["uiTime"].values.Add(uiTime);

            if (!alldata.ContainsKey("vncTime"))
            {
                Simple data = new Simple();
                data.field = "vncTime";
                data.values = new List<float>(totalcount);
                data.values.Add(vncTime);
                alldata.Add("vncTime", data);
            }
            else
                alldata["vncTime"].values.Add(vncTime);

            if (!alldata.ContainsKey("giTime"))
            {
                Simple data = new Simple();
                data.field = "giTime";
                data.values = new List<float>(totalcount);
                data.values.Add(giTime);
                alldata.Add("giTime", data);
            }
            else
                alldata["giTime"].values.Add(giTime);

            if (!alldata.ContainsKey("aniTime"))
            {
                Simple data = new Simple();
                data.field = "aniTime";
                data.values = new List<float>(totalcount);
                data.values.Add(aniTime);
                alldata.Add("aniTime", data);
            }
            else
                alldata["aniTime"].values.Add(aniTime);

            if (!alldata.ContainsKey("setpassCall"))
            {
                Simple data = new Simple();
                data.field = "setpassCall";
                data.values = new List<float>(totalcount);
                data.values.Add(setpasscallCount);
                alldata.Add("setpassCall", data);
            }
            else
                alldata["setpassCall"].values.Add(setpasscallCount);

            if (!alldata.ContainsKey("drawCall"))
            {
                Simple data = new Simple();
                data.field = "drawCall";
                data.values = new List<float>(totalcount);
                data.values.Add(drawcallCount);
                alldata.Add("drawCall", data);
            }
            else
                alldata["drawCall"].values.Add(drawcallCount);

            if (!alldata.ContainsKey("batchs"))
            {
                Simple data = new Simple();
                data.field = "batchs";
                data.values = new List<float>(totalcount);
                data.values.Add(batchesCount);
                alldata.Add("batchs", data);
            }
            else
                alldata["batchs"].values.Add(batchesCount);

            if (!alldata.ContainsKey("tris"))
            {
                Simple data = new Simple();
                data.field = "tris";
                data.values = new List<float>(totalcount);
                data.values.Add(trianglesCount);
                alldata.Add("tris", data);
            }
            else
                alldata["tris"].values.Add(trianglesCount);

            if (!alldata.ContainsKey("vertex"))
            {
                Simple data = new Simple();
                data.field = "vertex";
                data.values = new List<float>(totalcount);
                data.values.Add(verteicesCount);
                alldata.Add("vertex", data);
            }
            else
                alldata["vertex"].values.Add(verteicesCount);

            if (!alldata.ContainsKey("dynamicDrawCall"))
            {
                Simple data = new Simple();
                data.field = "dynamicDrawCall";
                data.values = new List<float>(totalcount);
                data.values.Add(dynamicbatchDrawCallCount);
                alldata.Add("dynamicDrawCall", data);
            }
            else
                alldata["dynamicDrawCall"].values.Add(dynamicbatchDrawCallCount);

            if (!alldata.ContainsKey("dynamicCount"))
            {
                Simple data = new Simple();
                data.field = "dynamicCount";
                data.values = new List<float>(totalcount);
                data.values.Add(dynamicbatchCount);
                alldata.Add("dynamicCount", data);
            }
            else
                alldata["dynamicCount"].values.Add(dynamicbatchCount);

            if (!alldata.ContainsKey("dynamicTrisCount"))
            {
                Simple data = new Simple();
                data.field = "dynamicTrisCount";
                data.values = new List<float>(totalcount);
                data.values.Add(dynamicbatchTrisCount);
                alldata.Add("dynamicTrisCount", data);
            }
            else
                alldata["dynamicTrisCount"].values.Add(dynamicbatchTrisCount);

            if (!alldata.ContainsKey("dynamicVertsCount"))
            {
                Simple data = new Simple();
                data.field = "dynamicVertsCount";
                data.values = new List<float>(totalcount);
                data.values.Add(dynamicbatchVertsCount);
                alldata.Add("dynamicVertsCount", data);
            }
            else
                alldata["dynamicVertsCount"].values.Add(dynamicbatchVertsCount);

            if (!alldata.ContainsKey("dynamicBatchTime"))
            {
                Simple data = new Simple();
                data.field = "dynamicBatchTime";
                data.values = new List<float>(totalcount);
                data.values.Add(dynamicbatchTime);
                alldata.Add("dynamicBatchTime", data);
            }
            else
                alldata["dynamicBatchTime"].values.Add(dynamicbatchTime);

            if (!alldata.ContainsKey("staticDrawCall"))
            {
                Simple data = new Simple();
                data.field = "staticDrawCall";
                data.values = new List<float>(totalcount);
                data.values.Add(staticbatchDrawCallCount);
                alldata.Add("staticDrawCall", data);
            }
            else
                alldata["staticDrawCall"].values.Add(staticbatchDrawCallCount);

            if (!alldata.ContainsKey("staticCount"))
            {
                Simple data = new Simple();
                data.field = "staticCount";
                data.values = new List<float>(totalcount);
                data.values.Add(staticbatchCount);
                alldata.Add("staticCount", data);
            }
            else
                alldata["staticCount"].values.Add(staticbatchCount);

            if (!alldata.ContainsKey("staticTrisCount"))
            {
                Simple data = new Simple();
                data.field = "staticTrisCount";
                data.values = new List<float>(totalcount);
                data.values.Add(staticbatchTrisCount);
                alldata.Add("staticTrisCount", data);
            }
            else
                alldata["staticTrisCount"].values.Add(staticbatchTrisCount);

            if (!alldata.ContainsKey("staticVertsCount"))
            {
                Simple data = new Simple();
                data.field = "staticVertsCount";
                data.values = new List<float>(totalcount);
                data.values.Add(staticbatchVertsCount);
                alldata.Add("staticVertsCount", data);
            }
            else
                alldata["staticVertsCount"].values.Add(staticbatchVertsCount);

            if (!alldata.ContainsKey("instanceDrawCall"))
            {
                Simple data = new Simple();
                data.field = "instanceDrawCall";
                data.values = new List<float>(totalcount);
                data.values.Add(instancebatchDrawCallCount);
                alldata.Add("instanceDrawCall", data);
            }
            else
                alldata["instanceDrawCall"].values.Add(instancebatchDrawCallCount);

            if (!alldata.ContainsKey("instanceCount"))
            {
                Simple data = new Simple();
                data.field = "instanceCount";
                data.values = new List<float>(totalcount);
                data.values.Add(instancebatchCount);
                alldata.Add("instanceCount", data);
            }
            else
                alldata["instanceCount"].values.Add(instancebatchCount);

            if (!alldata.ContainsKey("instanceTrisCount"))
            {
                Simple data = new Simple();
                data.field = "instanceTrisCount";
                data.values = new List<float>(totalcount);
                data.values.Add(instancebatchTrisCount);
                alldata.Add("instanceTrisCount", data);
            }
            else
                alldata["instanceTrisCount"].values.Add(instancebatchTrisCount);

            if (!alldata.ContainsKey("instanceVertsCount"))
            {
                Simple data = new Simple();
                data.field = "instanceVertsCount";
                data.values = new List<float>(totalcount);
                data.values.Add(instancebatchVertsCount);
                alldata.Add("instanceVertsCount", data);
            }
            else
                alldata["instanceVertsCount"].values.Add(instancebatchVertsCount);

            if (!alldata.ContainsKey("usedTextureCount"))
            {
                Simple data = new Simple();
                data.field = "usedTextureCount";
                data.values = new List<float>(totalcount);
                data.values.Add(usedTextureCount);
                alldata.Add("usedTextureCount", data);
            }
            else
                alldata["usedTextureCount"].values.Add(usedTextureCount);

            if (!alldata.ContainsKey("usedTextureMemory"))
            {
                Simple data = new Simple();
                data.field = "usedTextureMemory";
                data.values = new List<float>(totalcount);
                data.values.Add(usedTextureMemory);
                alldata.Add("usedTextureMemory", data);
            }
            else
                alldata["usedTextureMemory"].values.Add(usedTextureMemory);

            if (!alldata.ContainsKey("renderTextureCount"))
            {
                Simple data = new Simple();
                data.field = "renderTextureCount";
                data.values = new List<float>(totalcount);
                data.values.Add(renderTextureCount);
                alldata.Add("renderTextureCount", data);
            }
            else
                alldata["renderTextureCount"].values.Add(renderTextureCount);

            if (!alldata.ContainsKey("renderTextureMemory"))
            {
                Simple data = new Simple();
                data.field = "renderTextureMemory";
                data.values = new List<float>(totalcount);
                data.values.Add(renderTextureMemory);
                alldata.Add("renderTextureMemory", data);
            }
            else
                alldata["renderTextureMemory"].values.Add(renderTextureMemory);

            if (!alldata.ContainsKey("usedBufferCount"))
            {
                Simple data = new Simple();
                data.field = "usedBufferCount";
                data.values = new List<float>(totalcount);
                data.values.Add(usedBufferCount);
                alldata.Add("usedBufferCount", data);
            }
            else
                alldata["usedBufferCount"].values.Add(usedBufferCount);

            if (!alldata.ContainsKey("usedBufferMemory"))
            {
                Simple data = new Simple();
                data.field = "usedBufferMemory";
                data.values = new List<float>(totalcount);
                data.values.Add(usedBufferMemory);
                alldata.Add("usedBufferMemory", data);
            }
            else
                alldata["usedBufferMemory"].values.Add(usedBufferMemory);

            if (!alldata.ContainsKey("vBufferCount"))
            {
                Simple data = new Simple();
                data.field = "vBufferCount";
                data.values = new List<float>(totalcount);
                data.values.Add(vBufferCount);
                alldata.Add("vBufferCount", data);
            }
            else
                alldata["vBufferCount"].values.Add(vBufferCount);

            if (!alldata.ContainsKey("vBufferMemory"))
            {
                Simple data = new Simple();
                data.field = "vBufferMemory";
                data.values = new List<float>(totalcount);
                data.values.Add(vBufferMemory);
                alldata.Add("vBufferMemory", data);
            }
            else
                alldata["vBufferMemory"].values.Add(vBufferMemory);

            if (!alldata.ContainsKey("inBufferCount"))
            {
                Simple data = new Simple();
                data.field = "inBufferCount";
                data.values = new List<float>(totalcount);
                data.values.Add(inBufferCount);
                alldata.Add("inBufferCount", data);
            }
            else
                alldata["inBufferCount"].values.Add(inBufferCount);

            if (!alldata.ContainsKey("inBufferMemory"))
            {
                Simple data = new Simple();
                data.field = "inBufferMemory";
                data.values = new List<float>(totalcount);
                data.values.Add(inBufferMemory);
                alldata.Add("inBufferMemory", data);
            }
            else
                alldata["inBufferMemory"].values.Add(inBufferMemory);

            if (!alldata.ContainsKey("systemusedMemory"))
            {
                Simple data = new Simple();
                data.field = "systemusedMemory";
                data.values = new List<float>(totalcount);
                data.values.Add(systemusedMemory);
                alldata.Add("systemusedMemory", data);
            }
            else
                alldata["systemusedMemory"].values.Add(systemusedMemory);

            if (!alldata.ContainsKey("totalusedMemory"))
            {
                Simple data = new Simple();
                data.field = "totalusedMemory";
                data.values = new List<float>(totalcount);
                data.values.Add(totalusedMemory);
                alldata.Add("totalusedMemory", data);
            }
            else
                alldata["totalusedMemory"].values.Add(totalusedMemory);

            if (!alldata.ContainsKey("totalreservedMemory"))
            {
                Simple data = new Simple();
                data.field = "totalreservedMemory";
                data.values = new List<float>(totalcount);
                data.values.Add(totalreservedMemory);
                alldata.Add("totalreservedMemory", data);
            }
            else
                alldata["totalreservedMemory"].values.Add(totalreservedMemory);

            if (!alldata.ContainsKey("ManagedReservedHeap"))
            {
                Simple data = new Simple();
                data.field = "ManagedReservedHeap";
                data.values = new List<float>(totalcount);
                data.values.Add(ManagedReservedHeap);
                alldata.Add("ManagedReservedHeap", data);
            }
            else
                alldata["ManagedReservedHeap"].values.Add(ManagedReservedHeap);

            if (!alldata.ContainsKey("ManagedUsedHeap"))
            {
                Simple data = new Simple();
                data.field = "ManagedUsedHeap";
                data.values = new List<float>(totalcount);
                data.values.Add(ManagedUsedHeap);
                alldata.Add("ManagedUsedHeap", data);
            }
            else
                alldata["ManagedUsedHeap"].values.Add(ManagedUsedHeap);

            if (!alldata.ContainsKey("untrackedMemory"))
            {
                Simple data = new Simple();
                data.field = "untrackedMemory";
                data.values = new List<float>(totalcount);
                data.values.Add(untrackedMemory);
                alldata.Add("untrackedMemory", data);
            }
            else
                alldata["untrackedMemory"].values.Add(untrackedMemory);

            if (!alldata.ContainsKey("audioUsed"))
            {
                Simple data = new Simple();
                data.field = "audioUsed";
                data.values = new List<float>(totalcount);
                data.values.Add(audioUsed);
                alldata.Add("audioUsed", data);
            }
            else
                alldata["audioUsed"].values.Add(audioUsed);

            if (!alldata.ContainsKey("videoUsed"))
            {
                Simple data = new Simple();
                data.field = "videoUsed";
                data.values = new List<float>(totalcount);
                data.values.Add(videoUsed);
                alldata.Add("videoUsed", data);
            }
            else
                alldata["videoUsed"].values.Add(videoUsed);

            if (!alldata.ContainsKey("profilerReserved"))
            {
                Simple data = new Simple();
                data.field = "profilerReserved";
                data.values = new List<float>(totalcount);
                data.values.Add(profilerReserved);
                alldata.Add("profilerReserved", data);
            }
            else
                alldata["profilerReserved"].values.Add(profilerReserved);

            if (!alldata.ContainsKey("otherReserved"))
            {
                Simple data = new Simple();
                data.field = "otherReserved";
                data.values = new List<float>(totalcount);
                data.values.Add(otherReserved);
                alldata.Add("otherReserved", data);
            }
            else
                alldata["otherReserved"].values.Add(otherReserved);

            if (!alldata.ContainsKey("otherUsed"))
            {
                Simple data = new Simple();
                data.field = "otherUsed";
                data.values = new List<float>(totalcount);
                data.values.Add(otherUsed);
                alldata.Add("otherUsed", data);
            }
            else
                alldata["otherUsed"].values.Add(otherUsed);

            if (!alldata.ContainsKey("textureSize"))
            {
                Simple data = new Simple();
                data.field = "textureSize";
                data.values = new List<float>(totalcount);
                data.values.Add(textureSize);
                alldata.Add("textureSize", data);
            }
            else
                alldata["textureSize"].values.Add(textureSize);

            if (!alldata.ContainsKey("textureCount"))
            {
                Simple data = new Simple();
                data.field = "textureCount";
                data.values = new List<float>(totalcount);
                data.values.Add(textureCount);
                alldata.Add("textureCount", data);
            }
            else
                alldata["textureCount"].values.Add(textureCount);

            if (!alldata.ContainsKey("meshSize"))
            {
                Simple data = new Simple();
                data.field = "meshSize";
                data.values = new List<float>(totalcount);
                data.values.Add(meshSize);
                alldata.Add("meshSize", data);
            }
            else
                alldata["meshSize"].values.Add(meshSize);

            if (!alldata.ContainsKey("meshCount"))
            {
                Simple data = new Simple();
                data.field = "meshCount";
                data.values = new List<float>(totalcount);
                data.values.Add(meshCount);
                alldata.Add("meshCount", data);
            }
            else
                alldata["meshCount"].values.Add(meshCount);

            if (!alldata.ContainsKey("matCount"))
            {
                Simple data = new Simple();
                data.field = "matCount";
                data.values = new List<float>(totalcount);
                data.values.Add(matCount);
                alldata.Add("matCount", data);
            }
            else
                alldata["matCount"].values.Add(matCount);

            if (!alldata.ContainsKey("matSize"))
            {
                Simple data = new Simple();
                data.field = "matSize";
                data.values = new List<float>(totalcount);
                data.values.Add(matSize);
                alldata.Add("matSize", data);
            }
            else
                alldata["matSize"].values.Add(matSize);

            if (!alldata.ContainsKey("animationCount"))
            {
                Simple data = new Simple();
                data.field = "animationCount";
                data.values = new List<float>(totalcount);
                data.values.Add(animationCount);
                alldata.Add("animationCount", data);
            }
            else
                alldata["animationCount"].values.Add(animationCount);

            if (!alldata.ContainsKey("animaionSize"))
            {
                Simple data = new Simple();
                data.field = "animaionSize";
                data.values = new List<float>(totalcount);
                data.values.Add(animaionSize);
                alldata.Add("animaionSize", data);
            }
            else
                alldata["animaionSize"].values.Add(animaionSize);

            if (!alldata.ContainsKey("assetesCount"))
            {
                Simple data = new Simple();
                data.field = "assetesCount";
                data.values = new List<float>(totalcount);
                data.values.Add(assetesCount);
                alldata.Add("assetesCount", data);
            }
            else
                alldata["assetesCount"].values.Add(assetesCount);

            if (!alldata.ContainsKey("gameobjCount"))
            {
                Simple data = new Simple();
                data.field = "gameobjCount";
                data.values = new List<float>(totalcount);
                data.values.Add(gameobjCount);
                alldata.Add("gameobjCount", data);
            }
            else
                alldata["gameobjCount"].values.Add(gameobjCount);

            if (!alldata.ContainsKey("sceneobjCount"))
            {
                Simple data = new Simple();
                data.field = "sceneobjCount";
                data.values = new List<float>(totalcount);
                data.values.Add(sceneobjCount);
                alldata.Add("sceneobjCount", data);
            }
            else
                alldata["sceneobjCount"].values.Add(sceneobjCount);

            if (!alldata.ContainsKey("objCount"))
            {
                Simple data = new Simple();
                data.field = "objCount";
                data.values = new List<float>(totalcount);
                data.values.Add(objCount);
                alldata.Add("objCount", data);
            }
            else
                alldata["objCount"].values.Add(objCount);

            if (!alldata.ContainsKey("gcframeCount"))
            {
                Simple data = new Simple();
                data.field = "gcframeCount";
                data.values = new List<float>(totalcount);
                data.values.Add(gcframeCount);
                alldata.Add("gcframeCount", data);
            }
            else
                alldata["gcframeCount"].values.Add(gcframeCount);

            if (!alldata.ContainsKey("gcframeSize"))
            {
                Simple data = new Simple();
                data.field = "gcframeSize";
                data.values = new List<float>(totalcount);
                data.values.Add(gcframeSize);
                alldata.Add("gcframeSize", data);
            }
            else
                alldata["gcframeSize"].values.Add(gcframeSize);

            if (!alldata.ContainsKey("physicsUsedMemory"))
            {
                Simple data = new Simple();
                data.field = "physicsUsedMemory";
                data.values = new List<float>(totalcount);
                data.values.Add(physicsUsedMemory);
                alldata.Add("physicsUsedMemory", data);
            }
            else
                alldata["physicsUsedMemory"].values.Add(physicsUsedMemory);

            if (!alldata.ContainsKey("dynamicBodies"))
            {
                Simple data = new Simple();
                data.field = "dynamicBodies";
                data.values = new List<float>(totalcount);
                data.values.Add(dynamicBodies);
                alldata.Add("dynamicBodies", data);
            }
            else
                alldata["dynamicBodies"].values.Add(dynamicBodies);

            if (!alldata.ContainsKey("articulationBodies"))
            {
                Simple data = new Simple();
                data.field = "articulationBodies";
                data.values = new List<float>(totalcount);
                data.values.Add(articulationBodies);
                alldata.Add("articulationBodies", data);
            }
            else
                alldata["articulationBodies"].values.Add(articulationBodies);

            if (!alldata.ContainsKey("activeDynamicBodies"))
            {
                Simple data = new Simple();
                data.field = "activeDynamicBodies";
                data.values = new List<float>(totalcount);
                data.values.Add(activeDynamicBodies);
                alldata.Add("activeDynamicBodies", data);
            }
            else
                alldata["activeDynamicBodies"].values.Add(activeDynamicBodies);

            if (!alldata.ContainsKey("activeKinematicBodies"))
            {
                Simple data = new Simple();
                data.field = "activeKinematicBodies";
                data.values = new List<float>(totalcount);
                data.values.Add(activeKinematicBodies);
                alldata.Add("activeKinematicBodies", data);
            }
            else
                alldata["activeKinematicBodies"].values.Add(activeKinematicBodies);

            if (!alldata.ContainsKey("activeConstraints"))
            {
                Simple data = new Simple();
                data.field = "activeConstraints";
                data.values = new List<float>(totalcount);
                data.values.Add(activeConstraints);
                alldata.Add("activeConstraints", data);
            }
            else
                alldata["activeConstraints"].values.Add(activeConstraints);

            if (!alldata.ContainsKey("staticColliders"))
            {
                Simple data = new Simple();
                data.field = "staticColliders";
                data.values = new List<float>(totalcount);
                data.values.Add(staticColliders);
                alldata.Add("staticColliders", data);
            }
            else
                alldata["staticColliders"].values.Add(staticColliders);

            if (!alldata.ContainsKey("collidersSynced"))
            {
                Simple data = new Simple();
                data.field = "collidersSynced";
                data.values = new List<float>(totalcount);
                data.values.Add(collidersSynced);
                alldata.Add("collidersSynced", data);
            }
            else
                alldata["collidersSynced"].values.Add(collidersSynced);

            if (!alldata.ContainsKey("rigidbodiesSynced"))
            {
                Simple data = new Simple();
                data.field = "rigidbodiesSynced";
                data.values = new List<float>(totalcount);
                data.values.Add(rigidbodiesSynced);
                alldata.Add("rigidbodiesSynced", data);
            }
            else
                alldata["rigidbodiesSynced"].values.Add(rigidbodiesSynced);

            if (!alldata.ContainsKey("physicsQueries"))
            {
                Simple data = new Simple();
                data.field = "physicsQueries";
                data.values = new List<float>(totalcount);
                data.values.Add(physicsQueries);
                alldata.Add("physicsQueries", data);
            }
            else
                alldata["physicsQueries"].values.Add(physicsQueries);

            if (!alldata.ContainsKey("shadowCasterCount"))
            {
                Simple data = new Simple();
                data.field = "shadowCasterCount";
                data.values = new List<float>(totalcount);
                data.values.Add(shadowCasterCount);
                alldata.Add("shadowCasterCount", data);
            }
            else
                alldata["shadowCasterCount"].values.Add(shadowCasterCount);

            if (!alldata.ContainsKey("GraphicsUsedGD"))
            {
                Simple data = new Simple();
                data.field = "GraphicsUsedGD";
                data.values = new List<float>(totalcount);
                data.values.Add(GraphicsUsedGD);
                alldata.Add("GraphicsUsedGD", data);
            }
            else
                alldata["GraphicsUsedGD"].values.Add(GraphicsUsedGD);
        }
#else
        #region Rendering
        AreaData renderingData = RenderingDataAL.AnalyzeData(f, f);
        ODataDict defaultDict = new ODataDict();
        string DrawCalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Draw Calls", "0"));
        string SetPassCalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("SetPass Calls", "0"));
        string DBDrawcalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Dynamic Batching", defaultDict).DataDict.GetValue("Batched Draw Calls", "0"));
        string DBbatches = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Dynamic Batching", defaultDict).DataDict.GetValue("Batches", "0"));
        string SBDrawcalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Static Batching", defaultDict).DataDict.GetValue("Batched Draw Calls", "0"));
        string SBbatches = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Static Batching", defaultDict).DataDict.GetValue("Batches", "0"));
        string InsDrawCalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Instancing", defaultDict).DataDict.GetValue("Batched Draw Calls", "0"));
        string Insbatches = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Instancing", defaultDict).DataDict.GetValue("Batches", "0"));
        string[] complexData = new string[3];
        complexData = renderingData.GetVRAMData().Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
        string VBOTotalB = renderingData.GetVBOSize();  //待研究
        string VBOcount = renderingData.GetVBOCount();  //待研究
        string TotalBatches = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Total Batches", "0"));
        string Triangles = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Tris", "0"));
        string Verts = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Verts", "0"));
        #endregion
        #region Memory
        AreaData memoryData = MemoryDataAL.AnalyzeData(f, f);
        string totalusedMemory =  memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Total", "0"));
        if (totalusedMemory == 0)
        {
            Debug.LogWarning("数据异常，跳过该帧：" + f);
            return false;
        }
        string unityMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Unity", "0"));
        string gfxdriverMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("GfxDriver", "0"));
        string rtotalMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Total", "0"));
        string rgfxdriverMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("GfxDriver", "0"));
        string totalSysMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.DataDict.GetValue("Total System Memory Usage", "0"));
        string[] complexData = new string[3];
        complexData = memoryData.ProfilerDict.DataDict.GetValue("AudioClips", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        string audioClipsCount = memoryData.WipeOffUnit(complexData[0]);
        string audioClipsMemory = memoryData.ToMemory_KB(complexData[1]);
        string assets = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("Assets", "0"));
        string gameobjectInScene = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("GameObjects in Scene", "0"));
        string totalObjectsInScene = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("Total Objects in Scene", "0"));
        string totalObjectCount = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("Total Object Count", "0"));
        complexData = memoryData.ProfilerDict.DataDict.GetValue("GC Allocations per Frame", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        string gcAllocCount = memoryData.WipeOffUnit(complexData[0]);
        string gcAllocMemory = memoryData.ToMemory_KB(complexData[1]);
        string monoMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Mono", "0"));
        string rmonoMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Mono", "0"));
        string audioMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Audio", "0"));
        string videoMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Video", "0"));
        string profilerMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Profiler", "0"));
        string runityMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Unity", "0"));
        string raudioMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Audio", "0"));
        string rvideoMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Video", "0"));
        string rprofilerMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Profiler", "0"));
        complexData = memoryData.ProfilerDict.DataDict.GetValue("Textures", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        string texturesCount = memoryData.WipeOffUnit(complexData[0]);
        string texturesMemory = memoryData.ToMemory_KB(complexData[1]);

        complexData = memoryData.ProfilerDict.DataDict.GetValue("Meshes", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        string meshesCount = memoryData.WipeOffUnit(complexData[0]);
        string meshesMemory = memoryData.ToMemory_KB(complexData[1]);

        complexData = memoryData.ProfilerDict.DataDict.GetValue("Materials", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        string materialsCount = memoryData.WipeOffUnit(complexData[0]);
        string materialsMemory = memoryData.ToMemory_KB(complexData[1]);

        complexData = memoryData.ProfilerDict.DataDict.GetValue("AnimationClips", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        string animationClipsCount = memoryData.WipeOffUnit(complexData[0]);
        string animationClipsMemory = memoryData.ToMemory_KB(complexData[1]);
        #endregion
        #region Audio

        #endregion
        #region Video

        #endregion
        #region Physics
        AreaData physicsData = PhysicsDataAL.AnalyzeData(f, f);
        string activeDynamic = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Active Dynamic", "0"));
        string activeKinematic = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Active Kinematic", "0"));
        string staticColliders = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Static Colliders", "0"));
        string contacts = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Contacts", "0"));
        string triggerOL = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Trigger Overlaps", "0"));
        string activeCst = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Active Constraints", "0"));
        string rigbody = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Rigidbody", "0")); //待研究是否需要
        #endregion
#endif
        return true;
    }

//    public static bool GetParsedCsv(ProfilerProperty profilerProperty, ref int f, ref List<List<string>> data, ref Dictionary<string, string> funcnameReplace, ref List<string> funcPathstack, ref List<FunctionData> FunctionStackData)
//    {
//        BaseData baseData = BasicDataAL.AnalyzeData(profilerProperty, f);

//#if UNITY_2020_2_OR_NEWER
//        RawFrameDataView rawFrameDataView = ProfilerDriver.GetRawFrameDataView(f, 0);
//        AreaData memoryData = MemoryDataAL.AnalyzeData(f, f, rawFrameDataView);
//        AreaData renderingData = RenderingDataAL.AnalyzeData(f, f, rawFrameDataView);
//        string totalMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Total Memory", "0"));
//#else
//        AreaData renderingData = RenderingDataAL.AnalyzeData(f, f);
//        AreaData memoryData = MemoryDataAL.AnalyzeData(f, f);
//        string totalMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Total", "0"));
//#endif

//        AreaData physicsData = PhysicsDataAL.AnalyzeData(f, f);
//        AreaData cpuData = CPUDataAL.AnalyzeData(f, f);
//        ODataDict defaultDict = new ODataDict();
//        ProfilerHelper.ExtractFunctionData(profilerProperty, f, ref funcnameReplace, ref FunctionStackData, ref funcPathstack);
//        List<FunctionData> cacheFuncData = null;
//        List<FunctionData> cacheFuncData1 = null;
//        List<FunctionData> cacheFuncData2 = null;
//        if (string.IsNullOrEmpty(totalMemory) || totalMemory == "0")
//        {
//            Debug.LogWarning("数据异常，跳过该帧：" + f);
//            return false;
//        }

//        //暂时先不用屏蔽，因为有时候demo需要用到
//        //if (float.Parse(baseData.CPUTotalTime) < 4.0f)
//        //{
//        //    Debug.LogWarning("数据异常cpu耗时过低，跳过该帧：" + f);
//        //    return false;
//        //}

//        //ParticleSystem.Update耗时
//        double particle_ms = 0.0;
//        cacheFuncData1 = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("ParticleSystem.Update"));
//        cacheFuncData2 = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("ParticleSystem.Update2"));
//        if (cacheFuncData1.Count != 0 && cacheFuncData2.Count != 0)
//        {
//            for (int i = 0; i < cacheFuncData1.Count; i++)
//            {
//                particle_ms += cacheFuncData1[i].SelfTime_ms;
//            }
//            for (int i = 0; i < cacheFuncData2.Count; i++)
//            {
//                particle_ms += cacheFuncData2[i].SelfTime_ms;
//            }
//        }
//        string particleSystem_ms = particle_ms.ToString();

//        //PlayerLoop耗时
//        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("PlayerLoop"));
//        double plp_ms = cacheFuncData[0].TotalTime_ms;
//        string playerloop_ms = plp_ms.ToString();

//        //GameObject.Active次数
//        double goa = 0.0;
//        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("GameObject.Activate"));
//        if (cacheFuncData.Count != 0)
//        {
//            goa += cacheFuncData[0].Calls;
//        }
//        string active = goa.ToString();

//        //GameObject.Deactivate次数
//        double god = 0.0;
//        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("GameObject.Activate"));
//        if (cacheFuncData.Count != 0)
//        {
//            god += cacheFuncData[0].Calls;
//        }
//        string deactivate = god.ToString();

//        //Loading耗时与GC
//        cacheFuncData = GetLoadingFunction(FunctionStackData, "Loading");
//        double loadingms = 0.0;
//        double loadinggc = 0.0;
//        for (int i = 0; i < cacheFuncData.Count; i++)
//        {
//            loadingms += cacheFuncData[i].SelfTime_ms;
//            loadinggc += cacheFuncData[i].GCMemory_KB;
//        }
//        string Loading_ms = loadingms.ToString();
//        string Loading_gc = loadinggc.ToString();

//        //GC.Collect耗时
//        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("GC.Collect"));
//        double gcc = 0.0;
//        if (cacheFuncData.Count != 0)
//        {
//            for (int i = 0; i < cacheFuncData.Count; i++)
//            {
//                gcc += cacheFuncData[i].SelfTime_ms;
//            }
//        }
//        string GC_ms = gcc.ToString();

//        //Instantiate耗时及次数
//        double instanti = 0.0;
//        double instanti_call = 0.0;
//        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Instantiate"));
//        if (cacheFuncData.Count != 0)
//        {
//            for (int i = 0; i < cacheFuncData.Count; i++)
//            {
//                instanti += cacheFuncData[i].SelfTime_ms;
//                instanti_call += cacheFuncData[i].Calls;
//            }
//        }
//        string Instantiate_ms = instanti.ToString();
//        string Instantiate_calls = instanti_call.ToString();

//        //Destroy次数
//        double destro = 0.0;
//        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Destroy"));
//        if (cacheFuncData.Count != 0)
//        {
//            for (int i = 0; i < cacheFuncData.Count; i++)
//            {
//                destro += cacheFuncData[i].Calls;
//            }
//        }
//        string destroy = destro.ToString();

//        //Camera.Render耗时
//        double camer = 0.0;
//        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Camera.Render"));
//        if (cacheFuncData.Count != 0)
//        {
//            for (int i = 0; i < cacheFuncData.Count; i++)
//            {
//                camer += cacheFuncData[i].SelfTime_ms;
//            }
//        }
//        string cameraRender_ms = camer.ToString();

//        //Shader.CreateGPUProgram耗时
//        double shaderGpu = 0.0;
//        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Shader.CreateGPUProgram"));
//        if (cacheFuncData.Count != 0)
//        {
//            for (int i = 0; i < cacheFuncData.Count; i++)
//            {
//                shaderGpu += cacheFuncData[i].SelfTime_ms;
//            }
//        }
//        string shaderCGpu = shaderGpu.ToString();

//        //Shader.Parse耗时
//        double shaderp = 0.0;
//        cacheFuncData1 = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Shader.ParseThreaded"));
//        cacheFuncData2 = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Shader.ParseMainThread"));
//        if (cacheFuncData1.Count != 0)
//        {
//            for (int i = 0; i < cacheFuncData1.Count; i++)
//            {
//                shaderp += cacheFuncData1[i].SelfTime_ms;
//            }
//        }
//        if (cacheFuncData2.Count != 0)
//        {
//            for (int i = 0; i < cacheFuncData2.Count; i++)
//            {
//                shaderp += cacheFuncData2[i].SelfTime_ms;
//            }
//        }
//        string shaderParse = shaderp.ToString();

//        //MeshSkinning.Update耗时
//        double meshskinning = 0.0;
//        double meshscall = 0.0;
//        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("MeshSkinning.Update"));
//        if (cacheFuncData.Count != 0)
//        {
//            meshskinning += cacheFuncData[0].SelfTime_ms;
//            meshscall += cacheFuncData[0].Calls;
//        }
//        string meshSkinning_ms = meshskinning.ToString();
//        string meshSkinning_calls = meshscall.ToString();

//        //Render.OpaqueGeometry耗时
//        double opque = 0.0;
//        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Render.OpaqueGeometry"));
//        if (cacheFuncData.Count != 0)
//        {
//            opque += cacheFuncData[0].SelfTime_ms;
//        }
//        string RenderOpaque_ms = opque.ToString();

//        //Render.TransparentGeometry耗时
//        double transparent = 0.0;
//        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Render.TransparentGeometry"));
//        if (cacheFuncData.Count != 0)
//        {
//            transparent += cacheFuncData[0].SelfTime_ms;
//        }
//        string RenderTransparent_ms = transparent.ToString();

//        //UsedUnity内存
//        string unityMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Unity", "0"));

//#if UNITY_2020_1_OR_NEWER
//        string gfxdriverMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Gfx", "0"));
//        string rtotalMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Total Memory", "0"));
//        string rgfxdriverMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Gfx", "0"));
//        string totalSysMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("System Memory", "0"));
//        string LowusedVRAM = "0";
//        string HighusedVRAM = "0";
//        string VBOTotalB = "0";
//        string VBOcount = "0";
//        string TotalBatches = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Batches", "0"));
//        string Triangles = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Triangles", "0"));
//        string Verts = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Vertices", "0"));
//        string audioClipsCount = "0";
//        string audioClipsMemory = "0";
//        string assets = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("Asset Count", "0"));
//        string gameobjectInScene = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("Game Object Count", "0"));
//        string totalObjectsInScene = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("Scene Object Count", "0"));
//        string totalObjectCount = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("Object Count", "0"));
//        string[] complexData = new string[3];
//        complexData = memoryData.ProfilerDict.DataDict.GetValue("GC Allocation In Frame", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
//        string gcAllocCount = memoryData.WipeOffUnit(complexData[0]);
//        string gcAllocMemory = memoryData.ToMemory_KB(complexData[1]);
//        string monoMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("GC", "0"));
//        string rmonoMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("GC", "0"));
//#else
//        string[] complexData = new string[3];
//        string gfxdriverMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("GfxDriver", "0"));
//        string rtotalMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Total", "0"));
//        string rgfxdriverMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("GfxDriver", "0"));
//        string totalSysMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.DataDict.GetValue("Total System Memory Usage", "0"));
//        complexData = renderingData.GetVRAMData().Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
//        string LowusedVRAM = complexData[0];
//        string HighusedVRAM = complexData[1];
//        string VBOTotalB = renderingData.GetVBOSize();  //待研究
//        string VBOcount = renderingData.GetVBOCount();  //待研究
//        string TotalBatches = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Total Batches", "0"));
//        string Triangles = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Tris", "0"));
//        string Verts = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Verts", "0"));
//        complexData = memoryData.ProfilerDict.DataDict.GetValue("AudioClips", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
//        string audioClipsCount = memoryData.WipeOffUnit(complexData[0]);
//        string audioClipsMemory = memoryData.ToMemory_KB(complexData[1]);
//        string assets = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("Assets", "0"));
//        string gameobjectInScene = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("GameObjects in Scene", "0"));
//        string totalObjectsInScene = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("Total Objects in Scene", "0"));
//        string totalObjectCount = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("Total Object Count", "0"));
//        complexData = memoryData.ProfilerDict.DataDict.GetValue("GC Allocations per Frame", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
//        string gcAllocCount = memoryData.WipeOffUnit(complexData[0]);
//        string gcAllocMemory = memoryData.ToMemory_KB(complexData[1]);
//        string monoMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Mono", "0"));
//        string rmonoMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Mono", "0"));
//#endif
//        string audioMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Audio", "0"));
//        string videoMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Video", "0"));
//        string profilerMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Profiler", "0"));
//        string runityMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Unity", "0"));
//        string raudioMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Audio", "0"));
//        string rvideoMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Video", "0"));
//        string rprofilerMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Profiler", "0"));
//        complexData = memoryData.ProfilerDict.DataDict.GetValue("Textures", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
//        string texturesCount = memoryData.WipeOffUnit(complexData[0]);
//        string texturesMemory = memoryData.ToMemory_KB(complexData[1]);

//        complexData = memoryData.ProfilerDict.DataDict.GetValue("Meshes", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
//        string meshesCount = memoryData.WipeOffUnit(complexData[0]);
//        string meshesMemory = memoryData.ToMemory_KB(complexData[1]);

//        complexData = memoryData.ProfilerDict.DataDict.GetValue("Materials", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
//        string materialsCount = memoryData.WipeOffUnit(complexData[0]);
//        string materialsMemory = memoryData.ToMemory_KB(complexData[1]);

//        complexData = memoryData.ProfilerDict.DataDict.GetValue("AnimationClips", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
//        string animationClipsCount = memoryData.WipeOffUnit(complexData[0]);
//        string animationClipsMemory = memoryData.ToMemory_KB(complexData[1]);

//        string DrawCalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Draw Calls", "0"));
//        string SetPassCalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("SetPass Calls", "0"));
//        string activeDynamic = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Active Dynamic", "0"));
//        string activeKinematic = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Active Kinematic", "0"));
//        string staticColliders = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Static Colliders", "0"));
//        string contacts = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Contacts", "0"));
//        string triggerOL = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Trigger Overlaps", "0"));
//        string activeCst = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Active Constraints", "0"));
//        string rigbody = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Rigidbody", "0")); //待研究是否需要
//        string DBDrawcalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Dynamic Batching", defaultDict).DataDict.GetValue("Batched Draw Calls", "0"));
//        string DBbatches = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Dynamic Batching", defaultDict).DataDict.GetValue("Batches", "0"));
//        string SBDrawcalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Static Batching", defaultDict).DataDict.GetValue("Batched Draw Calls", "0"));
//        string SBbatches = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Static Batching", defaultDict).DataDict.GetValue("Batches", "0"));
//        string InsDrawCalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Instancing", defaultDict).DataDict.GetValue("Batched Draw Calls", "0"));
//        string Insbatches = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Instancing", defaultDict).DataDict.GetValue("Batches", "0"));
//        string renTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Rendering", "0"));
//        string scrisTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Scripts", "0"));
//        string physiTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Physics", "0"));
//        string otherTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Others", "0"));
//        string gcTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("GarbageCollector", "0"));
//        string uiTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("UI", "0"));
//        string vncTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("VSync", "0"));
//        string giTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Global Illumination", "0"));
//        string aniTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Animation", "0"));
//        string vskinmesh = "0";
//        int dbcas = int.Parse(DBDrawcalls);
//        int dbbach = int.Parse(DBbatches);
//        int dbSaved = dbcas - dbbach;
//        string DBBatchesSaved = dbSaved.ToString();
//        int sbcas = int.Parse(SBDrawcalls);
//        int sbbach = int.Parse(SBbatches);
//        int sbSaved = sbcas - sbbach;
//        string SBBatchesSaved = sbSaved.ToString();

//        List<string> row = new List<string>();
//        row.Add(f.ToString());          //B 片段帧数 
//        row.Add(baseData.frameFPS);     //C FPS
//        row.Add(baseData.CPUTotalTime); //D CPU total time

//        row.Add(totalMemory);           //E used total memory  总占用内存
//        row.Add(unityMemory);           //F used unity memory
//        row.Add(monoMemory);            //G used mono memory   mono占用内存
//        row.Add(gfxdriverMemory);       //H used gfxdriver     显存
//        row.Add(audioMemory);           //I used audio memory
//        row.Add(videoMemory);           //J used video memory
//        row.Add(profilerMemory);        //K used profiler memory

//        row.Add(rtotalMemory);           //L reserved  total memory  
//        row.Add(runityMemory);           //M reserved  unity memory
//        row.Add(rmonoMemory);            //N reserved  mono memory   
//        row.Add(rgfxdriverMemory);       //O reserved  gfxdriver     
//        row.Add(raudioMemory);           //P reserved  audio memory
//        row.Add(rvideoMemory);           //Q reserved  video memory
//        row.Add(rprofilerMemory);        //R reserved  profiler memory

//        row.Add(totalSysMemory);         //S Total System Memory Usage

//        row.Add(texturesCount);          //T textures Count 
//        row.Add(texturesMemory);         //U textures Memory
//        row.Add(meshesCount);               //V meshes Count
//        row.Add(meshesMemory);              //W meshes Memory
//        row.Add(materialsCount);            //X materials Count
//        row.Add(materialsMemory);           //Y materials Memory
//        row.Add(animationClipsCount);       //Z animationClips Count
//        row.Add(animationClipsMemory);      //AA animationClips Memory
//        row.Add(audioClipsCount);           //AB audioClips Count
//        row.Add(audioClipsMemory);          //AC audioClips Memory
//        row.Add(assets);                    //AD Assets
//        row.Add(gameobjectInScene);         //AE GameObjects in Scene
//        row.Add(totalObjectsInScene);       //AF total objects in scene
//        row.Add(totalObjectCount);          //AG total object count
//        row.Add(gcAllocCount);              //AH gc allocations per frame Count
//        row.Add(gcAllocMemory);             //AI gc allocations per frame Memory

//        row.Add(DrawCalls);                 //AJ drawcall
//        row.Add(SetPassCalls);              //AK setpass
//        row.Add(TotalBatches);              //AL total batches
//        row.Add(Triangles);                 //AM 三角形数
//        row.Add(Verts);                     //AN 顶点数
//        row.Add(activeDynamic);             //AO 非运动性Rigidbody数(Active Dynamic)
//        row.Add(activeKinematic);           //AP 运动性Rigidbody数(Active Kinematic)
//        row.Add(staticColliders);           //AQ 无Rigidbody的拥有collider的游戏对象个数(Static Colliders)
//        row.Add(contacts);                  //AR 碰撞器间接触总数
//        row.Add(triggerOL);                 //AS 重叠trigger数量(对)
//        row.Add(activeCst);                 //AT Active Constraints
//        row.Add(LowusedVRAM);               //AU VRAM Usage(lowVRam)
//        row.Add(HighusedVRAM);              //AV VRAM Usage(highVRam)
//        row.Add(DBDrawcalls);               //AW Dynamic Batching(Batched Draw Calls)
//        row.Add(DBbatches);                 //AX Dynamic Batching(Batches)
//        row.Add(SBDrawcalls);               //AY Static Batching(Batched Draw Calls)
//        row.Add(SBbatches);                 //AZ Static Batching(Batches)
//        row.Add(InsDrawCalls);              //BA Instancing(Batched Draw Calls)
//        row.Add(Insbatches);                //BB Instancing(Batches)
//        row.Add(DBBatchesSaved);            //BC dynamicBatchesSaved
//        row.Add(SBBatchesSaved);            //BD staticBatchesSaved
//        row.Add(renTime);                   //BE Rendering Time(渲染模块耗时)
//        row.Add(scrisTime);                 //BF Scripts Time(脚本模块耗时)
//        row.Add(physiTime);                 //BG Physics Time(物理模块耗时)
//        row.Add(otherTime);                 //BH Other Time(其他模块耗时)
//        row.Add(gcTime);                    //BI GC Time(GC耗时)
//        row.Add(uiTime);                    //BJ UI Time(UI耗时)
//        row.Add(vncTime);                   //BK VSync Time(异步耗时)
//        row.Add(giTime);                    //BL Global Illumination耗时
//        row.Add(aniTime);                   //BM Animation(动画耗时
//        row.Add(VBOTotalB);                 //BN VBO Total(VBOTotalBytes)
//        row.Add(VBOcount);                  //BO VBO Total(VBO Count)
//        row.Add(rigbody);                   //BP Rigidbody
//        row.Add(playerloop_ms);             //BQ PlayerLoop_ms
//        row.Add(Loading_ms);                //BR Loading_ms
//        row.Add(Loading_gc);                //BS Loading_GC
//        row.Add(particleSystem_ms);         //BT ParticleSystem.Update
//        row.Add(Instantiate_ms);            //BU Instantiate耗时
//        row.Add(Instantiate_calls);         //BV Instantiate次数
//        row.Add(meshSkinning_ms);           //BW MeshSkinning.Update耗时
//        row.Add(RenderOpaque_ms);           //BX Render.OpaqueGeometry耗时
//        row.Add(RenderTransparent_ms);      //BY Render.TransparentGeometry耗时
//        row.Add(vskinmesh);                 //BZ VSkinmesh蒙皮网格(暂时无法获取)
//        row.Add(active);                    //CA GameObject.Active次数
//        row.Add(deactivate);                //CB GameObject.Deactivate次数
//        row.Add(destroy);                   //CC Destroy次数
//        row.Add(cameraRender_ms);    //CD Camera.Render耗时
//        row.Add(shaderCGpu);               //CE Shader.CreateGPUProgram耗时
//        row.Add(shaderParse);              //CF Shader.Parse耗时
//        row.Add(meshSkinning_calls);   //CG MeshSkinning_calls次数
//        row.Add(GC_ms);                        //CH GC.Collect耗时

//        data.Add(row);

//        return true;
//    }
}

