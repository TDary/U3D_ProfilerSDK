using LitJson;
using OProcessedData;
using OProfilerData;
using System.Collections.Generic;
using System.IO;
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
            List<List<string>> data = new List<List<string>>();
            Dictionary<int, CaseFunRow> casefunrow = new Dictionary<int, CaseFunRow>();
            Dictionary<int, string> funhashMap = new Dictionary<int, string>();
            Dictionary<int, CaseRender> caserenfunrow = new Dictionary<int, CaseRender>();
            List<CaseFlame> listFrame = new List<CaseFlame>();
            #endregion

            #region  原始数据
            List<FunctionData> FunctionStackData = new List<FunctionData>();
            List<TimeLineData> timedata = new List<TimeLineData>();
            #endregion

            #region 杂项
            Dictionary<string, int> allfun = new Dictionary<string, int>();
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

            // 去掉首帧和尾帧
            for (int f = firstIndex; f < lastIndex; f++)
            {
                if (f == firstIndex)
                {
                    continue;
                }

                ProfilerHelper.GetProfilerProperty(f, ref profilerProperty);
                // 解析CSV及原始堆栈数据
                if (ParsingToCsv.GetParsedCsv(profilerProperty, ref f, ref data, ref funcnameReplace, ref funcPathstack, ref FunctionStackData))
                {
                    #region 处理函数堆栈数据
                    CaseFlame cf = new CaseFlame();
                    cf.flame = prt.GetChildFun(FunctionStackData, ref funhashMap);
                    listFrame.Add(cf);
                    ParseFun.GetParsedFunRowjson(f, ref casefunrow, prt, cf.flame); //主线程堆栈统计数据
                    #endregion
                    funcnameReplace.Clear();
                    funcPathstack.Clear();
                    #region 处理渲染线程数据
                    ProfilerHelper.GetProfilerFrameData(f, renderindex, ref profilerFrame);
                    ProfilerHelper.ExtractTimeLineData(profilerFrame, f, renderindex, ref funcnameReplace, ref timedata, ref funcPathstack);
                    ren = prt.GetRenFun(timedata, ref funhashMap);
                    ParseFun.GetParsedTimelineRowjson(f, ref caserenfunrow, prt, ren, ref funhashMap); //渲染线程统计数据
                    ren.init();
                    #endregion
                    #region 释放空间
                    FunctionStackData.Clear();
                    timedata.Clear();
                    prt.stafun.Clear();
                    funcnameReplace.Clear();
                    funcPathstack.Clear();
                    #endregion
                }
            }
            string jstr = JsonMapper.ToJson(listFrame);

            string filePath = @"D:\000";

            if (!Directory.Exists(filePath))
            {
                System.IO.Directory.CreateDirectory(filePath);
            }

            string filename = "t.json";
            using (var sw = new StreamWriter(new FileStream(Path.Combine(filePath, filename), FileMode.Create, FileAccess.Write, FileShare.ReadWrite), new System.Text.UTF8Encoding(false)))
            {
                sw.Write(jstr);
            }
        }
    }
}
