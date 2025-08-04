using Newtonsoft.Json;
using OProcessedData;
using OProfilerData;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;
using static ParseFuntion;
using ProfileMaRead;
using ProfileMaRead.BinaryData;
using UnityEngine.Profiling;

public class ProfilerTest : EditorWindow
{
    private static ProfilerTest Instance;
    int frame = -1;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    [MenuItem("profiler/测试")]
    public static void profilerTest()
    {
        if (Instance == null)
        {
            Instance = ScriptableObject.CreateInstance<ProfilerTest>();
        }
        Instance.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("解析测试"))
        {
            string UUID = "test";
            int beginFrame = 1;
            bool shieldSwitch = false;
            string threadMain = "Main Thread";
            string threadRender = "Render Thread";
            int mainindex = 0;
            int renderindex = 1;

            int firstIndex = ProfilerDriver.firstFrameIndex;
            int lastIndex = ProfilerDriver.lastFrameIndex;
            Debug.Log("ProfilerDriver.firstFrameIndex: " + ProfilerDriver.firstFrameIndex);
            Debug.Log("ProfilerDriver.lastFrameIndex: " + ProfilerDriver.lastFrameIndex);

            #region 初始化容器
            ParsingToCsv parcsv = new ParsingToCsv();
            Dictionary<string, string> caseFunName = new Dictionary<string, string>(5000);  //<path,funname>
            List<string> allFunName = new List<string>(5000);
            Dictionary<string, CaseFunRow> casefunrow = new Dictionary<string, CaseFunRow>(5000);
            List<CaseFunRow> allcaseFunRow = new List<CaseFunRow>(5000);
            Dictionary<string, CaseRender> caserenfunrow = new Dictionary<string, CaseRender>(500);
            int totalcount = lastIndex - firstIndex - 1;
            int rangeCount = totalcount;
            List<CaseFlame> listFrame = new List<CaseFlame>(totalcount);
            Dictionary<string,Simple> alldata = new Dictionary<string, Simple>(77);
            List<Simple> simpledatas = new List<Simple>(totalcount);
            List<TimeLineFunData> listRen = new List<TimeLineFunData>();
            FunctionData now = new FunctionData();
            TimeLineData currentRen = new TimeLineData();
            #endregion

            #region 杂项
            ProfilerTrees prt = new ProfilerTrees();
            prt.shieldSwitch = shieldSwitch;
            Dictionary<string, string> funcnameReplace = new Dictionary<string, string>();
            List<string> funcPathstack = new List<string>();
            #endregion
            #region Profiler
            ProfilerProperty profilerProperty = new ProfilerProperty();
            ProfilerFrameDataIterator profilerFrame = new ProfilerFrameDataIterator();
            #endregion
            // 去掉首帧和尾帧
            try
            {
                for (int f = firstIndex; f < lastIndex; f++)
                {
                    if (f == firstIndex)
                    {
                        continue;
                    }
                    ProfilerTool.GetProfilerProperty(f, ref profilerProperty);
                    //if (!parcsv.GetSimpleData(profilerProperty, ref f, ref alldata,ref totalcount))
                    //{
                    //    Debug.Log("跳过了一帧:" + f);
                    //}
                    #region 处理主线程函数堆栈数据
                    CaseFlame cf = new CaseFlame();
                    cf.frame_id = f;
                    cf.flame = prt.GetChildFunFast(profilerProperty, f, ref funcnameReplace, ref funcPathstack, now,ref caseFunName);
                    listFrame.Add(cf);
                    ParseFun.GetParsedFunRowjson(f, ref casefunrow, prt, cf.flame);
                    #endregion
                    funcnameReplace.Clear();
                    funcPathstack.Clear();
                    //#region 处理渲染线程数据
                    //ProfilerTool.GetProfilerFrameData(f, renderindex, ref profilerFrame);
                    //TimeLineFunData ren = new TimeLineFunData();
                    //ren = prt.GetRenFunFast(currentRen, profilerFrame, f, renderindex, ref funcnameReplace, ref funcPathstack);
                    //listRen.Add(ren);
                    //ParseFun.GetParsedTimelineRowjson(f, ref caserenfunrow, prt, ren); //渲染线程统计数据
                    //#endregion
                    #region 释放空间
                    prt.stafun.Clear();
                    //prt.stactime.Clear();
                    //funcnameReplace.Clear();
                    //funcPathstack.Clear();
                    #endregion
                }
            }
            catch(Exception ex)
            {
                //将trycatch提取出来
                Debug.LogError(ex);
            }
            foreach (var item in casefunrow.Values)
            {
                allcaseFunRow.Add(item);
            }
            foreach (var item in caseFunName.Keys)
            {
                allFunName.Add(item);
            }
            string filePath = @"D:\000";

            if (!Directory.Exists(filePath))
            {
                System.IO.Directory.CreateDirectory(filePath);
            }

            string filename = "result.bin";
            string filename2 = "result2.bin";
            var compressData = SerializerObj(allFunName);
            //var compressData2 = SerializerObj(casefunrow);
            using (FileStream str = new FileStream(Path.Combine(filePath, filename), FileMode.Create))
            {
                str.Write(compressData, 0, compressData.Length);
            }
            //using (FileStream str = new FileStream(Path.Combine(filePath, filename2), FileMode.Create))
            //{
            //    str.Write(compressData2, 0, compressData2.Length);
            //}
        }
        if (GUILayout.Button("反序列化测试"))
        {
            string path = "D:\\000\\result.bin";
            string filepath = "D:\\000\\result.txt";
            FileStream stream = new FileStream(path, FileMode.Open);
            List<string> originalData = ProtoBuf.Serializer.Deserialize<List<string>>(stream);
            //string res = JsonConvert.SerializeObject(originalData);
            stream.Close();
            using (var sw = new StreamWriter(new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), new System.Text.UTF8Encoding(false)))
            {
                foreach(var item in originalData)
                {
                    sw.WriteLine($"{item} 10");
                }
            }
        }
        if (GUILayout.Button("测试新接口"))
        {
            ProfilerFrameDataIterator profilerFrameDataIterator = new ProfilerFrameDataIterator();
            profilerFrameDataIterator.SetRoot(1, 0);
            string threadname = profilerFrameDataIterator.GetThreadName();
            Debug.Log("ThreadName:" + threadname);
            Debug.Log("TotalTime:" + profilerFrameDataIterator.durationMS);
            FrameDataView frameDataView = ProfilerDriver.GetRawFrameDataView(1,0);
            while (profilerFrameDataIterator.Next(true))
            {
                Debug.Log("FunName:" + profilerFrameDataIterator.name);
                Debug.Log("FunPath:" + profilerFrameDataIterator.path);
                Debug.Log("FunTime:"+ profilerFrameDataIterator.durationMS);
                int instId = profilerFrameDataIterator.instanceId;
                FrameDataView.UnityObjectInfo outobjectInfo;
                frameDataView.GetUnityObjectInfo(instId, out outobjectInfo);
                if (outobjectInfo.name != null)
                {
                    Debug.Log("UnityObjectName:" + outobjectInfo.name);
                }
            }
            profilerFrameDataIterator.Dispose();
        }
        if (GUILayout.Button("转换hash值测试"))
        {
            int val = 1520718697;
            string mainthread = "Main Thread";
            string mainthread2 = "Main Thread";
            string testname = "AK.Wwise.Unity.MonoBehaviour.dll!::AkDragDropTriggerHandler.OnDestroy() [Invoke]";
            string hashTest = "b7ea04a10aac1c0c103d28faa08fe342cb8f4fc153f474db27cafcedebc3b15f";
            string hashTest1 = "b7ea04a10aac1c0c103d28faa08fe342cb8f4fc153f474db27cafcedebc3b15f2";
            string hashTest2 = "b7ea04a10aac1c0c103d28faa08fe342cb8f4fc153f474db27cafcedebc3b15f311231asdaddassssssssssssa31";
            Debug.Log("测试值：" + GenerateUniqueIdentifier(mainthread).GetHashCode());
            Debug.Log("测试值2：" + GenerateUniqueIdentifier(mainthread2).GetHashCode());
            Debug.Log("测试值3：" + hashTest2.GetHashCode());
        }
        if (GUILayout.Button("试一下解析模式"))
        {
            Init();
            string filepath = "E:\\memoryclient\\Build\\jijia.raw";
            var res = AnalyzeTest(filepath);
            Debug.Log(res.Count);
        }
        if (GUILayout.Button("GPUData_Analyze"))
        {
            int TestFrameIndex = 91;
            ProfilerProperty profilerProperty = new ProfilerProperty();
            ProfilerTool.GetProfilerProperty(TestFrameIndex, ref profilerProperty);
            //string TestFunName = "";
            //TestFunName = profilerProperty.GetColumn(HierarchyFrameDataView.columnName);
            //Debug.Log("测试：" + TestFunName);

            while (profilerProperty.Next(true))
            {
                string funname = profilerProperty.GetColumn(HierarchyFrameDataView.columnName).Replace("\n", "");

                if (funname.Equals("[KEngine] LateUpdate"))
                {
                    string FunctionPath = profilerProperty.propertyPath.Replace("\n", "");
                    string FunctionName = profilerProperty.GetColumn(HierarchyFrameDataView.columnName).Replace("\n", "");
                    float DrawCalls = profilerProperty.GetColumnAsSingle(7);
                    string TotalGPUPersent = profilerProperty.GetColumn(10);
                    float TotalTime_ms = profilerProperty.GetColumnAsSingle(8);
                    Debug.Log("FunName: " + FunctionName);
                    Debug.Log("FunPath: " + FunctionPath);
                    Debug.Log("TotalPersent: " + TotalGPUPersent);
                    Debug.Log("DrawCalls: " + DrawCalls);
                    Debug.Log("GPUTime: " + TotalTime_ms);
                    break;
                }
            }
        }
    }

    public static void Init()
    {
        ProfilerLogUtil.logErrorException = (e) => {
            //    Debug.LogError(e);
        };
        ProfilerLogUtil.logErrorString = (str) => {
            //    Debug.LogError(str);
        };
    }
    
    private List<ProfileMaRead.BinaryData.ProfilerFrameData> AnalyzeTest(string filepath)
    {
        long currentPosition = 0L;
        int f = 0;
        List<ProfilerFrameData> result = new List<ProfilerFrameData>();
        using (FileStream fileStream = File.OpenRead(filepath))
        {
            if (fileStream.Length == 0)
            {
                return null;
            }
            while (true)
            {
                ProfilerFrameBlockData profilerFrameBlockData = new ProfilerFrameBlockData();
                fileStream.Position = currentPosition;
                if (fileStream == null || fileStream.Position >= fileStream.Length)
                    break;
                bool flag = profilerFrameBlockData.Read((Stream)fileStream, f, Application.unityVersion);
                if (flag)
                    ++f;
                currentPosition = fileStream.Position;
                if (profilerFrameBlockData.frameData != null)
                {
                    profilerFrameBlockData.frameData.ApplyCountersToDeplicatedStats();
                    //逻辑处理，去掉最后一帧的数据
                    result.Add(profilerFrameBlockData.frameData);
                }
            }
            fileStream.Close();
        }
    
        return result;
    }
    
    public string GenerateUniqueIdentifier(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                stringBuilder.Append(hashBytes[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }
    }

    public byte[] SerializerObj(List<string> info)
    {
        try
        {
            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, info);
                byte[] data = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(data, 0, data.Length);
                return data;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    public byte[] SerializerObj(List<TimeLineFunData> info)
    {
        try
        {
            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, info);
                byte[] data = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(data, 0, data.Length);
                return data;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    public byte[] SerializerObj(List<Simple> info)
    {
        try
        {
            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, info);
                byte[] data = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(data, 0, data.Length);
                return data;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    public byte[] SerializerObj(List<CaseFlame> info)
    {
        try
        {
            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, info);
                byte[] data = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(data, 0, data.Length);
                return data;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    public byte[] SerializerObj(List<CaseFunRow> info)
    {
        try
        {
            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, info);
                byte[] data = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(data, 0, data.Length);
                return data;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    public byte[] SerializerObj(Dictionary<int,CaseRender> info)
    {
        try
        {
            using(MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, info);
                byte[] data = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(data, 0, data.Length);
                return data;
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }
}
