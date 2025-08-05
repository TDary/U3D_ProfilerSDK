using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OProfilerData;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Profiling;
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
        string FunRowPath;
        //string FunRenderRowPath;
        string FunPath;
        string FunNamePath;
        //string RenFunPath;
        string AnalyzeType;
        string ServerUrl;
        string UUID;
        int renderindex;
        int mainthreadindex;
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

            UUID = comd.ContainsKey("-UUID") ? comd["-UUID"] : "";
            RawPath = comd.ContainsKey("-rawPath") ? comd["-rawPath"] : "";
            CsvPath = comd.ContainsKey("-csvPath") ? comd["-csvPath"] : "/csv";
            FunPath = comd.ContainsKey("-funPath") ? comd["-funPath"] : "/funbin";
            FunNamePath = comd.ContainsKey("-funnamePath") ? comd["-funnamePath"] : "/funbin";
            FunRowPath = comd.ContainsKey("-funrowPath") ? comd["-funrowPath"] : "/funbin";
            //RenFunPath = comd.ContainsKey("-renfunPath") ? comd["-renfunPath"] : "/funbin";
            //FunRenderRowPath = comd.ContainsKey("-funrenderrowPath") ? comd["-funrenderrowPath"] : "/funbin";
            AnalyzeType = comd.ContainsKey("-analyzeType") ? comd["-analyzeType"] : "";
            string switchS = comd.ContainsKey("-shieldSwitch") ? comd["-shieldSwitch"] : "";
            if (!string.IsNullOrEmpty(switchS))
            {
                shieldSwitch = Convert.ToBoolean(switchS);
            }
            ServerUrl = comd.ContainsKey("-ServerUrl") ? comd["-ServerUrl"] : "";

            if (RawPath == "")
            {
                Debug.LogWarning("Raw文件为空");
                return;
            }

            Debug.Log("RawPath:" + RawPath);
            Debug.Log("CvsPath:" + CsvPath);
            Debug.Log("FunPath:" + FunPath);
            Debug.Log("FunNamePath:" + FunNamePath);
            Debug.Log("FunRowPath:" + FunRowPath);
            //Debug.Log("RenFunPath:" + RenFunPath);
            //Debug.Log("FunRenderRowPath:" + FunRenderRowPath);

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

            if (AnalyzeType == "simple")
            {
                int totalCount = lastIndex - firstIndex - 1;
                List<Simple> simpledatas = new List<Simple>(77);  //转为List好反序列化
                Dictionary<string, Simple> alldata = new Dictionary<string, Simple>(77);   //根据原始数据，设置初始值
                ParsingToCsv parcsv = new ParsingToCsv();
                ProfilerProperty profilerProperty = new ProfilerProperty();
                for (int f = firstIndex; f < lastIndex; f++)
                {
                    if (f == firstIndex)
                    {
                        continue;
                    }
                    ProfilerTool.GetProfilerProperty(f, ref profilerProperty);
                    if (!parcsv.GetSimpleData(profilerProperty, ref f, ref alldata, ref totalCount))
                    {
                        Debug.Log("跳过了一帧:" + f);
                    }
                }
                foreach(var item in alldata.Values)
                {
                    simpledatas.Add(item);
                }
                #region SaveCsvData
                SaveCsv(CsvPath, simpledatas);
                #endregion
                #region SendEndData
                Debug.Log(AnalyzeType + "解析完成----");
                if (RawPath.Contains('\\'))
                {
                    RawPath.Replace("\\", "/");
                }
                var data = RawPath.Split('/');
                StringBuilder stringdata = new StringBuilder();
                stringdata.Append("http://");
                stringdata.Append(ServerUrl);
                stringdata.Append("/stop?uuid=");
                stringdata.Append(UUID);
                stringdata.Append("&rawfile=");
                stringdata.Append(data[data.Length - 1]);
                stringdata.Append("&anaType=");
                stringdata.Append(AnalyzeType);
                stringdata.Append("&state=");
                stringdata.Append("success");
                string Response = SHttpSender.SendGet(stringdata.ToString());
                Debug.Log("解析完成消息上报：" + "Response" + Response);
                #endregion
            }
            else if (AnalyzeType == "funprofiler")
            {
                Dictionary<string,string> caseFunName = new Dictionary<string, string>(5000);  //<path,funname>
                List<string> allFunName = new List<string>(5000);
                Dictionary<string, CaseFunRow> casefunrow = new Dictionary<string, CaseFunRow>(5000);
                List<CaseFunRow> allcaseFunRow = new List<CaseFunRow>(5000);
                //Dictionary<string, CaseRender> caserenfunrow = new Dictionary<string, CaseRender>(500);
                int totalCount = lastIndex - firstIndex - 1;
                FunctionData now = new FunctionData();
                //TimeLineData currentRen = new TimeLineData();
                List<CaseFlame> listFrame = new List<CaseFlame>(totalCount);
                //List<TimeLineFunData> listRen = new List<TimeLineFunData>();
                ProfilerTrees prt = new ProfilerTrees();
                prt.shieldSwitch = shieldSwitch;
                Dictionary<string, string> funcnameReplace = new Dictionary<string, string>(50);
                List<string> funcPathstack = new List<string>(50);
                ProfilerProperty profilerProperty = new ProfilerProperty();
                //ProfilerFrameDataIterator profilerFrame = new ProfilerFrameDataIterator();
                #region 处理大类
                for (int f = firstIndex; f < lastIndex; f++)
                {
                    if (f == firstIndex)
                    {
                        continue;
                    }

                    ProfilerTool.GetProfilerProperty(f, ref profilerProperty);
                    if (profilerProperty == null && profilerProperty.frameTime == "0")  //判断帧数据是否异常
                        continue;
                    #region 处理函数堆栈数据
                    CaseFlame cf = new CaseFlame();
                    cf.flame = prt.GetChildFunFast(profilerProperty, f, ref funcnameReplace, ref funcPathstack, now,ref caseFunName);
                    listFrame.Add(cf);
                    ParseFun.GetParsedFunRowjson(f, ref casefunrow, prt, cf.flame); //主线程堆栈统计数据
                    #endregion
                    //funcnameReplace.Clear();
                    //funcPathstack.Clear();
                    //#region 处理渲染线程数据
                    //ProfilerTool.GetProfilerFrameData(f, renderindex, ref profilerFrame);
                    //TimeLineFunData ren = new TimeLineFunData();
                    //ren = prt.GetRenFunFast(currentRen, profilerFrame, f, renderindex, ref funcnameReplace, ref funcPathstack);
                    //listRen.Add(ren);
                    //ParseFun.GetParsedTimelineRowjson(f, ref caserenfunrow, prt, ren); //渲染线程统计数据
                    //#endregion
                    #region 释放空间
                    prt.stafun.Clear();
                    funcnameReplace.Clear();
                    funcPathstack.Clear();
                    #endregion
                }
                foreach(var item in casefunrow.Values)
                {
                    allcaseFunRow.Add(item);
                }
                foreach (var item in caseFunName.Keys)
                {
                    allFunName.Add(item);
                }
                #region SaveData
                if (listFrame.Count != 0 && allcaseFunRow.Count != 0)
                {

                    SaveFun(FunPath, listFrame);
                    SaveFunRow(FunRowPath, allcaseFunRow);
                    SaveFunName(FunNamePath,allFunName);
                    //SaveRenderRow(FunRenderRowPath, caserenfunrow);   //暂时不用渲染线程
                    //SaveRenderFun(RenFunPath, listRen);
                }
                else
                {
                    string errorCount = $"listFrame Count:{listFrame.Count}，allcaseFunRow Count:{allcaseFunRow.Count}";
                    Debug.Log(errorCount);
                    throw new Exception(errorCount);
                }
                #region SendMessageForSuccess
                Debug.Log(AnalyzeType + "解析完成----");
                if (RawPath.Contains('\\'))
                {
                    RawPath.Replace("\\", "/");
                }
                var data = RawPath.Split('/');
                StringBuilder stringdata = new StringBuilder();
                stringdata.Append("http://");
                stringdata.Append(ServerUrl);
                stringdata.Append("/stop?uuid=");
                stringdata.Append(UUID);
                stringdata.Append("&rawfile=");
                stringdata.Append(data[data.Length - 1]);
                stringdata.Append("&anaType=");
                stringdata.Append(AnalyzeType);
                stringdata.Append("&state=");
                stringdata.Append("success");
                string Response = SHttpSender.SendGet(stringdata.ToString());
                Debug.Log("解析完成消息上报：" + "Response" + Response);
                #endregion
                #endregion
                #endregion
            }
            else if(AnalyzeType == "funtimeline")
            {
                Dictionary<string, CaseRender> caserenfunrow = new Dictionary<string, CaseRender>(500);
                TimeLineData currentRen = new TimeLineData();
                List<TimeLineFunData> listRen = new List<TimeLineFunData>();
                ProfilerFrameDataIterator profilerFrame = new ProfilerFrameDataIterator();
                ProfilerTrees prt = new ProfilerTrees();
                prt.shieldSwitch = shieldSwitch;
                Dictionary<string, string> funcnameReplace = new Dictionary<string, string>(50);
                List<string> funcPathstack = new List<string>(50);
                for (int f = firstIndex; f < lastIndex; f++)
                {
                    if (f == firstIndex)
                    {
                        continue;
                    }
                    FrameDataView frameDataView = ProfilerDriver.GetRawFrameDataView(f, mainthreadindex);
                    ProfilerTool.GetProfilerFrameData(f, mainthreadindex, ref profilerFrame);
                    TimeLineFunData ren = new TimeLineFunData();
                    ren = prt.GetRenFunFast(currentRen, profilerFrame, f, mainthreadindex, ref funcnameReplace, ref funcPathstack, frameDataView);
                    listRen.Add(ren);
                    ParseFun.GetParsedTimelineRowjson(f, ref caserenfunrow, prt, ren); //Timeline线程统计数据
                }
            }
            else //deep
            {
                //TODO
            }
            #region 旧版
            //#region 初始化容器
            //ParsingToCsv parcsv = new ParsingToCsv();
            //List<List<string>> data = new List<List<string>>();
            //Dictionary<int, CaseFunRow> casefunrow = new Dictionary<int, CaseFunRow>();
            //Dictionary<int, string> funhashMap = new Dictionary<int, string>();
            //Dictionary<int, CaseRender> caserenfunrow = new Dictionary<int, CaseRender>();
            //List<CaseFlame> listFrame = new List<CaseFlame>();
            //int rangeCount = lastIndex - firstIndex - 1;
            //#endregion

            //#region  原始数据
            //List<FunctionData> FunctionStackData = new List<FunctionData>();
            //List<TimeLineData> timedata = new List<TimeLineData>();
            //#endregion

            //#region 杂项
            //Dictionary<string, int> allfun = new Dictionary<string, int>();
            //ProfilerTrees prt = new ProfilerTrees();
            //prt.shieldSwitch = shieldSwitch;
            //TimeLineFunData ren = new TimeLineFunData();
            //Dictionary<string, string> funcnameReplace = new Dictionary<string, string>();
            //List<string> funcPathstack = new List<string>();
            //#endregion
            //#region Profiler
            //ProfilerProperty profilerProperty = new ProfilerProperty();
            //ProfilerFrameDataIterator profilerFrame = new ProfilerFrameDataIterator();
            //#endregion

            //#region 处理大类
            //for (int f = firstIndex; f < lastIndex; f++)
            //{
            //    if (f == firstIndex)
            //    {
            //        continue;
            //    }

            //ProfilerHelper.GetProfilerProperty(f, ref profilerProperty);
            //// 解析CSV及原始堆栈数据
            //if (parcsv.GetSimpleData(profilerProperty, ref f, ref data,ref rangeCount))
            //{
            //    int count = f;
            //    #region 处理函数堆栈数据
            //    ProfilerHelper.ExtractFunctionData(profilerProperty, f, ref funcnameReplace, ref FunctionStackData, ref funcPathstack);

            //    CaseFlame cf = new CaseFlame();
            //    cf.flame = prt.GetChildFun(FunctionStackData, ref funhashMap);
            //    listFrame.Add(cf);
            //    ParseFun.GetParsedFunRowjson(f, ref casefunrow, prt, cf.flame); //主线程堆栈统计数据
            //    #endregion
            //    funcnameReplace.Clear();
            //    funcPathstack.Clear();
            //    #region 处理渲染线程数据
            //    ProfilerHelper.GetProfilerFrameData(f, renderindex, ref profilerFrame);
            //    ProfilerHelper.ExtractTimeLineData(profilerFrame, f, renderindex, ref funcnameReplace, ref timedata, ref funcPathstack);
            //    ren = prt.GetRenFun(timedata, ref funhashMap);
            //    ParseFun.GetParsedTimelineRowjson(f, ref caserenfunrow, prt, ren, ref funhashMap); //渲染线程统计数据
            //    ren.init();
            //    #endregion
            //    #region 释放空间
            //        FunctionStackData.Clear();
            //        timedata.Clear();
            //        prt.stafun.Clear();
            //        funcnameReplace.Clear();
            //        funcPathstack.Clear();
            //        #endregion
            //    }
            //}
            ////保存json文件线程1
            //Thread thread1 = new Thread(() =>
            //{
            //    CsvFileHelper.SaveCsvFile(CsvPath, data);
            //    SaveFunRowjson(FunRowjsonPath, casefunrow);
            //    SaveFunHashJson(FunHashPath, funhashMap);
            //});
            //thread1.Start();

            ////保存json文件线程2
            //Thread thread2 = new Thread(() =>
            //{
            //    SaveRenderRowjson(FunRenderRowjsonPath, caserenfunrow);
            //    SaveFunJson(FunJsonPath, listFrame);
            //});
            //thread2.Start();

            //while (thread1.ThreadState != ThreadState.Stopped || thread2.ThreadState != ThreadState.Stopped)
            //{
            //    if (thread1.ThreadState != ThreadState.Stopped)
            //    {
            //        thread1.Join();
            //    }
            //    else
            //    {
            //        thread2.Join();
            //    }
            //}
            //#endregion
            #endregion
        }

        #region 保存文件
        public void SaveCsv(string filePath,List<Simple> csvdata)
        {
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

            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, csvdata);
                byte[] data = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(data, 0, data.Length);
                FileStream str = new FileStream(filePath, FileMode.Create);
                str.Write(data, 0, data.Length);
                stream.Close();
                str.Close();
            }
        }
        public void SaveFunName(string filePath, List<string> allfunNamePath)
        {

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

            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, allfunNamePath);
                byte[] data = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(data, 0, data.Length);
                FileStream str = new FileStream(filePath, FileMode.Create);
                str.Write(data, 0, data.Length);
                stream.Close();
                str.Close();
            }
        }
        public void SaveFun(string filePath, List<CaseFlame> listFrame)
        {
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

            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, listFrame);
                byte[] data = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(data, 0, data.Length);
                FileStream str = new FileStream(filePath, FileMode.Create);
                str.Write(data, 0, data.Length);
                stream.Close();
                str.Close();
            }
        }

        public void SaveFunRow(string filePath, List< CaseFunRow> casefunrow)
        {
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
            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, casefunrow);
                byte[] data = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(data, 0, data.Length);
                FileStream str = new FileStream(filePath, FileMode.Create);
                str.Write(data, 0, data.Length);
                stream.Close();
                str.Close();
            }
        }

        public void SaveRenderFun(string filePath, List<TimeLineFunData> allrenderFun)
        {
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
            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, allrenderFun);
                byte[] data = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(data, 0, data.Length);
                FileStream str = new FileStream(filePath, FileMode.Create);
                str.Write(data, 0, data.Length);
                stream.Close();
                str.Close();
            }
        }

        public void SaveRenderRow(string filePath, Dictionary<string, CaseRender> allcasefunrow)
        {
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
            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, allcasefunrow);
                byte[] data = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(data, 0, data.Length);
                FileStream str = new FileStream(filePath, FileMode.Create);
                str.Write(data, 0, data.Length);
                stream.Close();
                str.Close();
            }
        }
        #endregion
        //#region 压缩和反压缩二进制文件
        //public byte[] CompressGZip(byte[] rawData)
        //{
        //    MemoryStream ms = new MemoryStream();
        //    GZipOutputStream compressedzipStream = new GZipOutputStream(ms);
        //    compressedzipStream.Write(rawData, 0, rawData.Length);
        //    compressedzipStream.Close();
        //    return ms.ToArray();
        //}
        //public byte[] UnGZip(byte[] byteArray)
        //{
        //    GZipInputStream gzi = new GZipInputStream(new MemoryStream(byteArray));

        //    MemoryStream re = new MemoryStream(50000);
        //    int count;
        //    byte[] data = new byte[50000];
        //    while ((count = gzi.Read(data, 0, data.Length)) != 0)
        //    {
        //        re.Write(data, 0, count);
        //    }
        //    byte[] overarr = re.ToArray();
        //    return overarr;
        //}
        //#endregion
    }
}
