using System;
using System.Collections.Generic;
using UnityEditorInternal;
using OProfilerData;
using UnityEditor.Profiling;
using OProcessedData;
using static OProcessedData.AreaData;
using UnityEngine;
using Tools;

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

    public static bool GetParsedCsv(ProfilerProperty profilerProperty, ref int f, ref List<List<string>> data, ref Dictionary<string, string> funcnameReplace, ref List<string> funcPathstack, ref List<FunctionData> FunctionStackData)
    {
        BaseData baseData = BasicDataAL.AnalyzeData(profilerProperty, f);

#if UNITY_2020_2_OR_NEWER
        RawFrameDataView rawFrameDataView = ProfilerDriver.GetRawFrameDataView(f, 0);
        AreaData memoryData = MemoryDataAL.AnalyzeData(f, f, rawFrameDataView);
        AreaData renderingData = RenderingDataAL.AnalyzeData(f, f, rawFrameDataView);
        string totalMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Total Memory", "0"));
#else
        AreaData renderingData = RenderingDataAL.AnalyzeData(f, f);
        AreaData memoryData = MemoryDataAL.AnalyzeData(f, f);
        string totalMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Total", "0"));
#endif

        AreaData physicsData = PhysicsDataAL.AnalyzeData(f, f);
        AreaData cpuData = CPUDataAL.AnalyzeData(f, f);
        ODataDict defaultDict = new ODataDict();
        ProfilerHelper.ExtractFunctionData(profilerProperty, f, ref funcnameReplace, ref FunctionStackData, ref funcPathstack);
        List<FunctionData> cacheFuncData = null;
        List<FunctionData> cacheFuncData1 = null;
        List<FunctionData> cacheFuncData2 = null;
        if (string.IsNullOrEmpty(totalMemory) || totalMemory == "0")
        {
            Debug.LogWarning("数据异常，跳过该帧：" + f);
            return false;
        }

        //暂时先不用屏蔽，因为有时候demo需要用到
        //if (float.Parse(baseData.CPUTotalTime) < 4.0f)
        //{
        //    Debug.LogWarning("数据异常cpu耗时过低，跳过该帧：" + f);
        //    return false;
        //}

        //ParticleSystem.Update耗时
        double particle_ms = 0.0;
        cacheFuncData1 = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("ParticleSystem.Update"));
        cacheFuncData2 = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("ParticleSystem.Update2"));
        if (cacheFuncData1.Count != 0 && cacheFuncData2.Count != 0)
        {
            for (int i = 0; i < cacheFuncData1.Count; i++)
            {
                particle_ms += cacheFuncData1[i].SelfTime_ms;
            }
            for (int i = 0; i < cacheFuncData2.Count; i++)
            {
                particle_ms += cacheFuncData2[i].SelfTime_ms;
            }
        }
        string particleSystem_ms = particle_ms.ToString();

        //PlayerLoop耗时
        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("PlayerLoop"));
        double plp_ms = cacheFuncData[0].TotalTime_ms;
        string playerloop_ms = plp_ms.ToString();

        //GameObject.Active次数
        double goa = 0.0;
        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("GameObject.Activate"));
        if (cacheFuncData.Count != 0)
        {
            goa += cacheFuncData[0].Calls;
        }
        string active = goa.ToString();

        //GameObject.Deactivate次数
        double god = 0.0;
        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("GameObject.Activate"));
        if (cacheFuncData.Count != 0)
        {
            god += cacheFuncData[0].Calls;
        }
        string deactivate = god.ToString();

        //Loading耗时与GC
        cacheFuncData = GetLoadingFunction(FunctionStackData, "Loading");
        double loadingms = 0.0;
        double loadinggc = 0.0;
        for (int i = 0; i < cacheFuncData.Count; i++)
        {
            loadingms += cacheFuncData[i].SelfTime_ms;
            loadinggc += cacheFuncData[i].GCMemory_KB;
        }
        string Loading_ms = loadingms.ToString();
        string Loading_gc = loadinggc.ToString();

        //GC.Collect耗时
        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("GC.Collect"));
        double gcc = 0.0;
        if (cacheFuncData.Count != 0)
        {
            for (int i = 0; i < cacheFuncData.Count; i++)
            {
                gcc += cacheFuncData[i].SelfTime_ms;
            }
        }
        string GC_ms = gcc.ToString();

        //Instantiate耗时及次数
        double instanti = 0.0;
        double instanti_call = 0.0;
        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Instantiate"));
        if (cacheFuncData.Count != 0)
        {
            for (int i = 0; i < cacheFuncData.Count; i++)
            {
                instanti += cacheFuncData[i].SelfTime_ms;
                instanti_call += cacheFuncData[i].Calls;
            }
        }
        string Instantiate_ms = instanti.ToString();
        string Instantiate_calls = instanti_call.ToString();

        //Destroy次数
        double destro = 0.0;
        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Destroy"));
        if (cacheFuncData.Count != 0)
        {
            for (int i = 0; i < cacheFuncData.Count; i++)
            {
                destro += cacheFuncData[i].Calls;
            }
        }
        string destroy = destro.ToString();

        //Camera.Render耗时
        double camer = 0.0;
        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Camera.Render"));
        if (cacheFuncData.Count != 0)
        {
            for (int i = 0; i < cacheFuncData.Count; i++)
            {
                camer += cacheFuncData[i].SelfTime_ms;
            }
        }
        string cameraRender_ms = camer.ToString();

        //Shader.CreateGPUProgram耗时
        double shaderGpu = 0.0;
        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Shader.CreateGPUProgram"));
        if (cacheFuncData.Count != 0)
        {
            for (int i = 0; i < cacheFuncData.Count; i++)
            {
                shaderGpu += cacheFuncData[i].SelfTime_ms;
            }
        }
        string shaderCGpu = shaderGpu.ToString();

        //Shader.Parse耗时
        double shaderp = 0.0;
        cacheFuncData1 = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Shader.ParseThreaded"));
        cacheFuncData2 = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Shader.ParseMainThread"));
        if (cacheFuncData1.Count != 0)
        {
            for (int i = 0; i < cacheFuncData1.Count; i++)
            {
                shaderp += cacheFuncData1[i].SelfTime_ms;
            }
        }
        if (cacheFuncData2.Count != 0)
        {
            for (int i = 0; i < cacheFuncData2.Count; i++)
            {
                shaderp += cacheFuncData2[i].SelfTime_ms;
            }
        }
        string shaderParse = shaderp.ToString();

        //MeshSkinning.Update耗时
        double meshskinning = 0.0;
        double meshscall = 0.0;
        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("MeshSkinning.Update"));
        if (cacheFuncData.Count != 0)
        {
            meshskinning += cacheFuncData[0].SelfTime_ms;
            meshscall += cacheFuncData[0].Calls;
        }
        string meshSkinning_ms = meshskinning.ToString();
        string meshSkinning_calls = meshscall.ToString();

        //Render.OpaqueGeometry耗时
        double opque = 0.0;
        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Render.OpaqueGeometry"));
        if (cacheFuncData.Count != 0)
        {
            opque += cacheFuncData[0].SelfTime_ms;
        }
        string RenderOpaque_ms = opque.ToString();

        //Render.TransparentGeometry耗时
        double transparent = 0.0;
        cacheFuncData = FunctionStackData.FindAll(datas => datas.FunctionName.Equals("Render.TransparentGeometry"));
        if (cacheFuncData.Count != 0)
        {
            transparent += cacheFuncData[0].SelfTime_ms;
        }
        string RenderTransparent_ms = transparent.ToString();

        //UsedUnity内存
        string unityMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Unity", "0"));

