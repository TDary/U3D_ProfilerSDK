using OProcessedData;
using OProfilerData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OStatisticData
{
    internal class AreaStatisticData
    {
        public int Frame;
        public double FPS;
        public double CPUTotalTime_ms;
        public double ReservedTotal_MB;
        public double ReservedUnity_MB;
        public double ReservedGFX_MB;
        public double ReservedMono_MB;
        public double ReservedProfiler_MB;
        public int DrawCalls;
        public int SetPassCalls;
        public int TotalBatches;
        public int Triangles;
        public int Verts;
        public double VBOTotal_MB;
        public int ActiveRigidbodies;
        public int StaticColliders;
        public int DynamicColliders;
        public int Textures;
        public double Textures_MB;
        public int Meshes;
        public double Meshes_MB;
        public int AnimationClips;
        public double AnimationClips_MB;
        public int Materials;
        public double Materials_MB;
        public int AudioClips;
        public double AudioClips_MB;
        public int RenderTextures;
        public double RenderTextures_VMB;
        public int Assets;
        public int GameObjects;
        public double GcAlloc_KB;
        public double Rendering_ms;
        public double Scripts_ms;
        public double Physics_ms;
        public double GarbageCollector_ms;
        public double VSync_ms;
        public double Gi_ms;
        public double Others_ms;
        public double RenderTransparent_ms;
        public double RenderOpaque_ms;
        public double BehaviourUpdate_ms;
        public double PhysicsCompute_ms;
        public double PhysicsProcessReports_ms;
        public double Animator_ms;
        public double ParticleSystem_ms;
        public double DynamicBone_ms;
        public double Overhead_ms;
        public double Loading_ms;
        public double Instantiate_ms;
        public double ShaderParse_ms;
        public double UILogic_ms;
        public double Log_ms;
        public int WarningCount;
        public int PlayerCount;
        public int NpcCount;

        public const int DEFAULT_VALUE = 0;

        public AreaStatisticData()
        {
            this.Frame = DEFAULT_VALUE;
            this.FPS = DEFAULT_VALUE;
            this.CPUTotalTime_ms = DEFAULT_VALUE;
            this.ReservedTotal_MB = DEFAULT_VALUE;
            this.ReservedUnity_MB = DEFAULT_VALUE;
            this.ReservedGFX_MB = DEFAULT_VALUE;
            this.ReservedMono_MB = DEFAULT_VALUE;
            this.ReservedProfiler_MB = DEFAULT_VALUE;
            this.DrawCalls = DEFAULT_VALUE;
            this.SetPassCalls = DEFAULT_VALUE;
            this.TotalBatches = DEFAULT_VALUE;
            this.Triangles = DEFAULT_VALUE;
            this.Verts = DEFAULT_VALUE;
            this.VBOTotal_MB = DEFAULT_VALUE;
            this.ActiveRigidbodies = DEFAULT_VALUE;
            this.StaticColliders = DEFAULT_VALUE;
            this.DynamicColliders = DEFAULT_VALUE;
            this.Textures = DEFAULT_VALUE;
            this.Textures_MB = DEFAULT_VALUE;
            this.Meshes = DEFAULT_VALUE;
            this.Meshes_MB = DEFAULT_VALUE;
            this.AnimationClips = DEFAULT_VALUE;
            this.AnimationClips_MB = DEFAULT_VALUE;
            this.Materials = DEFAULT_VALUE;
            this.Materials_MB = DEFAULT_VALUE;
            this.AudioClips = DEFAULT_VALUE;
            this.AudioClips_MB = DEFAULT_VALUE;
            this.RenderTextures = DEFAULT_VALUE;
            this.RenderTextures_VMB = DEFAULT_VALUE;
            this.Assets = DEFAULT_VALUE;
            this.GameObjects = DEFAULT_VALUE;
            this.GcAlloc_KB = DEFAULT_VALUE;
            this.Rendering_ms = DEFAULT_VALUE;
            this.Scripts_ms = DEFAULT_VALUE;
            this.Physics_ms = DEFAULT_VALUE;
            this.GarbageCollector_ms = DEFAULT_VALUE;
            this.VSync_ms = DEFAULT_VALUE;
            this.Gi_ms = DEFAULT_VALUE;
            this.Others_ms = DEFAULT_VALUE;
            this.RenderTransparent_ms = DEFAULT_VALUE;
            this.RenderOpaque_ms = DEFAULT_VALUE;
            this.BehaviourUpdate_ms = DEFAULT_VALUE;
            this.PhysicsCompute_ms = DEFAULT_VALUE;
            this.PhysicsProcessReports_ms = DEFAULT_VALUE;
            this.Animator_ms = DEFAULT_VALUE;
            this.ParticleSystem_ms = DEFAULT_VALUE;
            this.DynamicBone_ms = DEFAULT_VALUE;
            this.Overhead_ms = DEFAULT_VALUE;
            this.Loading_ms = DEFAULT_VALUE;
            this.Instantiate_ms = DEFAULT_VALUE;
            this.ShaderParse_ms = DEFAULT_VALUE;
            this.UILogic_ms = DEFAULT_VALUE;
            this.Log_ms = DEFAULT_VALUE;
            this.WarningCount = DEFAULT_VALUE;
            this.PlayerCount = DEFAULT_VALUE;
            this.NpcCount = DEFAULT_VALUE;
        }

        public void SetStatisticValue(TotalProfilerData profilerData)
        {

            string[] cacheComplexData = new string[2];
            List<FunctionData> cacheFuncData = null;

            if (profilerData.baseData != null)
            {
                this.Frame = ToInt(profilerData.baseData.frame);//1
                this.FPS = ToDouble(profilerData.baseData.frameFPS);//2
                this.CPUTotalTime_ms = ToDouble(profilerData.baseData.CPUTotalTime);//3
            }

            if (profilerData.cpuData.ProfilerDict != null)
            {
                this.Rendering_ms = ToDouble(profilerData.cpuData.ProfilerDict.DataDict["Rendering"]);//5
                this.Scripts_ms = ToDouble(profilerData.cpuData.ProfilerDict.DataDict["Scripts"]);//6
                this.Physics_ms = ToDouble(profilerData.cpuData.ProfilerDict.DataDict["Physics"]);//7
                this.GarbageCollector_ms = ToDouble(profilerData.cpuData.ProfilerDict.DataDict["GarbageCollector"]);//8
                this.Gi_ms = ToDouble(profilerData.cpuData.ProfilerDict.DataDict["Gi"]);//10
                this.Others_ms = ToDouble(profilerData.cpuData.ProfilerDict.DataDict["Others"]);//11
            }

            if (profilerData.gpuData.ProfilerDict != null)
            {
                this.RenderTransparent_ms = ToDouble(profilerData.gpuData.ProfilerDict.DataDict["Transparent"]);//13
                this.RenderOpaque_ms = ToDouble(profilerData.gpuData.ProfilerDict.DataDict["Opaque"]);//12
            }

            if (profilerData.memoryData.ProfilerDict != null)
            {
                this.ReservedTotal_MB = ToMemory_MB(profilerData.memoryData.ProfilerDict.IndexDict["Reserved"].DataDict["Total"]);//46
                this.ReservedUnity_MB = ToMemory_MB(profilerData.memoryData.ProfilerDict.IndexDict["Reserved"].DataDict["Unity"]);//47
                this.ReservedGFX_MB = ToMemory_MB(profilerData.memoryData.ProfilerDict.IndexDict["Reserved"].DataDict["GfxDriver"]);//49
                this.ReservedMono_MB = ToMemory_MB(profilerData.memoryData.ProfilerDict.IndexDict["Reserved"].DataDict["Mono"]);//48
                this.ReservedProfiler_MB = ToMemory_MB(profilerData.memoryData.ProfilerDict.IndexDict["Reserved"].DataDict["Profiler"]);//51
                this.Assets = ToInt(profilerData.memoryData.ProfilerDict.DataDict["Assets"]);//71
                this.GameObjects = ToInt(profilerData.memoryData.ProfilerDict.DataDict["GameObjects in Scene"]);//72

                cacheComplexData = HandleComplexData(profilerData.memoryData.ProfilerDict.DataDict["Textures"]);
                this.Textures = ToInt(cacheComplexData[0]);//61
                this.Textures_MB = ToMemory_MB(cacheComplexData[1]);//62

                cacheComplexData = HandleComplexData(profilerData.memoryData.ProfilerDict.DataDict["Meshes"]);
                this.Meshes = ToInt(cacheComplexData[0]);//63
                this.Meshes_MB = ToMemory_MB(cacheComplexData[1]);//64

                cacheComplexData = HandleComplexData(profilerData.memoryData.ProfilerDict.DataDict["AnimationClips"]);
                this.AnimationClips = ToInt(cacheComplexData[0]);//67
                this.AnimationClips_MB = ToMemory_MB(cacheComplexData[1]);//68

                cacheComplexData = HandleComplexData(profilerData.memoryData.ProfilerDict.DataDict["Materials"]);
                this.Materials = ToInt(cacheComplexData[0]);//65
                this.Materials_MB = ToMemory_MB(cacheComplexData[1]);//66

                cacheComplexData = HandleComplexData(profilerData.memoryData.ProfilerDict.DataDict["AudioClips"]);
                this.AudioClips = ToInt(cacheComplexData[0]);//69
                this.AudioClips_MB = ToMemory_MB(cacheComplexData[1]);//70
            }

            if (profilerData.renderingData.ProfilerDict != null)
            {
                this.DrawCalls = ToInt(profilerData.renderingData.ProfilerDict.DataDict["Draw Calls"]);//20
                this.SetPassCalls = ToInt(profilerData.renderingData.ProfilerDict.DataDict["SetPass Calls"]);//19
                this.TotalBatches = ToInt(profilerData.renderingData.ProfilerDict.DataDict["Total Batches"]);//21
                this.Triangles = ToInt(profilerData.renderingData.ProfilerDict.DataDict["Tris"]);//22
                this.Verts = ToInt(profilerData.renderingData.ProfilerDict.DataDict["Verts"]);//23

                cacheComplexData = HandleComplexData(profilerData.renderingData.ProfilerDict.DataDict["VBO Total"]);
                this.VBOTotal_MB = ToMemory_MB(cacheComplexData[1]);//40

                cacheComplexData = HandleComplexData(profilerData.renderingData.ProfilerDict.DataDict["RenderTextures"]);
                this.RenderTextures = ToInt(cacheComplexData[0]);//34
                this.RenderTextures_VMB = ToMemory_MB(cacheComplexData[1]);//35
            }

            if (profilerData.physicsData.ProfilerDict != null)
            {
                this.ActiveRigidbodies = ToInt(profilerData.physicsData.ProfilerDict.DataDict["Active Rigidbodies"]);//77
                this.StaticColliders = ToInt(profilerData.physicsData.ProfilerDict.DataDict["Static Colliders"]);//80
                this.DynamicColliders = ToInt(profilerData.physicsData.ProfilerDict.DataDict["Dynamic Colliders"]);//81
            }

            if (profilerData.functionData.FunctionStackData != null)
            {

                for (int i = 0; i < profilerData.functionData.FunctionStackData.Count; i++)
                {
                    this.GcAlloc_KB += profilerData.functionData.FunctionStackData[i].GCMemory_KB;
                }

                cacheFuncData = profilerData.functionData.FunctionStackData.FindAll(data => data.FunctionPath.Equals("WaitForTargetFPS"));
                for (int i = 0; i < cacheFuncData.Count; i++)
                {
                    this.VSync_ms += cacheFuncData[i].TotalTime_ms;
                }

                cacheFuncData = GetRootFunction(profilerData.functionData.FunctionStackData, "BehaviourUpdate");
                for (int i = 0; i < cacheFuncData.Count; i++)
                {
                    this.BehaviourUpdate_ms += cacheFuncData[i].TotalTime_ms;
                }
                cacheFuncData.Clear();

                //找不到PhysicsCompute(ms)
                cacheFuncData = GetRootFunction(profilerData.functionData.FunctionStackData, "PhysicsCompute");
                for (int i = 0; i < cacheFuncData.Count; i++)
                {
                    this.PhysicsCompute_ms += cacheFuncData[i].TotalTime_ms;
                }
                cacheFuncData.Clear();

                cacheFuncData = GetRootFunction(profilerData.functionData.FunctionStackData, "Physics.Processing");
                for (int i = 0; i < cacheFuncData.Count; i++)
                {
                    this.PhysicsProcessReports_ms += cacheFuncData[i].TotalTime_ms;
                }
                cacheFuncData.Clear();

                cacheFuncData = GetRootFunction(profilerData.functionData.FunctionStackData, "Animators");
                for (int i = 0; i < cacheFuncData.Count; i++)
                {
                    this.Animator_ms += cacheFuncData[i].TotalTime_ms;
                }
                cacheFuncData.Clear();

                cacheFuncData = GetRootFunction(profilerData.functionData.FunctionStackData, "ParticleSystem");
                for (int i = 0; i < cacheFuncData.Count; i++)
                {
                    ParticleSystem_ms += cacheFuncData[i].TotalTime_ms;
                }
                cacheFuncData.Clear();

                cacheFuncData = GetRootFunction(profilerData.functionData.FunctionStackData, "DynamicBone");
                for (int i = 0; i < cacheFuncData.Count; i++)
                {
                    this.DynamicBone_ms += cacheFuncData[i].TotalTime_ms;
                }
                cacheFuncData.Clear();

                cacheFuncData = GetRootFunction(profilerData.functionData.FunctionStackData, "Overhead");
                for (int i = 0; i < cacheFuncData.Count; i++)
                {
                    this.Overhead_ms += cacheFuncData[i].TotalTime_ms;
                }
                cacheFuncData.Clear();

                cacheFuncData = GetRootFunction(profilerData.functionData.FunctionStackData, "Loading");
                for (int i = 0; i < cacheFuncData.Count; i++)
                {
                    this.Loading_ms += cacheFuncData[i].TotalTime_ms;
                }
                cacheFuncData.Clear();

                cacheFuncData = profilerData.functionData.FunctionStackData.FindAll(data => data.FunctionName.Equals("Instantiate"));
                for (int i = 0; i < cacheFuncData.Count; i++)
                {
                    this.Instantiate_ms += cacheFuncData[i].TotalTime_ms;
                }
                cacheFuncData.Clear();

                //ShaderParse(ms)
                //UILogic(ms)
                //Log(ms)
                //暂时使用通用方法统计

                cacheFuncData = profilerData.functionData.FunctionStackData.FindAll(data => IsRootFunction(data, "ShaderParse"));
                for (int i = 0; i < cacheFuncData.Count; i++)
                {
                    this.ShaderParse_ms += cacheFuncData[i].TotalTime_ms;
                }
                cacheFuncData.Clear();


                cacheFuncData = profilerData.functionData.FunctionStackData.FindAll(data => IsRootFunction(data, "UILogic"));
                for (int i = 0; i < cacheFuncData.Count; i++)
                {
                    this.UILogic_ms += cacheFuncData[i].TotalTime_ms;
                }
                cacheFuncData.Clear();


                cacheFuncData = profilerData.functionData.FunctionStackData.FindAll(data => data.FunctionName.Equals("LogStringToConsole"));
                for (int i = 0; i < cacheFuncData.Count; i++)
                {
                    this.Log_ms += cacheFuncData[i].TotalTime_ms;
                }
                cacheFuncData.Clear();

                //PlayerCount
                //NpcCount
                //无数据
                //暂不统计
                //this.PlayerCount = 0;
                //this.NpcCount = 0;

                this.WarningCount = 0;
                for (int i = 0; i < profilerData.functionData.FunctionStackData.Count; i++)
                {
                    this.WarningCount += Convert.ToInt32(profilerData.functionData.FunctionStackData[i].WarningCount);
                }
            }
        }

        Double ToMemory_KB(string dataStr)
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

        Double ToMemory_MB(string dataStr)
        {

            double memory = 0;

            if (dataStr.Contains("GB"))
            {
                memory = ToDouble(dataStr) * 1024;
            }
            else if (dataStr.Contains("MB"))
            {
                memory = ToDouble(dataStr);
            }
            else if (dataStr.Contains("KB"))
            {
                memory = ToDouble(dataStr) / 1024;
            }
            else if (dataStr.Contains("B"))
            {
                memory = ToDouble(dataStr) / 1024 / 1024;
            }
            return memory;
        }

        Double ToDouble(string dataStr)
        {
            if (string.IsNullOrEmpty(dataStr) || dataStr.Equals("N/A"))
            {
                dataStr = "0";
            }

            dataStr = Regex.Replace(dataStr, "[a-zA-Z]", "");
            dataStr = dataStr.Replace(" ", "");
            dataStr = dataStr.Replace("%", "");

            return double.Parse(dataStr);
        }

        int ToInt(string dataStr)
        {
            if (string.IsNullOrEmpty(dataStr) || dataStr.Equals("N/A"))
            {
                dataStr = "0";
            }

            int unitWeight = 1;

            if (dataStr.Contains("G"))
            {
                unitWeight = unitWeight * 1000 * 1000 * 1000;
            }
            else if (dataStr.Contains("M"))
            {
                unitWeight = unitWeight * 1000 * 1000;
            }
            else if (dataStr.Contains("K"))
            {
                unitWeight = unitWeight * 1000;
            }

            dataStr = Regex.Replace(dataStr, "[a-zA-Z]", "");
            dataStr = dataStr.Replace(" ", "");

            if (dataStr.Contains("."))
            {
                return (int)(ToDouble(dataStr) * unitWeight);
            }
            else
            {
                return int.Parse(dataStr) * unitWeight;
            }
        }

        string[] HandleComplexData(string dataStr)
        {

            dataStr = dataStr.Replace(" ", "");

            if (dataStr.Contains("-"))
            {
                return dataStr.Split(new char[] { '-' });
            }
            else if (dataStr.Contains("/"))
            {
                return dataStr.Split(new char[] { '/' });
            }
            else
            {
                return new string[2] { "0", "0" };
            }

        }

        bool IsRootFunction(FunctionData data, string Name)
        {
            if (data.FunctionName.Equals(data.FunctionPath))
            {
                if (data.FunctionName.Contains("."))
                {
                    return data.FunctionName.StartsWith(Name + ".");
                }
                else
                {
                    return data.FunctionName.Equals(Name);
                }
            }
            return false;
        }

        List<FunctionData> GetRootFunction(List<FunctionData> KFuncDataList, string FunctionName)
        {
            return KFuncDataList.FindAll(data => IsRootFunction(data, FunctionName));
        }

        public override string ToString()
        {
            StringBuilder sbFormat = null;
            for (var i = 0; i < 56; i++)
            {
                var strIndex = "{" + i.ToString() + "}";
                if (sbFormat == null)
                {
                    sbFormat = new StringBuilder();
                    sbFormat.Append(strIndex);
                }
                else
                {
                    sbFormat.Append("\t");
                    sbFormat.Append(strIndex);
                }
            }
            sbFormat.Append("\n");

            return String.Format(sbFormat.ToString(),
                    this.Frame, this.FPS, this.CPUTotalTime_ms, this.ReservedTotal_MB, this.ReservedUnity_MB, this.ReservedGFX_MB,
                    this.ReservedMono_MB, this.ReservedProfiler_MB, this.DrawCalls, this.SetPassCalls, this.TotalBatches, this.Triangles,
                    this.Verts, this.VBOTotal_MB, this.ActiveRigidbodies, this.StaticColliders, this.DynamicColliders, this.Textures,
                    this.Textures_MB, this.Meshes, this.Meshes_MB, this.AnimationClips, this.AnimationClips_MB, this.Materials,
                    this.Materials_MB, this.AudioClips, this.AudioClips_MB, this.RenderTextures, this.RenderTextures_VMB, this.Assets,
                    this.GameObjects, this.GcAlloc_KB, this.Rendering_ms, this.Scripts_ms, this.Physics_ms, this.GarbageCollector_ms,
                    this.VSync_ms, this.Gi_ms, this.Others_ms, this.RenderTransparent_ms, this.RenderOpaque_ms, this.BehaviourUpdate_ms,
                    this.PhysicsCompute_ms, this.PhysicsProcessReports_ms, this.Animator_ms, this.ParticleSystem_ms, this.DynamicBone_ms, this.Overhead_ms,
                    this.Loading_ms, this.Instantiate_ms, this.ShaderParse_ms, this.UILogic_ms, this.Log_ms, this.WarningCount,
                    this.PlayerCount, this.NpcCount);
        }
    }
}

