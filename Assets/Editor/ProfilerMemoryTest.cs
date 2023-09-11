using OProcessedData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Profiling;
using Tools;

public class ProfilerMemoryTest:EditorWindow
{
    private struct AllData
    {
        public long Reserved { get; set; }
        public long Used { get; set; }
    }
    private class FileSystem
    {
        public string Path { get;  set; }
        public int Timstemp { get;  set; }
    }
    private static ProfilerMemoryTest Instance;
    private string rawPath;
    [MenuItem("profiler/内存专项解析测试")]
    public static void profilerTest()
    {
        if (Instance == null)
        {
            Instance = ScriptableObject.CreateInstance<ProfilerMemoryTest>();
        }
        Instance.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("请输入解析的原文件目标文件夹：");
        rawPath = EditorGUILayout.TextField(rawPath);
        if (GUILayout.Button("解析内存专项测试"))
        {
            List<FileSystem> allRawFileList = new List<FileSystem>();
            DirectoryInfo folder = new DirectoryInfo(rawPath);
            foreach (FileInfo file in folder.GetFiles("*.*",SearchOption.AllDirectories))  //将原始文件收集起来
            {
                if (file.Name.Contains(".raw")||file.Name.Contains(".data"))
                {
                    FileSystem f = new FileSystem();
                    f.Timstemp = GetFileTimeStamp(file.Name);
                    f.Path = file.FullName;
                    allRawFileList.Add(f);
                }
            }
            SortList(allRawFileList);
            Analyze(allRawFileList);
        }
    }

    private void Analyze(List<FileSystem> RawsList)
    {
        List<AllData> all = new List<AllData>();
        foreach (var raw in RawsList)
        {
            ProfilerDriver.ClearAllFrames();
            Profiler.AddFramesFromFile(raw.Path);
            int firstIndex = ProfilerDriver.firstFrameIndex;
            int lastIndex = ProfilerDriver.lastFrameIndex;
            //Debug.Log("ProfilerDriver.firstFrameIndex: " + ProfilerDriver.firstFrameIndex);
            //Debug.Log("ProfilerDriver.lastFrameIndex: " + ProfilerDriver.lastFrameIndex);
            if (lastIndex == -1 || firstIndex == -1)
            {
                throw new Exception("ProfilerDriver can't work");
            }
            for (int f = firstIndex; f < lastIndex; f++)
            {
                RawFrameDataView rawFrameDataView = ProfilerDriver.GetRawFrameDataView(f, 0);
                if (rawFrameDataView != null && rawFrameDataView.valid)
                {
                    long totalusedMemory = GetContextValue(rawFrameDataView, "Total Used Memory");
                    long totalreservedMemory = GetContextValue(rawFrameDataView, "Total Reserved Memory");
                    if (totalreservedMemory ==0 || totalusedMemory==0)
                    {
                        Debug.LogWarning("数据异常，跳过该帧：" + f);
                        continue;
                    }
                    AllData ad = new AllData();
                    ad.Reserved = totalreservedMemory / 1024 / 1024;
                    ad.Used = totalusedMemory / 1024 / 1024;
                    all.Add(ad);
                }
            }
        }
        string filePath = @"D:\Memory";

        if (!Directory.Exists(filePath))
        {
            System.IO.Directory.CreateDirectory(filePath);
        }

        string filename = "Memory.csv";
        using (var sw = new StreamWriter(new FileStream(Path.Combine(filePath, filename), FileMode.Create, FileAccess.Write, FileShare.ReadWrite), new System.Text.UTF8Encoding(false)))
        {
            sw.WriteLine("TotalReservedMemory,TotalUsedMemory");
            foreach(var s in all)
            {
                sw.WriteLine($"{s.Reserved},{s.Used}");
            }
        }
    }
    private long GetContextValue(FrameDataView frameData, string name)
    {
        int markerId = frameData.GetMarkerId(name);
        return markerId == -1 ? -1L : frameData.GetCounterValueAsLong(markerId);
    }

    private void SortList(List<FileSystem> fileList) //冒泡排序
    {
        for (int i = 0; i < fileList.Count - 1; i++)
        {
            for (int j = 0; j < fileList.Count - i - 1; j++)
            {
                if (fileList[j].Timstemp > fileList[j + 1].Timstemp)
                {
                    fileList[j] = fileList[j + 1];
                    fileList[j + 1] = fileList[j];
                }
            }
        }
    }

    /// <summary>
    /// 把一个文件名转化为时间戳
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    private int GetFileTimeStamp(string filename)
    {
        string[] filenameArray = filename.Split('.');

        if (filenameArray != null && filenameArray.Length > 0)
        {
            int timestamp = 0;
            if (int.TryParse(filenameArray[0], out timestamp))
            {
                return timestamp;
            }
        }

        return 0;
    }
}
