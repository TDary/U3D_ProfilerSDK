using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LitJson;
using OProfilerData;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Profiling;
using static ParseFuntion;

namespace Analyze
{
    internal class AnalyzeSDK
    {
        private static AnalyzeSDK _AnalyzeSDK = null;

        string RawPath;
        string CsvPath;
        string FunRowjsonPath;
        string FunRenderRowjsonPath;
        string FunHashPath;
        string FunJsonPath;
        int renderindex;
        bool shieldSwitch;
        static AnalyzeSDK()
        {
            _AnalyzeSDK = new AnalyzeSDK();
        }

        public static AnalyzeSDK instance()
        {
            return _AnalyzeSDK;
        }

        public void start(Dictionary<string, string> comd)
        {
            Debug.Log("AnalyzeSDK Begin----");
            renderindex = 1;

            RawPath = comd.ContainsKey("-rawPath") ? comd["-rawPath"] : "";
            CsvPath = comd.ContainsKey("-csvPath") ? comd["-csvPath"] : "/csv";
            FunJsonPath = comd.ContainsKey("-funjsonPath") ? comd["-funjsonPath"] : "/funjson";
            FunRowjsonPath = comd.ContainsKey("-funrowjsonPath") ? comd["-funrowjsonPath"] : "/funjson";
            FunRenderRowjsonPath = comd.ContainsKey("-funrenderrowjsonPath") ? comd["-funrenderrowjsonPath"] : "/funjson";
            FunHashPath = comd.ContainsKey("-funhashPath") ? comd["-funhashPath"] : "/funjson";
            shieldSwitch = Convert.ToBoolean(comd.ContainsKey("-shieldSwitch") ? comd["-shieldSwitch"] : "");

            if (RawPath == "")
            {
                Debug.LogWarning("Raw文件为空");
                return;
            }

            Debug.Log("RawPath:" + RawPath);
            Debug.Log("CvsPath:" + CsvPath);
            Debug.Log("FunJsonPath:" + FunJsonPath);
            Debug.Log("FunRowjsonPath:" + FunRowjsonPath);
            Debug.Log("FunRenderRowjsonPath:" + FunRenderRowjsonPath);
            Debug.Log("FunHashPath:" + FunHashPath);

            ProfilerDriver.ClearAllFrames();

            Profiler.AddFramesFromFile(RawPath);

            int firstIndex = ProfilerDriver.firstFrameIndex;
            int lastIndex = ProfilerDriver.lastFrameIndex;
            Debug.Log("ProfilerDriver.firstFrameIndex: " + ProfilerDriver.firstFrameIndex);
            Debug.Log("ProfilerDriver.lastFrameIndex: " + ProfilerDriver.lastFrameIndex);

            if (lastIndex == -1 || firstIndex == -1)
            {
                throw new Exception("ProfilerDriver can't work");
            }

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

            #region 处理大类
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
            //保存json文件线程1
            Thread thread1 = new Thread(() =>
            {
                CsvFileHelper.SaveCsvFile(CsvPath, data);
                SaveFunRowjson(FunRowjsonPath, casefunrow);
                SaveFunHashJson(FunHashPath, funhashMap);
            });
            thread1.Start();

            //保存json文件线程2
            Thread thread2 = new Thread(() =>
            {
                SaveRenderRowjson(FunRenderRowjsonPath, caserenfunrow);
                SaveFunJson(FunJsonPath, listFrame);
            });
            thread2.Start();

            while (thread1.ThreadState != ThreadState.Stopped || thread2.ThreadState != ThreadState.Stopped)
            {
                if (thread1.ThreadState != ThreadState.Stopped)
                {
                    thread1.Join();
                }
                else
                {
                    thread2.Join();
                }
            }
#endregion
        }

        #region 保存文件
        public void SaveFunJson(string filePath, List<CaseFlame> listFrame)
        {
            string jstr = JsonMapper.ToJson(listFrame);
            if (filePath.Contains("\\"))
            {
                string newPath = filePath.Remove(filePath.LastIndexOf('\\'));
                if (!Directory.Exists(newPath))
                {
                    System.IO.Directory.CreateDirectory(newPath);
                }
            }
            //兼容Linux环境的路径
            else if (filePath.Contains("/"))
            {
                string newPath = filePath.Remove(filePath.LastIndexOf('/'));
                if (!Directory.Exists(newPath))
                {
                    System.IO.Directory.CreateDirectory(newPath);
                }
            }
            else
            {
                throw new Exception("在文件路径中查找错误");
            }
            using (var sw = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), new System.Text.UTF8Encoding(false)))
            {
                sw.Write(jstr);
            }
        }

        public void SaveFunRowjson(string filePath, Dictionary<int, CaseFunRow> casefunrow)
        {
            string jstr = JsonMapper.ToJson(casefunrow);
            if (filePath.Contains("\\"))
            {
                string newPath = filePath.Remove(filePath.LastIndexOf('\\'));
                if (!Directory.Exists(newPath))
                {
                    System.IO.Directory.CreateDirectory(newPath);
                }
            }
            //兼容Linux环境的路径
            else if (filePath.Contains("/"))
            {
                string newPath = filePath.Remove(filePath.LastIndexOf('/'));
                if (!Directory.Exists(newPath))
                {
                    System.IO.Directory.CreateDirectory(newPath);
                }
            }
            else
            {
                throw new Exception("在文件路径中查找错误");
            }
            using (var sw = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), new System.Text.UTF8Encoding(false)))
            {
                sw.Write(jstr);
            }
        }

        public void SaveFunHashJson(string filePath, Dictionary<int, string> funhashMap)
        {
            string jstr = JsonMapper.ToJson(funhashMap);
            if (filePath.Contains("\\"))
            {
                string newPath = filePath.Remove(filePath.LastIndexOf('\\'));
                if (!Directory.Exists(newPath))
                {
                    System.IO.Directory.CreateDirectory(newPath);
                }
            }
            //兼容Linux环境的路径
            else if (filePath.Contains("/"))
            {
                string newPath = filePath.Remove(filePath.LastIndexOf('/'));
                if (!Directory.Exists(newPath))
                {
                    System.IO.Directory.CreateDirectory(newPath);
                }
            }
            else
            {
                throw new Exception("在文件路径中查找错误");
            }
            using (var sw = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), new System.Text.UTF8Encoding(false)))
            {
                sw.Write(jstr);
            }
        }

        public void SaveRenderRowjson(string filePath, Dictionary<int, CaseRender> allcasefunrow)
        {
            string jstr = JsonMapper.ToJson(allcasefunrow);
            if (filePath.Contains("\\"))
            {
                string newPath = filePath.Remove(filePath.LastIndexOf('\\'));
                if (!Directory.Exists(newPath))
                {
                    System.IO.Directory.CreateDirectory(newPath);
                }
            }
            //兼容Linux环境的路径
            else if (filePath.Contains("/"))
            {
                string newPath = filePath.Remove(filePath.LastIndexOf('/'));
                if (!Directory.Exists(newPath))
                {
                    System.IO.Directory.CreateDirectory(newPath);
                }
            }
            else
            {
                throw new Exception("在文件路径中查找错误");
            }
            using (var sw = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), new System.Text.UTF8Encoding(false)))
            {
                sw.Write(jstr);
            }
        }
        #endregion
    }
}
