using UnityEditorInternal;
using OProfilerData;

internal class BasicDataAL
{
    /// <summary>
    /// 提取单帧Base数据。
    /// profilerProperty是由指定帧提取出来的profiler数据。
    /// actualFrame是目标数据在所有profiler data文件中所在的帧数,用于标识数据位置。
    /// </summary>
    /// <param name="profilerProperty"></param>
    /// <param name="actualFrame"></param>
    /// <returns></returns>
    public static BaseData AnalyzeData(ProfilerProperty profilerProperty, int actualFrame)
    {
        BaseData baseData = ProfilerHelper.ExtractBaseData(profilerProperty, actualFrame);
        return baseData;
    }
}
