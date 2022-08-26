using System;
using UnityEngine;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using KProcessedData;
using KProfilerData;
using System.Text;

public class TimeLineDataAnalyzer
{
    /// <summary>
    /// 将TimeLine数据添加到UploadData中。
    /// </summary>
    /// <returns></returns>
    //public static void AnalyzeData(UploadData timeLineUploadDatas, AddToSetDatas timeLineAddToSetDatas, List<KTimeLineData> timeLineDatas)
    //{
    //    try
    //    {
    //        for (int i = 0; i < timeLineDatas.Count; i++) {
    //            KTimeLineData tempData = timeLineDatas[i];
    //            timeLineUploadDatas.RightPush(tempData.functionPath, tempData.ToString(), tempData.frame);
    //            timeLineAddToSetDatas.AddToSet(tempData.functionPath);
    //        }
    //    }
    //    catch
    //    {
    //        //添加数据时出错
    //    }
    //}

    public static void AnalyzeData(StringBuilder sb_timeLineDatas, List<KTimeLineData> timeLineDatas) {
        try {
            for (int i = 0; i < timeLineDatas.Count; i++) {
                KTimeLineData tempData = timeLineDatas[i];
                sb_timeLineDatas.AppendLine(tempData.ToString());
            }
        }
        catch {
            //添加数据时出错
        }
    }
}
