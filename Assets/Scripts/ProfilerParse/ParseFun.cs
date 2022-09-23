using System.Collections.Generic;

namespace Analyze
{
    internal class ParseFun
    {
        public static void GetParsedFunRowjson(int f, ref Dictionary<int, CaseFunRow> allcasefunrow, ProfilerTree prt, SubData suda)
        {
            Dictionary<int, CaseFunRowInfo> framefunrow = new Dictionary<int, CaseFunRowInfo>();
            prt.GetFunRow(suda, ref f, ref framefunrow);
            foreach (var key in framefunrow.Keys)
            {
                if (!allcasefunrow.ContainsKey(key))
                {
                    CaseFunRow casf = new CaseFunRow();
                    casf._id = key;
                    casf.frames = new List<CaseFunRowInfo>();
                    allcasefunrow.Add(key, casf);
                }
                allcasefunrow[key].frames.Add(framefunrow[key]);
            }
        }

        public static void GetParsedTimelineRowjson(int f, ref Dictionary<int, CaseRender> allcasefunrow, ProfilerTree prt, TimeLineFunData surenda, ref Dictionary<int, string> funhash)
        {
            Dictionary<int, CaseRenderInfo> framefunrow = new Dictionary<int, CaseRenderInfo>();
            prt.GetRenderRow(surenda, ref f, ref framefunrow, ref funhash);
            foreach (var key in framefunrow.Keys)
            {
                if (!allcasefunrow.ContainsKey(key))
                {
                    CaseRender casf = new CaseRender();
                    casf._id = key;
                    casf.frames = new List<CaseRenderInfo>();
                    allcasefunrow.Add(key, casf);
                }
                allcasefunrow[key].frames.Add(framefunrow[key]);
            }
        }
    }
}