#if UNITY_2020_1_OR_NEWER
        string gfxdriverMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("Gfx", "0"));
        string rtotalMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Total Memory", "0"));
        string rgfxdriverMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Gfx", "0"));
        string totalSysMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("System Memory", "0"));
        string LowusedVRAM = "0";
        string HighusedVRAM = "0";
        string VBOTotalB = "0";
        string VBOcount = "0";
        string TotalBatches = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Batches", "0"));
        string Triangles = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Triangles", "0"));
        string Verts = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Vertices", "0"));
        string audioClipsCount = "0";
        string audioClipsMemory = "0";
        string assets = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("Asset Count", "0"));
        string gameobjectInScene = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("Game Object Count", "0"));
        string totalObjectsInScene = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("Scene Object Count", "0"));
        string totalObjectCount = memoryData.WipeOffUnit(memoryData.ProfilerDict.DataDict.GetValue("Object Count", "0"));
        string[] complexData = new string[3];
        complexData = memoryData.ProfilerDict.DataDict.GetValue("GC Allocation In Frame", "0/0").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        string gcAllocCount = memoryData.WipeOffUnit(complexData[0]);
        string gcAllocMemory = memoryData.ToMemory_KB(complexData[1]);
        string monoMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("GC", "0"));
        string rmonoMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("GC", "0"));
