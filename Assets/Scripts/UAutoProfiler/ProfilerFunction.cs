using KProfilerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ProfilerFunctionFactory
{

    public static string TreeFunctionToJson()
    {
        //string t = "{\"data\":{\"notify_url\":\"http://10.11.67.131:3000\"},\"code\":200,\"message\":\"succeed\"}";

        //float[] _v = {123, 1.5f, 2 };
        //string _f = "f";
        //var jobj = new { _f = 1, v =  _v};

        //Dictionary<uint, string> dic = new Dictionary<uint, string>();
        //dic.Add(35621, "abcdd");
        //dic.Add(1234433, "ooeiunb");

        //string jstr = JsonConvert.SerializeObject(dic);

        //JObject obj = JObject.Parse(jstr);

        //SimpleTreeFunction stf2 = new SimpleTreeFunction();
        //stf2.f = 1;
        //stf2.v = new float[] { 789f, 23.3f, 0f, 0f };
        //stf2.c = null;

        //List< SimpleTreeFunction > stf1List = new List<SimpleTreeFunction>();
        //stf1List.Add(stf2);

        //SimpleTreeFunction stf1 = new SimpleTreeFunction();
        //stf1.f = 1;
        //stf1.v = new float[] { 456f, 23.3f, 0f, 0f };
        //stf1.c = stf1List;

        //List<SimpleTreeFunction> stfList = new List<SimpleTreeFunction>();
        //stfList.Add(stf1);

        //SimpleTreeFunction stf = new SimpleTreeFunction();
        //stf.f = 1;
        //stf.v = new float[]{ 123f, 23.3f,0f,0f};
        //stf.c = stfList;

        //string jstr = JsonConvert.SerializeObject(stf);

        //SimpleTreeFunction obj = JsonConvert.DeserializeObject<SimpleTreeFunction>(jstr);

        //string jstr = "{\"1\":[\"aaa\",\"bbb\"]}";

        //JObject notifyData = JObject.Parse(getdata);

        //if (notifyData["code"].ToString() == "200")
        //{
        //    string url = notifyData["data"]["notify_url"].ToString();

        //JObject obj = JObject.Parse(jstr);


        //obj.Add("name", );

        return null;
    }

    public static List<ProfilerTimeLineFunction> ToProfilerTreeFunctionList(List<KTimeLineData> TimelinefunData)
    {
        List<ProfilerTimeLineFunction> rootFunctions = new List<ProfilerTimeLineFunction>();
        foreach (KTimeLineData fundata in TimelinefunData)
        {
            bool ishaveRoot = false;
            foreach (ProfilerTimeLineFunction root in rootFunctions)
            {
                if (root.AddDescendant(fundata))
                {
                    ishaveRoot = true;
                    break;
                }
            }

            if (!ishaveRoot)
            {
                ProfilerTimeLineFunction root = ProfilerFunctionFactory.Parsing(fundata);
                rootFunctions.Add(root);
            }
        }
        return rootFunctions;
    }

    public static ProfilerTimeLineFunction Parsing(KTimeLineData fundata)
    {
        ProfilerTimeLineFunction rootFunction = null;
        string[] functionnameArray = fundata.functionPath.Split('/');

        ProfilerTimeLineFunction tempParentFunction = null;

        foreach (string functionname in functionnameArray)
        {
            if (rootFunction == null)
            {
                rootFunction = new ProfilerTimeLineFunction(functionname, null);
                tempParentFunction = rootFunction;
                continue;
            }

            if (functionname == fundata.functionName)
            {
                ProfilerTimeLineFunction nodeFunction = tempParentFunction.GetChildWithCreate(functionname);
                nodeFunction.FunctionData = fundata;

                return rootFunction;
            }
            else
            {
                tempParentFunction = tempParentFunction.GetChildWithCreate(functionname);
            }
        }

        return null;

    }

    public static List<ProfilerTreeFunction> ToProfilerTreeFunctionList(List<KFunctionData> functionStackData)
    {
        List<ProfilerTreeFunction> rootFunctions = new List<ProfilerTreeFunction>();
        foreach (KFunctionData fundata in functionStackData)
        {
            bool ishaveRoot = false;
            foreach (ProfilerTreeFunction root in rootFunctions)
            {
                if (root.AddDescendant(fundata))
                {
                    ishaveRoot = true;
                    break;
                }
            }

            if (!ishaveRoot)
            {
                ProfilerTreeFunction root = ProfilerFunctionFactory.Parsing(fundata);
                rootFunctions.Add(root);
            }
        }

        return rootFunctions;
    }

    public static ProfilerTreeFunction Parsing(KFunctionData fundata)
    {
        ProfilerTreeFunction rootFunction = null;
        string[] functionnameArray = fundata.FunctionPath.Split('/');

        ProfilerTreeFunction tempParentFunction = null;

        foreach (string functionname in functionnameArray)
        {
            if (rootFunction == null)
            {
                rootFunction = new ProfilerTreeFunction(functionname, null);
                tempParentFunction = rootFunction;
                continue;
            }

            if (functionname == fundata.FunctionName)
            {
                ProfilerTreeFunction nodeFunction = tempParentFunction.GetChildWithCreate(functionname);
                nodeFunction.FunctionData = fundata;

                return rootFunction;
            }
            else
            {
                tempParentFunction = tempParentFunction.GetChildWithCreate(functionname);
            }
        }

        return null;

    }

    public static int GetSubFrame(List<SimpleTreeFunction> stFunList)
    {
        foreach(SimpleTreeFunction stFun in stFunList)
        {
            int sf = GetSubFrame(stFun);
            if (sf != -1)
            {
                return sf;
            }
        }

        return -1;
    }
    public static int GetSubFrame(SimpleTreeFunction stFun)
    {
        if (stFun.v != null)
        {
            return (int)stFun.v[0];
        }
        else
        {
            if (stFun.c != null)
            {
                foreach(SimpleTreeFunction stfun in stFun.c)
                {
                    int sf = GetSubFrame(stfun);
                    if (sf != -1)
                    {
                        return sf;
                    }
                }
            }
        }

        return -1;
    }
}

