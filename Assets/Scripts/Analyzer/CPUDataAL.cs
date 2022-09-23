using OProcessedData;
#if UNITY_2018_3_OR_NEWER
using UnityEngine.Profiling;
#endif

internal class CPUDataAL
{
    /// <summary>
    /// 提取单帧CPU数据。
    /// dataFrame是目标数据在当前profiler data中所在的帧数，用于提取目标数据, 不能小于0。
    /// actualFrame是目标数据在所有profiler data文件中所在的帧数,用于标识数据位置。
    /// </summary>
    /// <param name="dataFrame"></param>
    /// <param name="actualFrame"></param>
    /// <returns></returns>
    public static AreaData AnalyzeData(int dataFrame, int actualFrame)
    {
        AreaData cpuData = ProfilerHelper.ExtractAreaData(dataFrame, ProfilerArea.CPU);
        return cpuData;
    }
}
