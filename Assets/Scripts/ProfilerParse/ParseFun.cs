using System.Collections.Generic;
using static ParseFuntion;

public class ParseFun
{
    public static void GetParsedFunRowjson(int f, ref Dictionary<string, CaseFunRow> allcasefunrow, ProfilerTrees prt, SubData suda)
    {
        Dictionary<string, CaseFunRowInfo> framefunrow = new Dictionary<string, CaseFunRowInfo>();
        prt.GetFunRow(suda, ref f, ref framefunrow);
        foreach (var key in framefunrow.Keys)
        {
            if (!allcasefunrow.ContainsKey(key))
            {
                CaseFunRow casf = new CaseFunRow();
                casf.name = key;
                casf.frames = new List<CaseFunRowInfo>();
                allcasefunrow.Add(key, casf);
            }
            allcasefunrow[key].frames.Add(framefunrow[key]);
        }
    }

    public static void GetParsedTimelineRowjson(int f, ref Dictionary<string, CaseRender> allcasefunrow, ProfilerTrees prt, TimeLineFunData surenda)
    {
        Dictionary<string, CaseRenderInfo> framefunrow = new Dictionary<string, CaseRenderInfo>();
        prt.GetRenderRow(surenda, ref f, ref framefunrow);
        foreach (var key in framefunrow.Keys)
        {
            if (!allcasefunrow.ContainsKey(key))
            {
                CaseRender casf = new CaseRender();
                casf.name = key;
                casf.frames = new List<CaseRenderInfo>();
                allcasefunrow.Add(key, casf);
            }
            allcasefunrow[key].frames.Add(framefunrow[key]);
        }
    }
}