public class SubData
{
    //public int f { get; set; }
    public int name { get; set; }
    //值数组
    //public double[] val { get; set; }
    //*100
    public int total { get; set; }

    //*100
    public int self { get; set; }

    public int calls { get; set; }

    //*100
    public int gcalloc { get; set; }

    //*100
    public int timems { get; set; }

    //*100
    public int selfms { get; set; }

    public List<SubData> children { get; set; }
}

public class AllSub
{
    public List<SubData> allsub { get; set; }
}

public class RenderData
{
    public List<SubRenderData> renderData { get; set; }
}

public class SubRenderData
{

    public SubRenderData() { }
    public SubRenderData(long name, float beginTime, float durTime) 
    {
        this.name = name;
        this.beginTime = beginTime;
        this.durTime = durTime;
        this.children = new List<SubRenderData>();
    }
    public long name { get; set; }
    public float beginTime { get; set; }
    public float durTime { get; set; }
    public List<SubRenderData> children { get; set; }
}

public class AllTimeLine
{
    public List<TimeLineFunData> timelinList { get; set; }
}

public class TimeLineFunData
{
    public TimeLineFunData() { }

    public void init()
    {
        children.Clear();
    }

    public TimeLineFunData(int name, string funname, float beginTime, float durTime)
    {
        this.name = name;
        this.funname = funname;
        this.beginTime = (decimal)beginTime;
        this.durTime = (decimal)durTime;
        this.children = new List<TimeLineFunData>();
    }
    public int name { get; set; }
    public string funname { get; set; }
    public decimal beginTime { get; set; }
    public decimal durTime { get; set; }
    public List<TimeLineFunData> children { get; set; }
}

public class AllRenderData
{
    public List<RenderData> alldata { get; set; }
}

public class CaseData
{
    public List<AllSub> alldata { get; set; }
}

public class FunHash
{
    public Dictionary<long, string> funhash { get; set; }
}

public class SimpleRawFunctions
{
    public List<SimpleFrame> frame { get; set; }
    public Dictionary<uint, string> funhash { get; set; }
}
public class SimpleFrame
{
    //总帧数
    public int f { get; set; }
    //子帧数
    public int s { get; set; }

    //堆栈跟节点列表
    public List<SimpleTreeFunction> r { get; set; }
}

public class SimpleTreeFunction
{
    //函数名hash
    public uint h { get; set; }

    //值数组
    public double[] v { get; set; }

    //子函数列表
    public List<SimpleTreeFunction> c { get; set; }

}

public class CaseFunRow
{
    //函数hash为主键
    public int _id;
    public List<CaseFunRowInfo> frames { get; set; }
}

public class CaseRender
{
    //函数hash为主键
    public int _id;
    public List<CaseRenderInfo> frames { get; set; }
}

public class CaseRenderInfo
{
    public int frame { get; set; }
    public float timems { get; set; }
    public float selfms { get; set; }
}

public class CaseFunRowInfo
{
    public int frame { get; set; }
    public int total { get; set; }

    //*100
    public int self { get; set; }

    public int calls { get; set; }

    //*100
    public int gcalloc { get; set; }

    //*100
    public int timems { get; set; }

    //*100
    public int selfms { get; set; }
}
public class ProfilerTreeFunction
{
    public string NodeName = string.Empty;

    public KFunctionData FunctionData = null;

    public Dictionary<string, ProfilerTreeFunction> ChildFunctionMap = new Dictionary<string, ProfilerTreeFunction>();

    public ProfilerTreeFunction ParentFunction = null;

    public int count = 0;
    public ProfilerTreeFunction(string nodeName, ProfilerTreeFunction parentFunction)
    {
        NodeName = nodeName;
        ParentFunction = parentFunction;
    }

    public ProfilerTreeFunction(KFunctionData functionData, ProfilerTreeFunction parentFunction)
    {
        NodeName = functionData.FunctionName;
        FunctionData = functionData;
        ParentFunction = parentFunction;
    }

    public ProfilerTreeFunction GetChild(string nodeName)
    {
        ProfilerTreeFunction child;
        if (ChildFunctionMap.TryGetValue(nodeName, out child))
        {
            return child;
        }

        return null;
    }

    public ProfilerTreeFunction GetChildWithCreate(string nodeName)
    {
        ProfilerTreeFunction child = GetChild(nodeName);

        if (child == null)
        {
            child = new ProfilerTreeFunction(nodeName, this);
            ChildFunctionMap.Add(nodeName, child);
        }

        return child;
    }

