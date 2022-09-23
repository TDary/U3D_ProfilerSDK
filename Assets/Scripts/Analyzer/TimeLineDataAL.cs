using System.Collections.Generic;
using System.Text;
using OProfilerData;


internal class TimeLineDataAL
{
    public static void AnalyzeData(StringBuilder sb_timeLineDatas, List<TimeLineData> timeLineDatas)
    {
        try
        {
            for (int i = 0; i < timeLineDatas.Count; i++)
            {
                TimeLineData tempData = timeLineDatas[i];
                sb_timeLineDatas.AppendLine(tempData.ToString());
            }
        }
        catch
        {
            //添加数据出错
        }
    }
}
