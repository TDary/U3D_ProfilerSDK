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
            List<List<string>> data = new List<List<string>>();
            Dictionary<string, CaseFunRow> casefunrow = new Dictionary<string, CaseFunRow>();
            Dictionary<int, string> funhashMap = new Dictionary<int, string>();
            Dictionary<int, CaseRender> caserenfunrow = new Dictionary<int, CaseRender>();
            int totalcount = lastIndex - firstIndex - 1;
            int rangeCount = totalcount;
            List<CaseFlame> listFrame = new List<CaseFlame>(totalcount);
            FunctionData now = new FunctionData();
            TimeLineData currentRen = new TimeLineData();
            #endregion

            #region 杂项
            ProfilerTrees prt = new ProfilerTrees();
            prt.shieldSwitch = shieldSwitch;
            TimeLineFunData ren = new TimeLineFunData();
            Dictionary<string, string> funcnameReplace = new Dictionary<string, string>();
            List<string> funcPathstack = new List<string>();
            #endregion
            #region Profiler
            ProfilerProperty profilerProperty = new ProfilerProperty();
            ProfilerFrameDataIterator profilerFrame = new ProfilerFrameDataIterator();
            #endregion
            //// 去掉首帧和尾帧
            var currentTime = DateTime.Now;
            try
            {
                for (int f = firstIndex; f < lastIndex; f++)
                {
                    if (f == firstIndex)
                    {
                        continue;
                    }
                    ProfilerTool.GetProfilerProperty(f, ref profilerProperty);
                    #region 处理主线程函数堆栈数据
                    CaseFlame cf = new CaseFlame();
                    cf.frame_id = f;
                    cf.flame = prt.GetChildFunFast(profilerProperty, f, ref funcnameReplace, ref funhashMap, ref funcPathstack, now);
                    listFrame.Add(cf);
                    ParseFun.GetParsedFunRowjson(f, ref casefunrow, prt, cf.flame);
                    #endregion
                    funcnameReplace.Clear();
                    funcPathstack.Clear();
                    #region 处理渲染线程数据
                    //ProfilerTool.GetProfilerFrameData(f, renderindex, ref profilerFrame);
                    //ren = prt.GetRenFunFast(currentRen, profilerFrame, f, renderindex, ref funcnameReplace, ref funcPathstack, ref funhashMap);
                    //ParseFun.GetParsedTimelineRowjson(f, ref caserenfunrow, prt, ren, ref funhashMap); //渲染线程统计数据
                    //ren.init();
                    #endregion
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
            string filePath = @"D:\000";

            if (!Directory.Exists(filePath))
            {
                System.IO.Directory.CreateDirectory(filePath);
            }
            string filename = "result.bin";
            string filename2 = "result2.bin";
            var compressData = SerializerObj(listFrame);
            var compressData2 = SerializerObj(casefunrow);
            using (FileStream str = new FileStream(Path.Combine(filePath, filename), FileMode.Create))
            {
                str.Write(compressData, 0, compressData.Length);
            }
            using (FileStream str = new FileStream(Path.Combine(filePath, filename2), FileMode.Create))
            {
                str.Write(compressData2, 0, compressData2.Length);
            }
        }
        if (GUILayout.Button("反序列化测试"))
        {
            string path = "D:\\000\\result.bin";
            FileStream stream = new FileStream(path, FileMode.Open);
            List<CaseFlame> originalData = ProtoBuf.Serializer.Deserialize<List<CaseFlame>>(stream);
            
            stream.Close();
        }
        if (GUILayout.Button("测试新接口"))
        {
            RawFrameDataView rawFrameDataView = ProfilerDriver.GetRawFrameDataView(2, 1);
            if (rawFrameDataView != null && rawFrameDataView.valid)
            {
                Debug.Log(rawFrameDataView.frameStartTimeMs);
                Debug.Log(rawFrameDataView.frameFps);
                Debug.Log(rawFrameDataView.threadName);
            }
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

    public byte[] SerializerObj(Dictionary<string,CaseFunRow> info)
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