    public bool AddDescendant(KFunctionData fundata)
    {
        string[] functionnameArray = fundata.FunctionPath.Split('/');

        if (functionnameArray.Length <= 0)
        {
            return false;
        }

        if (functionnameArray[0] != NodeName)
        {
            return false;
        }

        ProfilerTreeFunction tempParentFunction = this;

        for (int i = 1; i < functionnameArray.Length; i++)
        {
            string functionname = functionnameArray[i];

            ProfilerTreeFunction function = tempParentFunction.GetChildWithCreate(functionname);

            if ((i + 1) == functionnameArray.Length && fundata.FunctionName == functionname )
            {
                function.FunctionData = fundata;
                return true;
            }
            else
            {
                tempParentFunction = function;
            }
            
        }

        return false;
    }

    //public double[] GetFunctionDataValues()
    //{
    //    if (FunctionData != null)
    //    {
    //        double[] val = new double[6];
    //        //val[0] = FunctionData.frame;
    //        val[0] = FunctionData.TotalCPUPercent;
    //        val[1] = FunctionData.SelfCPUPercent;
    //        val[2] = FunctionData.Calls;
    //        val[3] = FunctionData.GCMemory_KB;
    //        val[4] = FunctionData.TotalTime_ms;
    //        val[5] = FunctionData.SelfTime_ms;

    //        return val;
    //    }

    //    return null;
    //}

    //public SimpleTreeFunction ToSimpleTreeFunction(ref Dictionary<uint, string> funHashMap)
    //{
    //    SimpleTreeFunction stFun = new SimpleTreeFunction();

    //    if (FunctionData != null)
    //    {
    //        stFun.h = (uint)CommonTools.UniqueHash(FunctionData.FunctionName); // (uint)CommonTools.UniqueHash(FunctionData.FunctionPath);

    //        if (!funHashMap.ContainsKey(stFun.h))
    //        {
    //            funHashMap.Add(stFun.h, FunctionData.FunctionName);
    //        }
    //    }
    //    else
    //    {
    //        stFun.h = (uint)CommonTools.UniqueHash(NodeName);

    //        if (!funHashMap.ContainsKey(stFun.h))
    //        {
    //            funHashMap.Add(stFun.h, NodeName);
    //        }
    //    }

    //    stFun.v = GetFunctionDataValues();

    //    if (ChildFunctionMap != null)
    //    {
    //        if (stFun.c == null)
    //        {
    //            stFun.c = new List<SimpleTreeFunction>();
    //        }

    //        foreach (var item in ChildFunctionMap)
    //        {
    //            ProfilerTreeFunction ptfun = item.Value;                
    //            stFun.c.Add(ptfun.ToSimpleTreeFunction(ref funHashMap));
    //        }
    //    }

    //    return stFun;

    //}

    public SubData GetchildFunc(ref Dictionary<long, string> funHashMap,ref int count,ref Dictionary<string,int> Digui)
    {
        SubData Fflame = new SubData();
    //    string unknowFun = "UnknownFunction";
    //    long unknowhash = CommonTools.UniqueHash(unknowFun);
    //    if (FunctionData != null)
    //    {
    //        if (FunctionData.FunctionName != "")
    //        {
    //            Fflame.name = CommonTools.UniqueHash(FunctionData.FunctionName); // (uint)CommonTools.UniqueHash(FunctionData.FunctionPath);

    //            if (!funHashMap.ContainsKey(Fflame.name))
    //            {
    //                funHashMap.Add(Fflame.name, FunctionData.FunctionName);
    //            }
    //        }
    //        else
    //        {
    //            Fflame.name = unknowhash;
    //            if (!funHashMap.ContainsKey(Fflame.name))
    //            {
    //                funHashMap.Add(Fflame.name, unknowFun);
    //            }
    //        }
    //    }
    //    else
    //    {
    //        if (NodeName != "")
    //        {
    //            Fflame.name = CommonTools.UniqueHash(NodeName);

    //            if (!funHashMap.ContainsKey(Fflame.name))
    //            {
    //                funHashMap.Add(Fflame.name, NodeName);
    //            }
    //        }
    //        else
    //        {
    //            Fflame.name = unknowhash;
    //            if (!funHashMap.ContainsKey(Fflame.name))
    //            {
    //                funHashMap.Add(Fflame.name, unknowFun);
    //            }
    //        }
    //    }
    //    if (FunctionData!= null)
    //    {
    //        Fflame.total = (int)(FunctionData.TotalCPUPercent * 100.0f);
    //        Fflame.self = (int)(FunctionData.SelfCPUPercent * 100.0f);
    //        Fflame.calls = (int)(FunctionData.Calls);
    //        Fflame.gcalloc = (int)(FunctionData.GCMemory_KB * 100.0f);
    //        Fflame.timems = (int)(FunctionData.TotalTime_ms * 100.0f);
    //        Fflame.selfms = (int)(FunctionData.SelfTime_ms * 100.0f);
    //    }
    //    //Fflame.val = GetFunctionDataValues();
    //    if (ChildFunctionMap != null)
    //    {
    //        if (Fflame.children == null)
    //        {
    //            Fflame.children = new List<SubData>();
    //        }
    //        foreach (var item in ChildFunctionMap)
    //        {
    //            ProfilerTreeFunction ptfun = item.Value;
    //            if (FunctionData !=null && FunctionData.FunctionName == ptfun.FunctionData.FunctionName)
    //            {
    //                if (!Digui.ContainsKey(ptfun.FunctionData.FunctionName))
    //                {
    //                    Digui.Add(ptfun.FunctionData.FunctionName,1);
    //                }
    //                else
    //                {
    //                    Digui[ptfun.FunctionData.FunctionName] += 1;
    //                }
    //                count += 1;
    //            }
    //            if (count < 30)
    //            {
    //                Fflame.children.Add(ptfun.GetchildFunc(ref funHashMap, ref count,ref Digui));
    //            }
    //            else
    //            {
    //                string isDg = string.Empty;
    //                foreach(var i in Digui)
    //                {
    //                    if (i.Value > 20)
    //                    {
    //                        isDg = i.Key;
    //                        Digui[i.Key] = 0;
    //                        //Debug.Log("出现循环函数，请确认是否有无限递归的情况");
    //                        break;
    //                    }
    //                }
    //                if (isDg != string.Empty)
    //                {
    //                    count = 0;
    //                    break;
    //                }
    //            }
    //        }
    //    }

        return Fflame;

    }

