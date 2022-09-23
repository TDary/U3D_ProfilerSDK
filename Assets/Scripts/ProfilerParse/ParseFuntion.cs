using OProfilerData;
using System;
using System.Collections.Generic;

public class ParseFuntion
{
    public class FunSpeedscope
    {
        public List<Speedscope> speedscope { get; set; }
    }

    public class ProfilerTrees
    {
        public bool shieldSwitch;

        public Stack<StackNode> stac = new Stack<StackNode>();

        public Stack<SubData> stafun = new Stack<SubData>();

        public Stack<TimeLineFunData> stactime = new Stack<TimeLineFunData>();

        private int MaxDeepF = 10;
        //TODO:优化结构，有bug，重复性的函数会产生bug

        public TimeLineFunData GetRenFun(List<TimeLineData> sourcedata, ref Dictionary<int, string> funhashMap)
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
                TimeLineData now = sourcedata[i];
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

        public TimeLineFunData GoToSpeedTree(List<TimeLineData> sourcedata, string threadname, Speedscope speedscope, Dictionary<string, int> allfun, ref int funid)
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
            TimeLineFunData subre = new TimeLineFunData(threadname.GetHashCode(), profileNode.name, 0, 0);

            stac.Push(new StackNode(subre, evenetNode));


            for (int i = 0; i < sourcedata.Count; i++)
            {
                TimeLineData now = sourcedata[i];
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
        public TimeLineFunData GoToRenSpeed(List<TimeLineData> sourcedata, string threadname, Speedscope speedscope, Dictionary<string, int> allfun, ref int funid)
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
                TimeLineData now = sourcedata[i];
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

        public SubData GetChildFun(List<FunctionData> sourcedata, ref Dictionary<int, string> funhashMap)
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
                FunctionData now = sourcedata[i];
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
                SubData parentData = stafun.Peek();
                parentData.children.Add(su);
                stafun.Push(su);
            }
            return subda;
        }

        public SubData GetChildFunLimite(List<FunctionData> sourcedata, ref Dictionary<int, string> funhashMap)
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
                FunctionData now = sourcedata[i];
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

        public void GetRenderRow(TimeLineFunData subrenda, ref int f, ref Dictionary<int, CaseRenderInfo> renderfunrow, ref Dictionary<int, string> funhash)
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
                    GetRenderRow(item, ref f, ref renderfunrow, ref funhash);
                    child_times += renderinfo.timems;
                }
                renderinfo.selfms = renderinfo.timems - child_times;
                if (renderinfo.selfms < 0)
                {
                    renderinfo.selfms = 0;
                }
            }
        }

        public void GetFunRow(SubData subda, ref int f, ref Dictionary<int, CaseFunRowInfo> framefunrow)
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
                foreach (var item in subda.children)
                {
                    if (shieldSwitch == true)
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
}