#else
        string[] complexData = new string[3];
        string gfxdriverMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Used").DataDict.GetValue("GfxDriver", "0"));
        string rtotalMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("Total", "0"));
        string rgfxdriverMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.IndexDict.GetValue("Reserved").DataDict.GetValue("GfxDriver", "0"));
        string totalSysMemory = memoryData.ToMemory_KB(memoryData.ProfilerDict.DataDict.GetValue("Total System Memory Usage", "0"));
        complexData = renderingData.GetVRAMData().Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
        string LowusedVRAM = complexData[0];
        string HighusedVRAM = complexData[1];
        string VBOTotalB = renderingData.GetVBOSize();  //待研究
        string VBOcount = renderingData.GetVBOCount();  //待研究
        string TotalBatches = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Total Batches", "0"));
        string Triangles = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Tris", "0"));
        string Verts = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Verts", "0"));
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
#endif
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

        string DrawCalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("Draw Calls", "0"));
        string SetPassCalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.DataDict.GetValue("SetPass Calls", "0"));
        string activeDynamic = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Active Dynamic", "0"));
        string activeKinematic = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Active Kinematic", "0"));
        string staticColliders = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Static Colliders", "0"));
        string contacts = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Contacts", "0"));
        string triggerOL = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Trigger Overlaps", "0"));
        string activeCst = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Active Constraints", "0"));
        string rigbody = physicsData.WipeOffUnit(physicsData.ProfilerDict.DataDict.GetValue("Rigidbody", "0")); //待研究是否需要
        string DBDrawcalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Dynamic Batching", defaultDict).DataDict.GetValue("Batched Draw Calls", "0"));
        string DBbatches = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Dynamic Batching", defaultDict).DataDict.GetValue("Batches", "0"));
        string SBDrawcalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Static Batching", defaultDict).DataDict.GetValue("Batched Draw Calls", "0"));
        string SBbatches = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Static Batching", defaultDict).DataDict.GetValue("Batches", "0"));
        string InsDrawCalls = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Instancing", defaultDict).DataDict.GetValue("Batched Draw Calls", "0"));
        string Insbatches = renderingData.WipeOffUnit(renderingData.ProfilerDict.IndexDict.GetValue("Instancing", defaultDict).DataDict.GetValue("Batches", "0"));
        string renTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Rendering", "0"));
        string scrisTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Scripts", "0"));
        string physiTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Physics", "0"));
        string otherTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Others", "0"));
        string gcTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("GarbageCollector", "0"));
        string uiTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("UI", "0"));
        string vncTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("VSync", "0"));
        string giTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Global Illumination", "0"));
        string aniTime = cpuData.WipeOffUnit(cpuData.ProfilerDict.DataDict.GetValue("Animation", "0"));
        string vskinmesh = "0";
        int dbcas = int.Parse(DBDrawcalls);
        int dbbach = int.Parse(DBbatches);
        int dbSaved = dbcas - dbbach;
        string DBBatchesSaved = dbSaved.ToString();
        int sbcas = int.Parse(SBDrawcalls);
        int sbbach = int.Parse(SBbatches);
        int sbSaved = sbcas - sbbach;
        string SBBatchesSaved = sbSaved.ToString();

        List<string> row = new List<string>();
        row.Add(f.ToString());          //B 片段帧数 
        row.Add(baseData.frameFPS);     //C FPS
        row.Add(baseData.CPUTotalTime); //D CPU total time

        row.Add(totalMemory);           //E used total memory  总占用内存
        row.Add(unityMemory);           //F used unity memory
        row.Add(monoMemory);            //G used mono memory   mono占用内存
        row.Add(gfxdriverMemory);       //H used gfxdriver     显存
        row.Add(audioMemory);           //I used audio memory
        row.Add(videoMemory);           //J used video memory
        row.Add(profilerMemory);        //K used profiler memory

        row.Add(rtotalMemory);           //L reserved  total memory  
        row.Add(runityMemory);           //M reserved  unity memory
        row.Add(rmonoMemory);            //N reserved  mono memory   
        row.Add(rgfxdriverMemory);       //O reserved  gfxdriver     
        row.Add(raudioMemory);           //P reserved  audio memory
        row.Add(rvideoMemory);           //Q reserved  video memory
        row.Add(rprofilerMemory);        //R reserved  profiler memory

        row.Add(totalSysMemory);         //S Total System Memory Usage

        row.Add(texturesCount);          //T textures Count 
        row.Add(texturesMemory);         //U textures Memory
        row.Add(meshesCount);               //V meshes Count
        row.Add(meshesMemory);              //W meshes Memory
        row.Add(materialsCount);            //X materials Count
        row.Add(materialsMemory);           //Y materials Memory
        row.Add(animationClipsCount);       //Z animationClips Count
        row.Add(animationClipsMemory);      //AA animationClips Memory
        row.Add(audioClipsCount);           //AB audioClips Count
        row.Add(audioClipsMemory);          //AC audioClips Memory
        row.Add(assets);                    //AD Assets
        row.Add(gameobjectInScene);         //AE GameObjects in Scene
        row.Add(totalObjectsInScene);       //AF total objects in scene
        row.Add(totalObjectCount);          //AG total object count
        row.Add(gcAllocCount);              //AH gc allocations per frame Count
        row.Add(gcAllocMemory);             //AI gc allocations per frame Memory

        row.Add(DrawCalls);                 //AJ drawcall
        row.Add(SetPassCalls);              //AK setpass
        row.Add(TotalBatches);              //AL total batches
        row.Add(Triangles);                 //AM 三角形数
        row.Add(Verts);                     //AN 顶点数
        row.Add(activeDynamic);             //AO 非运动性Rigidbody数(Active Dynamic)
        row.Add(activeKinematic);           //AP 运动性Rigidbody数(Active Kinematic)
        row.Add(staticColliders);           //AQ 无Rigidbody的拥有collider的游戏对象个数(Static Colliders)
        row.Add(contacts);                  //AR 碰撞器间接触总数
        row.Add(triggerOL);                 //AS 重叠trigger数量(对)
        row.Add(activeCst);                 //AT Active Constraints
        row.Add(LowusedVRAM);               //AU VRAM Usage(lowVRam)
        row.Add(HighusedVRAM);              //AV VRAM Usage(highVRam)
        row.Add(DBDrawcalls);               //AW Dynamic Batching(Batched Draw Calls)
        row.Add(DBbatches);                 //AX Dynamic Batching(Batches)
        row.Add(SBDrawcalls);               //AY Static Batching(Batched Draw Calls)
        row.Add(SBbatches);                 //AZ Static Batching(Batches)
        row.Add(InsDrawCalls);              //BA Instancing(Batched Draw Calls)
        row.Add(Insbatches);                //BB Instancing(Batches)
        row.Add(DBBatchesSaved);            //BC dynamicBatchesSaved
        row.Add(SBBatchesSaved);            //BD staticBatchesSaved
        row.Add(renTime);                   //BE Rendering Time(渲染模块耗时)
        row.Add(scrisTime);                 //BF Scripts Time(脚本模块耗时)
        row.Add(physiTime);                 //BG Physics Time(物理模块耗时)
        row.Add(otherTime);                 //BH Other Time(其他模块耗时)
        row.Add(gcTime);                    //BI GC Time(GC耗时)
        row.Add(uiTime);                    //BJ UI Time(UI耗时)
        row.Add(vncTime);                   //BK VSync Time(异步耗时)
        row.Add(giTime);                    //BL Global Illumination耗时
        row.Add(aniTime);                   //BM Animation(动画耗时
        row.Add(VBOTotalB);                 //BN VBO Total(VBOTotalBytes)
        row.Add(VBOcount);                  //BO VBO Total(VBO Count)
        row.Add(rigbody);                   //BP Rigidbody
        row.Add(playerloop_ms);             //BQ PlayerLoop_ms
        row.Add(Loading_ms);                //BR Loading_ms
        row.Add(Loading_gc);                //BS Loading_GC
        row.Add(particleSystem_ms);         //BT ParticleSystem.Update
        row.Add(Instantiate_ms);            //BU Instantiate耗时
        row.Add(Instantiate_calls);         //BV Instantiate次数
        row.Add(meshSkinning_ms);           //BW MeshSkinning.Update耗时
        row.Add(RenderOpaque_ms);           //BX Render.OpaqueGeometry耗时
        row.Add(RenderTransparent_ms);      //BY Render.TransparentGeometry耗时
        row.Add(vskinmesh);                 //BZ VSkinmesh蒙皮网格(暂时无法获取)
        row.Add(active);                    //CA GameObject.Active次数
        row.Add(deactivate);                //CB GameObject.Deactivate次数
        row.Add(destroy);                   //CC Destroy次数
        row.Add(cameraRender_ms);    //CD Camera.Render耗时
        row.Add(shaderCGpu);               //CE Shader.CreateGPUProgram耗时
        row.Add(shaderParse);              //CF Shader.Parse耗时
        row.Add(meshSkinning_calls);   //CG MeshSkinning_calls次数
        row.Add(GC_ms);                        //CH GC.Collect耗时

        data.Add(row);

        return true;
    }
}