    public void GetFunRow(ref int f, ref Dictionary<long, CaseFunRowInfo> framefunrow)
    {
        long id = 0;
        long unknowhash = 91487915;
        CaseFunRowInfo cain = new CaseFunRowInfo();
        if (FunctionData != null)
        {
            if (FunctionData.FunctionName != "")
            {
                id = CommonTools.UniqueHash(FunctionData.FunctionName);
            }
            else
            {
                id = unknowhash;
            }
        }
        else if (NodeName != "")
        {
            id = CommonTools.UniqueHash(NodeName);
        }
        else if(NodeName == "")
        {
            id = unknowhash;
        }
        if (FunctionData != null)
        {
            cain.frame = f + 1;
            cain.total = (int)(FunctionData.TotalCPUPercent * 100.0f);
            cain.self = (int)(FunctionData.SelfCPUPercent * 100.0f);
            cain.calls = (int)(FunctionData.Calls);
            cain.gcalloc = (int)(FunctionData.GCMemory_KB * 100.0f);
            cain.timems = (int)(FunctionData.TotalTime_ms * 100.0f);
            cain.selfms = (int)(FunctionData.SelfTime_ms * 100.0f);
        }
        else
        {
            cain.frame = f + 1;
            cain.total = 0;
            cain.self = 0;
            cain.calls = 0;
            cain.gcalloc = 0;
            cain.timems = 0;
            cain.selfms = 0;
        }
        if (framefunrow.ContainsKey(id))
        {
            framefunrow[id].total += cain.total;
            framefunrow[id].self += cain.self;
            framefunrow[id].calls += cain.calls;
            framefunrow[id].gcalloc += cain.gcalloc;
            framefunrow[id].timems += cain.timems;
            framefunrow[id].selfms += cain.selfms;
        }
        else
        {
            framefunrow.Add(id, cain);
        }
        if (ChildFunctionMap != null)
        {
            foreach (var item in ChildFunctionMap)
            {
                ProfilerTreeFunction ptfun = item.Value;
                ptfun.GetFunRow(ref f,ref framefunrow);
            }
        }
    }
}

public class ProfilerTimeLineFunction
{
    public string NodeName = string.Empty;

    public KTimeLineData FunctionData = null;

    public Dictionary<string, ProfilerTimeLineFunction> ChildFunctionMap = new Dictionary<string, ProfilerTimeLineFunction>();

    public ProfilerTimeLineFunction ParentFunction = null;

    public int count = 0;
    public ProfilerTimeLineFunction(string nodeName, ProfilerTimeLineFunction parentFunction)
    {
        NodeName = nodeName;
        ParentFunction = parentFunction;
    }

    public ProfilerTimeLineFunction(KTimeLineData functionData, ProfilerTimeLineFunction parentFunction)
    {
        NodeName = functionData.functionName;
        FunctionData = functionData;
        ParentFunction = parentFunction;
    }

    public ProfilerTimeLineFunction GetChild(string nodeName)
    {
        ProfilerTimeLineFunction child;
        if (ChildFunctionMap.TryGetValue(nodeName, out child))
        {
            return child;
        }

        return null;
    }

    public ProfilerTimeLineFunction GetChildWithCreate(string nodeName)
    {
        ProfilerTimeLineFunction child = GetChild(nodeName);

        if (child == null)
        {
            child = new ProfilerTimeLineFunction(nodeName, this);
            ChildFunctionMap.Add(nodeName, child);
        }

        return child;
    }

    public bool AddDescendant(KTimeLineData fundata)
    {
        string[] functionnameArray = fundata.functionPath.Split('/');

        if (functionnameArray.Length <= 0)
        {
            return false;
        }

        if (functionnameArray[0] != NodeName)
        {
            return false;
        }

        ProfilerTimeLineFunction tempParentFunction = this;

        for (int i = 1; i < functionnameArray.Length; i++)
        {
            string functionname = functionnameArray[i];

            ProfilerTimeLineFunction function = tempParentFunction.GetChildWithCreate(functionname);

            if ((i + 1) == functionnameArray.Length && fundata.functionName == functionname)
            {
                function.FunctionData = fundata;
                return true;
            }
            else
            {
                tempParentFunction = function;
            }

        }

        return false;
    }

    public void GetRenderRow(ref int f, ref Dictionary<long, CaseRenderInfo> renderfunrow)
    {
        long id = 0;
        long unknowhash = 91487915;
        CaseRenderInfo renderinfo = new CaseRenderInfo();
        if (FunctionData != null)
        {
            if (FunctionData.functionName != "")
            {
                id = CommonTools.UniqueHash(FunctionData.functionName);
            }
            else
            {
                id = unknowhash;
            }
        }
        else if (NodeName != "")
        {
            id = CommonTools.UniqueHash(NodeName);
        }
        else if (NodeName == "")
        {
            id = unknowhash;
        }
        if (FunctionData != null)
        {
            renderinfo.frame = f + 1;
            renderinfo.timems = FunctionData.duration_ms;
        }
        else
        {
            renderinfo.frame = f + 1;
            renderinfo.selfms = 0;
        }
        if (renderfunrow.ContainsKey(id))
        {
            renderfunrow[id].timems += renderinfo.timems;
            renderfunrow[id].selfms += renderinfo.selfms;
        }
        else
        {
            renderfunrow.Add(id, renderinfo);
        }
        if (ChildFunctionMap != null)
        {
            double child_times = 0;
            foreach (var item in ChildFunctionMap)
            {
                ProfilerTimeLineFunction ptfun = item.Value;
                ptfun.GetRenderRow(ref f, ref renderfunrow);
                child_times += renderinfo.timems;
            }
            //renderinfo.selfms = renderinfo.timems - child_times;
        }
    }

    public SubRenderData GetchildTimeline(ref Dictionary<long, string> funHashMap, ref int count, ref Dictionary<string, int> Digui)
    {
        SubRenderData Fflame = new SubRenderData();
        string unknowFun = "UnknownFunction";
        long unknowhash = CommonTools.UniqueHash(unknowFun);
        if (FunctionData != null)
        {
            if (FunctionData.functionName != "")
            {
                Fflame.name = CommonTools.UniqueHash(FunctionData.functionName); // (uint)CommonTools.UniqueHash(FunctionData.FunctionPath);

                if (!funHashMap.ContainsKey(Fflame.name))
                {
                    funHashMap.Add(Fflame.name, FunctionData.functionName);
                }
            }
            else
            {
                Fflame.name = unknowhash;
                if (!funHashMap.ContainsKey(Fflame.name))
                {
                    funHashMap.Add(Fflame.name, unknowFun);
                }
            }
        }
        else
        {
            if (NodeName != "")
            {
                Fflame.name = CommonTools.UniqueHash(NodeName);

                if (!funHashMap.ContainsKey(Fflame.name))
                {
                    funHashMap.Add(Fflame.name, NodeName);
                }
            }
            else
            {
                Fflame.name = unknowhash;
                if (!funHashMap.ContainsKey(Fflame.name))
                {
                    funHashMap.Add(Fflame.name, unknowFun);
                }
            }
        }
        if (FunctionData != null)
        {
            Fflame.beginTime = FunctionData.funcStartTime_ms;
            Fflame.durTime = FunctionData.duration_ms;
        }
        if (ChildFunctionMap != null)
        {
            if (Fflame.children == null)
            {
                Fflame.children = new List<SubRenderData>();
            }
            foreach (var item in ChildFunctionMap)
            {
                ProfilerTimeLineFunction ptfun = item.Value;
                if (FunctionData != null && FunctionData.functionName == ptfun.FunctionData.functionName)
                {
                    if (!Digui.ContainsKey(ptfun.FunctionData.functionName))
                    {
                        Digui.Add(ptfun.FunctionData.functionName, 1);
                    }
                    else
                    {
                        Digui[ptfun.FunctionData.functionName] += 1;
                    }
                    count += 1;
                }
                if (count < 30)
                {
                    var child = ptfun.GetchildTimeline(ref funHashMap, ref count, ref Digui);
                    Fflame.children.Add(child);
                }
                else
                {
                    string isDg = string.Empty;
                    foreach (var i in Digui)
                    {
                        if (i.Value > 20)
                        {
                            isDg = i.Key;
                            Digui[i.Key] = 0;
                            //Debug.Log("出现循环函数，请确认是否有无限递归的情况");
                            break;
                        }
                    }
                    if (isDg != string.Empty)
                    {
                        count = 0;
                        break;
                    }
                }
            }
        }
        return Fflame;
    }

}

[Serializable]
public struct NameNode
{
    public string name;
}

[Serializable]
public class EvenetNode
{
    public string type;
    public int frame;
    public decimal at;
}

[Serializable]
public class ProfileNode
{
    public string type;
    public string name;
    public string unit;
    public decimal startValue;
    public decimal endValue;
    public List<EvenetNode> events;

    public ProfileNode()
    {
        this.type = "evented";
        this.unit = "milliseconds";
        this.events = new List<EvenetNode>();
    }
}

[Serializable]
public class Shared
{
    public List<NameNode> frames;

    public Shared()
    {
        this.frames = new List<NameNode>();
    }
}

public class FunSpeedscope
{
    public List<Speedscope> speedscope { get; set; }
}

[Serializable]
public class Speedscope
{
    public string schema = "https://www.speedscope.app/file-format-schema.json";
    public string case_uuid { get; set; }
    public string threadname { get; set; }
    public int frame_id { get; set; }
    public int activeProfileIndex;
    public Shared shared;
    public List<ProfileNode> profiles;

    public Speedscope(int activeProfileIndex,string uuid,string threadname,int frame_id)
    {
        this.case_uuid = uuid;
        this.frame_id = frame_id;
        this.threadname = threadname;
        this.activeProfileIndex = activeProfileIndex;
        this.shared = new Shared();
        this.profiles = new List<ProfileNode>();
    }
}

public class StackNode
{
    public TimeLineFunData timefundata;
    public EvenetNode eventData;

    public StackNode(TimeLineFunData subRenderData, EvenetNode eventData)
    {
        this.timefundata = subRenderData;
        this.eventData = eventData;
    }
}
public class ProfilerTree
{
    public bool shieldSwitch;

    public Stack<StackNode> stac = new Stack<StackNode>();

    public Stack<SubData> stafun = new Stack<SubData>();

    public Stack<TimeLineFunData> stactime = new Stack<TimeLineFunData>();

    private int MaxDeepF = 10;
    //TODO:优化结构，有bug，重复性的函数会产生bug

    public TimeLineFunData GetRenFun(List<KTimeLineData> sourcedata, ref Dictionary<int, string> funhashMap)
    {
        string unknowFun = "UnknownFunction";
        TimeLineFunData subre = new TimeLineFunData();
        subre.name = 0;
        subre.beginTime = 0;
        subre.durTime = 0;
        subre.children = new List<TimeLineFunData>();
        stactime.Push(subre);
        for (int i = 0; i < sourcedata.Count; i++)
        {
            KTimeLineData now = sourcedata[i];
            if (now.functionName == "")
            {
                now.functionName = unknowFun;
            }
            if (!funhashMap.ContainsKey(now.functionName.GetHashCode()))
            {
                funhashMap.Add(now.functionName.GetHashCode(), now.functionName);
            }
            string[] functionnameArray = now.functionPath.Split('/');
            if (subre.name == 0)
            {
                subre.name = functionnameArray[0].GetHashCode();
                subre.funname = functionnameArray[0];
            }
            string parentFunName = functionnameArray[functionnameArray.Length - 2];

            if (parentFunName == "")
            {
                parentFunName = unknowFun;
            }

            TimeLineFunData su = new TimeLineFunData(now.functionName.GetHashCode(), now.functionName, now.funcStartTime_ms, now.duration_ms);
            while (parentFunName.GetHashCode() != stactime.Peek().name)
            {
                stactime.Pop();
            }
            TimeLineFunData parentData = stactime.Peek();
            parentData.children.Add(su);
            stactime.Push(su);
        }
        return subre;
    }

    public TimeLineFunData GoToSpeedTree(List<KTimeLineData> sourcedata,string threadname, Speedscope speedscope,Dictionary<string,int> allfun, ref int funid)
    {
        string unknowFun = "UnknownFunction";

        ProfileNode profileNode = new ProfileNode();
        decimal threadbegin = 0;

        if (sourcedata.Count != 0)
        {
            threadbegin = (decimal)sourcedata[0].funcStartTime_ms;
        }

        profileNode.startValue = threadbegin;
        speedscope.shared.frames.Add(new NameNode { name = threadname });
        profileNode.name = threadname;

        if (!allfun.ContainsKey(threadname))
        {
            allfun.Add(threadname, funid++);
        }

        EvenetNode evenetNode = new EvenetNode { type = "O", frame = allfun[threadname], at = profileNode.startValue };
        profileNode.events.Add(evenetNode);

        speedscope.profiles.Add(profileNode);
        
        List<EvenetNode> events = speedscope.profiles[0].events;
        List<NameNode> frames = speedscope.shared.frames;
        TimeLineFunData subre = new TimeLineFunData(threadname.GetHashCode(), profileNode.name,  0, 0);
        
        stac.Push(new StackNode(subre, evenetNode));
        
       
        for (int i = 0; i < sourcedata.Count; i++)
        {
            KTimeLineData now = sourcedata[i];
            if (now.functionName == "")
            {
                now.functionName = unknowFun;
            }
            string[] functionnameArray = now.functionPath.Split('/');
            string parentFunName = functionnameArray[functionnameArray.Length - 2];

            if (parentFunName == "")
            {
                parentFunName = unknowFun;
            }
            if (!allfun.ContainsKey(now.functionName))
            {
                frames.Add(new NameNode { name = now.functionName });
                allfun.Add(now.functionName, funid++);
            }
            
            TimeLineFunData su = new TimeLineFunData(now.functionName.GetHashCode(), now.functionName, now.funcStartTime_ms, now.duration_ms);
            while (parentFunName != stac.Peek().timefundata.funname)
            {
                StackNode node = stac.Pop();
                EvenetNode beforeEventData1 = events[events.Count - 1];
                decimal endTime = Math.Max(beforeEventData1.at, node.eventData.at + node.timefundata.durTime);
                events.Add(new EvenetNode { type = "C", frame = node.eventData.frame, at = endTime });
            }
            TimeLineFunData parentData = stac.Peek().timefundata;
            EvenetNode beforeEventData2 = events[events.Count - 1];
            decimal beginTime = Math.Max(beforeEventData2.at, su.beginTime);
            EvenetNode eventData = new EvenetNode { type = "O", frame = allfun[su.funname], at = beginTime };
            parentData.children.Add(su);
            events.Add(eventData);
            stac.Push(new StackNode(su, eventData));
        }
        while (stac.Count != 0)
        {
            StackNode node = stac.Pop();
            EvenetNode beforeEventData3 = events[events.Count - 1];
            decimal endTime = Math.Max(beforeEventData3.at, node.eventData.at + node.timefundata.durTime);
            events.Add(new EvenetNode { type = "C", frame = node.eventData.frame, at = endTime });
        }

        EvenetNode eventNode = events[events.Count - 1];
        profileNode.endValue = eventNode.at = events[events.Count - 2].at;
        return subre;
    }

    //添加渲染线程数据
    public TimeLineFunData GoToRenSpeed(List<KTimeLineData> sourcedata, string threadname, Speedscope speedscope, Dictionary<string, int> allfun, ref int funid)
    {
        string unknowFun = "UnknownFunction";
        decimal threadbegin = 0;
        if (sourcedata.Count != 0)
        {
            threadbegin = (decimal)sourcedata[0].funcStartTime_ms;
        }

        ProfileNode profileNode = new ProfileNode();
        profileNode.startValue = threadbegin;
        speedscope.shared.frames.Add(new NameNode { name = threadname });
        profileNode.name = threadname;

        if (!allfun.ContainsKey(threadname))
        {
            allfun.Add(threadname, funid++);
        }

        EvenetNode evenetNode = new EvenetNode { type = "O", frame = allfun[threadname], at = profileNode.startValue };
        profileNode.events.Add(evenetNode);

        speedscope.profiles.Add(profileNode);

        List<EvenetNode> events = speedscope.profiles[1].events;
        List<NameNode> frames = speedscope.shared.frames;
        TimeLineFunData subre = new TimeLineFunData(threadname.GetHashCode(), profileNode.name, 0, 0);

        stac.Push(new StackNode(subre, evenetNode));

        for (int i = 0; i < sourcedata.Count; i++)
        {
            KTimeLineData now = sourcedata[i];
            if (now.functionName == "")
            {
                now.functionName = unknowFun;
            }
            string[] functionnameArray = now.functionPath.Split('/');
            string parentFunName = functionnameArray[functionnameArray.Length - 2];

            if (parentFunName == "")
            {
                parentFunName = unknowFun;
            }

            if (!allfun.ContainsKey(now.functionName))
            {
                frames.Add(new NameNode { name = now.functionName });
                allfun.Add(now.functionName, funid++);
            }

            TimeLineFunData su = new TimeLineFunData(now.functionName.GetHashCode(), now.functionName, now.funcStartTime_ms, now.duration_ms);
            while (parentFunName != stac.Peek().timefundata.funname)
            {
                StackNode node = stac.Pop();
                EvenetNode beforeEventData1 = events[events.Count - 1];
                decimal endTime = Math.Max(beforeEventData1.at, node.eventData.at + node.timefundata.durTime);
                events.Add(new EvenetNode { type = "C", frame = node.eventData.frame, at = endTime });
            }
            TimeLineFunData parentData = stac.Peek().timefundata;
            parentData.children.Add(su);

            EvenetNode beforeEventData2 = events[events.Count - 1];
            decimal beginTime = Math.Max(beforeEventData2.at, su.beginTime);
            EvenetNode eventData = new EvenetNode { type = "O", frame = allfun[su.funname], at = beginTime };

            events.Add(eventData);
            stac.Push(new StackNode(su, eventData));

        }
        while (stac.Count != 0)
        {
            StackNode node = stac.Pop();
            EvenetNode beforeEventData3 = events[events.Count - 1];
            decimal endTime = Math.Max(beforeEventData3.at, node.eventData.at + node.timefundata.durTime);
            events.Add(new EvenetNode { type = "C", frame = node.eventData.frame, at = endTime });
        }

        EvenetNode eventNode = events[events.Count - 1];
        profileNode.endValue = eventNode.at = events[events.Count - 2].at;
        return subre;
    }

    public SubData GetChildFun(List<KFunctionData> sourcedata, ref Dictionary<int, string> funhashMap)
    {
        string unknowFun = "UnknownFunction";
        SubData subda = new SubData();
        subda.name = 0;
        subda.children = new List<SubData>();
        subda.total = 0;
        subda.self = 0;
        subda.timems = 0;
        subda.selfms = 0;
        subda.calls = 0;
        subda.gcalloc = 0;
        stafun.Push(subda);
        for (int i = 0; i < sourcedata.Count; i++)
        {
            KFunctionData now = sourcedata[i];
            if (now.FunctionName == "")
            {
                now.FunctionName = unknowFun;
            }
            if (!funhashMap.ContainsKey(now.FunctionName.GetHashCode()))
            {
                funhashMap.Add(now.FunctionName.GetHashCode(), now.FunctionName);
            }
            string[] functionnameArray = now.FunctionPath.Split('/');
            if(subda.name == 0)
            {
                subda.name = functionnameArray[0].GetHashCode();
            }
            string parentFunName = functionnameArray[functionnameArray.Length - 2];

            if(parentFunName == "")
            {
                parentFunName = unknowFun;
            }

            SubData su = new SubData();
            su.name = now.FunctionName.GetHashCode();
            su.children = new List<SubData>();
            su.total = (int)(now.TotalCPUPercent * 100.0f);
            su.self = (int)(now.SelfCPUPercent * 100.0f);
            su.timems = (int)(now.TotalTime_ms * 100.0f);
            su.selfms = (int)(now.SelfTime_ms * 100.0f);
            su.calls = (int)(now.Calls * 100.0f);
            su.gcalloc = (int)(now.GCMemory_KB * 100.0f);
            while (parentFunName.GetHashCode() != stafun.Peek().name)
            {
                stafun.Pop();
            }
            SubData parentData = stafun.Peek();
            parentData.children.Add(su);
            stafun.Push(su);
        }
        return subda;
    }

    public SubData GetChildFunLimite(List<KFunctionData> sourcedata, ref Dictionary<int, string> funhashMap)
    {
        string unknowFun = "UnknownFunction";
        SubData subda = new SubData();
        subda.name = 0;
        subda.children = new List<SubData>();
        subda.total = 0;
        subda.self = 0;
        subda.timems = 0;
        subda.selfms = 0;
        subda.calls = 0;
        subda.gcalloc = 0;
        stafun.Push(subda);
        for (int i = 0; i < sourcedata.Count; i++)
        {
            KFunctionData now = sourcedata[i];
            if (now.FunctionName == "")
            {
                now.FunctionName = unknowFun;
            }
            if (!funhashMap.ContainsKey(now.FunctionName.GetHashCode()))
            {
                funhashMap.Add(now.FunctionName.GetHashCode(), now.FunctionName);
            }
            string[] functionnameArray = now.FunctionPath.Split('/');
            if (subda.name == 0)
            {
                subda.name = functionnameArray[0].GetHashCode();
            }
            string parentFunName = functionnameArray[functionnameArray.Length - 2];

            if (parentFunName == "")
            {
                parentFunName = unknowFun;
            }

            SubData su = new SubData();
            su.name = now.FunctionName.GetHashCode();
            su.children = new List<SubData>();
            su.total = (int)(now.TotalCPUPercent * 100.0f);
            su.self = (int)(now.SelfCPUPercent * 100.0f);
            su.timems = (int)(now.TotalTime_ms * 100.0f);
            su.selfms = (int)(now.SelfTime_ms * 100.0f);
            su.calls = (int)(now.Calls * 100.0f);
            su.gcalloc = (int)(now.GCMemory_KB * 100.0f);
            while (parentFunName.GetHashCode() != stafun.Peek().name)
            {
                stafun.Pop();
            }
            if (stafun.Count <= MaxDeepF)
            {
                SubData parentData = stafun.Peek();
                parentData.children.Add(su);
            }
            stafun.Push(su);
        }
        return subda;
    }

    public void GetRenderRow(TimeLineFunData subrenda, ref int f, ref Dictionary<int, CaseRenderInfo> renderfunrow, ref Dictionary<int,string> funhash)
    {
        if (!funhash.ContainsKey(subrenda.name))
        {
            funhash.Add(subrenda.name, subrenda.funname);
        }
        CaseRenderInfo renderinfo = new CaseRenderInfo();
        renderinfo.frame = f + 1;
        renderinfo.timems = (float)subrenda.durTime;
        if (renderfunrow.ContainsKey(subrenda.name))
        {
            renderfunrow[subrenda.name].timems += renderinfo.timems;
            renderfunrow[subrenda.name].selfms += renderinfo.selfms;
        }
        else
        {
            renderfunrow.Add(subrenda.name, renderinfo);
        }
        if (subrenda.children.Count != 0)
        {
            float child_times = 0;
            foreach (var item in subrenda.children)
            {
                if (!funhash.ContainsKey(item.name))
                {
                    funhash.Add(item.name, item.funname);
                }
                GetRenderRow(item, ref f, ref renderfunrow,ref funhash);
                child_times += renderinfo.timems;
            }
            renderinfo.selfms = renderinfo.timems - child_times;
            if (renderinfo.selfms < 0)
            {
                renderinfo.selfms = 0;
            }
        }
    }

    public void GetFunRow(SubData subda,ref int f, ref Dictionary<int, CaseFunRowInfo> framefunrow)
    {
        string shieldFun = "Semaphore.WaitForSignal";
        CaseFunRowInfo cain = new CaseFunRowInfo();
        cain.frame = f + 1;
        cain.total = subda.total;
        cain.self = subda.self;
        cain.timems = subda.timems;
        cain.selfms = subda.selfms;
        cain.calls = subda.calls;
        cain.gcalloc = subda.gcalloc;
        if (!framefunrow.ContainsKey(subda.name))
        {
            framefunrow.Add(subda.name, cain);
        }
        else
        {
            framefunrow[subda.name].total += cain.total;
            framefunrow[subda.name].self += cain.self;
            framefunrow[subda.name].calls += cain.calls;
            framefunrow[subda.name].gcalloc += cain.gcalloc;
            framefunrow[subda.name].timems += cain.timems;
            framefunrow[subda.name].selfms += cain.selfms;
        }
        if (subda.children.Count != 0)
        {
            foreach(var item in subda.children)
            {
                if(shieldSwitch == true)
                {
                    if (shieldFun.GetHashCode() == item.name)
                    {
                        subda.selfms += item.selfms;
                    }
                    else
                    {
                        GetFunRow(item, ref f, ref framefunrow);
                    }
                }
                else
                {
                    GetFunRow(item, ref f, ref framefunrow);
                }
            }
        }
    }
}